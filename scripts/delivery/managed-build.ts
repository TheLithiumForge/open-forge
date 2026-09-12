import { run } from "./process.ts";
import { Configuration } from "./layout.ts";

export function managedBuild(root: string, version: string): void {
  run(
    "dotnet",
    ["build", "OpenForge.Cli.slnx", "--configuration", Configuration, "--no-restore", `-p:OpenForgeCliVersion=${version}`, `-p:OpenForgeCliInformationalVersion=${version}`],
    root,
  );
}
