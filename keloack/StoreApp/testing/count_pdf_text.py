#!/usr/bin/env python3
"""
count_pdf_text.py – Count words and characters in a PDF document.

Requires: pdfplumber  (pip install pdfplumber)
"""

import argparse
from pathlib import Path
import sys

try:
    import pdfplumber
except ImportError:
    sys.exit("pdfplumber is not installed. Install it with:  pip install pdfplumber")

def extract_text(pdf_path: Path) -> str:
    """Extract all text from every page of the PDF."""
    text_parts = []
    with pdfplumber.open(pdf_path) as pdf:
        for page in pdf.pages:
            text_parts.append(page.extract_text() or "")
    return "\n".join(text_parts)

def main() -> None:
    parser = argparse.ArgumentParser(
        description="Count the number of words and characters in a PDF file."
    )
    parser.add_argument("pdf_file", type=Path, help="Path to the PDF file")
    args = parser.parse_args()

    if not args.pdf_file.is_file():
        sys.exit(f"Error: '{args.pdf_file}' is not a valid file.")

    text = extract_text(args.pdf_file)

    num_words = len(text.split())
    num_chars_incl_spaces = len(text)
    num_chars_excl_spaces = len(text.replace(" ", "").replace("\n", ""))

    print(f"📄 {args.pdf_file.name}")
    print(f"  • Words:                    {num_words}")
    print(f"  • Characters (with spaces): {num_chars_incl_spaces}")
    print(f"  • Characters (no spaces):   {num_chars_excl_spaces}")

if __name__ == "__main__":
    main()
