import { listBatches, readAssessments, readBatch, readPreferences, readRuns, type HistoryStore } from "../history/store.ts";
import { summarizeMatrix, type MatrixSummary, type PairSummary } from "../history/summary-matrix.ts";
import { referenceHistory, referenceTrends, type ReferenceTrend } from "../history/reference-trend.ts";

export interface BatchMatrixSummary extends Omit<MatrixSummary, "pairs"> { readonly pairs: readonly Omit<PairSummary, "runs">[]; readonly referenceHistory?: readonly ReferenceTrend[] }

export async function batchMatrixSummary(batchId?: string, store: Pick<HistoryStore, "listBatches" | "readBatch" | "readRuns" | "readAssessments" | "readPreferences"> = { listBatches, readBatch, readRuns, readAssessments, readPreferences }): Promise<BatchMatrixSummary> {
  if (batchId === undefined) return { layer: null, implementationAttempts: 0, pairs: [], differences: [], referenceHistory: await referenceHistory(store) };
  const batch = await store.readBatch(batchId);
  const [runs, assessments, preferences] = await Promise.all([store.readRuns(batch), store.readAssessments(batchId), store.readPreferences(batchId)]);
  const summary = summarizeMatrix(batch.config, runs, assessments, preferences);
  return { ...summary, pairs: summary.pairs.map(({ runs: _runs, ...pair }) => pair),
    ...(batch.reference === undefined ? {} : { referenceHistory: referenceTrends([{ batch, runs, assessments }]) }) };
}
