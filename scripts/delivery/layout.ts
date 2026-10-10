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
  const suffix = PlatformPackages[rid].nodePlatform === "win32" ? ".exe" : "";
  return [
    ...Object.entries(TestAssemblies).map(([name, assembly]) => ({ name, executable: `${base}/${name}/${assembly}.dll` })),
    { name: "native-integration", executable: `${nativeDirectory(rid)}/integration/${TestAssemblies.integration}${suffix}` },
    { name: "native-public", executable: `${nativeDirectory(rid)}/end-to-end/${TestAssemblies.public}${suffix}` },
    { name: "public-native", executable: `${base}/public-native/${TestAssemblies.public}.dll` },
  ];
}
