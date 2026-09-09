export const MainPackageName = "@thelithiumforge/open-forge";
export const TemplatePackageVersion = "0.0.0";
export const LocalVersionPrefix = "0.0.0-dev.sha-";
export const FullGitShaPattern = /^[0-9a-f]{40}$/u;
export const StableReleaseVersionPattern = /^(?:0|[1-9]\d*)\.(?:0|[1-9]\d*)\.(?:0|[1-9]\d*)$/u;

export const PlatformPackages = {
  "osx-x64": {
    runtime: "osx-x64",
    packageName: "@thelithiumforge/open-forge-darwin-x64",
    directoryName: "open-forge-darwin-x64",
    nodePlatform: "darwin",
    nodeArchitecture: "x64",
    nativeFileName: "open-forge",
  },
  "linux-x64": {
    runtime: "linux-x64",
    packageName: "@thelithiumforge/open-forge-linux-x64",
    directoryName: "open-forge-linux-x64",
    nodePlatform: "linux",
    nodeArchitecture: "x64",
    nativeFileName: "open-forge",
  },
  "win-x64": {
    runtime: "win-x64",
    packageName: "@thelithiumforge/open-forge-win-x64",
    directoryName: "open-forge-win-x64",
    nodePlatform: "win32",
    nodeArchitecture: "x64",
    nativeFileName: "open-forge.exe",
  },
  "osx-arm64": {
    runtime: "osx-arm64",
    packageName: "@thelithiumforge/open-forge-darwin-arm64",
    directoryName: "open-forge-darwin-arm64",
    nodePlatform: "darwin",
    nodeArchitecture: "arm64",
    nativeFileName: "open-forge",
  },
  "linux-arm64": {
    runtime: "linux-arm64",
    packageName: "@thelithiumforge/open-forge-linux-arm64",
    directoryName: "open-forge-linux-arm64",
    nodePlatform: "linux",
    nodeArchitecture: "arm64",
    nativeFileName: "open-forge",
  },
  "win-arm64": {
    runtime: "win-arm64",
    packageName: "@thelithiumforge/open-forge-win-arm64",
    directoryName: "open-forge-win-arm64",
    nodePlatform: "win32",
    nodeArchitecture: "arm64",
    nativeFileName: "open-forge.exe",
  },
} as const;

export type SupportedRuntime = keyof typeof PlatformPackages;
