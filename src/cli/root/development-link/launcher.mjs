import { spawnSync } from "node:child_process";
import { statSync } from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

export const configurationEnvironmentVariable = "OPEN_FORGE_DEV_CONFIGURATION";

const defaultConfiguration = "Debug";
const supportedConfigurations = new Set([defaultConfiguration, "Release"]);
const executableSuffixes = new Map([
  ["linux", ""],
  ["win32", ".exe"],
]);
const failureExitCode = 1;
const repositoryRootRelativeUrl = "../../../../";

export function resolveConfiguration(value) {
  if (value === undefined) {
    return defaultConfiguration;
  }

  if (supportedConfigurations.has(value)) {
    return value;
  }

  const received = value.length === 0 ? "an empty value" : JSON.stringify(value);
  throw new Error(`${configurationEnvironmentVariable} must be exactly "Debug" or "Release"; received ${received}.`);
}

export function resolveDevelopmentExecutable(launcherUrl, platform, configuration) {
  const executableSuffix = executableSuffixes.get(platform);
  if (executableSuffix === undefined) {
    throw new Error(`open-forge-dev supports only Node platforms "linux" and "win32"; received ${JSON.stringify(platform)}.`);
  }

  const repositoryRoot = fileURLToPath(new URL(repositoryRootRelativeUrl, launcherUrl));
  return path.join(repositoryRoot, "artifacts", "publish", "open-forge-dev", configuration, `open-forge-dev${executableSuffix}`);
}

export function runDevelopmentLink(runtime, spawnProcess = spawnSync) {
  let configuration;
  try {
    configuration = resolveConfiguration(runtime.environment[configurationEnvironmentVariable]);
  } catch (error) {
    return failure(messageFrom(error));
  }

  let executable;
  try {
    executable = resolveDevelopmentExecutable(runtime.launcherUrl, runtime.platform, configuration);
  } catch (error) {
    return failure(messageFrom(error));
  }

  let candidate;
  try {
    candidate = statSync(executable);
  } catch (error) {
    const reason = error?.code === "ENOENT" ? "does not exist" : `could not be inspected: ${messageFrom(error)}`;
    return failure(`Managed development executable ${reason}: ${executable}. Build the ${configuration} configuration before invoking open-forge-dev.`);
  }

  if (!candidate.isFile()) {
    return failure(`Managed development executable is not a file: ${executable}. Build the ${configuration} configuration before invoking open-forge-dev.`);
  }

  let result;
  try {
    result = spawnProcess(executable, runtime.arguments, {
      cwd: runtime.currentDirectory,
      env: runtime.environment,
      shell: false,
      stdio: "inherit",
    });
  } catch (error) {
    return failure(`Unable to start managed development executable: ${messageFrom(error)}`);
  }

  if (result.error !== undefined) {
    return failure(`Unable to start managed development executable: ${messageFrom(result.error)}`);
  }

  if (Number.isInteger(result.status)) {
    return { kind: "exit", exitCode: result.status };
  }

  if (typeof result.signal === "string") {
    return { kind: "signal", signal: result.signal };
  }

  return failure("Managed development executable ended without an exit code or signal.");
}

export function finishDevelopmentLink(completion, writeError, signalProcess) {
  if (completion.kind === "exit") {
    return completion.exitCode;
  }

  if (completion.kind === "failure") {
    writeError(`${completion.message}\n`);
    return completion.exitCode;
  }

  try {
    signalProcess(completion.signal);
    return undefined;
  } catch (error) {
    writeError(`Unable to mirror managed development executable signal ${completion.signal}: ${messageFrom(error)}\n`);
    return failureExitCode;
  }
}

function failure(message) {
  return { kind: "failure", exitCode: failureExitCode, message };
}

function messageFrom(error) {
  return error instanceof Error ? error.message : String(error);
}
