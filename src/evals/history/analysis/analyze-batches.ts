import { ReferenceImplementer } from "../reference.ts";
import type { ExperimentRecord } from "../records.types.ts";
import { FailureKind, RunStatus } from "../statuses.ts";
import type { HistoryStore } from "../store.ts";
import type { AnalysisRecord } from "./analysis.types.ts";
import { AnalysisMethod, AnalysisVersion } from "./classes.ts";
import { analyzeSavedRun, latestAnalyses } from "./saved-run.ts";

/** A run needs an analysis when it has none, when its latest one failed or when the analyzer changed since. */
export function needsAnalysis(record: AnalysisRecord | undefined): boolean {
  return record === undefined || record.error !== null || record.version !== AnalysisVersion;
}

const MethodNames = {
  [AnalysisMethod.rollout]: "from Codex's session log",
  [AnalysisMethod.stream]: "from Claude's stream",
  [AnalysisMethod.events]: "estimated from events alone",
  [AnalysisMethod.none]: "without token data",
} as const satisfies Record<AnalysisMethod, string>;

/**
 * Analyzes the saved runs whose agent finished and appends a record for each. With missing, only runs that need one.
 * Runs that wait to be redone, and failures a replacement redid, stay out as they do in every comparison.
 */
export async function analyzeBatch(store: HistoryStore, batch: ExperimentRecord, missing: boolean): Promise<string> {
  const all = await store.readRuns(batch);
  const replaced = new Set(all.flatMap((run) => (run.replacesRunId === null ? [] : [run.replacesRunId])));
  const runs = all.filter((run) => run.agentSeconds !== null && run.implementer !== ReferenceImplementer && run.status !== RunStatus.limited
    && !(run.failureKind === FailureKind.infrastructure && replaced.has(run.id)));
  const setups = await store.readSetups(batch);
  const latest = latestAnalyses(await store.readAnalyses(batch.id));
  const due = runs.filter((run) => !missing || needsAnalysis(latest.get(run.id)));
  const methods = new Map<AnalysisMethod, number>();
  let errors = 0;
  for (const run of due) {
    const record = await analyzeSavedRun(store, batch, run, setups, latest.get(run.id)?.id ?? null);
    await store.appendAnalysis(record);
    if (record.error !== null) errors += 1;
    else methods.set(record.method, (methods.get(record.method) ?? 0) + 1);
  }
  const counted = [...methods].map(([method, count]) => `${count} ${MethodNames[method]}`);
  return [`Batch ${batch.id}: analyzed ${due.length} of ${runs.length} finished runs.`, ...(counted.length === 0 ? [] : [`${counted.join(", ")}.`]),
    ...(errors === 0 ? [] : [`${errors} failed and keep their error for analyze --missing.`])].join(" ");
}
