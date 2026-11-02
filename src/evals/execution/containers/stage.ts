import { randomUUID } from "node:crypto";
import type { ChildProcessWithoutNullStreams } from "node:child_process";
import { docker, requireDockerResult, type Docker, type DockerOptions } from "./docker.ts";
import { containerLimits, ContainerPaths, ContainerTimeouts, ContainerUsers, type ContainerLimits } from "./settings.ts";
import { registerStageCleanup } from "./shutdown.ts";
import type { ProcessResult } from "../process.ts";

export interface StageOptions {
  readonly docker?: Docker;
  readonly limits?: ContainerLimits;
  readonly timeoutMs: number;
  readonly signal?: AbortSignal;
}

// Eval networks carry this label, so a later process can find the ones a killed process left behind.
export const StageLabel = "open-forge-evals";
// A labelled network this old with no container belongs to a process that stopped without cleaning up.
const LeftoverNetworkAge = "30m";
let leftoversSwept: Promise<unknown> | null = null;

/** Docker's default address pools hold about thirty networks, so leftovers from killed processes block new stages. */
function sweepLeftoverNetworks(client: Docker): Promise<unknown> {
  leftoversSwept ??= client.run(["network", "prune", "--force", "--filter", `label=${StageLabel}`, "--filter", `until=${LeftoverNetworkAge}`],
    { timeoutMs: ContainerTimeouts.commandMs }).catch(() => undefined);
  return leftoversSwept;
}

export function createArguments(name: string, image: string, limits: ContainerLimits): string[] {
  return ["create", "--name", name, "--network", `${name}-net`, "--cap-drop", "ALL", "--security-opt", "no-new-privileges", "--init",
    "--cpus", String(limits.cpus), "--memory", limits.memory, "--pids-limit", String(limits.processes),
    "--entrypoint", "sleep", image, "infinity"];
}

export class ContainerStage {
  readonly name = `open-forge-eval-${randomUUID()}`;
  private readonly client: Docker;
  private readonly options: StageOptions;
  private readonly deadline: number;
  private readonly children = new Map<ChildProcessWithoutNullStreams, (signal?: NodeJS.Signals | number) => boolean>();
  private timer: ReturnType<typeof setTimeout> | undefined;
  private removal: Promise<void> | undefined;
  private stopping: Promise<void> | undefined;
  private cancelled = false;
  private containerAttempted = false;
  private networkAttempted = false;
  private unregisterShutdown = (): void => {};
  private readonly abort = (): void => { this.cancelled = true; void this.stop().catch(() => undefined); };

  constructor(options: StageOptions) {
    if (!Number.isFinite(options.timeoutMs) || options.timeoutMs <= 0) throw new Error("A stage needs a positive timeout.");
    this.options = options;
    this.client = options.docker ?? docker;
    this.deadline = Date.now() + options.timeoutMs;
  }

  async open(image: string): Promise<void> {
    const limits = containerLimits(this.options.limits);
    this.options.signal?.throwIfAborted();
    this.options.signal?.addEventListener("abort", this.abort, { once: true });
    this.timer = setTimeout(this.abort, this.options.timeoutMs);
    this.unregisterShutdown = registerStageCleanup(() => this.remove());
    try {
      await sweepLeftoverNetworks(this.client);
      this.networkAttempted = true;
      await this.required(["network", "create", "--label", StageLabel, `${this.name}-net`]);
      this.containerAttempted = true;
      await this.required(createArguments(this.name, image, limits));
      await this.required(["start", this.name]);
    } catch (error) {
      // A failed cleanup must not hide why the stage couldn't open.
      await this.remove().catch(() => undefined);
      throw error;
    }
  }

  private commandOptions(env?: Readonly<Record<string, string>>, input?: string): DockerOptions {
    this.checkActive();
    return { timeoutMs: this.deadline - Date.now(), ...(this.options.signal === undefined ? {} : { signal: this.options.signal }),
      ...(env === undefined ? {} : { env }), ...(input === undefined ? {} : { input }) };
  }

  checkActive(): void {
    this.options.signal?.throwIfAborted();
    const remaining = this.deadline - Date.now();
    if (this.cancelled || remaining <= 0) throw new Error("The container stage was cancelled or timed out.");
  }

  async required(args: readonly string[]): Promise<ProcessResult> {
    return requireDockerResult(await this.client.run(args, this.commandOptions()));
  }

  async copyIn(source: string, target: string): Promise<void> {
    await this.required(["cp", source, `${this.name}:${target}`]);
  }

  async copyOut(source: string, target: string): Promise<void> {
    // Recovery and removal still run after cancellation or the stage deadline.
    requireDockerResult(await this.client.run(["cp", `${this.name}:${source}`, target], { timeoutMs: ContainerTimeouts.commandMs }));
  }

  execArguments(command: readonly string[], user: string, env: Readonly<Record<string, string>>, auth: readonly string[] = []): string[] {
    return ["exec", "-i", "--user", user, "--workdir", ContainerPaths.work,
      ...Object.entries(env).flatMap(([key, value]) => ["--env", `${key}=${value}`]),
      ...auth.flatMap((key) => ["--env", key]), this.name, ...command];
  }

  async exec(command: readonly string[], user: string = ContainerUsers.agent, env: Readonly<Record<string, string>> = {}, input?: string, maxBytes?: number): Promise<ProcessResult> {
    return this.client.run(this.execArguments(command, user, env), { ...this.commandOptions(undefined, input), ...(maxBytes === undefined ? {} : { maxBytes }) });
  }

  spawn(command: readonly string[], env: Readonly<Record<string, string>>, auth: Readonly<Record<string, string>>): ChildProcessWithoutNullStreams {
    const child = this.client.spawn(this.execArguments(command, ContainerUsers.agent, env, Object.keys(auth)), this.commandOptions(auth));
    const kill = child.kill.bind(child);
    this.children.set(child, kill);
    child.once("exit", () => this.children.delete(child));
    child.once("error", () => { this.children.delete(child); void this.stop().catch(() => undefined); });
    child.kill = (signal) => {
      // End CLI helpers without destroying the home needed by a later conversation turn.
      void this.client.run(this.execArguments(["node", ContainerPaths.stopTools], ContainerUsers.agent, {}), { timeoutMs: ContainerTimeouts.commandMs })
        .catch(() => undefined).finally(() => kill(signal));
      return true;
    };
    return child;
  }

  async prepare(...folders: readonly string[]): Promise<void> {
    requireDockerResult(await this.exec(["node", ContainerPaths.prepare, ...folders], ContainerUsers.root));
  }

  async offline(): Promise<void> {
    await this.required(["network", "disconnect", `${this.name}-net`, this.name]);
  }

  stop(): Promise<void> {
    if (!this.containerAttempted) return Promise.resolve();
    this.stopping ??= this.client.run(["kill", this.name], { timeoutMs: ContainerTimeouts.commandMs }).then(() => {
      for (const kill of this.children.values()) kill("SIGKILL");
    });
    return this.stopping;
  }

  remove(): Promise<void> {
    this.removal ??= this.removeResources();
    return this.removal;
  }

  private async removeResources(): Promise<void> {
    clearTimeout(this.timer);
    this.options.signal?.removeEventListener("abort", this.abort);
    this.unregisterShutdown();
    const errors: unknown[] = [];
    try { await this.stopping; } catch (error) { errors.push(error); }
    // The container must leave its network before the network can be removed.
    const commands = [...(this.containerAttempted ? [["rm", "--force", this.name]] : []), ...(this.networkAttempted ? [["network", "rm", `${this.name}-net`]] : [])];
    for (const args of commands) {
      try { requireDockerResult(await this.client.run(args, { timeoutMs: ContainerTimeouts.commandMs })); }
      catch (error) { errors.push(error); }
    }
    if (errors.length > 0) throw new AggregateError(errors, `Container cleanup failed: ${errors.map((error) => error instanceof Error ? error.message : String(error)).join(" | ")}`);
  }
}
