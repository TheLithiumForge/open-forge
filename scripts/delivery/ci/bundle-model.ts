import { nativeDirectory, downloadDirectory, DownloadContent, CliAssembly, CliExecutable, WindowsExecutableSuffix, ArchiveSuffix } from "../layout.ts";
import { PlatformPackages, type SupportedRuntime } from "../package-model.ts";

export const BundleFile = { license: "LICENSE", readme: "README.txt", checksums: "SHA256SUMS" } as const;
export const BundleMode = 0o755;
export const BundleHash = { algorithm: "sha256", encoding: "hex" } as const;
export const TarExecutable = "tar";
export const TarArgument = { createGzip: "-czf", directory: "-C", content: "." } as const;

export function bundleNames(rid: SupportedRuntime) {
  const suffix = PlatformPackages[rid].nodePlatform === "win32" ? WindowsExecutableSuffix : "";
  const directory = downloadDirectory(rid);
  const file = `${CliExecutable}-${rid}${ArchiveSuffix}`;
  return {
    directory,
    content: `${directory}/${DownloadContent}`,
    source: `${nativeDirectory(rid)}/${CliExecutable}/${CliAssembly}${suffix}`,
    executable: `${CliExecutable}${suffix}`,
    file,
    archive: `${directory}/${file}`,
  };
}

export function bundleReadme(rid: SupportedRuntime): string {
  return `Development build for ${rid}. Check this workflow run for test results before use.\n`;
}

export function checksumLine(hash: string, file: string): string {
  return `${hash}  ${file}\n`;
}
