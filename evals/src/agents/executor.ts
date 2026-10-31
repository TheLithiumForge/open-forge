import { spawn, type ChildProcessWithoutNullStreams } from "node:child_process";
import { join } from "node:path";
import { Isolation } from "../config/choices.ts";
import { authVariableNames } from "../config/auth.ts";
import { imageToolCommand, localToolCommand, type PinnedTool, type ToolCommand } from "./pinned-tools.ts";
import { evalImageTag } from "./docker-image.ts";

/** One run's folders: the project the agent works in, and a fresh home for the tools' own files. */
export interface RunSpace {
  readonly root: string;
  readonly work: string;
  readonly home: string;
  readonly project: string;
}

export const ContainerPaths = { home: "/home/agent", work: "/work" } as const;

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

export function localEnvironment(home: string, network: boolean): Record<string, string> {
  const base = Object.entries(process.env).filter(([name]) => passedVariables.has(name.toUpperCase()));
  const auth = authVariableNames().map((name) => [name, process.env[name] ?? ""]);
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

function dockerArguments(request: ExecRequest, tool: ToolCommand): string[] {
  const { space } = request;
  const containerWork = `${ContainerPaths.work}/${space.project}`;
  const env = Object.entries(toolEnvironment(ContainerPaths.home)).map(([name, value]) => `${name}=${value.replaceAll("\\", "/")}`);
  return [
    "run", "--rm", "-i",
    "--network", request.network ? "bridge" : "none",
    "-v", `${space.work}:${containerWork}`,
    "-v", `${space.home}:${ContainerPaths.home}`,
    "-w", containerWork,
    ...env.flatMap((pair) => ["-e", pair]),
    // By name only: Docker reads each value from this process, so tokens never appear in arguments.
    ...authVariableNames().flatMap((name) => ["-e", name]),
    evalImageTag(), tool.command, ...tool.args, ...request.args,
  ];
}

export function spawnInRun(request: ExecRequest): ChildProcessWithoutNullStreams {
  if (request.isolation === Isolation.docker) {
    const tool = request.command === NpmCommand ? { command: NpmCommand, args: [] } : imageToolCommand(request.command);
    return spawn("docker", dockerArguments(request, tool), { env: process.env, windowsHide: true });
  }
  const tool = localCommand(request.command);
  return spawn(tool.command, [...tool.args, ...request.args], {
    cwd: request.space.work,
    env: localEnvironment(request.space.home, request.network),
    windowsHide: true,
  });
}
