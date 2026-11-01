import { IgnoreFile } from "./ignore.ts";
import { safeRelative, sha256, type FileBytes, type FileHash } from "./files.ts";
import type { ScopePolicy } from "./scope.types.ts";

export const ForgeScope = { agents: "AGENTS.md", root: ".agents/", lock: ".agents/open-forge.lock.json" } as const;
export const isIgnoreFile = (path: string): boolean => path === IgnoreFile || path.endsWith(`/${IgnoreFile}`);

export function captureScope(files: readonly FileBytes[], openForge: boolean, project: readonly FileBytes[] = []): ScopePolicy {
  const originals = new Set(project.map((file) => file.path));
  return { ignores: files.filter((file) => isIgnoreFile(file.path)).map((file) => ({ path: file.path, content: file.bytes.toString("utf8") })), openForge,
    forgeFiles: openForge ? files.filter((file) => !originals.has(file.path) && file.path !== "README.md" && !isIgnoreFile(file.path)).map((file) => file.path) : [] };
}

export function verifyScope(policy: ScopePolicy, baseline: readonly FileHash[]): void {
  const ignores = baseline.filter((file) => isIgnoreFile(file.path));
  if (policy.ignores.length !== ignores.length || new Set(policy.ignores.map((file) => file.path)).size !== ignores.length
    || policy.ignores.some((file) => !ignores.some((original) => original.path === safeRelative(file.path) && original.sha256 === sha256(file.content)))) throw new Error("The saved ignore rules differ from the starting setup.");
  if (policy.openForge && !baseline.some((file) => file.path === ForgeScope.lock) || !policy.openForge && policy.forgeFiles.length) throw new Error("The saved Open Forge scope differs from the starting setup.");
  if (policy.forgeFiles.some((path) => !baseline.some((file) => file.path === safeRelative(path)))) throw new Error("An Open Forge file isn't in the starting setup.");
}

export function forgeScopePath(path: string, policy?: ScopePolicy): boolean {
  return policy?.openForge === true && (path === ForgeScope.agents || path.startsWith(ForgeScope.root) || policy.forgeFiles.includes(path));
}
