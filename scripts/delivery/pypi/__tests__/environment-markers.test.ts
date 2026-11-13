import assert from "node:assert/strict";
import { test } from "node:test";
import { AliasPackageName, MainPackageName, PlatformPackages } from "../../package-model.ts";
import { AllTargets } from "../../targets.ts";
import { platformMarker, pythonEnvironment } from "../environment-markers.ts";
import { PythonAliasPackage, PythonMainPackage, pythonPackageName, pythonPlatformPackage } from "../package-model.ts";

test("PyPI names and markers follow the npm target order without overlapping hosts", () => {
  assert.equal(PythonMainPackage, pythonPackageName(MainPackageName));
  assert.equal(PythonAliasPackage, pythonPackageName(AliasPackageName));
  const targets = AllTargets.map((runtime) => ({
    runtime,
    npm: PlatformPackages[runtime].packageName,
    name: pythonPlatformPackage(runtime),
    ...pythonEnvironment(runtime),
    marker: platformMarker(runtime),
  }));
  for (const target of targets)
    for (const machine of target.machines)
      assert.deepEqual(
        targets.filter((candidate) => candidate.platform === target.platform && candidate.machines.some((value) => value === machine)).map((candidate) => candidate.runtime),
        [target.runtime],
      );
  for (const target of targets) {
    const machines = target.machines.map((machine) => `platform_machine == "${machine}"`).join(" or ");
    assert.equal(target.marker, `sys_platform == "${target.platform}" and ${target.machines.length === 1 ? machines : `(${machines})`}`);
  }
});
