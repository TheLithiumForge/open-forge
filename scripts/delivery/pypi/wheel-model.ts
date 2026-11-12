import type { SupportedRuntime } from "../package-model.ts";
import { pythonModule } from "./package-model.ts";

export const PythonPackage = {
  suffix: ".whl",
  tag: "py3-none",
  minimum: ">=3.9",
  metadata: "METADATA",
  wheel: "WHEEL",
  record: "RECORD",
  entryPoints: "entry_points.txt",
  init: "__init__.py",
  license: "licenses/LICENSE",
  licenseFile: "LICENSE",
  universal: "any",
} as const;

// Match the existing ubuntu-24.04 and macos-15 native build hosts.
export const WheelPlatforms = {
  "win-x64": "win_amd64",
  "win-arm64": "win_arm64",
  "osx-x64": "macosx_15_0_x86_64",
  "osx-arm64": "macosx_15_0_arm64",
  "linux-x64": "manylinux_2_39_x86_64",
  "linux-arm64": "manylinux_2_39_aarch64",
} as const satisfies Record<SupportedRuntime, string>;

export function wheelTag(runtime?: SupportedRuntime): string {
  return `${PythonPackage.tag}-${runtime === undefined ? PythonPackage.universal : WheelPlatforms[runtime]}`;
}

export function wheelName(version: string, name: string, runtime?: SupportedRuntime): string {
  return `${pythonModule(name)}-${version}-${wheelTag(runtime)}${PythonPackage.suffix}`;
}

export function wheelInfo(version: string, name: string): string {
  return `${pythonModule(name)}-${version}.dist-info`;
}
