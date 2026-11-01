import type { AssessmentRecord } from "../../history/records.types.ts";
import type { Preference, RunRecord } from "../../history/results.types.ts";
import { summarize } from "../../history/summary.ts";
import { summarizeMatrix, summaryGradingKey } from "../../history/summary-matrix.ts";
import type { EvalConfig } from "../../config/config.types.ts";
import type { ResultGroup } from "./api.types.ts";

export function gradingKey(run: RunRecord, assessments: readonly AssessmentRecord[]): string {
  return summaryGradingKey(run, assessments);
}
export function resultGroups(runs: readonly RunRecord[], preferences: readonly Preference[], assessments: readonly AssessmentRecord[], config?: EvalConfig): ResultGroup[] {
  if (config !== undefined) return [...summarizeMatrix(config, runs, assessments, preferences).pairs];
  const groups = new Map<string, RunRecord[]>();
  for (const run of runs) {
    const key = JSON.stringify([gradingKey(run, assessments), run.reportedModel]);
    groups.set(key, [...groups.get(key) ?? [], run]);
  }
  return [...groups.values()].map((members) => {
    const first = members[0];
    if (first === undefined) throw new Error("Empty result group.");
    return { grading: gradingKey(first, assessments), reportedModel: first.reportedModel, runs: members, summary: summarize(members, preferences) };
  });
}
