import { MainPackageName, PlatformPackages, type SupportedRuntime } from "../package-model.ts";
import { validateVersion } from "../version.ts";

import { AllTargets, targetDependencies } from "../targets.ts";

export const WrapperReadme = "README.md";

const publication = { license: "MIT", publishConfig: { access: "public" } } as const;

// Shown on the npm package page for every published package.
const projectLinks = {
  homepage: "https://thelithiumforge.github.io/open-forge/",
  repository: { type: "git", url: "git+https://github.com/TheLithiumForge/open-forge.git" },
  bugs: { url: "https://github.com/TheLithiumForge/open-forge/issues" },
} as const;

export function wrapperManifest(version: string, targets: readonly SupportedRuntime[] = AllTargets) {
  return {
    ...publication,
    ...projectLinks,
    name: MainPackageName,
    version: validateVersion(version),
    description: "Open Forge CLI: find context, keep navigation correct, and manage a small Markdown framework for working with AI agents.",
    keywords: ["ai-agents", "agents-md", "context-engineering", "markdown", "cli"],
    type: "module",
    bin: { "open-forge": "./bin/open-forge.js" },
    files: ["bin", "package-model.js", "LICENSE", WrapperReadme],
    engines: { node: ">=22.18.0" },
    optionalDependencies: targetDependencies(version, targets),
  };
}

export function nativeManifest(runtime: SupportedRuntime, version: string) {
  const platform = PlatformPackages[runtime];
  return {
    ...publication,
    ...projectLinks,
    name: platform.packageName,
    version: validateVersion(version),
    description: `Open Forge native CLI for ${runtime}.`,
    os: [platform.nodePlatform],
    cpu: [platform.nodeArchitecture],
    ...(platform.nodePlatform === "linux" ? { libc: "glibc" } : {}),
    files: ["bin", "LICENSE"],
  };
}
