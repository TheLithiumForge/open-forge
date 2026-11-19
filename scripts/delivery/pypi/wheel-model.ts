import { AliasPackageName, PlatformPackages, type SupportedRuntime } from "../package-model.ts";
import { ThirdPartyNotices } from "../layout.ts";

// PyPI has no scopes, so the one distribution takes the unscoped npm alias name.
export const PythonPackageName = AliasPackageName;

export const PythonPackage = {
  suffix: ".whl",
  tag: "py3-none",
  minimum: ">=3.9",
  metadata: "METADATA",
  wheel: "WHEEL",
  record: "RECORD",
  license: "licenses/LICENSE",
  licenseFile: "LICENSE",
  notices: `licenses/${ThirdPartyNotices}`,
  noticesFile: ThirdPartyNotices,
  // Installers copy `<name>-<version>.data/scripts/` onto the environment's scripts directory, which is on PATH.
  scripts: "scripts",
} as const;

export const WheelField = {
  name: "Name",
  version: "Version",
  python: "Requires-Python",
  license: "License-File",
  pure: "Root-Is-Purelib",
  tag: "Tag",
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

// Wheel file and directory names use the normalized distribution name.
const wheelPrefix = (version: string) => `${PythonPackageName.replaceAll("-", "_")}-${version}`;

export function wheelTag(runtime: SupportedRuntime): string {
  return `${PythonPackage.tag}-${WheelPlatforms[runtime]}`;
}

export function wheelName(version: string, runtime: SupportedRuntime): string {
  return `${wheelPrefix(version)}-${wheelTag(runtime)}${PythonPackage.suffix}`;
}

export function wheelInfo(version: string): string {
  return `${wheelPrefix(version)}.dist-info`;
}

export function wheelScript(version: string, runtime: SupportedRuntime): string {
  return `${wheelPrefix(version)}.data/${PythonPackage.scripts}/${PlatformPackages[runtime].nativeFileName}`;
}
