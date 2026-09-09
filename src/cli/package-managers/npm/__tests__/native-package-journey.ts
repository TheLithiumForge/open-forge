import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { createHash } from "node:crypto";
import { existsSync, mkdirSync, readFileSync, rmSync, statSync, writeFileSync } from "node:fs";
import { EOL } from "node:os";
import { join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

import { IsolatedNpm } from "./isolated-npm.ts";
import { ExpectedLauncherPath, ExpectedMainDirectory, ExpectedMainPackageName, ExpectedPlatforms } from "./platform-package-fixtures.ts";

const repositoryRoot = fileURLToPath(new URL("../../../../../", import.meta.url));
const managerPath = fileURLToPath(new URL("../manage.ts", import.meta.url));
const versionArgument = "--version";
const manifestName = "package.json";

const [resultRootArgument, runtime, nativeArgument, versionKind, versionValue, expectedVersion, ...extra] = process.argv.slice(2);
assert.ok(
  resultRootArgument && runtime && nativeArgument && versionKind && versionValue && expectedVersion && extra.length === 0,
  "Provide artifacts-root, RID, native-artifact, version-kind, version-value and independent expected-version.",
);
const platform = ExpectedPlatforms.find((candidate) => candidate.runtime === runtime);
assert.ok(platform, `Unsupported native package journey runtime ${JSON.stringify(runtime)}.`);
assert.equal(process.platform, platform.nodePlatform, "The package journey requires the matching native operating system.");
assert.equal(process.arch, platform.nodeArchitecture, "The package journey requires the matching native architecture.");
assert.ok(versionKind === "local" || versionKind === "release");
const resultRoot = resolve(resultRootArgument);
const nativeArtifact = resolve(nativeArgument);
assert.equal(statSync(nativeArtifact).isFile(), true, "The supplied native artifact must be a regular file.");
assert.equal(existsSync(resultRoot), false, "The package journey requires a fresh caller-owned result root.");
mkdirSync(resultRoot, { recursive: true });
const scratchRoot = join(resultRoot, "scratch");
const inputHash = hashFile(nativeArtifact);
const stageArguments = [managerPath, "stage", resultRoot, runtime, nativeArtifact, versionKind, versionValue];
let npm: IsolatedNpm | undefined;
let phase = "preparing npm";

try {
  npm = new IsolatedNpm(scratchRoot);
  phase = "staging";
  const stageCompletion = spawnSync(process.execPath, stageArguments, { cwd: repositoryRoot, env: npm.environment, encoding: "utf8", shell: false });
  assert.equal(stageCompletion.error, undefined);
  assert.equal(stageCompletion.signal, null);
  assert.equal(stageCompletion.status, 0, stageCompletion.stderr);
  assert.equal(stageCompletion.stderr, "");
  phase = "packing";
  const stagedRoot = join(resultRoot, "npm", "stage", runtime);
  const mainPack = npm.pack(join(stagedRoot, ExpectedMainDirectory), join(resultRoot, "packs", "main"));
  const platformPack = npm.pack(join(stagedRoot, platform.directoryName), join(resultRoot, "packs", runtime));
  assert.equal(mainPack.name, ExpectedMainPackageName);
  assert.equal(platformPack.name, platform.packageName);
  assert.equal(mainPack.version, expectedVersion);
  assert.equal(platformPack.version, expectedVersion);
  phase = "installing";
  const installRoot = join(scratchRoot, "install");
  npm.run(["install", "--prefix", installRoot, mainPack.path, platformPack.path]);
  const modulesRoot = join(installRoot, "node_modules");
  const mainDirectory = join(modulesRoot, ExpectedMainPackageName);
  const platformDirectory = join(modulesRoot, platform.packageName);
  const installedLauncher = join(mainDirectory, ExpectedLauncherPath);
  const installedNative = join(platformDirectory, "bin", platform.nativeFileName);
  assert.equal(statSync(mainDirectory).isDirectory(), true);
  assert.equal(statSync(platformDirectory).isDirectory(), true);
  assert.equal(statSync(installedLauncher).isFile(), true);
  assert.equal(statSync(installedNative).isFile(), true);
  assertInstalledManifest(mainDirectory, ExpectedMainPackageName, expectedVersion);
  assertInstalledManifest(platformDirectory, platform.packageName, expectedVersion);
  const installedHash = hashFile(installedNative);
  assert.equal(installedHash, inputHash, "The installed payload must equal the exact supplied native artifact.");
  phase = "invoking installed launcher";
  const completion = spawnSync(process.execPath, [installedLauncher, versionArgument], { cwd: installRoot, env: npm.environment, shell: false, encoding: "utf8" });
  assert.equal(completion.error, undefined);
  assert.equal(completion.signal, null);
  assert.equal(completion.status, 0);
  assert.equal(completion.stdout, `${expectedVersion}${EOL}`);
  assert.equal(completion.stderr, "");
  assert.equal(hashFile(nativeArtifact), inputHash, "The native source artifact must remain unchanged.");
  const receipt = {
    evidence: "PackageEndToEnd",
    outcome: "passed",
    invocation: "Node invocation of the installed Open Forge launcher; npm shim execution is not claimed.",
    runtime,
    platform: process.platform,
    architecture: process.arch,
    node: process.version,
    repositoryRoot,
    nativeArtifact,
    expectedVersion,
    versionKind,
    versionValue,
    nativeSha256: inputHash,
    installedNativeSha256: installedHash,
    mainTarball: { path: mainPack.path, sha256: hashFile(mainPack.path) },
    platformTarball: { path: platformPack.path, sha256: hashFile(platformPack.path) },
    commands: [[process.execPath, ...stageArguments], ...npm.commands, [process.execPath, installedLauncher, versionArgument]],
    status: completion.status,
    signal: completion.signal,
    stdout: completion.stdout,
    stderr: completion.stderr,
    completedJourneys: 1,
  };
  writeFileSync(join(resultRoot, "receipt.json"), `${JSON.stringify(receipt, undefined, 2)}\n`, "utf8");
  process.stdout.write(`PackageEndToEnd: installed ${runtime} native candidate ${expectedVersion} passed.\n`);
} catch (error) {
  const receipt = {
    evidence: "PackageEndToEnd",
    outcome: "failed",
    phase,
    runtime,
    nativeArtifact,
    nativeSha256: inputHash,
    expectedVersion,
    versionKind,
    versionValue,
    configuredStageCommand: [process.execPath, ...stageArguments],
    npmCommandsAttempted: npm?.commands ?? [],
    completedJourneys: 0,
    error: error instanceof Error ? error.message : String(error),
  };
  writeFileSync(join(resultRoot, "receipt.json"), `${JSON.stringify(receipt, undefined, 2)}\n`, "utf8");
  throw error;
} finally {
  rmSync(scratchRoot, { recursive: true, force: true });
}

function hashFile(path: string): string {
  return createHash("sha256").update(readFileSync(path)).digest("hex");
}

function assertInstalledManifest(directory: string, expectedName: string, version: string): void {
  const manifest: unknown = JSON.parse(readFileSync(join(directory, manifestName), "utf8"));
  assert.ok(typeof manifest === "object" && manifest !== null);
  assert.ok("name" in manifest && "version" in manifest);
  assert.equal(manifest.name, expectedName);
  assert.equal(manifest.version, version);
}
