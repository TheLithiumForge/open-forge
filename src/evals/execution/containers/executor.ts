import { spawn, type ChildProcessWithoutNullStreams } from "node:child_process";
import { join } from "node:path";
import { Isolation } from "../../config/choices.ts";
import { Tool } from "../../config/choices.ts";
import { authVariableNames } from "../../config/auth.ts";
import { localToolCommand, PinnedTool, type ToolCommand } from "../providers/pinned-tools.ts";
export { ContainerPaths } from "./settings.ts";

/** One run's folders: the project the agent works in, and a fresh home for the tools' own files. */
export interface RunSpace {
  readonly root: string;
  readonly work: string;
  readonly home: string;
  readonly project: string;
}

export interface ContainerExecutor {
  spawn(command: ExecRequest["command"], args: readonly string[]): ChildProcessWithoutNullStreams;
  syncHome(): Promise<void>;
}
const activeExecutors = new WeakMap<RunSpace, ContainerExecutor>();

export function registerContainer(space: RunSpace, executor: ContainerExecutor): () => void {
  if (activeExecutors.has(space)) throw new Error("A container is already running for this space.");
  activeExecutors.set(space, executor);
  return () => { activeExecutors.delete(space); };
}

export async function syncRunHome(space: RunSpace): Promise<void> {
  await activeExecutors.get(space)?.syncHome();
}

export interface ExecRequest {
  readonly isolation: Isolation;
  readonly space: RunSpace;
  readonly command: PinnedTool | typeof NpmCommand;
  readonly args: readonly string[];
  readonly network: boolean;
}

export const NpmCommand = "npm";

// Only what a process needs to start. The maintainer's other variables, settings and sign-ins stay out.
const passedVariables = new Set(["PATH", "SYSTEMROOT", "COMSPEC", "PATHEXT", "TEMP", "TMP", "WINDIR", "LANG", "TERM", "APPDATA", "LOCALAPPDATA", "PROGRAMDATA", "PROGRAMFILES", "NUMBER_OF_PROCESSORS", "PROCESSOR_ARCHITECTURE", "OS"]);

function toolEnvironment(home: string): Record<string, string> {
  return {
    HOME: home,
    USERPROFILE: home,
    CLAUDE_CONFIG_DIR: join(home, ".claude"),
    CODEX_HOME: join(home, ".codex"),
    CLAUDE_CODE_DISABLE_AUTO_MEMORY: "1",
    DISABLE_AUTOUPDATER: "1",
    IS_SANDBOX: "1",
  };
}

export function localEnvironment(home: string, network: boolean, provider?: Tool): Record<string, string> {
  const base = Object.entries(process.env).filter(([name]) => passedVariables.has(name.toUpperCase()));
  const auth = authVariableNames(provider).map((name) => [name, process.env[name] ?? ""]);
  const offline: Record<string, string> = network ? {} : { npm_config_offline: "true" };
  return { ...Object.fromEntries([...base, ...auth]), ...toolEnvironment(home), ...offline };
}

function localCommand(command: ExecRequest["command"]): ToolCommand {
  if (command !== NpmCommand) return localToolCommand(command);
  // npm sets npm_execpath when it starts this server, which avoids a shell for npm.cmd on Windows.
  const npmCli = process.env["npm_execpath"];
  if (npmCli === undefined) throw new Error("Start the evals with npm start, so the runner can find npm.");
  return { command: process.execPath, args: [npmCli] };
}

export function spawnInRun(request: ExecRequest): ChildProcessWithoutNullStreams {
  if (request.isolation === Isolation.docker) {
    const executor = activeExecutors.get(request.space);
    if (executor === undefined) throw new Error("Open the container stage before starting a tool.");
    return executor.spawn(request.command, request.args);
  }
  const tool = localCommand(request.command);
  return spawn(tool.command, [...tool.args, ...request.args], {
    cwd: request.space.work,
    env: localEnvironment(request.space.home, request.network, request.command === PinnedTool.codex ? Tool.codex : request.command === PinnedTool.claude ? Tool.claude : Tool.mock),
    windowsHide: true,
  });
}
