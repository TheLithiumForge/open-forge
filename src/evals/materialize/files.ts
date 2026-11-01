import { createHash } from "node:crypto";
import { lstat, mkdir, readFile, readdir, writeFile } from "node:fs/promises";
import { dirname, isAbsolute, join, relative, resolve, sep } from "node:path";

export interface FileBytes { readonly path: string; readonly bytes: Buffer }
export interface FileHash { readonly path: string; readonly sha256: string }
export const MarkdownSuffix = ".md";
export const GitDirectory = ".git";

export function sha256(bytes: string | Uint8Array): string {
  return createHash("sha256").update(bytes).digest("hex");
}

export function jsonBytes(value: unknown): Buffer {
  return Buffer.from(`${JSON.stringify(value, null, 2)}\n`);
}

export function sourceFingerprint(files: readonly FileHash[]): string {
  return sha256(files.map((file) => `${file.path}\0${file.sha256}\n`).join(""));
}

export function safeRelative(path: string): string {
  if (!path || path === "." || isAbsolute(path) || /[\\:\0]/.test(path) || path.split("/").some((part) => !part || part === ".." || part === ".")) {
    throw new Error(`Unsafe relative path: ${path}`);
  }
  return path;
}

export function contained(parent: string, child: string): boolean {
  const path = relative(resolve(parent), resolve(child));
  return path === "" || (path !== ".." && !path.startsWith(`..${sep}`) && !isAbsolute(path));
}

export async function ordinaryPath(path: string): Promise<void> {
  const absolute = resolve(path);
  if (dirname(absolute) !== absolute) await ordinaryPath(dirname(absolute));
  try {
    if ((await lstat(absolute)).isSymbolicLink()) throw new Error("Links are unsupported in materialize paths.");
  } catch (error) {
    if (!(error instanceof Error && "code" in error && error.code === "ENOENT")) throw error;
  }
}

/** `excluded` names are skipped at any depth. `rootExcluded` names are skipped only at the root, so a nested folder of the same name still counts. */
export async function inventory(root: string, excluded: ReadonlySet<string> = new Set([GitDirectory]), rootExcluded: ReadonlySet<string> = new Set()): Promise<FileBytes[]> {
  await ordinaryPath(root);
  const files: FileBytes[] = [];
  async function walk(folder: string, prefix: string): Promise<void> {
    for (const entry of await readdir(folder, { withFileTypes: true })) {
      if (excluded.has(entry.name) || (prefix === "" && rootExcluded.has(entry.name))) continue;
      const path = prefix ? `${prefix}/${entry.name}` : entry.name;
      const fullPath = join(folder, entry.name);
      if (entry.isSymbolicLink()) throw new Error(`Links are unsupported: ${path}`);
      if (entry.isDirectory()) await walk(fullPath, path);
      else if (entry.isFile()) files.push({ path: safeRelative(path), bytes: await readFile(fullPath) });
      else throw new Error(`Unsupported file: ${path}`);
    }
  }
  await walk(root, "");
  return files.sort((left, right) => left.path < right.path ? -1 : left.path > right.path ? 1 : 0);
}

export function hashes(files: readonly FileBytes[]): FileHash[] {
  return files.map((file) => ({ path: file.path, sha256: sha256(file.bytes) }));
}

export async function writeNew(root: string, path: string, bytes: string | Uint8Array): Promise<void> {
  const target = join(root, safeRelative(path));
  await ordinaryPath(target);
  await mkdir(dirname(target), { recursive: true });
  await writeFile(target, bytes, { flag: "wx" });
}

export async function emptyDestination(destination: string, sources: readonly string[]): Promise<string> {
  const target = resolve(destination);
  if (sources.some((source) => contained(source, target) || contained(target, source))) throw new Error("Source and destination need separate directories.");
  await ordinaryPath(target);
  await mkdir(target, { recursive: true });
  if ((await readdir(target)).length) throw new Error("Destination must be empty.");
  return target;
}
