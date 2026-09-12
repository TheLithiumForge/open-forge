import { execFileSync } from "node:child_process";
import { chmodSync, copyFileSync, mkdirSync, rmSync, statSync, writeFileSync } from "node:fs";
import { join } from "node:path";
import { candidateVersion, committedVersion, validateVersion } from "../version.ts";
import { nativeManifest } from "./package-manifests.ts";
import { stageWrapperPackage } from "./wrapper-stage.ts";

import { PlatformPackages, type SupportedRuntime } from "../package-model.ts";
import { FullGitShaPattern } from "../source-identity.ts";

export type StageVersion = { kind: "release"; value: string } | { kind: "local"; sha: string };

export interface StageRequest {
  repositoryRoot: string;
  artifactsRoot: string;
  runtime: SupportedRuntime;
  nativeArtifact: string;
  version: StageVersion;
  compiledLauncher?: string;
}

export interface StagedPackages {
  version: string;
  mainPackageDirectory: string;
  platformPackageDirectory: string;
}

export function stagePackages(request: StageRequest): StagedPackages {
  const platformPackage = PlatformPackages[request.runtime];
  const version = resolveVersion(request.repositoryRoot, request.version);
  if (!statSync(request.nativeArtifact).isFile()) throw new Error(`The native artifact is not a regular file: ${request.nativeArtifact}`);

  const npmArtifacts = join(request.artifactsRoot, "npm");
  const buildDirectory = join(npmArtifacts, "build", request.runtime);
  const stageDirectory = join(npmArtifacts, "stage", request.runtime);
  const mainPackageDirectory = join(stageDirectory, "open-forge");
  const platformPackageDirectory = join(stageDirectory, platformPackage.directoryName);

  rmSync(buildDirectory, { force: true, recursive: true });
  rmSync(stageDirectory, { force: true, recursive: true });
  if (request.compiledLauncher === undefined) compileLauncher(request.repositoryRoot, buildDirectory);
  const launcherDirectory = request.compiledLauncher ?? buildDirectory;

  const platformBinDirectory = join(platformPackageDirectory, "bin");
  mkdirSync(platformBinDirectory, { recursive: true });

  stageWrapperPackage(request.repositoryRoot, mainPackageDirectory, version, launcherDirectory);
  copyExecutable(request.nativeArtifact, join(platformBinDirectory, platformPackage.nativeFileName));
  copyFileSync(join(request.repositoryRoot, "LICENSE"), join(platformPackageDirectory, "LICENSE"));

  writeManifest(join(platformPackageDirectory, "package.json"), nativeManifest(request.runtime, version));

  return { version, mainPackageDirectory, platformPackageDirectory };
}

function resolveVersion(root: string, version: StageVersion): string {
  if (version.kind === "release") {
    return validateVersion(version.value);
  }

  if (!FullGitShaPattern.test(version.sha)) {
    throw new Error("The local version requires one full lowercase 40-character Git SHA.");
  }

  return candidateVersion(committedVersion(root), version.sha);
}

export function compileLauncher(repositoryRoot: string, outputDirectory: string): void {
  const npmCli = process.env["npm_execpath"];
  if (npmCli === undefined) throw new Error("Run launcher compilation through npm run.");
  execFileSync(process.execPath, [npmCli, "run", "build:launcher", "--", "--outDir", outputDirectory], { cwd: repositoryRoot, stdio: "inherit" });
}

function copyExecutable(source: string, destination: string): void {
  copyFileSync(source, destination);
  chmodSync(destination, 0o755);
}

function writeManifest(path: string, manifest: object): void {
  writeFileSync(path, `${JSON.stringify(manifest, undefined, 2)}\n`, "utf8");
}
