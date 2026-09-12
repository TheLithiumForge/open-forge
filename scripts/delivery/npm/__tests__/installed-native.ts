import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { createHash } from "node:crypto";
import { mkdirSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { EOL } from "node:os";
import { join } from "node:path";
import { IsolatedNpm } from "./isolated-npm.ts";
import { ExpectedLauncherPath, ExpectedMainPackageName, ExpectedPlatforms } from "./platform-package-fixtures.ts";

export interface NativeInstallation {
  resultRoot: string;
  runtime: string;
  nativeArtifact: string;
  expectedVersion: string;
  mainTarball: string;
  platformTarball: string;
}

export function testInstalledNative(input: NativeInstallation): void {
  const platform = ExpectedPlatforms.find((candidate) => candidate.runtime === input.runtime);
  assert.ok(platform, `Unsupported package runtime: ${input.runtime}`);
  assert.equal(process.platform, platform.nodePlatform, "The package test needs the matching OS.");
  assert.equal(process.arch, platform.nodeArchitecture, "The package test needs the matching architecture.");
  mkdirSync(input.resultRoot, { recursive: true });
  const scratch = join(input.resultRoot, "scratch");
  const hash = (path: string) => createHash("sha256").update(readFileSync(path)).digest("hex");
  const nativeSha256 = hash(input.nativeArtifact);
  const mainSha256 = hash(input.mainTarball);
  const platformSha256 = hash(input.platformTarball);
  let phase = "installing npm packages";
  try {
    const npm = new IsolatedNpm(scratch);
    const installRoot = join(scratch, "install");
    npm.run(["install", "--prefix", installRoot, input.mainTarball, input.platformTarball]);
    const modules = join(installRoot, "node_modules");
    for (const name of [ExpectedMainPackageName, platform.packageName]) {
      const manifest: unknown = JSON.parse(readFileSync(join(modules, name, "package.json"), "utf8"));
      assert.ok(typeof manifest === "object" && manifest !== null && "name" in manifest && "version" in manifest);
      assert.equal(manifest.name, name);
      assert.equal(manifest.version, input.expectedVersion);
    }
    const installedNativeSha256 = hash(join(modules, platform.packageName, "bin", platform.nativeFileName));
    assert.equal(installedNativeSha256, nativeSha256, "The installed native bytes must match the build.");
    phase = "invoking installed launcher";
    const launcher = join(modules, ExpectedMainPackageName, ExpectedLauncherPath);
    const result = spawnSync(process.execPath, [launcher, "--version"], { cwd: installRoot, env: npm.environment, shell: false, encoding: "utf8" });
    assert.equal(result.error, undefined);
    assert.equal(result.signal, null);
    assert.equal(result.status, 0, result.stderr);
    assert.equal(result.stdout, `${input.expectedVersion}${EOL}`);
    assert.equal(result.stderr, "");
    assert.equal(hash(input.nativeArtifact), nativeSha256);
    assert.equal(hash(input.mainTarball), mainSha256);
    assert.equal(hash(input.platformTarball), platformSha256);
    writeFileSync(
      join(input.resultRoot, "receipt.json"),
      `${JSON.stringify(
        {
          evidence: "PackageEndToEnd",
          outcome: "passed",
          ...input,
          nativeSha256,
          installedNativeSha256,
          mainSha256,
          platformSha256,
          completedJourneys: 1,
          invocation: "Node invocation of the installed launcher; npm shim execution is not claimed.",
        },
        null,
        2,
      )}\n`,
    );
    process.stdout.write(`PackageEndToEnd: installed ${input.runtime} native candidate ${input.expectedVersion} passed.\n`);
  } catch (error) {
    writeFileSync(join(input.resultRoot, "receipt.json"), `${JSON.stringify({ outcome: "failed", phase, ...input, completedJourneys: 0, error: String(error) }, null, 2)}\n`);
    throw error;
  } finally {
    rmSync(scratch, { recursive: true, force: true });
  }
}
