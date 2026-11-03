import { EvalPaths } from "../paths.ts";
import { analyzeBatch } from "./analysis/analyze-batches.ts";
import { batchProgress, progressLines } from "./progress-report.ts";
import { HistoryStore } from "./store.ts";
import { usageLines } from "./usage-report.ts";

/** The reports the CLI prints from saved batches, without running an agent. */

/** The named batches, or every model batch, newest first. */
export async function namedBatches(batches: readonly string[], store: HistoryStore) {
  return batches.length > 0 ? await Promise.all(batches.map((id) => store.readBatch(id)))
    : (await store.listBatches()).filter((batch) => !batch.legacy && batch.reference === undefined);
}

export async function savedUsage(batches: readonly string[], store = new HistoryStore(EvalPaths.runs)): Promise<string[]> {
  const lines: string[] = [];
  for (const batch of await namedBatches(batches, store)) lines.push(...await usageLines(batch, await store.readRuns(batch), store));
  return lines;
}

/** Progress for the named batches, or for every model batch, newest first. */
export async function savedProgress(batches: readonly string[], store = new HistoryStore(EvalPaths.runs)): Promise<string[]> {
  const lines: string[] = [];
  for (const batch of await namedBatches(batches, store)) lines.push(...progressLines(batchProgress(batch, await store.readRuns(batch), await store.readAssessments(batch.id))), "");
  return lines;
}

/** Analyzes the named batches, or every model batch, and says what each analysis could read. */
export async function savedAnalysis(batches: readonly string[], missing: boolean, store = new HistoryStore(EvalPaths.runs)): Promise<string[]> {
  const lines: string[] = [];
  for (const batch of await namedBatches(batches, store)) lines.push(await analyzeBatch(store, batch, missing));
  return lines;
}
