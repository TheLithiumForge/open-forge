import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { existsSync, mkdirSync, rmSync, statSync, writeFileSync } from "node:fs";
import { join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

import { testInstalledNative } from "./installed-native.ts";
import { IsolatedNpm } from "./isolated-npm.ts";
import { ExpectedMainDirectory, ExpectedMainPackageName, ExpectedPlatforms } from "./platform-package-fixtures.ts";

const repositoryRoot = fileURLToPath(new URL("../../../../", import.meta.url));
const managerPath = fileURLToPath(new URL("../manage.ts", import.meta.url));

const [resultRootArgument, runtime, nativeArgument, versionKind, versionValue, expectedVersion, compiledLauncher, ...extra] = process.argv.slice(2);
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
const stageArguments = [managerPath, "stage", resultRoot, runtime, nativeArtifact, versionKind, versionValue];
if (compiledLauncher !== undefined) stageArguments.push(compiledLauncher);
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
  phase = "installing and invoking";
  testInstalledNative({ resultRoot, runtime, nativeArtifact, expectedVersion, mainTarball: mainPack.path, platformTarball: platformPack.path });
} catch (error) {
  const receipt = {
    evidence: "PackageEndToEnd",
    outcome: "failed",
    phase,
    runtime,
    nativeArtifact,
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
