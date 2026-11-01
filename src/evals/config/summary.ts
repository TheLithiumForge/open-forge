import { readAssessments, readBatch, readPreferences, readRuns, type HistoryStore } from "../history/store.ts";
import { summarizeMatrix, type MatrixSummary, type PairSummary } from "../history/summary-matrix.ts";

export interface BatchMatrixSummary extends Omit<MatrixSummary, "pairs"> { readonly pairs: readonly Omit<PairSummary, "runs">[] }

export async function batchMatrixSummary(batchId: string, store: Pick<HistoryStore, "readBatch" | "readRuns" | "readAssessments" | "readPreferences"> = { readBatch, readRuns, readAssessments, readPreferences }): Promise<BatchMatrixSummary> {
  const batch = await store.readBatch(batchId);
  const [runs, assessments, preferences] = await Promise.all([store.readRuns(batch), store.readAssessments(batchId), store.readPreferences(batchId)]);
  const summary = summarizeMatrix(batch.config, runs, assessments, preferences);
  return { ...summary, pairs: summary.pairs.map(({ runs: _runs, ...pair }) => pair) };
}
