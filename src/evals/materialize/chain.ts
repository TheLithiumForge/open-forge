import { mkdir, mkdtemp } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { GitDirectory, hashes, inventory, writeNew, type FileBytes, type FileHash } from "./files.ts";
import { ignoredBy } from "./ignore.ts";
import { applyPatch } from "./patch.ts";
import type { IgnoreDocument } from "./scope.types.ts";

/**
 * A chained milestone starts from its parent run's saved files with the next registered defect planted, so earlier
 * changes and Open Forge records carry over. Build output and other files the starting project's .gitignore ignores stay
 * behind, apart from the context files a setup installs, which carry over even when an ignore rule names them.
 */
export const ChainCarried = [".agents", "docs", "AGENTS.md", "CLAUDE.md", ".open-forge-state"] as const;
const NeverCopied = new Set([GitDirectory, "node_modules"]);

/** The next defect no longer applies, because the parent changed the lines its patch expects. */
export class SeedConflict extends Error {
  constructor(cause: unknown) {
    super(`The next defect doesn't apply to the parent's files: ${cause instanceof Error ? cause.message : String(cause)}`);
    this.name = "SeedConflict";
  }
}

function carried(path: string): boolean {
  return ChainCarried.some((root) => path === root || path.startsWith(`${root}/`));
}

/** Which parent files a chained start keeps. */
export function chainIgnored(ignores: readonly IgnoreDocument[]): (path: string, directory: boolean) => boolean {
  const ignored = ignoredBy(ignores);
  return (path, directory) => !carried(path) && ignored(path, directory);
}

/** The chained start's files: the kept parent files with the defect applied. */
export function chainedFiles(parent: readonly FileBytes[], seedPatch: string): FileBytes[] {
  try { return applyPatch(parent, seedPatch); }
  catch (error) { throw new SeedConflict(error); }
}

export interface ChainedStart { readonly folder: string; readonly files: readonly FileHash[] }

/** Writes a chained start into a new temporary folder named after the project, as a setup snapshot is. */
export async function writeChainedStart(parentFiles: string, ignores: readonly IgnoreDocument[], seedPatch: string, project: string): Promise<ChainedStart> {
  const files = chainedFiles(await inventory(parentFiles, NeverCopied, new Set(), chainIgnored(ignores)), seedPatch);
  const folder = join(await mkdtemp(join(tmpdir(), "eval-chain-")), project);
  await mkdir(folder);
  for (const file of files) await writeNew(folder, file.path, file.bytes);
  return { folder, files: hashes(files) };
}
