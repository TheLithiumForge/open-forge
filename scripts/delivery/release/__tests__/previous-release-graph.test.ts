import assert from "node:assert/strict";
import { test } from "node:test";
import { AliasPackageName, MainPackageName, PlatformPackages } from "../../package-model.ts";
import { AllTargets } from "../../targets.ts";
import { recordedReleaseGraph, registryReleaseGraph } from "../previous-release-graph.ts";

const Version = "0.9.0-beta.18";
const Native = AllTargets.map((target) => PlatformPackages[target].packageName);
const Legacy = [...Native, MainPackageName];
const Tarball = (name: string) => `${name.replace(/^@/u, "").replaceAll("/", "-")}-${Version}.tgz`;
const Wrapper = { version: Version, optionalDependencies: Object.fromEntries(Native.map((name) => [name, Version])) };

test("registry graph keeps an absent previous alias out of recovery and retains a published alias", () => {
  assert.deepEqual(registryReleaseGraph(Wrapper, Version, false), { version: Version, packages: Legacy });
  assert.deepEqual(registryReleaseGraph(Wrapper, Version, true), { version: Version, packages: [...Legacy, AliasPackageName] });
});

test("recorded pre-alias releases keep their original graph without requiring new archive kinds", () => {
  const artifacts = Legacy.map((name) => ({ path: Tarball(name) }));
  const recorded = { version: Version, targets: AllTargets, artifacts };
  assert.deepEqual(recordedReleaseGraph(recorded, Version), { version: Version, packages: Legacy });
  assert.deepEqual(recordedReleaseGraph({ ...recorded, artifacts: [...artifacts, { path: Tarball(AliasPackageName) }] }, Version), {
    version: Version,
    packages: [...Legacy, AliasPackageName],
  });
});

test("previous target selection controls recovery instead of the current default target set", () => {
  const target = AllTargets[0] ?? assert.fail();
  const native = PlatformPackages[target].packageName;
  assert.deepEqual(registryReleaseGraph({ version: Version, optionalDependencies: { [native]: Version } }, Version, false).packages, [native, MainPackageName]);
});

test("previous graph rejects mismatched versions, dependencies and incomplete archive membership", () => {
  assert.throws(() => registryReleaseGraph({ ...Wrapper, version: "1.0.0" }, Version, false));
  assert.throws(() => registryReleaseGraph({ version: Version, optionalDependencies: { [Native[0] ?? assert.fail()]: "1.0.0" } }, Version, false));
  assert.throws(() => recordedReleaseGraph({ version: Version, targets: AllTargets, artifacts: [] }, Version));
});
