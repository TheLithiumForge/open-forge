import type { ModelChoice } from "../config/config.types.ts";
import type { JudgeAnswer, Preference, RunRecord } from "./results.types.ts";
import { PreferenceWinner, RunStatus } from "./statuses.ts";

/** Checks of what the user wanted but didn't say. They never count toward functional success. */
export const IntentCheckArea = "Intent";
export const JudgeAreas = { rules: "rules", code: "code", intent: "intent" } as const;

/** One row of the results table: one setup on one tool and model. Null means no run reported it. */
export interface SetupSummary {
  readonly agent: string;
  readonly setup: string;
  readonly runs: number;
  readonly finished: number;
  readonly success: number | null;
  readonly coverage: number | null;
  readonly intent: number | null;
  readonly rules: number | null;
  readonly code: number | null;
  readonly preference: number | null;
  readonly tokens: number | null;
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
  const known = values.filter((value): value is number => typeof value === "number");
  return known.length === 0 ? null : known.reduce((sum, value) => sum + value, 0) / known.length;
}

function yesShare(answers: readonly JudgeAnswer[], area: string): number | null {
  const scoped = answers.filter((answer) => answer.area === area);
  const asked = scoped.reduce((sum, answer) => sum + answer.yes + answer.no, 0);
  return asked === 0 ? null : scoped.reduce((sum, answer) => sum + answer.yes, 0) / asked;
}

function runScores(run: RunRecord): { success: number | null; coverage: number | null; intent: number | null } {
  const stated = run.checks.filter((check) => check.area !== IntentCheckArea && check.passed !== null);
  const intentChecks = run.checks.filter((check) => check.area === IntentCheckArea && check.passed !== null);
  const passed = stated.filter((check) => check.passed === true).length;
  const intentParts = [intentChecks.length === 0 ? null : intentChecks.filter((check) => check.passed === true).length / intentChecks.length, yesShare(run.judge, JudgeAreas.intent)];
  return {
    success: stated.length === 0 ? null : Number(passed === stated.length),
    coverage: stated.length === 0 ? null : passed / stated.length,
    intent: mean(intentParts),
  };
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

/** Means per setup. Quality and cost stay in separate columns, with no blended score. */
export function summarize(runs: readonly RunRecord[], preferences: readonly Preference[]): SetupSummary[] {
  const groups = new Map<string, RunRecord[]>();
  for (const run of runs) groups.set(`${agentLabel(run.agent)}\n${run.setup}`, [...(groups.get(`${agentLabel(run.agent)}\n${run.setup}`) ?? []), run]);
  return [...groups.entries()].map(([key, group]) => {
    const [agent = "", setup = ""] = key.split("\n");
    const finished = group.filter((run) => run.status === RunStatus.done);
    const scores = finished.map(runScores);
    const tokens = finished.map((run) => run.telemetry?.tokens).map((counts) => (counts === null || counts === undefined ? null : counts.input + counts.cachedInput + counts.cacheWrite + counts.output));
    return {
      agent, setup, runs: group.length, finished: finished.length,
      success: mean(scores.map((score) => score.success)),
      coverage: mean(scores.map((score) => score.coverage)),
      intent: mean(scores.map((score) => score.intent)),
      rules: mean(finished.map((run) => yesShare(run.judge, JudgeAreas.rules))),
      code: mean(finished.map((run) => yesShare(run.judge, JudgeAreas.code))),
      preference: preferenceShare(new Set(group.map((run) => run.id)), preferences),
      tokens: mean(tokens),
      peakContext: mean(finished.map((run) => run.telemetry?.peakContext)),
      compactions: mean(finished.map((run) => run.telemetry?.compactions)),
      openForgeReads: mean(finished.map((run) => run.telemetry?.openForgeReads)),
      questions: mean(finished.map((run) => run.questions.length)),
      seconds: mean(finished.map((run) => run.agentSeconds)),
      costUsd: mean(finished.map((run) => run.telemetry?.reportedCostUsd)),
      bytes: mean(finished.map((run) => run.build?.bytes)),
      tapP95Ms: mean(finished.map((run) => run.performance?.largeBoardTapP95Ms)),
    };
  });
}
