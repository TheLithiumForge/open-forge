import { randomInt, randomUUID } from "node:crypto";
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
// Docker's answer for a container or network that no longer exists.
const AlreadyRemoved = /no such (container|network)|\bnot found\b/i;
let leftoversSwept: Promise<unknown> | null = null;

// Docker's words when its default pools are full and when a requested subnet is already in use.
export const DockerNetworkMessages = { poolsExhausted: "all predefined address pools have been fully subnetted", subnetTaken: "Pool overlaps with other one on this address space" } as const;
// Past Docker's default pools, which hold about thirty networks, a stage takes a /24 from this private range.
export const OverflowSubnets = { prefix: "10.213", count: 256 } as const;

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
      await this.createNetwork();
      this.containerAttempted = true;
      await this.required(createArguments(this.name, image, limits));
      await this.required(["start", this.name]);
    } catch (error) {
      // A failed cleanup must not hide why the stage couldn't open.
      await this.remove().catch(() => undefined);
      throw error;
    }
  }

  /** Creates the stage network in Docker's default pools, or in the overflow range once those are full, as many parallel runs fill them. */
  private async createNetwork(): Promise<void> {
    const create = async (subnet: readonly string[]) => {
      const args = ["network", "create", "--label", StageLabel, ...subnet, `${this.name}-net`];
      return { args, result: await this.client.run(args, this.commandOptions()) };
    };
    const first = await create([]);
    if (first.result.code === 0 || !first.result.stderr.includes(DockerNetworkMessages.poolsExhausted)) { requireDockerResult(first.result, first.args); return; }
    const start = randomInt(OverflowSubnets.count);
    for (let step = 0; step < OverflowSubnets.count; step += 1) {
      const attempt = await create(["--subnet", `${OverflowSubnets.prefix}.${(start + step) % OverflowSubnets.count}.0/24`]);
      if (attempt.result.code === 0 || !attempt.result.stderr.includes(DockerNetworkMessages.subnetTaken)) { requireDockerResult(attempt.result, attempt.args); return; }
    }
    requireDockerResult(first.result, first.args);
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
    return requireDockerResult(await this.client.run(args, this.commandOptions()), args);
  }

  async copyIn(source: string, target: string): Promise<void> {
    await this.required(["cp", source, `${this.name}:${target}`]);
  }

  async copyOut(source: string, target: string): Promise<void> {
    // Recovery and removal still run after cancellation or the stage deadline.
    const args = ["cp", `${this.name}:${source}`, target];
    requireDockerResult(await this.client.run(args, { timeoutMs: ContainerTimeouts.copyMs }), args);
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
      try {
        const result = await this.client.run(args, { timeoutMs: ContainerTimeouts.commandMs });
        // A container or network something else already removed is gone, which is what cleanup wants.
        if (result.code !== 0 && !result.timedOut && AlreadyRemoved.test(result.stderr)) continue;
        requireDockerResult(result, args);
      }
      catch (error) { errors.push(error); }
    }
    if (errors.length > 0) throw new AggregateError(errors, `Container cleanup failed: ${errors.map((error) => error instanceof Error ? error.message : String(error)).join(" | ")}`);
  }
}
