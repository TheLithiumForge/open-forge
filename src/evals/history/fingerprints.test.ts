import assert from "node:assert/strict";
import { test } from "node:test";
import { createFingerprints, fingerprint, sha256, type FingerprintInputs } from "./fingerprints.ts";

const inputs: FingerprintInputs = {
  cliPackage: { files: ["launcher"] }, benchmarkInputs: { prompt: "make a game" }, setupContent: { content: "rules" },
  checks: ["opens"], rubric: ["legibility"], harnessSource: ["runner"], tooling: { node: "pinned", image: null },
  requestedModels: [null], reportedModels: null,
};

test("every fingerprint is its own full SHA-256 and changes independently", () => {
  const before = createFingerprints(inputs);
  for (const key of Object.keys(inputs)) {
    const after = createFingerprints({ ...inputs, [key]: { changed: true } });
    assert.deepEqual(Object.keys(before).filter((field) => Reflect.get(before, field) !== Reflect.get(after, field)), [key]);
  }
  for (const hash of Object.values(before)) assert.match(hash ?? "", /^[0-9a-f]{64}$/);
});

test("object order doesn't affect hashes but content and array order do", () => {
  assert.equal(fingerprint({ b: 2, a: 1 }), fingerprint({ a: 1, b: 2 }));
  assert.notEqual(fingerprint([1, 2]), fingerprint([2, 1]));
  assert.notEqual(sha256("first"), sha256("second"));
});
