import type { ModelChoice } from "../config/config.types.ts";
import { coverage, summarizeMeasurements, tokenTotal } from "../grading/counting.ts";
import type { JudgeAnswer, Preference, RunRecord } from "./results.types.ts";
import type { AssessmentRecord, JudgeAssessment } from "./records.types.ts";
import { modelKey } from "../config/judges.ts";
import { AssessmentKind, FailureKind, IntentCheckArea, PreferenceWinner, RunStatus } from "./statuses.ts";
import { ReferenceImplementer } from "./reference.ts";

export { IntentCheckArea } from "./statuses.ts";
export const JudgeAreas = { rules: "rules", code: "code", intent: "intent" } as const;

export function latestJudgeFor(runId: string, assessments: readonly AssessmentRecord[], choice: ModelChoice, legacyChoice?: ModelChoice): JudgeAssessment | undefined {
  return assessments.filter((item): item is JudgeAssessment => item.kind === AssessmentKind.judge && item.runId === runId
    && (item.requestedModel !== null ? modelKey(item.requestedModel) === modelKey(choice) : legacyChoice !== undefined && modelKey(legacyChoice) === modelKey(choice))).at(-1);
}

export interface SetupSummary {
  readonly agent: string;
  readonly setup: string;
  readonly runs: number;
  readonly finished: number;
  readonly attempts: number;
  readonly success: number | null;
  readonly statedSuccess: number | null;
  readonly workingGate: number | null;
  readonly coverage: number | null;
  readonly unresolvedErrors: number;
  readonly intent: number | null;
  readonly rules: number | null;
  readonly code: number | null;
  readonly preference: number | null;
  readonly tokens: number | null;
  readonly tokenSamples: number;
  readonly tokenMissing: number;
  readonly toolCalls: number | null;
  readonly failedToolCalls: number | null;
  readonly peakContext: number | null;
  readonly compactions: number | null;
  readonly openForgeReads: number | null;
  readonly questions: number | null;
  readonly seconds: number | null;
  readonly costUsd: number | null;
  readonly bytes: number | null;
  readonly tapP95Ms: number | null;
}

export function agentLabel(agent: ModelChoice): string {
  return [agent.tool, agent.model ?? "default model", agent.effort ?? "default effort"].join(" · ");
}

function mean(values: readonly (number | null | undefined)[]): number | null {
  return summarizeMeasurements(values.map((value) => value ?? null)).mean;
}

export function yesShare(answers: readonly JudgeAnswer[], area: string): number | null {
  const scoped = answers.filter((answer) => answer.area === area);
  const asked = scoped.reduce((sum, answer) => sum + answer.yes + answer.no, 0);
  return asked === 0 ? null : scoped.reduce((sum, answer) => sum + answer.yes, 0) / asked;
}

function preferenceShare(runIds: ReadonlySet<string>, preferences: readonly Preference[]): number | null {
  let points = 0;
  let games = 0;
  for (const preference of preferences) {
    for (const [side, winner] of [[preference.left, PreferenceWinner.left], [preference.right, PreferenceWinner.right]] as const) {
      if (!runIds.has(side)) continue;
      games += 1;
      points += preference.winner === winner ? 1 : preference.winner === PreferenceWinner.tie ? 0.5 : 0;
    }
  }
  return games === 0 ? null : points / games;
}

export function summarize(runs: readonly RunRecord[], preferences: readonly Preference[]): SetupSummary[] {
  const groups = new Map<string, RunRecord[]>();
  for (const run of runs) {
    if (run.implementer === ReferenceImplementer) continue;
    const key = `${agentLabel(run.agent)}\n${run.setup}`;
    groups.set(key, [...groups.get(key) ?? [], run]);
  }
  const replaced = new Set(runs.flatMap((run) => run.replacesRunId === null ? [] : [run.replacesRunId]));
  return [...groups.entries()].map(([key, group]) => {
    const [agent = "", setup = ""] = key.split("\n");
    const attempted = group.filter((run) => run.startedAt !== null || run.status === RunStatus.done || run.status === RunStatus.failed);
    const attempts = attempted.filter((run) => !(run.failureKind === FailureKind.infrastructure && replaced.has(run.id)));
    const tokens = summarizeMeasurements(attempted.map((run) => tokenTotal(run.telemetry?.tokens)));
    const counts = attempts.map((run) => run.functionalResult?.coverage ?? coverage([], run.checkSelection.applicable));
    const expected = counts.reduce((sum, result) => sum + result.applicable, 0);
    const statedSuccess = mean(attempts.map((run) => run.functionalResult?.statedSuccess == null ? null : Number(run.functionalResult.statedSuccess)));
    return {
      agent, setup, runs: group.length, finished: group.filter((run) => run.status === RunStatus.done).length, attempts: attempts.length,
      success: statedSuccess, statedSuccess,
      workingGate: mean(attempts.map((run) => run.functionalResult?.workingGate == null ? null : Number(run.functionalResult.workingGate))),
      coverage: expected === 0 ? null : counts.reduce((sum, result) => sum + result.passed, 0) / expected,
      unresolvedErrors: counts.reduce((sum, result) => sum + result.errors, 0),
      intent: mean(attempts.map((run) => mean([coverage(run.checkOutcomes.filter((check) => check.area === IntentCheckArea)).share, yesShare(run.judge, JudgeAreas.intent)]))),
      rules: mean(attempts.map((run) => yesShare(run.judge, JudgeAreas.rules))),
      code: mean(attempts.map((run) => yesShare(run.judge, JudgeAreas.code))),
      preference: preferenceShare(new Set(group.map((run) => run.id)), preferences),
      tokens: tokens.mean, tokenSamples: tokens.samples, tokenMissing: tokens.missing,
      toolCalls: mean(attempted.map((run) => run.telemetry?.toolCalls)),
      failedToolCalls: mean(attempted.map((run) => run.telemetry?.failedToolCalls)),
      peakContext: mean(attempts.map((run) => run.telemetry?.peakContext)),
      compactions: mean(attempts.map((run) => run.telemetry?.compactions)),
      openForgeReads: mean(attempts.map((run) => run.telemetry?.openForgeReads)),
      questions: mean(attempts.map((run) => run.questions.length)), seconds: mean(attempts.map((run) => run.agentSeconds)),
      costUsd: mean(attempts.map((run) => run.telemetry?.reportedCostUsd)), bytes: mean(attempts.map((run) => run.build?.bytes)),
      tapP95Ms: mean(attempts.map((run) => run.performance?.largeBoardTapP95Ms)),
    };
  });
}
