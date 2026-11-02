import type { Batch } from "../records/records.types.ts";
import { ballotShare, gradingScope, interval, proportionInterval, tokenTotal, type AssessedRun, type Interval } from "./analysis.ts";
import { observations, choiceLabel } from "./observations.ts";
export interface Summary {
  readonly layer: number | null;
  readonly judge: string;
  readonly grading: string;
  readonly judgeMissing: number;
  readonly judgeErrors: number;
  readonly scope: string;
  readonly version: string;
  readonly batchId: string;
  readonly createdAt: string;
  readonly setup: string;
  readonly model: string;
  readonly tool: string;
  readonly effort: string | null;
  readonly scoring: boolean;
  readonly runs: number;
  readonly coverage: Interval;
  readonly working: Interval;
  readonly stated: Interval;
  readonly rules: Interval;
  readonly code: Interval;
  readonly intent: Interval;
  readonly intentJudge: Interval;
  readonly tokens: Interval;
  readonly tools: Interval;
  readonly failedTools: Interval;
  readonly context: Interval;
  readonly minutes: Interval;
  readonly picks: Interval;
  readonly errors: number;
  readonly unsure: number;
  readonly revealedPicks: Interval;
  readonly unknownPicks: number;
}
function intent(run: AssessedRun): number | null {
  if (run.checksAssessment?.error != null)
    return null;
  const wishes = run.checksAssessment?.outcomes.filter((item) => item.area.toLowerCase() === "intent") ?? [];
  return wishes.length === 0 ? null : wishes.filter((item) => item.status === "pass").length / wishes.length;
}
export function summaries(batch: Batch): Summary[] {
  const assessed = observations(batch);
  const replaced = new Set(assessed.flatMap((run) => run.replacesRunId === null ? [] : [run.replacesRunId]));
  const groups = new Map<string, AssessedRun[]>();
  for (const run of assessed) {
    const key = JSON.stringify([gradingScope(batch, run), run.setup]);
    groups.set(key, [...groups.get(key) ?? [], run]);
  }
  return [...groups.values()].map((runs) => {
    const first = runs[0];
    if (first === undefined)
      throw new Error("Empty comparison group.");
    const scored = runs.filter((run) => !(run.failureKind === "infrastructure" && replaced.has(run.id)));
    const points = (revealed: boolean) => batch.assessments.flatMap((record) => {
      if (record.kind !== "pick" || record.revealed !== revealed)
        return [];
      return [record.left, record.right].flatMap((side) => runs.some((run) => run.id === side.runId)
        ? [record.winner === "tie" ? 0.5 : record.winner === side.side ? 1 : 0] : []);
    });
    const descriptive = (values: readonly number[]) => ({ ...interval(values), low: null, high: null });
    return {
      layer: batch.experiment.config.layer ?? null, judge: choiceLabel(first.judgeChoice), grading: JSON.stringify([first.checksAssessment?.versions ?? null, first.judgeAssessment?.versions ?? null]), judgeMissing: scored.filter((run) => run.judgeAssessment === null).length, judgeErrors: scored.filter((run) => run.judgeAssessment?.error != null).length, scope: gradingScope(batch, first), version: batch.experiment.cliVersion ?? "unknown", batchId: batch.experiment.id, createdAt: batch.experiment.createdAt,
      setup: first.setup, model: first.reportedModel ?? first.requestedModel ?? "unreported model", tool: first.agent.tool, effort: first.agent.effort,
      scoring: batch.experiment.config.isolation !== "local", runs: runs.length,
      coverage: interval(scored.map((run) => run.result?.coverage.share ?? null)), working: proportionInterval(scored.map((run) => run.result?.workingGate ?? null)), stated: proportionInterval(scored.map((run) => run.result?.statedSuccess ?? null)),
      rules: interval(scored.map((run) => ballotShare(run, "rules"))), code: interval(scored.map((run) => ballotShare(run, "code"))),
      intent: interval(scored.map(intent)), intentJudge: interval(scored.map((run) => ballotShare(run, "intent"))),
      tokens: interval(runs.map(tokenTotal)), tools: interval(runs.map((run) => run.telemetry?.toolCalls ?? null)), failedTools: interval(runs.map((run) => run.telemetry?.failedToolCalls ?? null)),
      context: interval(runs.map((run) => run.telemetry?.peakContext ?? null)), minutes: interval(runs.map((run) => run.agentSeconds === null ? null : run.agentSeconds / 60)),
      picks: descriptive(points(false)), revealedPicks: descriptive(points(true)), errors: scored.reduce((sum, run) => sum + (run.result?.coverage.errors ?? 0), 0),
      unknownPicks: batch.assessments.filter((record) => record.kind === "pick" && record.revealed === null && runs.some((run) => run.id === record.left.runId || run.id === record.right.runId)).length,
      unsure: scored.reduce((sum, run) => sum + (run.judgeAssessment?.answers.reduce((count, answer) => count + answer.unsure, 0) ?? 0), 0),
    };
  });
}
export function historyStatus(previous: Summary | undefined, current: Summary): string {
  if (previous === undefined || previous.scope !== current.scope || previous.setup !== current.setup || current.coverage.low === null || previous.coverage.high === null)
    return "Inconclusive";
  if ((current.coverage.high ?? 1) < (previous.coverage.low ?? 0))
    return "Regression in screening interval";
  return "Inconclusive";
}
