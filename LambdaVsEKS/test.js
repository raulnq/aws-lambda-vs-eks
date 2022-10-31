import http from 'k6/http';
import { randomString } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';
import { check, sleep } from 'k6';
export const options = {
    stages: [
        { duration: '4m', target: 1 },
        { duration: '4m', target: 2 },
        { duration: '4m', target: 3 },
    ],
};
export default function () {
    const minimalapi = '<MINIMALURL>';
    const lambda = '<LAMBDAURL>';
    const eks = '<EKSURL>';

    const payload = JSON.stringify({
        description: randomString(255),
        title: randomString(32),
    });
    const params = {
        headers: {
            'Content-Type': 'application/json',
        },
    };
    const res1 = http.post(minimalapi, payload, params);
    check(res1, {
        'is status 200 (MINIMAL API LAMBDA)': (r) => r.status === 200,
        'duration was <= 500ms(MINIMAL API LAMBDA)': (r) => r.timings.duration <= 500
    });
    sleep(1);
    const res2 = http.post(lambda, payload, params);
    check(res2, {
        'is status 200 (LAMBDA)': (r) => r.status === 200,
        'duration was <= 500ms (LAMBDA)': (r) => r.timings.duration <= 500
    });
    sleep(1);
    const res3 = http.post(eks, payload, params);
    check(res3, {
        'is status 200 (EKS)': (r) => r.status === 200,
        'duration was <= 500ms (EKS)': (r) => r.timings.duration <= 500
    });
    sleep(1);
}