import { existsSync } from "node:fs";
import { readdir, readFile } from "node:fs/promises";
import { join } from "node:path";
import { ResultFiles } from "../paths.ts";
import type { ArtifactRecord } from "./records.types.ts";
import { sha256, fingerprint } from "./fingerprints.ts";
import { RecordSchemaVersion } from "./statuses.ts";

export const HistoryFiles = {
  experiment: "experiment.json", setups: "setups.jsonl", runs: "runs.jsonl",
  assessments: "assessments", artifacts: "artifacts.json", overlay: ".assessments", reveal: "reveal.json", evidence: "evidence",
} as const;

const excluded = new Set(["node_modules", ".git", ".auth", ".codex", ".claude", "auth.json", "credentials.json", ".credentials.json"]);
// A task project's .gitignore names these for its downloads. An agent that follows its README installs a browser there.
const ToolFolders = new Set([".npm-cache", ".playwright-browsers", ".browser-work", ".test-work"]);
// GitHub warns above 50 MB and refuses 100 MB, and an agent can name its download folder anything.
export const PublishedFileBytes = 50 * 1024 * 1024;

export interface FileDigest {
  readonly path: string;
  readonly bytes: number;
  readonly sha256: string;
}

/** Whether a file goes into published evidence. Every file stays in the run store either way. */
export function publishedFile(file: Pick<FileDigest, "path" | "bytes">): boolean {
  return file.bytes <= PublishedFileBytes && !file.path.split("/").some((part) => ToolFolders.has(part));
}

export async function fileDigests(root: string, omit: ReadonlySet<string> = excluded, prefix = ""): Promise<FileDigest[]> {
  if (!existsSync(root)) return [];
  const files: FileDigest[] = [];
  for (const entry of (await readdir(root, { withFileTypes: true })).sort((a, b) => a.name.localeCompare(b.name))) {
    if (omit.has(entry.name) || excluded.has(entry.name) || entry.name.startsWith(".env") || entry.isSymbolicLink()) continue;
    const path = `${prefix}${entry.name}`;
    if (entry.isDirectory()) files.push(...await fileDigests(join(root, entry.name), omit, `${path}/`));
    if (!entry.isFile()) continue;
    const content = await readFile(join(root, entry.name));
    files.push({ path, bytes: content.length, sha256: sha256(content) });
  }
  return files;
}

export async function artifactManifest(folder: string, runId: string, assessmentId: string | null = null): Promise<ArtifactRecord[]> {
  const omit = new Set([ResultFiles.run, HistoryFiles.artifacts, HistoryFiles.assessments]);
  return (await fileDigests(folder, omit)).map((file) => ({ schemaVersion: RecordSchemaVersion, runId, assessmentId, ...file }));
}

export function artifactHash(files: readonly FileDigest[]): string {
  return fingerprint(files.map(({ path, bytes, sha256: hash }) => ({ path, bytes, sha256: hash })).sort((a, b) => a.path.localeCompare(b.path)));
}
