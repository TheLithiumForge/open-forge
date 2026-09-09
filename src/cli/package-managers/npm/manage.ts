import { fileURLToPath } from "node:url";

import { linkLocalPackages, unlinkLocalPackages } from "./local-link.ts";
import type { SupportedRuntime } from "./package-model.ts";
import { stagePackages, type StageVersion } from "./stage.ts";

const repositoryRoot = fileURLToPath(new URL("../../../../", import.meta.url));
const failureExitCode = 1;

function main(argumentsToRead: readonly string[]): void {
  const [action, ...argumentsForAction] = argumentsToRead;

  if (action === "stage") {
    stage(argumentsForAction);
    return;
  }

  if (action === "link" || action === "unlink") {
    if (argumentsForAction.length !== 0) {
      throw new Error(`${action} does not accept arguments.`);
    }

    const npmCli = process.env["npm_execpath"];
    if (npmCli === undefined) {
      throw new Error(`Run ${action} through the root npm script.`);
    }

    if (action === "link") {
      linkLocalPackages(repositoryRoot, npmCli);
    } else {
      unlinkLocalPackages(repositoryRoot, npmCli);
    }

    return;
  }

  throw new Error('The package manager action must be exactly "stage", "link", or "unlink".');
}

function stage(argumentsToRead: readonly string[]): void {
  const [artifactsRoot, runtimeValue, nativeArtifact, versionKind, versionValue] = argumentsToRead;
  if (
    artifactsRoot === undefined ||
    runtimeValue === undefined ||
    nativeArtifact === undefined ||
    versionKind === undefined ||
    versionValue === undefined ||
    argumentsToRead.length !== 5
  ) {
    throw new Error("stage requires artifacts-root, runtime, native-artifact, version-kind, and version-value.");
  }

  const runtime = readRuntime(runtimeValue);
  const version = readVersion(versionKind, versionValue);
  stagePackages({ repositoryRoot, artifactsRoot, runtime, nativeArtifact, version });
}

function readRuntime(value: string): SupportedRuntime {
  if (value === "linux-x64" || value === "osx-x64" || value === "win-x64" || value === "linux-arm64" || value === "osx-arm64" || value === "win-arm64") {
    return value;
  }

  throw new Error(`Unsupported package runtime ${JSON.stringify(value)}.`);
}

function readVersion(kind: string, value: string): StageVersion {
  if (kind === "release") {
    return { kind, value };
  }

  if (kind === "local") {
    return { kind, sha: value };
  }

  throw new Error('The stage version kind must be exactly "release" or "local".');
}

try {
  main(process.argv.slice(2));
} catch (error) {
  const message = error instanceof Error ? error.message : String(error);
  process.stderr.write(`${message}\n`);
  process.exitCode ??= failureExitCode;
}
