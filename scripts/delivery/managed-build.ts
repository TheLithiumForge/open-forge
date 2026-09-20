import { run } from "./process.ts";
import { Configuration } from "./layout.ts";
import { restoreDependencies } from "./restore-dependencies.ts";

export interface BuildRestoreOptions {
  offline?: boolean;
  "no-restore"?: boolean;
}

export function managedBuild(root: string, version: string, options: BuildRestoreOptions): void {
  if (options.offline || !options["no-restore"]) restoreDependencies(root, options.offline);
  run("dotnet", ["build", "OpenForge.Cli.slnx", "--configuration", Configuration, "--no-restore", `-p:OpenForgeCliVersion=${version}`], root);
}
