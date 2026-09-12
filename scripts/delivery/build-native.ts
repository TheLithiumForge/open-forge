import { cpSync, existsSync, mkdirSync, renameSync, writeFileSync } from "node:fs";
import { join } from "node:path";
import type { SupportedRuntime } from "./package-model.ts";
import { compileLauncher } from "./npm/stage.ts";
import { reportFailure, run } from "./process.ts";
import { managedBuild } from "./managed-build.ts";
import { readBuildOptions } from "./options.ts";
import { repositoryRoot } from "./repository.ts";
import { candidateVersion, committedVersion } from "./version.ts";
import { Configuration, hostRuntime, deliveryDirectory, DevelopmentPublish, nativeDirectory, Projects, suites, TestAssemblies } from "./layout.ts";
import { createManifest } from "./manifest.ts";
import { sourceIdentity } from "./source.ts";

function nativeBuild(root: string, rid: SupportedRuntime, version: string): void {
  const directory = deliveryDirectory(rid);
  const absolute = join(root, directory);
  if (existsSync(absolute)) renameSync(absolute, `${absolute}-${Date.now()}`);
  mkdirSync(join(absolute, "build"), { recursive: true });
  const source = sourceIdentity(root);
  compileLauncher(root, join(absolute, "build/launcher"));
  managedBuild(root, version);
  for (const [name, assembly] of Object.entries(TestAssemblies)) cpSync(join(root, "artifacts/bin", assembly, "release"), join(absolute, "build", name), { recursive: true });
  const properties = ["-p:OpenForgeSkipDevelopmentPublish=true", `-p:OpenForgeCliVersion=${version}`, `-p:OpenForgeCliInformationalVersion=${version}`];
  for (const [project, folder] of [
    [Projects.cli, "open-forge"],
    [Projects.integration, "integration"],
    [Projects.public, "end-to-end"],
  ]) {
    if (project === undefined || folder === undefined) throw new Error("Missing publish project.");
    run(
      "dotnet",
      [
        "publish",
        project,
        "--configuration",
        Configuration,
        "--runtime",
        rid,
        "--self-contained",
        "true",
        "--no-restore",
        "--output",
        `${nativeDirectory(rid)}/${folder}`,
        ...properties,
      ],
      root,
    );
  }
  run("dotnet", ["build", Projects.public, "--configuration", Configuration, "--no-restore", `-p:OpenForgeEndToEndTargetRuntimeIdentifier=${rid}`, ...properties], root);
  cpSync(join(root, "artifacts/bin", TestAssemblies.public, "release"), join(absolute, "build/public-native"), { recursive: true });
  const native = suites(rid)
    .filter((suite) => suite.name.startsWith("native-"))
    .map((suite) => suite.executable);
  native.unshift(`${nativeDirectory(rid)}/open-forge/OpenForge.Cli${rid.startsWith("win-") ? ".exe" : ""}`);
  const manifest = { ...createManifest(root, { ...source, version, rid }, native, [`${directory}/build`, DevelopmentPublish, nativeDirectory(rid)]), ...source, tested: false };
  writeFileSync(join(absolute, "manifest.json"), `${JSON.stringify(manifest, null, 2)}\n`);
  process.stdout.write(`Built ${version}: ${directory}/manifest.json\n`);
}

try {
  const values = readBuildOptions();
  const version = committedVersion(repositoryRoot);
  nativeBuild(repositoryRoot, hostRuntime(values.rid), candidateVersion(version, values.sha ? sourceIdentity(repositoryRoot).sha : undefined));
} catch (error) {
  reportFailure(error);
}
