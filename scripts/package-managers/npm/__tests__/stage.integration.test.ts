import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { existsSync, mkdtempSync, readFileSync, readdirSync, rmSync, statSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import test from "node:test";
import { fileURLToPath } from "node:url";

import { PlatformPackages } from "../package-model.ts";
import { IsolatedNpm } from "./isolated-npm.ts";
import {
  ExpectedLauncherPath,
  ExpectedMainDirectory,
  ExpectedMainFiles,
  ExpectedMainPackageName,
  ExpectedOptionalDependencies,
  ExpectedPlatforms,
  FixtureGitSha,
  FixtureNativeBytes,
  FixtureVersion,
  UnsupportedRuntime,
} from "./platform-package-fixtures.ts";

const repositoryRoot = fileURLToPath(new URL("../../../../", import.meta.url));
const managerPath = fileURLToPath(new URL("../manage.ts", import.meta.url));
const executablePermissions = 0o111;
const manifestName = "package.json";
const licenseName = "LICENSE";

for (const platform of ExpectedPlatforms) {
  test(`Integration: stage and pack the complete Open Forge ${platform.runtime} layout`, (context) => {
    const temporaryRoot = mkdtempSync(join(tmpdir(), "open-forge-package-layout-"));
    try {
      const npm = new IsolatedNpm(join(temporaryRoot, "scratch"));
      const artifactsRoot = join(temporaryRoot, "artifacts");
      const nativeArtifact = join(temporaryRoot, "native-fixture");
      writeFileSync(nativeArtifact, FixtureNativeBytes);
      const completion = spawnSync(process.execPath, [managerPath, "stage", artifactsRoot, platform.runtime, nativeArtifact, "local", FixtureGitSha], {
        cwd: repositoryRoot,
        env: npm.environment,
        encoding: "utf8",
        shell: false,
      });
      assert.equal(completion.error, undefined);
      assert.equal(completion.signal, null);
      assert.equal(completion.status, 0, `Open Forge staging must accept ${platform.runtime}: ${completion.stderr}`);
      assert.equal(completion.stderr, "");
      const stageRoot = join(artifactsRoot, "npm", "stage", platform.runtime);
      const mainDirectory = join(stageRoot, ExpectedMainDirectory);
      const platformDirectory = join(stageRoot, platform.directoryName);
      const nativePath = `bin/${platform.nativeFileName}`;
      const platformFiles = [licenseName, nativePath, manifestName].sort();
      assert.deepEqual(readdirSync(stageRoot).sort(), [ExpectedMainDirectory, platform.directoryName].sort());
      assert.deepEqual(fileInventory(mainDirectory), ExpectedMainFiles);
      assert.deepEqual(fileInventory(platformDirectory), platformFiles);
      assert.deepEqual(readFileSync(join(platformDirectory, nativePath)), FixtureNativeBytes);
      const licenseBytes = readFileSync(join(repositoryRoot, licenseName));
      assert.deepEqual(readFileSync(join(mainDirectory, licenseName)), licenseBytes);
      assert.deepEqual(readFileSync(join(platformDirectory, licenseName)), licenseBytes);
      if (process.platform !== "win32") {
        assert.equal(statSync(join(platformDirectory, nativePath)).mode & executablePermissions, executablePermissions);
        assert.equal(statSync(join(mainDirectory, ExpectedLauncherPath)).mode & executablePermissions, executablePermissions);
      }

      const main = readManifest(join(mainDirectory, manifestName));
      const native = readManifest(join(platformDirectory, manifestName));
      assert.equal(main["name"], ExpectedMainPackageName);
      assert.equal(native["name"], platform.packageName);
      assert.equal(main["version"], FixtureVersion);
      assert.equal(native["version"], FixtureVersion);
      assert.deepEqual(main["bin"], { "open-forge": "./bin/open-forge.js" });
      assert.deepEqual(native["os"], [platform.nodePlatform]);
      assert.deepEqual(native["cpu"], [platform.nodeArchitecture]);
      const { libc, ...expectedDefinition } = platform;
      assert.equal(native["libc"], libc);
      assert.equal(Object.hasOwn(native, "libc"), libc !== undefined);
      assert.deepEqual(Object.entries(PlatformPackages).find(([runtime]) => runtime === platform.runtime)?.[1], expectedDefinition);
      for (const manifest of [main, native]) {
        for (const absent of ["private", "scripts", "dependencies", "peerDependencies"]) {
          assert.equal(Object.hasOwn(manifest, absent), false, `${absent} must be absent from the public manifest.`);
        }
      }
      assert.equal(Object.hasOwn(native, "bin"), false);
      assert.equal(Object.hasOwn(native, "optionalDependencies"), false);
      for (const absent of ["os", "cpu", "libc"]) {
        assert.equal(Object.hasOwn(main, absent), false);
      }

      const mainPack = npm.pack(mainDirectory, join(temporaryRoot, "packs", "main"));
      const nativePack = npm.pack(platformDirectory, join(temporaryRoot, "packs", platform.runtime));
      assert.deepEqual(mainPack.files, ExpectedMainFiles);
      assert.deepEqual(nativePack.files, platformFiles);
      assert.equal(mainPack.name, ExpectedMainPackageName);
      assert.equal(nativePack.name, platform.packageName);
      assert.equal(mainPack.version, FixtureVersion);
      assert.equal(nativePack.version, FixtureVersion);
      context.diagnostic(`Verified ${platform.runtime} staged bytes, manifests and both packed inventories before the complete-graph assertion.`);
      assert.deepEqual(main["optionalDependencies"], ExpectedOptionalDependencies, "The main package must contain exactly six synchronized optional dependencies.");
      assert.deepEqual(Object.keys(PlatformPackages).sort(), ExpectedPlatforms.map((candidate) => candidate.runtime).sort());
    } finally {
      rmSync(temporaryRoot, { recursive: true, force: true });
    }
  });
}

test("Integration: unsupported package runtime fails before creating stage output", () => {
  const temporaryRoot = mkdtempSync(join(tmpdir(), "open-forge-package-unsupported-"));
  try {
    const artifactsRoot = join(temporaryRoot, "artifacts");
    const nativeArtifact = join(temporaryRoot, "native-fixture");
    writeFileSync(nativeArtifact, FixtureNativeBytes);
    const completion = spawnSync(process.execPath, [managerPath, "stage", artifactsRoot, UnsupportedRuntime, nativeArtifact, "local", FixtureGitSha], {
      cwd: repositoryRoot,
      encoding: "utf8",
      shell: false,
    });
    assert.equal(completion.error, undefined);
    assert.equal(completion.signal, null);
    assert.equal(completion.status, 1);
    assert.equal(completion.stdout, "");
    assert.equal(completion.stderr, `Unsupported package runtime "${UnsupportedRuntime}".\n`);
    assert.equal(existsSync(artifactsRoot), false);
    assert.deepEqual(readFileSync(nativeArtifact), FixtureNativeBytes);
  } finally {
    rmSync(temporaryRoot, { recursive: true, force: true });
  }
});

function fileInventory(directory: string): string[] {
  return readdirSync(directory, { recursive: true, withFileTypes: true })
    .filter((entry) => entry.isFile())
    .map((entry) =>
      join(entry.parentPath, entry.name)
        .slice(directory.length + 1)
        .replaceAll("\\", "/"),
    )
    .sort();
}

function readManifest(path: string): Record<string, unknown> {
  const value: unknown = JSON.parse(readFileSync(path, "utf8"));
  assert.ok(typeof value === "object" && value !== null && !Array.isArray(value));
  return Object.fromEntries(Object.entries(value));
}
