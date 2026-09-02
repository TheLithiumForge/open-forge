import { spawnSync } from "node:child_process";
import { fileURLToPath, pathToFileURL } from "node:url";

const packageName = "@thelithiumforge/open-forge-development-link";
const packageDirectoryArgument = "./src/cli/root/development-link";
const repositoryRootRelativeUrl = "../../../../";
const stateProtectionArguments = ["--no-save", "--no-package-lock"];
const failureExitCode = 1;

export function runLinkManager(request, spawnProcess = spawnSync) {
  if (request.npmCli === undefined) {
    return failure("Run the development-link action through the root npm script.");
  }

  const commands = commandsFor(request.action, request.forwardedArguments);
  if (commands === undefined) {
    return failure('The development-link action must be exactly "link" or "unlink".');
  }

  for (const command of commands) {
    let result;
    try {
      result = spawnProcess(request.nodeExecutable, [request.npmCli, ...command], {
        cwd: request.repositoryRoot,
        env: request.environment,
        shell: false,
        stdio: "inherit",
      });
    } catch (error) {
      return failure(`Unable to run npm ${request.action}: ${messageFrom(error)}`);
    }

    if (result.error !== undefined) {
      return failure(`Unable to run npm ${request.action}: ${messageFrom(result.error)}`);
    }

    if (Number.isInteger(result.status)) {
      if (result.status !== 0) {
        return { kind: "exit", exitCode: result.status };
      }

      continue;
    }

    if (typeof result.signal === "string") {
      return { kind: "signal", signal: result.signal };
    }

    return failure(`npm ${request.action} ended without an exit code or signal.`);
  }

  return { kind: "exit", exitCode: 0 };
}

export function finishLinkManager(completion, writeError, signalProcess) {
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
    writeError(`Unable to mirror npm signal ${completion.signal}: ${messageFrom(error)}\n`);
    return failureExitCode;
  }
}

function commandsFor(action, forwardedArguments) {
  if (action === "link") {
    return [["link", packageDirectoryArgument, ...forwardedArguments, ...stateProtectionArguments]];
  }

  if (action === "unlink") {
    return [
      ["unlink", packageName, ...forwardedArguments, ...stateProtectionArguments],
      ["unlink", "--global", packageName, ...forwardedArguments, ...stateProtectionArguments],
    ];
  }

  return undefined;
}

function failure(message) {
  return { kind: "failure", exitCode: failureExitCode, message };
}

function messageFrom(error) {
  return error instanceof Error ? error.message : String(error);
}

const isEntryPoint = process.argv[1] !== undefined && pathToFileURL(process.argv[1]).href === import.meta.url;
if (isEntryPoint) {
  const repositoryRoot = fileURLToPath(new URL(repositoryRootRelativeUrl, import.meta.url));
  const completion = runLinkManager({
    action: process.argv[2],
    forwardedArguments: process.argv.slice(3),
    nodeExecutable: process.execPath,
    npmCli: process.env.npm_execpath,
    repositoryRoot,
    environment: process.env,
  });
  const exitCode = finishLinkManager(
    completion,
    (message) => process.stderr.write(message),
    (signal) => process.kill(process.pid, signal),
  );

  if (exitCode !== undefined) {
    process.exitCode = exitCode;
  }
}
