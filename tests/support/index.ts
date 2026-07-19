import { afterAll } from "bun:test";
import fs from "node:fs/promises";
import os from "node:os";
import path from "node:path";

export const repoRoot = path.resolve(import.meta.dir, "..", "..");

export interface ProcessResult {
  exitCode: number;
  stdout: string;
  stderr: string;
}

export interface ProcessOptions {
  cwd?: string;
  env?: Record<string, string | undefined>;
  unsetEnv?: string[];
}

export interface TestSandbox {
  createDirectory(label?: string): Promise<string>;
}

export type TreeStateEntry =
  | { kind: "directory" }
  | { kind: "file"; content: string }
  | { kind: "symlink"; target: string };

export function repoPath(...segments: string[]): string {
  return path.join(repoRoot, ...segments);
}

export function useTestSandbox(prefix: string): TestSandbox {
  let root: string | null = null;
  let counter = 0;

  afterAll(async () => {
    if (root) {
      await fs.rm(root, { recursive: true, force: true });
    }
  });

  return {
    async createDirectory(label = "case"): Promise<string> {
      root ??= await fs.mkdtemp(path.join(os.tmpdir(), `${safeSegment(prefix)}-`));
      const directory = path.join(root, `${String(counter++).padStart(3, "0")}-${safeSegment(label)}`);
      await fs.mkdir(directory, { recursive: true });
      return directory;
    }
  };
}

export async function runProcess(command: readonly string[], options: ProcessOptions = {}): Promise<ProcessResult> {
  const env = { ...process.env, ...options.env };
  for (const key of options.unsetEnv ?? []) {
    delete env[key];
  }

  const child = Bun.spawn([...command], {
    cwd: options.cwd ?? repoRoot,
    env,
    stdin: "ignore",
    stdout: "pipe",
    stderr: "pipe"
  });
  const [exitCode, stdout, stderr] = await Promise.all([
    child.exited,
    new Response(child.stdout).text(),
    new Response(child.stderr).text()
  ]);
  return { exitCode, stdout, stderr };
}

export async function runCli(
  cliFile: string,
  args: readonly string[],
  options: ProcessOptions = {}
): Promise<ProcessResult> {
  return runProcess([process.execPath, cliFile, ...args], options);
}

export async function runGit(root: string, ...args: string[]): Promise<ProcessResult> {
  return runProcess(["git", "-C", root, ...args], { cwd: root });
}

export async function initializeGitRepository(root: string): Promise<void> {
  requireSuccess(await runGit(root, "init"), "git init");
  requireSuccess(
    await runGit(root, "config", "user.email", "open-forge-tests@example.invalid"),
    "git config user.email"
  );
  requireSuccess(await runGit(root, "config", "user.name", "Open Forge Tests"), "git config user.name");
}

export async function commitAll(root: string, message: string): Promise<void> {
  requireSuccess(await runGit(root, "add", "-A"), "git add");
  requireSuccess(await runGit(root, "commit", "-m", message), "git commit");
}

export function requireSuccess(result: ProcessResult, label: string): void {
  if (result.exitCode !== 0) {
    throw new Error(`${label} failed with exit code ${result.exitCode}:\n${result.stdout}${result.stderr}`);
  }
}

export async function writeText(file: string, contents: string): Promise<void> {
  await fs.mkdir(path.dirname(file), { recursive: true });
  await fs.writeFile(file, contents);
}

export async function pathExists(value: string): Promise<boolean> {
  return fs.access(value).then(() => true, () => false);
}

export async function isFile(value: string): Promise<boolean> {
  return fs.stat(value).then((stat) => stat.isFile(), () => false);
}

export async function isDirectory(value: string): Promise<boolean> {
  return fs.stat(value).then((stat) => stat.isDirectory(), () => false);
}

export async function listFiles(
  root: string,
  predicate: (file: string) => boolean = () => true
): Promise<string[]> {
  const entries = await fs.readdir(root, { withFileTypes: true });
  const files: string[] = [];

  for (const entry of entries) {
    const fullPath = path.join(root, entry.name);
    if (entry.isDirectory()) {
      files.push(...await listFiles(fullPath, predicate));
    } else if (entry.isFile() && predicate(fullPath)) {
      files.push(fullPath);
    }
  }

  return files.sort((left, right) => left.localeCompare(right, "en"));
}

export async function copyTreeContents(source: string, target: string): Promise<void> {
  for (const sourceFile of await listFiles(source)) {
    const targetFile = path.join(target, path.relative(source, sourceFile));
    await fs.mkdir(path.dirname(targetFile), { recursive: true });
    await fs.copyFile(sourceFile, targetFile);
  }
}

export async function snapshotTree(root: string): Promise<Record<string, string>> {
  const snapshot: Record<string, string> = {};
  for (const file of await listFiles(root)) {
    snapshot[toPosix(path.relative(root, file))] = (await fs.readFile(file)).toString("base64");
  }
  return snapshot;
}

export async function snapshotTreeState(root: string): Promise<Record<string, TreeStateEntry>> {
  const snapshot: Record<string, TreeStateEntry> = {};
  await walkTreeState(root, root, snapshot);
  return snapshot;
}

export function toPosix(value: string): string {
  return value.split(path.sep).join("/");
}

function safeSegment(value: string): string {
  const normalized = value.toLowerCase().replace(/[^a-z0-9-]+/g, "-").replace(/^-+|-+$/g, "");
  return normalized || "test";
}

async function walkTreeState(root: string, directory: string, snapshot: Record<string, TreeStateEntry>): Promise<void> {
  const entries = await fs.readdir(directory, { withFileTypes: true });
  entries.sort((left, right) => left.name.localeCompare(right.name, "en"));
  for (const entry of entries) {
    const absolute = path.join(directory, entry.name);
    const relative = toPosix(path.relative(root, absolute));
    if (entry.isDirectory()) {
      snapshot[relative] = { kind: "directory" };
      await walkTreeState(root, absolute, snapshot);
    } else if (entry.isFile()) {
      snapshot[relative] = { kind: "file", content: (await fs.readFile(absolute)).toString("base64") };
    } else if (entry.isSymbolicLink()) {
      snapshot[relative] = { kind: "symlink", target: await fs.readlink(absolute) };
    }
  }
}
