import { execFileSync, spawnSync } from "node:child_process";
import { existsSync } from "node:fs";
import { join } from "node:path";

import { FullGitShaPattern, LocalVersionPrefix, MainPackageName, PlatformPackages } from "./package-model.ts";
import { stagePackages } from "./stage.ts";

const npmLinkArguments = ["--offline", "--ignore-scripts", "--no-save", "--no-package-lock", "--no-audit", "--no-fund"];

export function linkLocalPackages(repositoryRoot: string, npmCli: string): void {
  const platformPackage = currentPlatformPackage();
  const sha = readHeadSha(repositoryRoot);
  const version = `${LocalVersionPrefix}${sha}`;
  const publishDirectory = join(repositoryRoot, "artifacts", "publish", platformPackage.runtime, "open-forge");
  const publishedFileName = platformPackage.nodePlatform === "win32" ? "OpenForge.Cli.exe" : "OpenForge.Cli";

  runCommand(
    "dotnet",
    [
      "publish",
      join(repositoryRoot, "src", "cli", "root", "OpenForge.Cli", "OpenForge.Cli.csproj"),
      "--configuration",
      "Release",
      "--runtime",
      platformPackage.runtime,
      "--self-contained",
      "true",
      "--no-restore",
      "--output",
      publishDirectory,
      "-p:OpenForgeSkipDevelopmentPublish=true",
      `-p:OpenForgeCliInformationalVersion=${version}`,
    ],
    repositoryRoot,
  );

  const staged = stagePackages({
    repositoryRoot,
    artifactsRoot: join(repositoryRoot, "artifacts"),
    runtime: platformPackage.runtime,
    nativeArtifact: join(publishDirectory, publishedFileName),
    version: { kind: "local", sha },
  });

  runNpm(npmCli, ["link"], staged.platformPackageDirectory);
  runNpm(npmCli, ["link", platformPackage.packageName], staged.mainPackageDirectory);
  runNpm(npmCli, ["link"], staged.mainPackageDirectory);
  runNpm(npmCli, ["link", MainPackageName], repositoryRoot);
}

export function unlinkLocalPackages(repositoryRoot: string, npmCli: string): void {
  const platformPackage = currentPlatformPackage();
  const mainPackageDirectory = join(repositoryRoot, "artifacts", "npm", "stage", platformPackage.runtime, "open-forge");

  runNpm(npmCli, ["unlink", MainPackageName], repositoryRoot);
  runNpm(npmCli, ["unlink", "--global", MainPackageName], repositoryRoot);
  if (existsSync(mainPackageDirectory)) {
    runNpm(npmCli, ["unlink", platformPackage.packageName], mainPackageDirectory);
  }
  runNpm(npmCli, ["unlink", "--global", platformPackage.packageName], repositoryRoot);
}

function currentPlatformPackage(): (typeof PlatformPackages)[keyof typeof PlatformPackages] {
  const platformPackage = Object.values(PlatformPackages).find((candidate) => candidate.nodePlatform === process.platform && candidate.nodeArchitecture === process.arch);

  if (platformPackage === undefined) {
    throw new Error(`Local npm linking does not support ${process.platform}/${process.arch}.`);
  }

  return platformPackage;
}

function readHeadSha(repositoryRoot: string): string {
  const sha = execFileSync("git", ["rev-parse", "--verify", "HEAD"], { cwd: repositoryRoot, encoding: "utf8" }).trim();
  if (!FullGitShaPattern.test(sha)) {
    throw new Error("Git did not return one full lowercase 40-character HEAD SHA.");
  }

  return sha;
}

function runNpm(npmCli: string, command: readonly string[], currentDirectory: string): void {
  runCommand(process.execPath, [npmCli, ...command, ...npmLinkArguments], currentDirectory);
}

function runCommand(executable: string, argumentsToPass: readonly string[], currentDirectory: string): void {
  const completion = spawnSync(executable, argumentsToPass, {
    cwd: currentDirectory,
    env: process.env,
    shell: false,
    stdio: "inherit",
  });

  if (completion.error !== undefined) {
    throw completion.error;
  }

  if (completion.status === 0) {
    return;
  }

  if (completion.status !== null) {
    process.exitCode = completion.status;
    throw new Error(`${executable} exited with status ${completion.status}.`);
  }

  if (completion.signal !== null) {
    process.kill(process.pid, completion.signal);
    return;
  }

  throw new Error(`${executable} ended without an exit code or signal.`);
}
