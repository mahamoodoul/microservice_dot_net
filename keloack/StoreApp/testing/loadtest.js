import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  vus: 10,           // number of virtual users
  duration: '30s',   // total test duration
};

export default function () {
  let res = http.get('http://localhost:5279/'); // Replace with your endpoint URL
  check(res, { 'status was 200': (r) => r.status === 200 });
  sleep(1); // wait for 1 second between iterations
}

// commant to run: k6 run loadtest.js    