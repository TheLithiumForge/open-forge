import { execFileSync } from "node:child_process";
import { join } from "node:path";
import { PlatformPackages, type SupportedRuntime } from "../package-model.ts";
import { hostWheels } from "./wheel.ts";
import { AllTargets } from "../targets.ts";

export function expectedHostWheels(directory: string, version: string, runtime: SupportedRuntime, targets: readonly SupportedRuntime[] = AllTargets): Map<string, Buffer> {
  const archive = join(directory, `open-forge-${version}-${runtime}.tar.gz`);
  const extract = (path: string) => execFileSync("tar", ["-xOf", archive, path], { maxBuffer: 256 * 1024 * 1024 });
  return hostWheels(version, runtime, extract(PlatformPackages[runtime].nativeFileName), extract("LICENSE"), targets);
}
