import { execFileSync } from "node:child_process";
import { chmodSync, copyFileSync, mkdirSync, rmSync, statSync, writeFileSync } from "node:fs";
import { join } from "node:path";
import rootPackage from "../../../package.json" with { type: "json" };
import { candidateVersion, validateVersion } from "../../delivery/version.ts";

import darwinArm64PackageTemplate from "./darwin-arm64/package.json" with { type: "json" };
import darwinPackageTemplate from "./darwin-x64/package.json" with { type: "json" };
import linuxArm64PackageTemplate from "./linux-arm64/package.json" with { type: "json" };
import linuxPackageTemplate from "./linux-x64/package.json" with { type: "json" };
import mainPackageTemplate from "./main/package.json" with { type: "json" };
import { FullGitShaPattern, MainPackageName, PlatformPackages, type SupportedRuntime } from "./package-model.ts";
import windowsArm64PackageTemplate from "./win-arm64/package.json" with { type: "json" };
import windowsPackageTemplate from "./win-x64/package.json" with { type: "json" };

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

const platformPackageTemplates = {
  "osx-x64": darwinPackageTemplate,
  "linux-x64": linuxPackageTemplate,
  "win-x64": windowsPackageTemplate,
  "osx-arm64": darwinArm64PackageTemplate,
  "linux-arm64": linuxArm64PackageTemplate,
  "win-arm64": windowsArm64PackageTemplate,
} as const satisfies Record<SupportedRuntime, { name: string; version: string; private: boolean }>;

export function stagePackages(request: StageRequest): StagedPackages {
  const platformPackage = PlatformPackages[request.runtime];
  const platformTemplate = platformPackageTemplates[request.runtime];
  const version = resolveVersion(request.version);
  validateInputs(request, platformTemplate.name, platformPackage.packageName);

  const npmArtifacts = join(request.artifactsRoot, "npm");
  const buildDirectory = join(npmArtifacts, "build", request.runtime);
  const stageDirectory = join(npmArtifacts, "stage", request.runtime);
  const mainPackageDirectory = join(stageDirectory, "open-forge");
  const platformPackageDirectory = join(stageDirectory, platformPackage.directoryName);

  rmSync(buildDirectory, { force: true, recursive: true });
  rmSync(stageDirectory, { force: true, recursive: true });
  if (request.compiledLauncher === undefined) compileLauncher(request.repositoryRoot, buildDirectory);
  const launcherDirectory = request.compiledLauncher ?? buildDirectory;

  const mainBinDirectory = join(mainPackageDirectory, "bin");
  const platformBinDirectory = join(platformPackageDirectory, "bin");
  mkdirSync(mainBinDirectory, { recursive: true });
  mkdirSync(platformBinDirectory, { recursive: true });

  copyExecutable(join(launcherDirectory, "main", "open-forge.js"), join(mainBinDirectory, "open-forge.js"));
  copyFileSync(join(launcherDirectory, "package-model.js"), join(mainPackageDirectory, "package-model.js"));
  copyExecutable(request.nativeArtifact, join(platformBinDirectory, platformPackage.nativeFileName));
  copyFileSync(join(request.repositoryRoot, "LICENSE"), join(mainPackageDirectory, "LICENSE"));
  copyFileSync(join(request.repositoryRoot, "LICENSE"), join(platformPackageDirectory, "LICENSE"));

  const { private: mainPrivate, ...publicMainTemplate } = mainPackageTemplate;
  const { private: platformPrivate, ...publicPlatformTemplate } = platformTemplate;
  void mainPrivate;
  void platformPrivate;
  writeManifest(join(mainPackageDirectory, "package.json"), {
    ...publicMainTemplate,
    version,
    optionalDependencies: {
      [PlatformPackages["osx-x64"].packageName]: version,
      [PlatformPackages["linux-x64"].packageName]: version,
      [PlatformPackages["win-x64"].packageName]: version,
      [PlatformPackages["osx-arm64"].packageName]: version,
      [PlatformPackages["linux-arm64"].packageName]: version,
      [PlatformPackages["win-arm64"].packageName]: version,
    },
  });
  writeManifest(join(platformPackageDirectory, "package.json"), { ...publicPlatformTemplate, version });

  return { version, mainPackageDirectory, platformPackageDirectory };
}

function resolveVersion(version: StageVersion): string {
  if (version.kind === "release") {
    return validateVersion(version.value);
  }

  if (!FullGitShaPattern.test(version.sha)) {
    throw new Error("The local version requires one full lowercase 40-character Git SHA.");
  }

  return candidateVersion(rootPackage.version, version.sha);
}

function validateInputs(request: StageRequest, templateName: string, packageName: string): void {
  if (!statSync(request.nativeArtifact).isFile()) {
    throw new Error(`The native artifact is not a regular file: ${request.nativeArtifact}`);
  }

  if (mainPackageTemplate.name !== MainPackageName || mainPackageTemplate.version !== rootPackage.version) {
    throw new Error("The tracked main package template does not match the package model.");
  }

  if (templateName !== packageName || platformPackageTemplates[request.runtime].version !== rootPackage.version) {
    throw new Error(`The tracked ${request.runtime} package template does not match the package model.`);
  }
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
