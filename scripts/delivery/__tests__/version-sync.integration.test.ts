import assert from "node:assert/strict";
import { mkdtempSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { test } from "node:test";
import { synchronizeVersion } from "../version-sync.ts";
import { committedVersion } from "../version.ts";
import { nativeManifest, wrapperManifest } from "../npm/package-manifests.ts";
import { PlatformPackages } from "../package-model.ts";

const version = "0.1.0-beta.1";

test("one root version projects to .NET and every generated npm package", (context) => {
  const root = mkdtempSync(join(tmpdir(), "open-forge-version-"));
  context.after(() => rmSync(root, { recursive: true, force: true }));
  writeFileSync(join(root, "package.json"), JSON.stringify({ version }));
  const properties =
    "<Project><OpenForgeCliVersion>0.0.0</OpenForgeCliVersion><OpenForgeCliInformationalVersion>$(OpenForgeCliVersion)</OpenForgeCliInformationalVersion></Project>";
  writeFileSync(join(root, "Directory.Build.props"), properties);
  synchronizeVersion(root);
  assert.equal(readFileSync(join(root, "Directory.Build.props"), "utf8"), properties.replace("0.0.0", version));
  const effectiveVersion = committedVersion(root);
  assert.equal(wrapperManifest(effectiveVersion).version, version);
  assert.deepEqual(wrapperManifest(effectiveVersion).optionalDependencies, Object.fromEntries(Object.values(PlatformPackages).map((platform) => [platform.packageName, version])));
  for (const platform of Object.values(PlatformPackages)) assert.equal(nativeManifest(platform.runtime, effectiveVersion).version, version);
});

test("version projection rejects invalid root versions and a missing .NET property without changing props", (context) => {
  const root = mkdtempSync(join(tmpdir(), "open-forge-version-invalid-"));
  context.after(() => rmSync(root, { recursive: true, force: true }));
  const properties = "<Project />";
  writeFileSync(join(root, "Directory.Build.props"), properties);
  for (const invalidVersion of ["invalid", version]) {
    writeFileSync(join(root, "package.json"), JSON.stringify({ version: invalidVersion }));
    assert.throws(() => synchronizeVersion(root), /version/i);
    assert.equal(readFileSync(join(root, "Directory.Build.props"), "utf8"), properties);
  }
});
