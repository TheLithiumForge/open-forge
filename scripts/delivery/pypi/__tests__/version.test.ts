import assert from "node:assert/strict";
import { test } from "node:test";
import { candidateVersion } from "../../version.ts";
import { pythonVersion } from "../version.ts";

const Versions = [
  ["1.2.3", "1.2.3"],
  ["1.2.3-alpha.1", "1.2.3a1"],
  ["0.9.0-beta.18", "0.9.0b18"],
  ["1.2.3-rc.0", "1.2.3rc0"],
] as const;
for (const [npm, python] of Versions) test(`${npm} maps to PEP 440 ${python}`, () => assert.equal(pythonVersion(npm), python));

test("SHA candidates keep their identity in local development wheel versions", () => {
  const sha = "a".repeat(40);
  for (const [npm, python] of Versions) assert.equal(pythonVersion(candidateVersion(npm, sha)), `${python}.dev0+sha.${sha}`);
});

test("ambiguous and unsupported wheel versions fail instead of losing identity", () => {
  for (const version of ["invalid", "1.2.3-beta", "1.2.3-beta.1.extra", "1.2.3-preview.1", "1.2.3+build.1", "1.2.3-beta.1.sha-invalid"]) {
    assert.throws(() => pythonVersion(version));
  }
});
