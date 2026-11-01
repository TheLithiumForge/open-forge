import { ChildProcess, type ChildProcessWithoutNullStreams } from "node:child_process";
import { cp, mkdir, mkdtemp, readFile, readdir, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import { PassThrough } from "node:stream";
import type { Docker, DockerOptions } from "./docker.ts";
import type { ProcessResult } from "../process.ts";
import type { RunSpace } from "./executor.ts";
import { ContainerPaths, DefaultContainerLimits, type StageKind as ImageKind } from "./settings.ts";
import { evalImageTag } from "./docker-image.ts";
import assert from "node:assert/strict";

export const FakeResult: ProcessResult = { code: 0, stdout: "", stderr: "", timedOut: false };
export const FixtureFiles = { source: "source.txt", built: "dist/index.html", config: "tsconfig.json", lock: "package-lock.json", manifest: "package.json" } as const;
export const FixtureText = { source: "project input", built: "<html>built</html>", before: '{"token":"before"}', after: '{"token":"after"}', response: '{"result":"done"}' } as const;

export interface DockerCall { readonly args: readonly string[]; readonly options: DockerOptions; }

// Docker is unavailable in this worker. Real temporary files still exercise our copy and recovery effects.
export class FakeDocker implements Docker {
  readonly calls: DockerCall[] = [];
  readonly inputs: string[] = [];
  readonly names: string[] = [];
  readonly root: string;
  hook: ((args: readonly string[]) => Promise<ProcessResult | undefined>) | undefined;
  spawnOutput: string = FixtureText.response;
  spawnFailure = false;

  constructor(root: string) { this.root = root; }

  path(container: string, path: string): string { return join(this.root, container, ...path.split("/")); }

  private hostPath(path: string): string {
    for (const name of this.names) if (path.startsWith(`${name}:`)) return this.path(name, path.slice(name.length + 1));
    return resolve(path);
  }

  async run(args: readonly string[], options: DockerOptions): Promise<ProcessResult> {
    this.calls.push({ args: [...args], options });
    const hooked = await this.hook?.(args);
    if (hooked !== undefined) return hooked;
    if (args[0] === "create") {
      const name = args[args.indexOf("--name") + 1];
      if (name === undefined) throw new Error("Missing fake container name.");
      this.names.push(name);
      for (const path of [ContainerPaths.work, ContainerPaths.output, `${ContainerPaths.home}/.codex`, `${ContainerPaths.home}/.claude`]) await mkdir(this.path(name, path), { recursive: true });
    }
    if (args[0] === "cp") {
      const source = args[1], target = args[2];
      if (source === undefined || target === undefined) throw new Error("Missing fake copy path.");
      const from = this.hostPath(source), to = this.hostPath(target);
      await mkdir(dirname(to), { recursive: true });
      await cp(from, to, { recursive: true });
    }
    if (args.includes(ContainerPaths.collect)) {
      const name = this.names.at(-1);
      if (name !== undefined) await cp(this.path(name, ContainerPaths.work), this.path(name, ContainerPaths.output), { recursive: true });
    }
    return FakeResult;
  }

  spawn(args: readonly string[], options: DockerOptions): ChildProcessWithoutNullStreams {
    this.calls.push({ args: [...args], options });
    const stdin = new PassThrough(), stdout = new PassThrough(), stderr = new PassThrough();
    const stdio: [PassThrough, PassThrough, PassThrough, undefined, undefined] = [stdin, stdout, stderr, undefined, undefined];
    const child = Object.assign(new ChildProcess(), { stdin, stdout, stderr, stdio });
    let finished = false;
    const finish = (): void => {
      if (finished) return;
      finished = true;
      stdout.end(`${this.spawnOutput}\n`);
      stderr.end();
      child.emit("exit", 0);
      child.emit("close", 0);
    };
    child.kill = () => { finish(); return true; };
    stdin.on("data", (chunk: Buffer) => this.inputs.push(chunk.toString()));
    stdin.on("finish", finish);
    if (this.spawnFailure) queueMicrotask(() => { child.emit("error", new Error("Fake spawn failure.")); child.emit("close", null); });
    return child;
  }
}

export async function containerFixture(): Promise<{ space: RunSpace; client: FakeDocker; signInHome: string; dispose(): Promise<void> }> {
  const root = await mkdtemp(join(tmpdir(), "eval-containers-"));
  const space: RunSpace = { root, work: join(root, "project"), home: join(root, "home"), project: "project" };
  const signInHome = join(root, "sign-in");
  await mkdir(space.work);
  await mkdir(space.home);
  await mkdir(signInHome);
  await writeFile(join(space.work, FixtureFiles.source), FixtureText.source);
  return { space, client: new FakeDocker(join(root, "docker")), signInHome,
    async dispose() { await rm(root, { recursive: true, force: true }); } };
}

export function assertStrictStage(client: FakeDocker, kind: ImageKind, limits = DefaultContainerLimits): string {
  const create = client.calls.find((call) => call.args[0] === "create");
  assert.ok(create);
  const name = create.args[create.args.indexOf("--name") + 1];
  assert.ok(name);
  assert.deepEqual(create.args, ["create", "--name", name, "--network", `${name}-net`, "--cap-drop", "ALL", "--security-opt", "no-new-privileges", "--init",
    "--cpus", String(limits.cpus), "--memory", limits.memory, "--pids-limit", String(limits.processes), "--entrypoint", "sleep", evalImageTag(kind), "infinity"]);
  for (const call of client.calls) {
    if (call.args[0] === "create") assert.equal(call.args.some((arg) => ["-v", "--volume", "--volumes-from", "--mount", "-p", "--publish", "--publish-all", "--privileged", "--cap-add"].includes(arg)), false);
    assert.equal(call.args.some((arg) => arg.includes("docker.sock")), false);
  }
  assert.ok(client.calls.some((call) => JSON.stringify(call.args) === JSON.stringify(["rm", "--force", name])));
  assert.ok(client.calls.some((call) => JSON.stringify(call.args) === JSON.stringify(["network", "rm", `${name}-net`])));
  return name;
}

export async function readTree(folder: string): Promise<Record<string, string>> {
  const files: Record<string, string> = {};
  for (const path of await readdir(folder, { recursive: true, withFileTypes: true })) {
    if (path.isFile()) files[join(path.parentPath, path.name)] = await readFile(join(path.parentPath, path.name), "utf8");
  }
  return files;
}

