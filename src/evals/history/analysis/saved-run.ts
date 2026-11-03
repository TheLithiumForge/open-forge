import { randomUUID } from "node:crypto";
import { existsSync } from "node:fs";
import { readFile } from "node:fs/promises";
import { join } from "node:path";
import { CodexRolloutFile } from "../../execution/providers/codex-telemetry.ts";
import { ResultFiles } from "../../paths.ts";
import { HistoryFiles } from "../artifacts.ts";
import { readJson } from "../json-files.ts";
import type { ArtifactRecord, ExperimentRecord, SetupRecord } from "../records.types.ts";
import type { RunRecord } from "../results.types.ts";
import { RecordSchemaVersion } from "../statuses.ts";
import type { HistoryStore } from "../store.ts";
import type { AnalysisRecord, RunAnalysis } from "./analysis.types.ts";
import { analyzeRun } from "./analyze.ts";
import { AnalysisKind, AnalysisMethod, AnalysisVersion } from "./classes.ts";

export type AnalysisStore = Pick<HistoryStore, "runFolder" | "readSetups" | "appendAnalysis" | "readAnalyses">;

/** The saved files of a run start in a files folder, which artifacts.json lists with their sizes. */
const SavedFilesPrefix = `${ResultFiles.files}/`;

const Unanalyzed: RunAnalysis = { method: AnalysisMethod.none, sources: [], tokens: null, totalsAgree: null, requests: null, peakContext: null, compactions: null, beforeEdit: null, parts: [], process: null, turns: [], threads: [] };

export function analysisRecord(experimentId: string, runId: string, analysis: RunAnalysis | null, error: string | null, replacesAnalysisId: string | null): AnalysisRecord {
  return { schemaVersion: RecordSchemaVersion, kind: AnalysisKind, id: randomUUID(), experimentId, runId, createdAt: new Date().toISOString(), version: AnalysisVersion,
    replacesAnalysisId, error, ...(analysis ?? Unanalyzed) };
}

/** The newest analysis of each run. Records are append only, so the last one wins. */
export function latestAnalyses(records: readonly AnalysisRecord[]): Map<string, AnalysisRecord> {
  return new Map(records.map((record) => [record.runId, record]));
}

/** The project's files with their sizes: the setup's start files, then the files the run saved, for files created on the way. */
async function knownFiles(folder: string, setup: SetupRecord | undefined): Promise<Map<string, number>> {
  const files = new Map((setup?.files ?? []).map((file) => [file.path, file.bytes]));
  const manifest = join(folder, HistoryFiles.artifacts);
  if (!existsSync(manifest)) return files;
  for (const artifact of await readJson<ArtifactRecord[]>(manifest)) {
    const path = artifact.path.replaceAll("\\", "/");
    if (path.startsWith(SavedFilesPrefix) && !files.has(path.slice(SavedFilesPrefix.length))) files.set(path.slice(SavedFilesPrefix.length), artifact.bytes);
  }
  return files;
}

const optionalText = async (path: string) => (existsSync(path) ? readFile(path, "utf8") : null);

/** Analyzes one saved run from its folder. It never throws for the run's content: a failure becomes the record's error. */
export async function analyzeSavedRun(store: Pick<HistoryStore, "runFolder">, batch: ExperimentRecord, run: RunRecord, setups: readonly SetupRecord[], replaces: string | null): Promise<AnalysisRecord> {
  try {
    const folder = store.runFolder(batch.id, run.id);
    const analysis = analyzeRun({
      tool: run.agent.tool, telemetry: run.telemetry, answers: run.questions.map((round) => round.answer),
      events: await optionalText(join(folder, ResultFiles.events)), rollout: await optionalText(join(folder, CodexRolloutFile)),
      files: await knownFiles(folder, setups.find((setup) => setup.id === run.setup)),
    });
    return analysisRecord(batch.id, run.id, analysis, null, replaces);
  } catch (error) {
    return analysisRecord(batch.id, run.id, null, error instanceof Error ? error.message : String(error), replaces);
  }
}

/** Analyzes a run that just finished and saves the record, so a batch's objective measures fill in as its runs end. */
export async function analyzeFinishedRun(store: AnalysisStore & Pick<HistoryStore, "readBatch">, run: RunRecord): Promise<void> {
  const batch = await store.readBatch(run.batchId);
  const replaces = latestAnalyses(await store.readAnalyses(batch.id)).get(run.id)?.id ?? null;
  await store.appendAnalysis(await analyzeSavedRun(store, batch, run, await store.readSetups(batch), replaces));
}
