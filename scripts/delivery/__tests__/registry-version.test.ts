import assert from "node:assert/strict";
import { test } from "node:test";
import { readRegistryVersion } from "../npm/registry-version.ts";
import { MainPackageName } from "../package-model.ts";

const version = "1.2.3";

test("registry checks distinguish an existing exact version from a missing package or version", () => {
  assert.equal(readRegistryVersion(0, JSON.stringify(version), MainPackageName, version), true);
  assert.equal(readRegistryVersion(1, JSON.stringify({ error: { code: "E404" } }), MainPackageName, version), false);
});

test("registry errors and unexpected responses cannot authorize publication", () => {
  for (const code of ["E401", "E403", "E429", "E500", "ENOTFOUND", "ETIMEDOUT"]) {
    assert.throws(() => readRegistryVersion(1, JSON.stringify({ error: { code } }), MainPackageName, version), /Registry check failed/);
  }
  for (const [status, output] of [
    [0, '"9.9.9"'],
    [0, ""],
    [0, "{}"],
    [1, '"1.2.3"'],
    [0, '{"error":{"code":"E404"}}'],
    [null, "{}"],
  ] as const) {
    assert.throws(() => readRegistryVersion(status, output, MainPackageName, version), /[Rr]egistry/);
  }
});
