import assert from "node:assert/strict";
import { readFileSync, writeFileSync } from "node:fs";
import { join, resolve } from "node:path";
import { smokeAliases } from "../../aliases/smoke.ts";
import { hostRuntime } from "../../layout.ts";
import { packNpmPackages } from "../../npm/pack-packages.ts";
import { compileLauncher } from "../../npm/stage.ts";
import { resetOutput } from "../../output.ts";
import { repositoryRoot } from "../../repository.ts";
import { AllTargets } from "../../targets.ts";
import { committedVersion } from "../../version.ts";
import { hostWheels } from "../wheel.ts";

// Explicit host-only installation proof using an existing executable. Other targets are inert selection fixtures.
const native = process.argv[2];
assert.ok(native && process.argv.length === 3, "Provide one existing matching-version host executable.");
const runtime = hostRuntime();
const version = committedVersion(repositoryRoot);
const output = resetOutput(repositoryRoot, "artifacts/alias-smoke-layout");
const launcher = join(output, "launcher");
compileLauncher(repositoryRoot, launcher);
const packed = packNpmPackages({
  repositoryRoot,
  artifactsRoot: output,
  outputDirectory: output,
  runtime,
  version: { kind: "release", value: version },
  nativeArtifact: resolve(native),
  compiledLauncher: launcher,
});
const license = readFileSync(join(repositoryRoot, "LICENSE"));
const wheels = new Map<string, Buffer>();
for (const target of AllTargets) {
  const bytes = target === runtime ? readFileSync(packed.stagedNative) : Buffer.from(`owned inert non-host ${target} selection fixture`);
  for (const [name, wheel] of hostWheels(version, target, bytes, license)) {
    const earlier = wheels.get(name);
    if (earlier !== undefined) assert.deepEqual(wheel, earlier);
    wheels.set(name, wheel);
  }
}
for (const [name, bytes] of wheels) writeFileSync(join(output, name), bytes);
smokeAliases(output, runtime, version);
