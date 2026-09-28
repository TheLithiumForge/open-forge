import assert from "node:assert/strict";
import { test } from "node:test";
import { isPrerelease, readHasStableVersion, readRegistryVersion } from "../npm/registry-version.ts";
import { promotesLatest } from "../npm/publish-packages.ts";
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

test("a package has a stable version only when a published version has no prerelease part", () => {
  assert.equal(readHasStableVersion(0, JSON.stringify(["0.9.0-beta.1", "0.9.0-beta.2"]), MainPackageName), false);
  assert.equal(readHasStableVersion(0, JSON.stringify(["0.9.0-beta.2", "1.0.0"]), MainPackageName), true);
  assert.equal(readHasStableVersion(0, JSON.stringify("1.0.0+build.5"), MainPackageName), true);
  assert.equal(readHasStableVersion(1, JSON.stringify({ error: { code: "E404" } }), MainPackageName), false);
  assert.throws(() => readHasStableVersion(1, JSON.stringify({ error: { code: "E500" } }), MainPackageName), /Unexpected registry/);
  assert.throws(() => readHasStableVersion(0, "", MainPackageName), /invalid JSON/);
});

test("a new prerelease also becomes latest only until the first stable release", () => {
  assert.equal(isPrerelease("0.9.0-beta.2"), true);
  assert.equal(isPrerelease("1.0.0+build.5"), false);
  const never = (): boolean => assert.fail("The registry is asked only for a prerelease.");
  assert.equal(
    promotesLatest("0.9.0-beta.2", "beta", () => false),
    true,
  );
  assert.equal(
    promotesLatest("0.9.0-beta.2", "beta", () => true),
    false,
  );
  assert.equal(promotesLatest("1.0.0", "latest", never), false);
  assert.equal(promotesLatest("1.2.3", "preview", never), false);
});
