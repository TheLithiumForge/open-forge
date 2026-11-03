import type { Artifact, Batch } from "../records/records.types.ts";
import { loadRecords, NotFound, RawRoot, ResultsRepository, type ReadText } from "./load.ts";
import type { EvalsData, Snapshot } from "./snapshot.types.ts";

export const SourceKind = { published: "published", latest: "latest", local: "local", folder: "folder" } as const;
export type SourceKind = (typeof SourceKind)[keyof typeof SourceKind];

/** Where the shown records came from, and how to open a file of that results tree. */
export interface Source {
  readonly kind: SourceKind;
  readonly label: string;
  /** A link to a file by its path in the results tree, or null when this source can't open files. */
  readonly file: (path: string) => string | null;
  /** A link that browses a folder of the results tree, or null when this source has none. */
  readonly folder: (path: string) => string | null;
}

export interface Loaded { readonly snapshot: Snapshot; readonly source: Source }

/** The `local` query parameter names the port of the data server that `npm run evals -- site` starts. */
export const LocalParameter = "local";
export const ArchiveUrl = `https://github.com/${ResultsRepository}/archive/refs/heads/main.zip`;
const Commit = /^[a-f0-9]{40}$/;

export function batchPath(batch: Batch): string {
  return `versions/${batch.experiment.cliVersion ?? "unknown"}/batches/${batch.experiment.id}`;
}

/** Evidence sits beside a batch's records, at the path its artifact record gives. */
export function evidencePath(batch: Batch, artifact: Artifact): string {
  const owner = artifact.assessmentId === null ? `runs/${artifact.runId}` : `assessments/${artifact.assessmentId}`;
  return `${batchPath(batch)}/evidence/${owner}/${artifact.path}`;
}

/** Records at one commit of the public results repository. A commit that was never published has no files to open. */
export function commitSource(kind: SourceKind, label: string, commit: string): Source {
  const published = Commit.test(commit);
  return { kind, label,
    file: (path) => published ? `${RawRoot}/${commit}/${path}` : null,
    folder: (path) => published ? `https://github.com/${ResultsRepository}/tree/${commit}/${path}` : null };
}

/** The data server's port from a page address, or null when the page wasn't opened by the site command. */
export function localPort(search: string): number | null {
  const value = new URLSearchParams(search).get(LocalParameter);
  const port = value === null ? NaN : Number(value);
  return Number.isInteger(port) && port >= 1024 && port <= 65535 ? port : null;
}

/** Local runs from the data server on this machine, including batches that are still running. */
export async function loadLocal(data: EvalsData, port: number, request: typeof fetch = fetch): Promise<Loaded> {
  const base = `http://localhost:${port}`;
  const read: ReadText = async (path) => {
    const response = await request(`${base}/${path}`, { cache: "no-store", credentials: "omit" });
    if (response.status === NotFound) return null;
    if (!response.ok) throw new Error(`Local results failed (${response.status}) for ${path}`);
    return response.text();
  };
  const records = await loadRecords(read, data.schemas, data.indexSchema, { unfinished: true });
  return {
    snapshot: { ...records, commit: "local", fetchedAt: new Date().toISOString() },
    source: { kind: SourceKind.local, label: `Local runs from port ${port}`, file: (path) => `${base}/${path}`, folder: () => null },
  };
}

/** A picked folder's files by their path inside the results tree, whose root is the folder holding index.json. */
export function folderFiles<T extends { readonly webkitRelativePath: string }>(picked: Iterable<T>): Map<string, T> {
  const files = [...picked];
  const index = files.map((file) => file.webkitRelativePath).filter((path) => path === "index.json" || path.endsWith("/index.json"))
    .sort((a, b) => a.length - b.length)[0];
  if (index === undefined) throw new Error("The folder has no index.json. Pick the results repository folder");
  const root = index.slice(0, -"index.json".length);
  return new Map(files.filter((file) => file.webkitRelativePath.startsWith(root)).map((file) => [file.webkitRelativePath.slice(root.length), file]));
}

/** Results from a folder picked on this computer, such as a downloaded copy of the results repository. */
export async function loadFolder(data: EvalsData, files: ReadonlyMap<string, Blob>, name: string, link: (file: Blob) => string = URL.createObjectURL): Promise<Loaded> {
  const read: ReadText = async (path) => {
    const file = files.get(path);
    return file === undefined ? null : file.text();
  };
  const records = await loadRecords(read, data.schemas, data.indexSchema, { unfinished: true });
  const links = new Map<string, string>();
  return {
    snapshot: { ...records, commit: "folder", fetchedAt: new Date().toISOString() },
    source: { kind: SourceKind.folder, label: `Folder ${name}`, folder: () => null, file: (path) => {
      const file = files.get(path);
      if (file === undefined) return null;
      if (!links.has(path)) links.set(path, link(file));
      return links.get(path) ?? null;
    } },
  };
}

/** One batch's records as a single file, for analysis elsewhere. */
export function batchDownload(batch: Batch): string {
  return `${JSON.stringify(batch, null, 2)}\n`;
}
