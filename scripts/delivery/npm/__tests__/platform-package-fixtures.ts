export const ExpectedMainPackageName = "@thelithiumforge/open-forge";
export const ExpectedMainDirectory = "open-forge";
export const ExpectedLauncherPath = "bin/open-forge.js";
export const ExpectedMainFiles = ["LICENSE", "bin/open-forge.js", "package-model.js", "package.json"];
export const FixtureVersion = "2.3.4-beta.5";
export const UnsupportedRuntime = "linux-musl-arm64";
export const FixtureNativeBytes = Buffer.from("Open Forge inert package layout fixture. Never execute.\n", "utf8");

// These literals are independent conformance oracles for the accepted distribution.
export const ExpectedPlatforms = [
  {
    runtime: "linux-x64",
    packageName: "@thelithiumforge/open-forge-linux-x64",
    directoryName: "open-forge-linux-x64",
    nodePlatform: "linux",
    nodeArchitecture: "x64",
    nativeFileName: "open-forge",
    libc: "glibc",
  },
  {
    runtime: "osx-x64",
    packageName: "@thelithiumforge/open-forge-darwin-x64",
    directoryName: "open-forge-darwin-x64",
    nodePlatform: "darwin",
    nodeArchitecture: "x64",
    nativeFileName: "open-forge",
    libc: undefined,
  },
  {
    runtime: "win-x64",
    packageName: "@thelithiumforge/open-forge-win-x64",
    directoryName: "open-forge-win-x64",
    nodePlatform: "win32",
    nodeArchitecture: "x64",
    nativeFileName: "open-forge.exe",
    libc: undefined,
  },
  {
    runtime: "linux-arm64",
    packageName: "@thelithiumforge/open-forge-linux-arm64",
    directoryName: "open-forge-linux-arm64",
    nodePlatform: "linux",
    nodeArchitecture: "arm64",
    nativeFileName: "open-forge",
    libc: "glibc",
  },
  {
    runtime: "osx-arm64",
    packageName: "@thelithiumforge/open-forge-darwin-arm64",
    directoryName: "open-forge-darwin-arm64",
    nodePlatform: "darwin",
    nodeArchitecture: "arm64",
    nativeFileName: "open-forge",
    libc: undefined,
  },
  {
    runtime: "win-arm64",
    packageName: "@thelithiumforge/open-forge-win-arm64",
    directoryName: "open-forge-win-arm64",
    nodePlatform: "win32",
    nodeArchitecture: "arm64",
    nativeFileName: "open-forge.exe",
    libc: undefined,
  },
] as const;

export const ExpectedOptionalDependencies = {
  "@thelithiumforge/open-forge-linux-x64": FixtureVersion,
  "@thelithiumforge/open-forge-darwin-x64": FixtureVersion,
  "@thelithiumforge/open-forge-win-x64": FixtureVersion,
  "@thelithiumforge/open-forge-linux-arm64": FixtureVersion,
  "@thelithiumforge/open-forge-darwin-arm64": FixtureVersion,
  "@thelithiumforge/open-forge-win-arm64": FixtureVersion,
};
