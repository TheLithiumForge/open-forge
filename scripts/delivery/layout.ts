import assert from "node:assert/strict";
import { PlatformPackages, type SupportedRuntime } from "./package-model.ts";

export const Configuration = "Release";
export const Projects = {
  cli: "src/cli/root/OpenForge.Cli/OpenForge.Cli.csproj",
  integration: "src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj",
  public: "src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj",
} as const;
export const TestAssemblies = { unit: "OpenForge.Cli.Core.UnitTests", integration: "OpenForge.Cli.IntegrationTests", public: "OpenForge.Cli.EndToEndTests" } as const;
export const DevelopmentPublish = "artifacts/publish/open-forge-dev/Release";
export const WrapperOutput = "artifacts/delivery/wrapper";
export const DownloadsRoot = "artifacts/downloads";
export const DeliveryLogs = "artifacts/delivery/logs";
export const DocumentationRoot = "src/docusaurus";
export const DownloadContent = "content";
export const CliAssembly = "OpenForge.Cli";
export const CliExecutable = "open-forge";
export const WindowsExecutableSuffix = ".exe";
export const ArchiveSuffix = ".tar.gz";
/** Licenses of the third-party code compiled into the native executable. It ships next to LICENSE wherever the executable does. */
export const ThirdPartyNotices = "THIRD-PARTY-NOTICES.txt";

/** Test projects that build:native publishes as native executables. Each runs as its own test:built suite. */
export const NativeTestSuites = [
  { name: "native-integration", project: Projects.integration, folder: "integration", assembly: TestAssemblies.integration },
  { name: "native-public", project: Projects.public, folder: "end-to-end", assembly: TestAssemblies.public },
] as const;
export type NativeTestSuite = (typeof NativeTestSuites)[number];

export interface SkippedSuite {
  readonly name: NativeTestSuite["name"];
  readonly reason: string;
}

// Maintainer decision, 2026-11-16. On the Intel macOS runner, publishing the native Integration
// executable took 21-23 minutes on 14 November, for a suite that then ran in 3. On 16 November the
// Intel runners were 1.5 to 2 times slower: that publish took 30-36 minutes and the job hit its
// 90-minute limit. osx-arm64 still runs native Integration for macOS, as do the other four hosts.
// Revisit when Intel runner speed recovers or the Build job gets more headroom.
const SkippedSuites: Readonly<Partial<Record<SupportedRuntime, readonly SkippedSuite[]>>> = {
  "osx-x64": [
    {
      name: "native-integration",
      reason: "Its native publish is too slow on the Intel macOS runner for the Build time limit. osx-arm64 runs it for macOS.",
    },
  ],
};

/** Suites this RID neither builds nor runs, each with its reason. */
export function skippedSuites(rid: SupportedRuntime): readonly SkippedSuite[] {
  return SkippedSuites[rid] ?? [];
}

export function skippedSuiteNotice(rid: SupportedRuntime, skip: SkippedSuite): string {
  return `${skip.name}: skipped on ${rid}. ${skip.reason}`;
}

/** The native test executables build:native publishes for this RID. */
export function nativeTestSuites(rid: SupportedRuntime): readonly NativeTestSuite[] {
  const skipped = new Set(skippedSuites(rid).map((skip) => skip.name));
  return NativeTestSuites.filter((suite) => !skipped.has(suite.name));
}

export function downloadDirectory(rid: string): string {
  return `${DownloadsRoot}/${rid}`;
}
export function deliveryLog(rid: string, stage: string): string {
  return `${DeliveryLogs}/${rid}/${stage}.log`;
}

export function hostRuntime(requested?: string): SupportedRuntime {
  const platform = Object.values(PlatformPackages).find((entry) => entry.nodePlatform === process.platform && entry.nodeArchitecture === process.arch);
  assert.ok(platform, `Unsupported native host ${process.platform}/${process.arch}.`);
  assert.ok(requested === undefined || requested === platform.runtime, "The RID must match the current native host.");
  return platform.runtime;
}

export function deliveryDirectory(rid: string): string {
  return `artifacts/delivery/${rid}`;
}
export function nativeDirectory(rid: string): string {
  return `artifacts/publish/${rid}`;
}

export function suites(rid: SupportedRuntime): readonly { name: string; executable: string }[] {
  const base = `${deliveryDirectory(rid)}/build`;
  const suffix = PlatformPackages[rid].nodePlatform === "win32" ? WindowsExecutableSuffix : "";
  return [
    ...Object.entries(TestAssemblies).map(([name, assembly]) => ({ name, executable: `${base}/${name}/${assembly}.dll` })),
    ...nativeTestSuites(rid).map((suite) => ({ name: suite.name, executable: `${nativeDirectory(rid)}/${suite.folder}/${suite.assembly}${suffix}` })),
    { name: "public-native", executable: `${base}/public-native/${TestAssemblies.public}.dll` },
  ];
}
