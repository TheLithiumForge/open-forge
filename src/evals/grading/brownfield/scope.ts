import { matchesGlob } from "node:path";
import { GitDirectory, inventory, sha256, type FileHash } from "../../materialize/files.ts";
import type { CheckOutcome } from "../grading.types.ts";
import { CheckStatus } from "../../history/statuses.ts";
import { ignoredBy, IgnoreFile } from "../../materialize/ignore.ts";
import { forgeScopePath } from "../../materialize/scope-policy.ts";
import type { ScopePolicy } from "../../materialize/scope.types.ts";

export const BrownfieldGuard = { scope: "brownfield-diff-scope", protected: "brownfield-protected-inputs" } as const;
// Installs and build output at the project root are regenerated, so they never count as the agent's change.
// They're skipped only at the root: a file in a nested folder of the same name, or in an agent's config folder, still counts.
export const TransientRootFolders = new Set(["node_modules", "dist", "coverage", "test-results", "playwright-report"]);
export const NeverSource = new Set([GitDirectory]);
export interface ChangedFile { readonly path: string; readonly change: "added" | "changed" | "deleted"; readonly allowed: boolean; readonly openForge?: boolean }

export function sourcePath(path: string): boolean {
  const parts = path.split("/");
  return !TransientRootFolders.has(parts[0] ?? "") && !parts.some((part) => NeverSource.has(part));
}

export function changedFiles(before: readonly FileHash[], after: readonly FileHash[], patterns: readonly string[], policy?: ScopePolicy): ChangedFile[] {
  const ignored = ignoredBy(policy?.ignores ?? []);
  const kept = (file: FileHash): boolean => sourcePath(file.path) && (file.path === IgnoreFile || !ignored(file.path));
  const old = new Map(before.filter(kept).map((file) => [file.path, file.sha256])), next = new Map(after.filter(kept).map((file) => [file.path, file.sha256]));
  return [...new Set([...old.keys(), ...next.keys()])].sort().flatMap((path): ChangedFile[] => {
    if (old.get(path) === next.get(path)) return [];
    const openForge = forgeScopePath(path, policy);
    return [{ path, change: !old.has(path) ? "added" : !next.has(path) ? "deleted" : "changed", allowed: openForge || patterns.some((pattern) => matchesGlob(path, pattern)), ...(openForge ? { openForge } : {}) }];
  });
}

export async function projectHashes(project: string, policy?: ScopePolicy): Promise<FileHash[]> {
  const ignored = ignoredBy(policy?.ignores ?? []);
  return (await inventory(project, NeverSource, TransientRootFolders, (path, directory) => path !== IgnoreFile && ignored(path, directory))).map((file) => ({ path: file.path, sha256: sha256(file.bytes) }));
}

export function scopeOutcome(changes: readonly ChangedFile[]): CheckOutcome {
  return { id: BrownfieldGuard.scope, area: "scope", status: changes.some((file) => !file.allowed) ? CheckStatus.fail : CheckStatus.pass,
    evidence: changes.length ? changes.map((file) => `${file.change} ${file.path}: ${file.openForge ? "Open Forge work record" : file.allowed ? "within diffScope" : "outside diffScope"}`).join("\n") : "No source paths changed." };
}
