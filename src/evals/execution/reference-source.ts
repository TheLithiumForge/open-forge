import { readFile } from "node:fs/promises";
import { join } from "node:path";
import { ordinaryPath, safeRelative, writeNew } from "../materialize/files.ts";
import { fingerprint, sha256 } from "../history/fingerprints.ts";
import type { ReferenceVersion } from "../history/reference.ts";
import { runCommand } from "./process.ts";
import { SpecVersion } from "../benchmarks/neon-pairs/checks/spec-layers/requirements.ts";

export const ReferenceSourceCommands = {
  tracked: ["ls-files", "--cached", "-z", "--", "."],
  ignored: ["ls-files", "--cached", "--ignored", "--exclude-standard", "-z", "--", "."],
  commit: ["log", "-1", "--format=%H", "--", "."],
} as const;
export const ReferenceExcluded = new Set([".git", "node_modules", "dist", "build", "coverage", "test-results", "playwright-report", ".cache", ".vite"]);

export type SourceCommand = (args: readonly string[], cwd: string) => Promise<string>;
const sourceCommand: SourceCommand = async (args, cwd) => {
  const result = await runCommand("git", args, { cwd, timeoutMs: 30_000, maxBytes: 8 * 1024 * 1024 });
  if (result.code !== 0 || result.timedOut || result.truncated) throw new Error("Cannot read the reference's tracked source.");
  return result.stdout;
};

/** Freeze the tracked working files so later edits cannot change an in-progress grading. */
export async function copyReference(source: string, target: string, specHash: string, command: SourceCommand = sourceCommand): Promise<ReferenceVersion> {
  const commit = (await command(ReferenceSourceCommands.commit, source)).trim();
  if (!/^[a-f\d]{40,64}$/.test(commit)) throw new Error("The reference needs a recorded commit.");
  const ignored = new Set((await command(ReferenceSourceCommands.ignored, source)).split("\0"));
  const names = [...new Set((await command(ReferenceSourceCommands.tracked, source)).split("\0").filter(Boolean))].sort();
  const files = [];
  for (const name of names) {
    const path = safeRelative(name);
    if (ignored.has(path) || path.split("/").some((part) => ReferenceExcluded.has(part))) continue;
    await ordinaryPath(join(source, path));
    const bytes = await readFile(join(source, path));
    await writeNew(target, path, bytes);
    files.push({ path, bytes: bytes.length, sha256: sha256(bytes) });
  }
  if (!files.length) throw new Error("The reference has no tracked project files.");
  return { commit, contentHash: fingerprint(files), specHash, specVersion: SpecVersion, files };
}
