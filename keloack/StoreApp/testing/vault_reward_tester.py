#!/usr/bin/env python3
"""
vault_reward_tester.py
Validate the cryptographic strength and performance of the Rewards API
that uses HashiCorp Vault Transit to encrypt discounts at‑rest.

-------------------------------------------------------------
Usage
-------------------------------------------------------------
# 1. Ensure Rewards API & Vault are running locally
export REWARDS_URL="http://localhost:5268/api/rewards"   # change if needed

# 2. Install dependencies (once)
python -m pip install --upgrade requests pandas matplotlib numpy scipy tqdm

# 3. Execute the test suite
python vault_reward_tester.py
-------------------------------------------------------------
Outputs
-------------------------------------------------------------
• latency_hist.png   – histogram of encrypt / decrypt latency
• entropy_hist.png   – histogram of ciphertext Shannon entropy
• Console summary    – p95 latency, entropy stats, semantic‑security check
"""

import base64
import binascii
import os
import random
import time
from collections import Counter

import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
import requests
from scipy.stats import entropy
from tqdm import tqdm

# ---------------------------------------------------------------------------
# Config – tweak these to taste
# ---------------------------------------------------------------------------
BASE_URL   = os.getenv("REWARDS_URL", "http://localhost:5268/api/rewards")
N_IDENTICAL = 100    # identical plaintexts (semantic‑security proof)
N_RANDOM    = 400    # random plaintexts (typical workload)

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------
def rand_discount() -> str:
    """Return a random discount string like '37.45'."""
    return f"{random.randint(1, 100)}.{random.randint(0,99):02d}"

def _safe_b64_decode(vault_ct: str) -> bytes | None:
    """
    Extract and Base‑64‑decode the payload from a Vault ciphertext.
    Vault format:  vault:<version>:<b64>
    Returns raw bytes, or None on decode failure.
    """
    if vault_ct.startswith("vault:"):
        vault_ct = vault_ct.split(":", 2)[2]

    # Pad so length % 4 == 0
    vault_ct += "=" * (-len(vault_ct) % 4)

    try:
        return base64.b64decode(vault_ct, validate=True)
    except binascii.Error:
        return None

def shannon_entropy(blob: bytes) -> float:
    """Compute Shannon entropy (bits per byte) of a byte sequence."""
    if not blob:
        return 0.0
    counts = np.array(list(Counter(blob).values()), dtype=float)
    probs  = counts / counts.sum()
    return entropy(probs, base=2)

def create_reward(session, name: str, discount: str):
    t0 = time.perf_counter()
    r  = session.post(
        BASE_URL,
        json={"name": name, "discount": discount},
        timeout=10,
    )
    latency_ms = (time.perf_counter() - t0) * 1000
    r.raise_for_status()
    return r.json(), latency_ms

def decrypt_reward(session, rid: int):
    t0 = time.perf_counter()
    r  = session.get(f"{BASE_URL}/decrypt/{rid}", timeout=10)
    latency_ms = (time.perf_counter() - t0) * 1000
    r.raise_for_status()
    return r.json(), latency_ms

# ---------------------------------------------------------------------------
# Main battery of tests
# ---------------------------------------------------------------------------
def main():
    print(f"Testing Rewards API at {BASE_URL}\n")
    session = requests.Session()
    rows    = []

    # ---------- identical‑plaintext (semantic‑security) ----------------------
    print(f"▶ Inserting {N_IDENTICAL} *identical* discounts (semantic‑security test)")
    identical_plain = "42.00"
    for i in tqdm(range(N_IDENTICAL), unit="req"):
        name = f"id_same_{i}"
        created, enc_ms = create_reward(session, name, identical_plain)
        rid = created["id"]
        cipher = created["encryptedDiscount"]

        decrypted, dec_ms = decrypt_reward(session, rid)
        assert decrypted["discount"] == float(identical_plain), "round‑trip failed!"

        rows.append(
            {
                "cipher": cipher,
                "plain": identical_plain,
                "enc_ms": enc_ms,
                "dec_ms": dec_ms,
                "kind": "identical",
            }
        )

    # ---------- random‑plaintext workload -----------------------------------
    print(f"▶ Inserting {N_RANDOM} random discounts (workload / latency test)")
    for i in tqdm(range(N_RANDOM), unit="req"):
        disc = rand_discount()
        name = f"id_rand_{i}"
        created, enc_ms = create_reward(session, name, disc)
        rid = created["id"]
        cipher = created["encryptedDiscount"]

        decrypted, dec_ms = decrypt_reward(session, rid)
        assert decrypted["discount"] == float(disc), "round‑trip failed!"

        rows.append(
            {
                "cipher": cipher,
                "plain": disc,
                "enc_ms": enc_ms,
                "dec_ms": dec_ms,
                "kind": "random",
            }
        )

    df = pd.DataFrame(rows)
    print("\n✓ Functional round‑trip: all discounts decrypted correctly")

    # -----------------------------------------------------------------------
    # Metrics & assertions
    # -----------------------------------------------------------------------
    # A) Semantic security: equal plaintext → unique ciphertext
    uniques = df[df.kind == "identical"]["cipher"].nunique()
    print(f"Semantic security check: {uniques}/{N_IDENTICAL} ciphertexts "
          f"unique ({uniques / N_IDENTICAL:.0%})")

    # B) Compute entropy for every ciphertext
    ent_vals = df["cipher"].apply(lambda c: shannon_entropy(_safe_b64_decode(c)))
    mean_entropy = ent_vals.mean()
    print(f"Ciphertext Shannon entropy: mean={mean_entropy:.2f} bits/byte "
          f"(ideal random ≈ 8)")

    # C) Latency statistics
    p95_enc = np.percentile(df["enc_ms"], 95)
    p95_dec = np.percentile(df["dec_ms"], 95)
    print(f"Encrypt latency: p95 = {p95_enc:.2f} ms (mean={df['enc_ms'].mean():.2f})")
    print(f"Decrypt latency: p95 = {p95_dec:.2f} ms (mean={df['dec_ms'].mean():.2f})")

    # -----------------------------------------------------------------------
    # Graph A – latency histogram
    # -----------------------------------------------------------------------
    plt.figure(figsize=(8, 4))
    plt.hist(df["enc_ms"], bins=30, alpha=0.6, label="Encrypt")
    plt.hist(df["dec_ms"], bins=30, alpha=0.6, label="Decrypt")
    plt.xlabel("Latency (ms)")
    plt.ylabel("Count")
    plt.title("Vault Transit‑Encryption Latency")
    plt.legend()
    plt.tight_layout()
    plt.savefig("latency_hist.png", dpi=150)

    # -----------------------------------------------------------------------
    # Graph B – entropy distribution
    # -----------------------------------------------------------------------
    plt.figure(figsize=(5, 4))
    plt.hist(ent_vals, bins=25, color="orange")
    plt.xlabel("Shannon entropy (bits per byte)")
    plt.title("Ciphertext Randomness")
    plt.tight_layout()
    plt.savefig("entropy_hist.png", dpi=150)

    print("\nGraphs saved →  latency_hist.png   entropy_hist.png")
    print("Done.")

if __name__ == "__main__":
    main()
