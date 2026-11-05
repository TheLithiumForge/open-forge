import { lstat, readdir, readFile } from "node:fs/promises";
import { join, relative, resolve, sep } from "node:path";
import * as ts from "typescript";
import { object } from "./requirements.ts";

export const SourceExcluded = new Set(["node_modules", ".git", ".agents", ".codex", ".claude", "dist", "test-results", "playwright-report", "docs", "art", ".eval-command-results", ".npm-cache", ".playwright-browsers", ".browser-work", ".test-tmp"]);
/** A game may name its own folder like these, so only the project's top-level ones are left out. */
const TopLevelOnly = new Set(["docs", "art"]);
export interface SourceFile { readonly path: string; readonly text: string }
export async function sourceFiles(root: string, folder = root): Promise<SourceFile[]> {
  const result: SourceFile[] = [];
  for (const entry of await readdir(folder, { withFileTypes: true })) {
    if ((SourceExcluded.has(entry.name) && (folder === root || !TopLevelOnly.has(entry.name))) || entry.name.startsWith(".env")) continue;
    const path = join(folder, entry.name), info = await lstat(path);
    if (info.isSymbolicLink()) continue;
    if (info.isDirectory()) result.push(...await sourceFiles(root, path));
    else if (info.isFile()) {
      if (info.size > 2 * 1024 * 1024) throw new Error("Source file exceeds the checker limit.");
      result.push({ path: relative(root, path).split(sep).join("/"), text: await readFile(path, "utf8") });
    }
  }
  return result;
}
export function jsonObject(text: string): Record<string, unknown> {
  const value: unknown = JSON.parse(text);
  if (!object(value)) throw new Error("Expected a JSON object.");
  return value;
}
export function jsonConfig(text: string): Record<string, unknown> {
  const parsed = ts.parseConfigFileTextToJson("tsconfig.json", text);
  if (parsed.error || !object(parsed.config)) throw new Error("Invalid compiler JSON.");
  return parsed.config;
}
export function compilerOptions(files: readonly SourceFile[], path: string, visited: readonly string[] = []): Record<string, unknown> {
  if (visited.includes(path)) throw new Error("Compiler configuration extends a cycle.");
  const file = files.find((item) => item.path === path);
  if (!file) throw new Error(`Missing compiler configuration ${path}.`);
  const config = jsonConfig(file.text), base = config["extends"];
  let inherited: Record<string, unknown> = {};
  if (typeof base === "string") {
    const root = resolve("config-root"), full = resolve(root, path, "..", base);
    const parent = relative(root, full).split(sep).join("/");
    if (parent.startsWith("../") || parent === "..") throw new Error("Compiler extends leaves the project.");
    inherited = compilerOptions(files, parent.endsWith(".json") ? parent : `${parent}.json`, [...visited, path]);
  }
  return { ...inherited, ...(object(config["compilerOptions"]) ? config["compilerOptions"] : {}) };
}
export function syntax(file: SourceFile): ts.SourceFile { return ts.createSourceFile(file.path, file.text, ts.ScriptTarget.Latest, true, file.path.endsWith("x") ? ts.ScriptKind.TSX : ts.ScriptKind.TS); }
export function descendants(node: ts.Node): ts.Node[] {
  const found: ts.Node[] = [];
  const visit = (item: ts.Node): void => { found.push(item); item.forEachChild(visit); };
  visit(node); return found;
}
export function authored(file: SourceFile): boolean { return /\.(?:ts|tsx)$/.test(file.path) && !/(?:^|\/)(?:__tests__|tests|scripts)\/|\.test\./.test(file.path); }
