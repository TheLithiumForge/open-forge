import type { ModelChoice } from "../config/config.types.ts";
import { modelKey } from "../config/judges.ts";
import { agentLabel, latestJudgeFor } from "./summary.ts";
import type { AssessmentRecord, JudgeAssessment, RunView } from "./records.types.ts";
import type { BatchRecord } from "./results.types.ts";
import { AssessmentKind, FailureKind, RunStatus } from "./statuses.ts";

/** How far one setup has come: its implemented runs, their mean check coverage and each judge's yes share. */
export interface SetupProgress {
  readonly agent: string;
  readonly setup: string;
  readonly implemented: number;
  readonly planned: number;
  readonly checked: number;
  readonly coverage: number | null;
  readonly judges: readonly { readonly judge: string; readonly judged: number; readonly yesShare: number | null }[];
}

export interface BatchProgress {
  readonly id: string;
  readonly layer: number | null;
  readonly suite: string;
  readonly status: string;
  readonly planned: number;
  readonly implemented: number;
  readonly checked: number;
  readonly judges: readonly { readonly judge: string; readonly judged: number }[];
  readonly setups: readonly SetupProgress[];
}

function mean(values: readonly number[]): number | null {
  return values.length === 0 ? null : values.reduce((sum, value) => sum + value, 0) / values.length;
}

/** Yes ballots among all ballots, over every question. An unsure ballot counts as not yes, since some judges rarely answer no. */
function yesShare(judgment: JudgeAssessment): number | null {
  const yes = judgment.answers.reduce((sum, answer) => sum + answer.yes, 0);
  const all = judgment.answers.reduce((sum, answer) => sum + answer.yes + answer.no + answer.unsure, 0);
  return all === 0 ? null : yes / all;
}

/** Whether a run's latest check graded at least one clause and no clause ended in an error. */
export function checkedWithoutErrors(run: Pick<RunView, "functionalResult">): boolean {
  return run.functionalResult !== null && run.functionalResult.coverage.errors === 0 && run.functionalResult.coverage.applicable > 0;
}

/** Completion and preliminary scores for a batch. A replaced infrastructure failure or a run waiting to be redone isn't counted. */
export function batchProgress(batch: BatchRecord, runs: readonly RunView[], assessments: readonly AssessmentRecord[]): BatchProgress {
  const replaced = new Set(runs.flatMap((run) => run.replacesRunId === null ? [] : [run.replacesRunId]));
  const counted = runs.filter((run) => run.status !== RunStatus.limited && !(run.failureKind === FailureKind.infrastructure && replaced.has(run.id)));
  // A run is implemented once its agent finished, while its grading may still be running.
  const implemented = counted.filter((run) => run.agentSeconds !== null);
  const checked = checkedWithoutErrors;
  const judges = new Map<string, ModelChoice>();
  for (const item of assessments) if (item.kind === AssessmentKind.judge && item.requestedModel !== null) judges.set(modelKey(item.requestedModel), item.requestedModel);
  const judgment = (run: RunView, choice: ModelChoice): JudgeAssessment | undefined => {
    const found = latestJudgeFor(run.id, assessments, choice);
    return found !== undefined && found.error === null && found.answers.length > 0 ? found : undefined;
  };
  const setups = batch.config.agents.flatMap((agent) => batch.config.setups.map((setup): SetupProgress => {
    const done = implemented.filter((run) => run.setup === setup && modelKey(run.agent) === modelKey(agent));
    const scored = done.filter(checked);
    return {
      agent: agentLabel(agent), setup, implemented: done.length, planned: batch.config.runsPerSetup, checked: scored.length,
      coverage: mean(scored.map((run) => run.functionalResult?.coverage.share ?? 0)),
      judges: [...judges.values()].map((choice) => {
        const shares = done.flatMap((run) => { const found = judgment(run, choice); return found === undefined ? [] : [yesShare(found)]; });
        return { judge: agentLabel(choice), judged: shares.length, yesShare: mean(shares.filter((share): share is number => share !== null)) };
      }),
    };
  }));
  return {
    id: batch.id, layer: batch.config.layer ?? null, suite: batch.suite, status: batch.status,
    planned: batch.config.agents.length * batch.config.setups.length * batch.config.runsPerSetup,
    implemented: implemented.length, checked: implemented.filter(checked).length,
    judges: [...judges.values()].map((choice) => ({ judge: agentLabel(choice), judged: implemented.filter((run) => judgment(run, choice) !== undefined).length })),
    setups,
  };
}

const percent = (value: number | null): string => value === null ? "-" : `${Math.round(value * 100)}%`;

/** A plain table per batch, readable in a terminal. */
export function progressLines(progress: BatchProgress): string[] {
  const share = progress.planned === 0 ? null : progress.implemented / progress.planned;
  const judged = progress.judges.map((judge) => `judged by ${judge.judge}: ${judge.judged}`);
  const header = ["implementer", "setup", "implemented", "checked", "coverage", ...progress.judges.map((judge) => `${judge.judge} yes`)];
  const rows = progress.setups.map((setup) => [setup.agent, setup.setup, `${setup.implemented}/${setup.planned}`, String(setup.checked), percent(setup.coverage),
    ...setup.judges.map((judge) => judge.judged === 0 ? "-" : `${percent(judge.yesShare)} (${judge.judged})`)]);
  const widths = header.map((title, column) => Math.max(title.length, ...rows.map((row) => row[column]?.length ?? 0)));
  const line = (cells: readonly string[]): string => cells.map((cell, column) => cell.padEnd(widths[column] ?? 0)).join("  ").trimEnd();
  return [
    `Layer ${progress.layer ?? "?"} ${progress.suite}, batch ${progress.id}, ${progress.status}`,
    `Implemented ${progress.implemented} of ${progress.planned} (${percent(share)}), checked ${progress.checked}${judged.length === 0 ? "" : `, ${judged.join(", ")}`}`,
    line(header), ...rows.map(line),
  ];
}
