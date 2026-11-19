import { execFileSync } from "node:child_process";
import { join } from "node:path";
import { PlatformPackages, type SupportedRuntime } from "../package-model.ts";
import { hostWheel } from "./wheel.ts";

export function expectedHostWheel(directory: string, version: string, runtime: SupportedRuntime): { name: string; bytes: Buffer } {
  const archive = join(directory, `open-forge-${version}-${runtime}.tar.gz`);
  const extract = (path: string) => execFileSync("tar", ["-xOf", archive, path], { maxBuffer: 256 * 1024 * 1024 });
  return hostWheel(version, runtime, extract(PlatformPackages[runtime].nativeFileName), extract("LICENSE"));
}
