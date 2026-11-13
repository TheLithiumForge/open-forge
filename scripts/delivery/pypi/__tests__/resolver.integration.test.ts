import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { mkdtempSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { PlatformPackages } from "../../package-model.ts";
import { AllTargets } from "../../targets.ts";
import { PythonAliasPackage, PythonMainPackage, pythonPlatformPackage } from "../package-model.ts";
import { hostWheels } from "../wheel.ts";
import { findPython } from "../python.ts";
import { ResolverFile, ResolverLimits, ResolverProbe, resolverArguments, WindowsResolverCases } from "./resolver-fixture.ts";
import { WheelFixture } from "./wheel-fixtures.ts";
import { test } from "node:test";

test("pip resolves an installable three-package graph for each Windows interpreter and hardware pair", async (context) => {
  const python = findPython(true);
  assert.ok(python, "Resolver regression requires Python with pip.");
  const root = mkdtempSync(join(tmpdir(), "open-forge-resolver-"));
  context.after(() => rmSync(root, { recursive: true, force: true }));
  for (const runtime of AllTargets)
    for (const [name, bytes] of hostWheels(WheelFixture.version, runtime, WheelFixture.native, WheelFixture.license)) writeFileSync(join(root, name), bytes);
  const script = join(root, ResolverFile.script);
  const request = join(root, ResolverFile.request);
  writeFileSync(script, ResolverProbe);
  writeFileSync(
    request,
    JSON.stringify(
      WindowsResolverCases.map((scenario, index) => ({
        platform: PlatformPackages[scenario.runtime].nodePlatform,
        machine: scenario.machine,
        arguments: resolverArguments(root, join(root, ResolverFile.report(index)), scenario.interpreterPlatform),
      })),
    ),
  );
  const result = spawnSync(python.executable, [...python.args, script, request], {
    cwd: root,
    env: { ...process.env, PYTHONNOUSERSITE: "1", PYTHONPATH: "" },
    encoding: "utf8",
    windowsHide: true,
    timeout: ResolverLimits.milliseconds,
  });
  assert.equal(result.error, undefined, result.stdout + result.stderr);
  assert.equal(result.status, 0, result.stdout + result.stderr);
  for (const [index, scenario] of WindowsResolverCases.entries())
    await context.test(scenario.name, () => {
      const value: unknown = JSON.parse(readFileSync(join(root, ResolverFile.report(index)), "utf8"));
      assert.ok(typeof value === "object" && value !== null && "install" in value && Array.isArray(value.install));
      const packages = value.install
        .map((item: unknown) => {
          assert.ok(typeof item === "object" && item !== null && "metadata" in item);
          const metadata = item.metadata;
          assert.ok(typeof metadata === "object" && metadata !== null && "name" in metadata && typeof metadata.name === "string" && "version" in metadata);
          assert.equal(metadata.version, WheelFixture.pythonVersion);
          return metadata.name;
        })
        .sort();
      assert.deepEqual(packages, [PythonAliasPackage, PythonMainPackage, pythonPlatformPackage(scenario.runtime)].sort());
    });
});
