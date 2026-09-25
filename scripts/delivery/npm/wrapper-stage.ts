import { chmodSync, copyFileSync, mkdirSync, writeFileSync } from "node:fs";
import { join } from "node:path";
import { WrapperReadme, wrapperManifest } from "./package-manifests.ts";

import { AllTargets } from "../targets.ts";
import type { SupportedRuntime } from "../package-model.ts";

export function stageWrapperPackage(root: string, directory: string, version: string, launcherDirectory: string, targets: readonly SupportedRuntime[] = AllTargets): void {
  mkdirSync(join(directory, "bin"), { recursive: true });
  const executable = join(directory, "bin/open-forge.js");
  copyFileSync(join(launcherDirectory, "npm/open-forge.js"), executable);
  chmodSync(executable, 0o755);
  copyFileSync(join(launcherDirectory, "package-model.js"), join(directory, "package-model.js"));
  copyFileSync(join(root, "LICENSE"), join(directory, "LICENSE"));
  // The repository README is the npm package page.
  copyFileSync(join(root, WrapperReadme), join(directory, WrapperReadme));
  writeFileSync(join(directory, "package.json"), `${JSON.stringify(wrapperManifest(version, targets), null, 2)}\n`);
}
