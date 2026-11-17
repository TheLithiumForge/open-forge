import assert from "node:assert/strict";
import { readFileSync, writeFileSync } from "node:fs";
import { join, resolve } from "node:path";
import { smokeAliases } from "../../aliases/smoke.ts";
import { hostRuntime } from "../../layout.ts";
import { packNpmPackages } from "../../npm/pack-packages.ts";
import { compileLauncher } from "../../npm/stage.ts";
import { resetOutput } from "../../output.ts";
import { repositoryRoot } from "../../repository.ts";
import { committedVersion } from "../../version.ts";
import { hostWheel } from "../wheel.ts";

// Explicit host-only installation proof using an existing executable.
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
const wheel = hostWheel(version, runtime, readFileSync(packed.stagedNative), readFileSync(join(repositoryRoot, "LICENSE")));
writeFileSync(join(output, wheel.name), wheel.bytes);
smokeAliases(output, runtime, version);
