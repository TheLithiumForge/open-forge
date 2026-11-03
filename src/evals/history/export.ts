import { mkdir, rename, rm, writeFile } from "node:fs/promises";
import { existsSync } from "node:fs";
import { dirname, join, resolve, sep } from "node:path";
import { HistoryFiles } from "./artifacts.ts";
import { AnalysisFile } from "./analysis/classes.ts";
import { storedRun } from "./assessments.ts";
import { jsonLines, readJson, safeId } from "./json-files.ts";
import { legacyAssessments, legacyPicks } from "./legacy.ts";
import type { AssessmentRecord, ExperimentRecord, RunRecord, RunView } from "./records.types.ts";
import type { LegacyRunRecord, Preference } from "./results.types.ts";
import { sanitizeRecords, type SanitizingOptions } from "./sanitize.ts";
import { AssessmentKind, BatchStatus, RunStatus } from "./statuses.ts";
import type { HistoryStore } from "./store.ts";
import { ResultFiles } from "../paths.ts";

export const ExportFolders = { versions: "versions", batches: "batches" } as const;
const finishedStatuses: readonly string[] = [RunStatus.done, RunStatus.failed, RunStatus.limited];

export async function exportBatch(store: HistoryStore, batchId: string, destination: string, options: SanitizingOptions = {}): Promise<string> {
  const batch = await store.readBatch(batchId);
  if (batch.cliVersion === null) throw new Error("The batch needs a known CLI version before export.");
  const views = await store.readRuns(batch);
  if (![BatchStatus.done, BatchStatus.failed].some((status) => status === batch.status)
    || views.length === 0 || views.some((run) => !finishedStatuses.includes(run.status) || run.finishedAt === null)) throw new Error("Finish every attempt before exporting the batch.");
  const target = resolve(destination, ExportFolders.versions, safeId(batch.cliVersion), ExportFolders.batches, safeId(batch.id));
  const source = resolve(store.batchFolder(batch.id));
  if (target === source || target.startsWith(source + sep)) throw new Error("Export outside the source batch.");
  if (existsSync(target)) throw new Error("An exported batch is immutable. Export a linked correction instead.");
  const files = await batchFiles(store, batch, views);
  // Scrub the whole pack together so a name found in one record is removed from every record.
  const userNames = [process.env["USERNAME"], process.env["USER"]].filter((name): name is string => name !== undefined);
  const sanitized = sanitizeRecords(files, { userNames: [...userNames, ...options.userNames ?? []] });
  if (sanitized === null || typeof sanitized !== "object" || Array.isArray(sanitized)) throw new Error("Invalid export pack.");
  await mkdir(dirname(target), { recursive: true });
  const staging = `${target}.staging`;
  await mkdir(staging);
  try {
    for (const [name, value] of Object.entries(sanitized)) {
      const path = join(staging, name);
      await mkdir(dirname(path), { recursive: true });
      await writeFile(path, recordText(name, value));
    }
    await rename(staging, target);
  } catch (error) {
    await rm(staging, { recursive: true, force: true });
    throw error;
  }
  return target;
}

/** A record file's text: JSON Lines for a .jsonl name, indented JSON otherwise. */
export function recordText(name: string, value: unknown): string {
  return name.endsWith(".jsonl") && Array.isArray(value) ? jsonLines(value) : `${JSON.stringify(value, null, 2)}\n`;
}

/** A batch's record files in the published layout, keyed by their path inside the batch folder. */
export async function batchFiles(store: HistoryStore, batch: ExperimentRecord, views: readonly RunView[]): Promise<Readonly<Record<string, unknown>>> {
  const runs = await Promise.all(views.map(async (view): Promise<RunRecord> => batch.legacy ? storedRun(view)
    : readJson(join(store.runFolder(batch.id, view.id), ResultFiles.run))));
  const assessments: AssessmentRecord[] = [];
  if (batch.legacy) {
    for (const view of views) assessments.push(...legacyAssessments(await readJson<LegacyRunRecord>(join(store.runFolder(batch.id, view.id), ResultFiles.run))));
    const preferences = join(store.batchFolder(batch.id), ResultFiles.preferences);
    if (existsSync(preferences)) assessments.push(...legacyPicks(batch.id, await readJson<Preference[]>(preferences)));
  }
  assessments.push(...await store.readAssessments(batch.id));
  return {
    [HistoryFiles.experiment]: batch, [HistoryFiles.setups]: await store.readSetups(batch),
    [HistoryFiles.runs]: runs, [HistoryFiles.artifacts]: await store.readArtifacts(batch),
    ...Object.fromEntries(Object.values(AssessmentKind).map((kind) => [`${HistoryFiles.assessments}/${kind}.jsonl`, assessments.filter((record) => record.kind === kind)])),
    [`${HistoryFiles.assessments}/${AnalysisFile}`]: await store.readAnalyses(batch.id),
  };
}
