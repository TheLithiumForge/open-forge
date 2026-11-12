import assert from "node:assert/strict";
import { test } from "node:test";
import { AliasPackageName, MainPackageName } from "../../package-model.ts";
import { aliasManifest, LauncherPath } from "../package-manifests.ts";
import { readRegistryVersion } from "../registry-version.ts";

const Version = "0.9.0-beta.18";

test("alias manifest pins the selected scoped version and ships only its forwarding bin", () => {
  const manifest = aliasManifest(Version);
  assert.equal(manifest.name, AliasPackageName);
  assert.equal(manifest.version, Version);
  assert.deepEqual(manifest.dependencies, { [MainPackageName]: Version });
  assert.deepEqual(manifest.bin, { [AliasPackageName]: `./${LauncherPath}` });
  for (const field of ["optionalDependencies", "os", "cpu", "libc", "scripts"]) assert.equal(Object.hasOwn(manifest, field), false);
  assert.throws(() => aliasManifest("invalid"));
});

test("publication preflight requires the exact published alias dependency", () => {
  const dependencies = aliasManifest(Version).dependencies;
  const response = (value: object) => JSON.stringify({ version: Version, ...value });
  assert.equal(readRegistryVersion(0, response({ dependencies }), AliasPackageName, Version, undefined, dependencies), true);
  for (const value of [{}, { dependencies: {} }, { dependencies: { [MainPackageName]: "0.1.0" } }]) {
    assert.throws(() => readRegistryVersion(0, response(value), AliasPackageName, Version, undefined, dependencies));
  }
});
