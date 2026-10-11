import assert from "node:assert/strict";
import { test } from "node:test";
import { checkpointSeconds, CheckpointWaitSeconds, publicationPlan, readReleaseTarget, ReleaseFile, ReleaseTarget } from "../release-plan.ts";
import { releaseChannel } from "../release-selection.ts";
import { MainPackageName, PlatformPackages } from "../../package-model.ts";
import { AllTargets } from "../../targets.ts";
import { BuildSha } from "../../__tests__/build-fixtures.ts";

const Version = "0.9.0-beta.11";
const Packages = [...AllTargets.map((rid) => PlatformPackages[rid].packageName), MainPackageName];
const Files = [...AllTargets.map((rid) => `open-forge-${Version}-${rid}${ReleaseFile.portableSuffix}`), ReleaseFile.checksums];

test("checkpoint prints target, channel, version, commit, every npm package and every GitHub release file", () => {
  assert.equal(
    publicationPlan({ target: ReleaseTarget.all, channel: releaseChannel(Version), version: Version, sha: BuildSha, packages: Packages, files: Files }),
    `Publication plan
Target: all
Channel: beta
Version: ${Version}
Source commit: ${BuildSha}
npm packages:
  @thelithiumforge/open-forge-darwin-x64@${Version}
  @thelithiumforge/open-forge-linux-x64@${Version}
  @thelithiumforge/open-forge-win-x64@${Version}
  @thelithiumforge/open-forge-darwin-arm64@${Version}
  @thelithiumforge/open-forge-linux-arm64@${Version}
  @thelithiumforge/open-forge-win-arm64@${Version}
  @thelithiumforge/open-forge@${Version}
GitHub release files:
  open-forge-${Version}-osx-x64.tar.gz
  open-forge-${Version}-linux-x64.tar.gz
  open-forge-${Version}-win-x64.tar.gz
  open-forge-${Version}-osx-arm64.tar.gz
  open-forge-${Version}-linux-arm64.tar.gz
  open-forge-${Version}-win-arm64.tar.gz
  SHA256SUMS`,
  );
});

test("checkpoint wait defaults to 60 seconds and permits an explicit zero", () => {
  assert.equal(checkpointSeconds(), CheckpointWaitSeconds);
  assert.equal(checkpointSeconds("0"), 0);
  assert.throws(() => checkpointSeconds("-1"));
  assert.throws(() => checkpointSeconds("invalid"));
});

test("release destinations default to all and reject unsupported inputs", () => {
  assert.equal(readReleaseTarget(), ReleaseTarget.all);
  for (const target of Object.values(ReleaseTarget)) assert.equal(readReleaseTarget(target), target);
  assert.throws(() => readReleaseTarget("invalid"));
});
