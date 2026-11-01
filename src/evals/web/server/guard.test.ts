import assert from "node:assert/strict";
import { test } from "node:test";
import { refusal, type RequestFacts } from "./guard.ts";

const port = 4318;
const appHost = `localhost:${port}`;
const appOrigin = `http://${appHost}`;
const gameHost = `0123456789ab.localhost:${port}`;
const json = "application/json";

function facts(overrides: Partial<RequestFacts>): RequestFacts {
  return { method: "GET", host: appHost, origin: undefined, fetchSite: undefined, contentType: undefined, ...overrides };
}

test("the app's own page may call the API", () => {
  assert.equal(refusal(facts({ method: "POST", origin: appOrigin, fetchSite: "same-origin", contentType: json }), port, true), null);
});

test("another site can't start a batch", () => {
  assert.notEqual(refusal(facts({ method: "POST", origin: "https://example.com", fetchSite: "cross-site", contentType: "text/plain" }), port, true), null);
});

test("a hostname that resolves here by rebinding is refused", () => {
  assert.notEqual(refusal(facts({ host: `example.com:${port}` }), port, true), null);
  assert.notEqual(refusal(facts({ host: `example.com:${port}` }), port, false), null);
});

test("a game can load but can't call the API", () => {
  assert.equal(refusal(facts({ host: gameHost }), port, false), null);
  assert.notEqual(refusal(facts({ host: gameHost, method: "POST", contentType: json }), port, true), null);
  assert.notEqual(refusal(facts({ method: "POST", origin: `http://${gameHost}`, fetchSite: "same-site", contentType: json }), port, true), null);
});

test("a change must be sent as JSON", () => {
  assert.notEqual(refusal(facts({ method: "POST", origin: appOrigin }), port, true), null);
});
