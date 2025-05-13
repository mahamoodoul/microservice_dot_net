import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  vus: 50,          // Number of virtual users
  duration: '1m',   // Test duration
  thresholds: {
    http_req_duration: ['p(95)<500'],  // 95% of requests should complete below 500ms
  },
};

// Keycloak token endpoint details (adjust as needed)
const KEYCLOAK_URL = 'http://localhost:8080'; // e.g., http://localhost:8080
const REALM = 'StoreRealm';
const CLIENT_ID = 'store-app-client';
const CLIENT_SECRET = 'k1eSMlvWfduL0f6LxHJ2fE7zJHMYDknL'; // optional, if required by your client configuration

// User credentials for authentication
const USERNAME = 'admin';
const PASSWORD = 'Admin123@';

// Construct the Keycloak token URL
const tokenUrl = `${KEYCLOAK_URL}/auth/realms/${REALM}/protocol/openid-connect/token`;

export default function () {
  // --- Step 1: Obtain an Access Token from Keycloak ---
  let payload = {
    grant_type: 'password',
    client_id: CLIENT_ID,
    username: USERNAME,
    password: PASSWORD,
  };

  // Include client_secret if your client configuration requires it
  if (CLIENT_SECRET) {
    payload.client_secret = CLIENT_SECRET;
  }

  // Keycloak expects the data as x-www-form-urlencoded
  let params = {
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
  };

  // Convert payload to URL-encoded string
  let body = Object.keys(payload)
    .map(key => `${encodeURIComponent(key)}=${encodeURIComponent(payload[key])}`)
    .join('&');

  let authRes = http.post(tokenUrl, body, params);
  check(authRes, {
    'Keycloak authentication status is 200': (r) => r.status === 200,
    'access token is returned': (r) => r.json('access_token') !== '',
  });

  let token = authRes.json('access_token');

  // --- Step 2: Call the Secured API Endpoint Through Your API Gateway ---
  let apiHeaders = {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json',
  };

  // Replace with your API gateway's secured endpoint URL
  let apiUrl = 'http://localhost:5279/';

  let apiRes = http.get(apiUrl, { headers: apiHeaders });
  check(apiRes, {
    'secured endpoint status is 200': (r) => r.status === 200,
  });

  // Pause for 1 second between iterations
  sleep(1);
}
