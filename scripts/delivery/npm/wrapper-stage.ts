import { chmodSync, copyFileSync, mkdirSync, writeFileSync } from "node:fs";
import { join } from "node:path";
import { wrapperManifest } from "./package-manifests.ts";

export function stageWrapperPackage(root: string, directory: string, version: string, launcherDirectory: string): void {
  mkdirSync(join(directory, "bin"), { recursive: true });
  const executable = join(directory, "bin/open-forge.js");
  copyFileSync(join(launcherDirectory, "npm/open-forge.js"), executable);
  chmodSync(executable, 0o755);
  copyFileSync(join(launcherDirectory, "package-model.js"), join(directory, "package-model.js"));
  copyFileSync(join(root, "LICENSE"), join(directory, "LICENSE"));
  writeFileSync(join(directory, "package.json"), `${JSON.stringify(wrapperManifest(version), null, 2)}\n`);
}
