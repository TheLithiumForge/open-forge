import { existsSync } from "node:fs";
import { readdir, readFile, stat } from "node:fs/promises";
import { join, relative, sep } from "node:path";
import type { Isolation } from "../config/choices.ts";
import { NpmCommand, spawnInRun, type RunSpace } from "../agents/executor.ts";
import { waitForProcess } from "../agents/process.ts";
import type { BuildResult } from "../results/results.types.ts";

interface ProjectManifest {
  readonly scripts?: Readonly<Record<string, string>>;
}

interface LockFile {
  readonly packages?: Readonly<Record<string, unknown>>;
}

const BuiltPage = "index.html";
const skippedFolders = new Set(["node_modules", ".git", ".agents"]);

async function findPages(folder: string, since: number, found: string[] = []): Promise<string[]> {
  for (const entry of await readdir(folder, { withFileTypes: true })) {
    const path = join(folder, entry.name);
    if (entry.isDirectory() && !skippedFolders.has(entry.name)) await findPages(path, since, found);
    if (entry.isFile() && entry.name === BuiltPage && (await stat(path)).mtimeMs >= since) found.push(path);
  }
  return found;
}

async function readJson<T>(path: string): Promise<T | null> {
  return existsSync(path) ? JSON.parse(await readFile(path, "utf8")) : null;
}

/** Runs one npm command inside the run, with no network, like a user's machine offline. */
async function npmOffline(isolation: Isolation, space: RunSpace, args: readonly string[], timeoutMs: number): Promise<{ ok: boolean; log: string }> {
  const result = await waitForProcess(spawnInRun({ isolation, space, command: NpmCommand, args, network: false }), { timeoutMs });
  return { ok: result.code === 0 && !result.timedOut, log: `${result.stdout}\n${result.stderr}`.slice(-4000) };
}

/** Type-checks every root tsconfig that lists files, which also covers Vite's split configs. */
async function typecheck(isolation: Isolation, space: RunSpace, timeoutMs: number): Promise<boolean | null> {
  const configs = (await readdir(space.work)).filter((name) => /^tsconfig.*\.json$/.test(name));
  const checked: boolean[] = [];
  for (const config of configs) {
    const text = await readFile(join(space.work, config), "utf8");
    if (!text.includes('"include"') && !text.includes('"files"')) continue;
    checked.push((await npmOffline(isolation, space, ["exec", "--no", "--", "tsc", "--noEmit", "-p", config], timeoutMs)).ok);
  }
  return checked.length === 0 ? null : checked.every(Boolean);
}

/** Runs the project's own build script again, offline, and finds the index.html it writes. */
export async function buildProject(isolation: Isolation, space: RunSpace, timeoutMs: number): Promise<BuildResult> {
  const manifest = await readJson<ProjectManifest>(join(space.work, "package.json"));
  const failed = (log: string): BuildResult => ({ passed: false, typecheck: null, seconds: 0, packages: null, html: null, bytes: null, log });
  if (manifest?.scripts?.["build"] === undefined) return failed("The project has no build script.");
  const started = Date.now();
  const build = await npmOffline(isolation, space, ["run", "build"], timeoutMs);
  const seconds = (Date.now() - started) / 1000;
  const pages = (await findPages(space.work, started - 1000)).sort((a, b) => Number(b.includes(`${sep}dist${sep}`)) - Number(a.includes(`${sep}dist${sep}`)));
  const html = pages[0] ?? null;
  const lock = await readJson<LockFile>(join(space.work, "package-lock.json"));
  return {
    passed: build.ok && html !== null,
    typecheck: await typecheck(isolation, space, timeoutMs),
    seconds,
    packages: lock?.packages === undefined ? null : Object.keys(lock.packages).length - 1,
    html: html === null ? null : relative(space.work, html),
    bytes: html === null ? null : (await stat(html)).size,
    log: build.ok && html === null ? `${build.log}\nThe build wrote no index.html.` : build.log,
  };
}
