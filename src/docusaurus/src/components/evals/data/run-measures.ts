import type { Assessment, Batch, Choice, Judge, Run } from "../records/records.types.ts";
import { latestRun, tokenTotal } from "./analysis.ts";
import { choiceKey, isReference } from "./observations.ts";
import { runCost } from "./prices.ts";

/** A run stopped by a usage limit or a rejected sign-in waits to be redone when its batch resumes. */
const WaitingStatus = "limited";
const InfrastructureFailure = "infrastructure";

/** Where a run's checks stand. A grader error waits on a recheck of the saved files and stays out of coverage meanwhile. */
export const CheckState = { checked: "checked", recheck: "recheck", unchecked: "unchecked" } as const;
export type CheckState = (typeof CheckState)[keyof typeof CheckState];

/** Runs that count: finished agents that weren't replaced and aren't waiting to be redone. */
export function countedRuns(batch: Batch): Run[] {
  const replaced = new Set(batch.runs.flatMap((run) => run.replacesRunId === null ? [] : [run.replacesRunId]));
  return batch.runs.filter((run) => !isReference(run) && run.pipelineStatus !== WaitingStatus && run.agentSeconds !== null
    && !(run.failureKind === InfrastructureFailure && replaced.has(run.id)));
}

export function checkState(run: Run, assessments: readonly Assessment[]): CheckState {
  const latest = latestRun(run, assessments);
  // Older records carry their result on the run itself, without a checks assessment.
  if (latest.checksAssessment === null && latest.result === null) return CheckState.unchecked;
  const coverage = latest.result?.coverage;
  return coverage === undefined || coverage.errors > 0 || coverage.applicable === 0 ? CheckState.recheck : CheckState.checked;
}

/** The share of applicable checks passed, or null while the run waits on checks or a recheck. */
export function coverageOf(run: Run, assessments: readonly Assessment[]): number | null {
  return checkState(run, assessments) === CheckState.checked ? latestRun(run, assessments).result?.coverage.share ?? null : null;
}

/** The share of yes ballots among all of one judge's ballots on the run's latest judgment. */
export function judgeScoreOf(run: Run, choice: Choice, assessments: readonly Assessment[]): number | null {
  const judgment = assessments.filter((item): item is Judge => item.kind === "judge" && item.runId === run.id && choiceKey(item.requestedModel) === choiceKey(choice)).at(-1);
  if (judgment === undefined || judgment.error !== null || judgment.answers.length === 0) return null;
  const yes = judgment.answers.reduce((sum, answer) => sum + answer.yes, 0);
  const all = judgment.answers.reduce((sum, answer) => sum + answer.yes + answer.no + answer.unsure, 0);
  return all === 0 ? null : yes / all;
}

/** One class of a run's tokens. New input already excludes cached input and cache writes. */
export const TokenClass = { newInput: "input", cacheWrites: "cacheWrite", cachedInput: "cachedInput", output: "output" } as const;
export type TokenClass = (typeof TokenClass)[keyof typeof TokenClass];

export function tokenClass(run: Run, kind: TokenClass): number | null {
  return run.telemetry?.tokens?.[kind] ?? null;
}

/** Cached input among all input: new input, cache writes and cached input. Every request sends the whole context again, so most input is cached. */
export function cacheHitRate(run: Run): number | null {
  const counts = run.telemetry?.tokens;
  if (counts == null) return null;
  const input = counts.input + counts.cacheWrite + counts.cachedInput;
  return input === 0 ? null : counts.cachedInput / input;
}

/** The judges that graded a batch, by their choice key. */
export function judgeChoices(batch: Batch): Map<string, Choice> {
  const judges = new Map<string, Choice>();
  for (const item of batch.assessments) if (item.kind === "judge" && item.requestedModel !== null) judges.set(choiceKey(item.requestedModel), item.requestedModel);
  return judges;
}

/** One run's measures side by side, so a chart can plot two of them for the same run. The judge score belongs to one judge. */
export interface RunValues {
  readonly runId: string;
  readonly repeat: number;
  readonly coverage: number | null;
  readonly judgeScore: number | null;
  readonly tokens: number | null;
  readonly cost: number | null;
  readonly newInput: number | null;
  readonly cacheWrites: number | null;
  readonly cachedInput: number | null;
  readonly output: number | null;
  readonly cacheHitRate: number | null;
  readonly minutes: number | null;
}

export function runValues(run: Run, assessments: readonly Assessment[], judge: Choice | null): RunValues {
  return {
    runId: run.id, repeat: run.repeat, coverage: coverageOf(run, assessments), judgeScore: judge === null ? null : judgeScoreOf(run, judge, assessments),
    tokens: tokenTotal(run), cost: runCost(run), newInput: tokenClass(run, TokenClass.newInput), cacheWrites: tokenClass(run, TokenClass.cacheWrites),
    cachedInput: tokenClass(run, TokenClass.cachedInput), output: tokenClass(run, TokenClass.output), cacheHitRate: cacheHitRate(run),
    minutes: run.agentSeconds === null ? null : run.agentSeconds / 60,
  };
}
