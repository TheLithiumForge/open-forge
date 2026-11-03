import { existsSync } from "node:fs";
import { readFile, stat } from "node:fs/promises";
import { extname, join, relative, resolve, sep } from "node:path";
import { catalogue } from "../../docusaurus/src/components/evals/records/catalogue.ts";
import type { Batch } from "../../docusaurus/src/components/evals/records/records.types.ts";
import { batchFiles, ExportFolders, recordText } from "../history/export.ts";
import type { HistoryStore } from "../history/store.ts";
import { HistoryFiles } from "../history/artifacts.ts";
import { AnalysisFile } from "../history/analysis/classes.ts";

/** One file the Results page asked for, or null when there is none. */
export interface ResultsFile { readonly bytes: Buffer; readonly type: string }
/** Reads a tree in the results repository's layout, which is what the docs Results page loads. */
export type ResultsReader = (path: string) => Promise<ResultsFile | null>;

export const IndexFile = "index.json";
const AnalysisRecords = `${HistoryFiles.assessments}/${AnalysisFile}`;
const EvidenceFolder = "evidence";
// Agent code and games are shown as text, so nothing an agent wrote runs on the data server's origin.
const Types: Readonly<Record<string, string>> = { ".json": "application/json", ".png": "image/png", ".jpg": "image/jpeg", ".jpeg": "image/jpeg", ".webp": "image/webp", ".svg": "text/plain" };

function contentType(path: string): string {
  return Types[extname(path).toLowerCase()] ?? "text/plain; charset=utf-8";
}

/** A file under root, or null when the path leaves root or names no file. */
async function fileWithin(root: string, path: string): Promise<ResultsFile | null> {
  const base = resolve(root), target = resolve(base, path);
  const inside = relative(base, target);
  if (inside === "" || inside.startsWith(`..${sep}`) || inside === ".." || !existsSync(target) || !(await stat(target)).isFile()) return null;
  return { bytes: await readFile(target), type: contentType(target) };
}

/** A folder already in the published layout, such as the results repository or a downloaded copy of it. */
export function folderReader(root: string): ResultsReader {
  return (path) => fileWithin(root, path);
}

/** How long one read of a batch's records serves the requests of a page load before running batches are read again. */
export const LocalReadMs = 5_000;

/** Local runs in the published layout, read again after a few seconds so running batches show their latest state. */
export function localRunsReader(store: HistoryStore, now: () => number = Date.now): ResultsReader {
  const batchesWithVersion = async () => (await store.listBatches()).filter((batch) => !batch.legacy && batch.cliVersion !== null);
  // A page load asks for every file of every batch at once, so each batch's records are built once and shared.
  const recent = new Map<string, { readonly at: number; readonly files: ReturnType<typeof batchFiles> }>();
  const files = (id: string) => {
    const cached = recent.get(id);
    if (cached !== undefined && now() - cached.at < LocalReadMs) return cached.files;
    const read = store.readBatch(id).then(async (batch) => batchFiles(store, batch, await store.readRuns(batch)));
    recent.set(id, { at: now(), files: read });
    read.catch(() => recent.delete(id));
    return read;
  };
  return async (path) => {
    if (path === IndexFile) {
      const batches = await Promise.all((await batchesWithVersion()).map(async (batch) => {
        const records = await files(batch.id);
        // Records here are the ones results add publishes, so they already have the published shape.
        return { experiment: records["experiment.json"], setups: records["setups.jsonl"], runs: records["runs.jsonl"],
          assessments: [...Object.entries(records)].filter(([name]) => name.startsWith("assessments/") && name !== AnalysisRecords).flatMap(([, value]) => value as unknown[]),
          artifacts: records["artifacts.json"], analyses: records[AnalysisRecords] } as unknown as Batch;
      }));
      return { bytes: Buffer.from(`${JSON.stringify(catalogue(batches), null, 2)}\n`), type: contentType(path) };
    }
    const [versions, version, batches, id, ...rest] = path.split("/");
    if (versions !== ExportFolders.versions || batches !== ExportFolders.batches || id === undefined || version === undefined || rest.length === 0) return null;
    const batch = (await batchesWithVersion()).find((item) => item.id === id && item.cliVersion === version);
    if (batch === undefined) return null;
    const name = rest.join("/");
    if (rest[0] !== EvidenceFolder) {
      const records = await files(batch.id);
      return name in records ? { bytes: Buffer.from(recordText(name, records[name])), type: contentType(name) } : null;
    }
    // Evidence paths follow artifacts.json: runs/<run>/<path> in the run folder, assessments/<id>/<path> in the grading overlay.
    const [, kind, owner, ...inner] = rest;
    if (owner === undefined || inner.length === 0) return null;
    const folder = kind === "runs" ? store.runFolder(batch.id, owner) : kind === "assessments" ? store.assessmentEvidenceFolder(batch.id, owner) : null;
    return folder === null ? null : fileWithin(folder, join(...inner));
  };
}
