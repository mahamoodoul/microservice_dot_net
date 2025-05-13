// ------------------------------------------------------------------
// load_test_both_fixed.js

import http from 'k6/http';
import { check, sleep } from 'k6';
import { b64decode } from 'k6/encoding';
import { randomOrderRequest } from './productGenerator.js';


// Decode a URL‑safe base64 string into a UTF‑8 JS string
function urlSafeB64Decode(input) {
    let str = input.replace(/-/g, '+').replace(/_/g, '/');
    const pad = str.length % 4;
    if (pad > 0) {
      str += '='.repeat(4 - pad);
    }
    // pass 'utf-8' so b64decode returns a proper JS string
    return b64decode(str, 'utf-8');
  }

// ─── CONFIGURATION ──────────────────────────────────────────────────
// the root URL is just http://host:port; pre‑17 often requires /auth.
const KEYCLOAK_URL   = __ENV.KEYCLOAK_URL   || 'http://localhost:8080';
const REALM          = __ENV.REALM           || 'StoreRealm';
const CLIENT_ID      = __ENV.CLIENT_ID       || 'store-app-client';
const CLIENT_SECRET  = __ENV.CLIENT_SECRET   || 'x3t7EBxXHMpCx9GnZy3PcwpfMPTd9vtF';
const USERNAME       = __ENV.USERNAME        || 'admin';
const PASSWORD       = __ENV.PASSWORD        || 'Admin123@';
// Choose "password" or "client_credentials" via ENV or default to password
const GRANT_TYPE     = __ENV.GRANT_TYPE      || 'password';

// Construct the token endpoint URL from the discovery document
const TOKEN_URL      = `${KEYCLOAK_URL}/realms/${REALM}/protocol/openid-connect/token`;

// Your application endpoints
const API_URL        = __ENV.API_URL         || 'http://localhost:5279/';
const ORDER_PATH     = __ENV.ORDER_PATH      || 'http://localhost:5199';
const REWARDS_PATH   = __ENV.REWARDS_PATH    || 'http://localhost:5268/api/rewards';

// ─── k6 OPTIONS ─────────────────────────────────────────────────────
export let options = {
  scenarios: {
    normalLoad:   { executor: 'constant-vus', exec: 'runStoreAndOrder', vus: 20,  duration: '2m' },
    peakLoad:     { executor: 'constant-vus', exec: 'runStoreAndOrder', vus: 100, duration: '30s', startTime: '2m' },
    rewardsSpike: { executor: 'constant-vus', exec: 'runRewards',       vus: 50,  duration: '1m',  startTime: '2m30s' },
  },
  thresholds: {
    // Latency & error rate SLAs
    'http_req_duration':                 ['p(95)<500'],
    'http_req_failed':                   ['rate<0.01'],
    'http_req_failed{endpoint:store}':   ['rate<0.01'],
    'http_req_failed{endpoint:order}':   ['rate<0.01'],
    'http_req_failed{endpoint:rewards}':['rate<0.01'],
    'http_req_duration{endpoint:order}': ['p(95)<700'],
  },
};

// ─── TOKEN FETCHER ──────────────────────────────────────────────────
function getToken() {
  // Build form‑data payload
  let payload = {
    grant_type: GRANT_TYPE,
    client_id:  CLIENT_ID,
  };

  if (GRANT_TYPE === 'password') {
    // Resource Owner Password Credentials flow
    payload.username      = USERNAME;
    payload.password      = PASSWORD;
    payload.scope         = 'openid';
    // Must include client_secret for confidential clients
    payload.client_secret = CLIENT_SECRET;
  } else if (GRANT_TYPE === 'client_credentials') {
    // Client Credentials flow
    payload.client_secret = CLIENT_SECRET;
  }

  // URL‑encode the form payload
  const body = Object.entries(payload)
    .map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(v)}`)
    .join('&');

  const params = {
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    tags:    { endpoint: 'token' },
  };

  // Fetch the token
  const res = http.post(TOKEN_URL, body, params);

  // Check for success, log body on failure
  check(res, { 'token 200': (r) => r.status === 200 }) ||
    console.error(`Token fetch failed (${res.status}): ${res.body}`);

  const token = res.json('access_token') || '';
  check(token, { 'got token': (t) => t.length > 0 }) ||
    console.error('access_token missing in response');



  return token;
}

// ─── SCENARIO: STORE + ORDER ────────────────────────────────────────
export function runStoreAndOrder() {
  const token = getToken();
  if (!token) {
    // Abort this iteration if we couldn't get a token
    return;
  }

  // Hit the store homepage
  const storeRes = http.get(API_URL, {
    headers: { Authorization: `Bearer ${token}` },
    tags:    { endpoint: 'store' },
  });
  check(storeRes, { 'store 200': (r) => r.status === 200 });

  // Create a new random order
  const orderRes = http.post(
    `${ORDER_PATH}/orders`,
    JSON.stringify(randomOrderRequest()),
    {
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      tags:    { endpoint: 'order' },
    }
  );
  check(orderRes, { 'order 201': (r) => r.status === 201 }) ||
    console.error(`Order error ${orderRes.status}: ${orderRes.body}`);

  sleep(1);
}

// ─── SCENARIO: REWARDS API ──────────────────────────────────────────
export function runRewards() {
  const token = getToken();
  if (!token) {
    return;
  }

  const res = http.get(REWARDS_PATH, {
    headers: { Authorization: `Bearer ${token}` },
    tags:    { endpoint: 'rewards' },
  });
  check(res, { 'rewards 200': (r) => r.status === 200 }) ||
    console.error(`Rewards error ${res.status}: ${res.body}`);

  sleep(1);
}
