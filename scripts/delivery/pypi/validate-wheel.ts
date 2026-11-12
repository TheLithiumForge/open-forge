import assert from "node:assert/strict";
import { execFileSync } from "node:child_process";
import { readFileSync } from "node:fs";
import { join } from "node:path";
import { PlatformPackages, type SupportedRuntime } from "../package-model.ts";
import { createWheel } from "./wheel.ts";
import { pythonVersion } from "./version.ts";
import { wheelName } from "./wheel-model.ts";

export function validateWheel(directory: string, version: string, runtime: SupportedRuntime): void {
  const archive = join(directory, `open-forge-${version}-${runtime}.tar.gz`);
  const extract = (path: string) => execFileSync("tar", ["-xOf", archive, path], { maxBuffer: 256 * 1024 * 1024 });
  const expected = createWheel(version, runtime, extract(PlatformPackages[runtime].nativeFileName), extract("LICENSE"));
  assert.deepEqual(readFileSync(join(directory, wheelName(pythonVersion(version), runtime))), expected, "Wheel differs from the staged native release.");
}
