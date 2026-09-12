import { MainPackageName, PlatformPackages, type SupportedRuntime } from "../package-model.ts";
import { validateVersion } from "../version.ts";

const publication = { license: "MIT", publishConfig: { access: "public" } } as const;

export function wrapperManifest(version: string) {
  return {
    ...publication,
    name: MainPackageName,
    version: validateVersion(version),
    description: "Thin npm launcher for the Open Forge native CLI.",
    type: "module",
    bin: { "open-forge": "./bin/open-forge.js" },
    files: ["bin", "package-model.js", "LICENSE"],
    engines: { node: ">=22.18.0" },
    optionalDependencies: Object.fromEntries(Object.values(PlatformPackages).map((platform) => [platform.packageName, version])),
  };
}

export function nativeManifest(runtime: SupportedRuntime, version: string) {
  const platform = PlatformPackages[runtime];
  return {
    ...publication,
    name: platform.packageName,
    version: validateVersion(version),
    description: `Open Forge native CLI for ${runtime}.`,
    os: [platform.nodePlatform],
    cpu: [platform.nodeArchitecture],
    ...(platform.nodePlatform === "linux" ? { libc: "glibc" } : {}),
    files: ["bin", "LICENSE"],
  };
}
