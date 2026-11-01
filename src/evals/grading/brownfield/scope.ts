import { matchesGlob } from "node:path";
import { inventory, sha256, type FileHash } from "../../materialize/files.ts";
import { ReferenceExcluded } from "../../materialize/brownfield-inputs.ts";
import type { CheckOutcome } from "../grading.types.ts";
import { CheckStatus } from "../../history/statuses.ts";

export const BrownfieldGuard = { scope: "brownfield-diff-scope", protected: "brownfield-protected-inputs" } as const;
export const TransientFiles = new Set([...ReferenceExcluded, ".open-forge-state"]);
export interface ChangedFile { readonly path: string; readonly change: "added" | "changed" | "deleted"; readonly allowed: boolean }

export function changedFiles(before: readonly FileHash[], after: readonly FileHash[], patterns: readonly string[]): ChangedFile[] {
  const kept = (file: FileHash): boolean => !file.path.split("/").some((part) => TransientFiles.has(part));
  const old = new Map(before.filter(kept).map((file) => [file.path, file.sha256])), next = new Map(after.filter(kept).map((file) => [file.path, file.sha256]));
  return [...new Set([...old.keys(), ...next.keys()])].sort().flatMap((path): ChangedFile[] => {
    if (old.get(path) === next.get(path)) return [];
    return [{ path, change: !old.has(path) ? "added" : !next.has(path) ? "deleted" : "changed", allowed: patterns.some((pattern) => matchesGlob(path, pattern)) }];
  });
}

export async function projectHashes(project: string): Promise<FileHash[]> {
  return (await inventory(project, TransientFiles)).map((file) => ({ path: file.path, sha256: sha256(file.bytes) }));
}

export function scopeOutcome(changes: readonly ChangedFile[]): CheckOutcome {
  return { id: BrownfieldGuard.scope, area: "scope", status: changes.some((file) => !file.allowed) ? CheckStatus.fail : CheckStatus.pass,
    evidence: changes.length ? changes.map((file) => `${file.change} ${file.path}: ${file.allowed ? "within diffScope" : "outside diffScope"}`).join("\n") : "No source paths changed." };
}
