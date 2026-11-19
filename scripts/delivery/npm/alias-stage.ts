import { chmodSync, copyFileSync, mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { join } from "node:path";
import { aliasManifest, AliasLauncher, LauncherPath, WrapperReadme } from "./package-manifests.ts";
import { packageReadme } from "./package-readme.ts";

export function stageAlias(root: string, directory: string, version: string): void {
  mkdirSync(join(directory, "bin"), { recursive: true });
  writeFileSync(join(directory, LauncherPath), AliasLauncher);
  chmodSync(join(directory, LauncherPath), 0o755);
  copyFileSync(join(root, "LICENSE"), join(directory, "LICENSE"));
  writeFileSync(join(directory, WrapperReadme), packageReadme(readFileSync(join(root, WrapperReadme), "utf8")));
  writeFileSync(join(directory, "package.json"), `${JSON.stringify(aliasManifest(version), null, 2)}\n`);
}
