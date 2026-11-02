import type { Assessment, Batch, Choice, Judge, Run } from "../records/records.types.ts";
import { latestRun } from "./analysis.ts";
import { choiceKey, isReference } from "./observations.ts";

/**
 * The reader-facing view of the eval records. A starting point is what the agent receives, a setup is how its project is
 * prepared, and every comparison is against the control: the plan's first setup for the same starting point and model.
 */
export const StartingPoints = [
  { id: "idea", layer: 1, title: "An idea" },
  { id: "brief", layer: 2, title: "A short brief" },
  { id: "full-spec", layer: 3, title: "A full spec" },
  { id: "existing-app", layer: 4, title: "An existing app" },
] as const;
export type StartingPointId = (typeof StartingPoints)[number]["id"];
/** Setups that install Open Forge, from smallest to largest. */
export const OpenForgeSetups = ["minimal", "essentials", "full-core"] as const;
export const All = "all";

export interface Measure {
  readonly mean: number | null;
  /** Every run's value, so a chart can show the spread. */
  readonly values: readonly number[];
}

/** One starting point, case, setup, implementer and judge. Checks and tokens don't depend on the judge, so judge rows repeat them. */
export interface Cell {
  readonly startingPoint: StartingPointId;
  /** The Layer 4 case, such as l4-f01, or the suite for earlier starting points. */
  readonly caseId: string;
  readonly batchId: string;
  readonly cliVersion: string | null;
  readonly setup: string;
  readonly control: string;
  readonly model: string;
  readonly modelLabel: string;
  readonly judge: string | null;
  readonly judgeLabel: string | null;
  /** Runs whose agent finished, without replaced failures or runs waiting to be redone. */
  readonly runs: number;
  readonly runIds: readonly string[];
  readonly coverage: Measure;
  readonly judgeScore: Measure;
  readonly tokens: Measure;
  readonly outputTokens: Measure;
  readonly minutes: Measure;
}

/** A cell set beside its control on the same starting point, case, model and judge. */
export interface Comparison {
  readonly cell: Cell;
  readonly controlCell: Cell | undefined;
  /** Percentage points above or below the control. */
  readonly coverageDelta: number | null;
  readonly judgeDelta: number | null;
  /** Tokens as a ratio of the control's, so 1.2 is 20% more. */
  readonly tokenRatio: number | null;
  readonly minuteRatio: number | null;
}

export interface Filters {
  readonly model: string;
  readonly judge: string;
  readonly startingPoint: StartingPointId | typeof All;
}

/** One setup's comparison with the control, averaged over the starting points, cases and models that pass the filters. */
export interface SetupSummary {
  readonly setup: string;
  /** Starting point, case and model combinations that have both this setup and its control. Each counts once. */
  readonly groups: number;
  readonly runs: number;
  readonly coverageDelta: number | null;
  readonly judgeDelta: number | null;
  /** Geometric mean of the token ratios, so 20% more and 20% less don't cancel to a false zero. */
  readonly tokenRatio: number | null;
  readonly coverage: number | null;
  readonly judgeScore: number | null;
  readonly tokens: number | null;
}

const ModelNames: Readonly<Record<string, string>> = { "claude-opus-5-5": "Claude Opus 5.5", opus: "Claude Opus", sonnet: "Claude Sonnet", "gpt-6.1-sol": "GPT-6.1 Sol", "gpt-6-luna": "GPT-6 Luna" };

export function modelLabel(choice: Choice, reported?: string | null): string {
  const name = ModelNames[reported ?? ""] ?? ModelNames[choice.model ?? ""] ?? reported ?? choice.model ?? choice.tool;
  return choice.effort === null ? name : `${name}, ${choice.effort} reasoning`;
}

function measure(values: readonly (number | null)[]): Measure {
  const present = values.filter((value): value is number => value !== null && Number.isFinite(value));
  return { mean: present.length === 0 ? null : present.reduce((sum, value) => sum + value, 0) / present.length, values: present };
}

function startingPointOf(batch: Batch): StartingPointId | null {
  return StartingPoints.find((point) => point.layer === batch.experiment.config.layer)?.id ?? null;
}

/** Runs that count: finished agents that weren't replaced and aren't waiting to be redone. */
function countedRuns(batch: Batch): Run[] {
  const replaced = new Set(batch.runs.flatMap((run) => run.replacesRunId === null ? [] : [run.replacesRunId]));
  return batch.runs.filter((run) => !isReference(run) && run.pipelineStatus !== "limited" && run.agentSeconds !== null
    && !(run.failureKind === "infrastructure" && replaced.has(run.id)));
}

function coverageOf(run: Run, assessments: readonly Assessment[]): number | null {
  const coverage = latestRun(run, assessments).result?.coverage;
  return coverage === undefined || coverage.errors > 0 || coverage.applicable === 0 ? null : coverage.share;
}

function judgeScoreOf(run: Run, choice: Choice, assessments: readonly Assessment[]): number | null {
  const judgment = assessments.filter((item): item is Judge => item.kind === "judge" && item.runId === run.id && choiceKey(item.requestedModel) === choiceKey(choice)).at(-1);
  if (judgment === undefined || judgment.error !== null || judgment.answers.length === 0) return null;
  const yes = judgment.answers.reduce((sum, answer) => sum + answer.yes, 0);
  const all = judgment.answers.reduce((sum, answer) => sum + answer.yes + answer.no + answer.unsure, 0);
  return all === 0 ? null : yes / all;
}

function tokens(run: Run): number | null {
  const counts = run.telemetry?.tokens;
  return counts == null ? null : counts.input + counts.cachedInput + counts.cacheWrite + counts.output;
}

/** The newest batch for each case and implementer. Older batches used earlier inputs or grading, so they stay in history only. */
export function newestBatches(batches: readonly Batch[]): Map<string, string> {
  const newest = new Map<string, Batch>();
  for (const batch of batches) for (const agent of batch.experiment.config.agents) {
    const key = JSON.stringify([batch.experiment.suite, choiceKey(agent)]);
    const current = newest.get(key);
    if (current === undefined || current.experiment.createdAt < batch.experiment.createdAt) newest.set(key, batch);
  }
  return new Map([...newest].map(([key, batch]) => [key, batch.experiment.id]));
}

/** Every cell of the newest batch for each case and implementer. Batches without a known starting point, and reference grading, stay out. */
export function cells(batches: readonly Batch[]): Cell[] {
  const comparable = batches.filter((batch) => startingPointOf(batch) !== null && batch.experiment.reference === undefined);
  const newest = newestBatches(comparable);
  return comparable.flatMap((batch) => {
    const startingPoint = startingPointOf(batch);
    if (startingPoint === null) return [];
    const runs = countedRuns(batch);
    const judges = new Map<string, Choice>();
    for (const item of batch.assessments) if (item.kind === "judge" && item.requestedModel !== null) judges.set(choiceKey(item.requestedModel), item.requestedModel);
    const control = batch.experiment.config.setups[0] ?? "";
    const agents = batch.experiment.config.agents.filter((agent) => newest.get(JSON.stringify([batch.experiment.suite, choiceKey(agent)])) === batch.experiment.id);
    return agents.flatMap((agent) => batch.experiment.config.setups.flatMap((setup): Cell[] => {
      const group = runs.filter((run) => run.setup === setup && choiceKey(run.agent) === choiceKey(agent));
      const reported = group.find((run) => run.reportedModel !== null)?.reportedModel ?? null;
      const base = {
        startingPoint, caseId: batch.experiment.suite, batchId: batch.experiment.id, cliVersion: batch.experiment.cliVersion, setup, control,
        model: choiceKey(agent), modelLabel: modelLabel(agent, reported), runs: group.length, runIds: group.map((run) => run.id),
        coverage: measure(group.map((run) => coverageOf(run, batch.assessments))),
        tokens: measure(group.map(tokens)), outputTokens: measure(group.map((run) => run.telemetry?.tokens?.output ?? null)),
        minutes: measure(group.map((run) => run.agentSeconds === null ? null : run.agentSeconds / 60)),
      };
      if (judges.size === 0) return [{ ...base, judge: null, judgeLabel: null, judgeScore: measure([]) }];
      return [...judges.values()].map((choice) => ({ ...base, judge: choiceKey(choice), judgeLabel: modelLabel(choice),
        judgeScore: measure(group.map((run) => judgeScoreOf(run, choice, batch.assessments))) }));
    }));
  });
}

const ratio = (value: number | null, control: number | null) => value === null || control === null || control === 0 ? null : value / control;
const delta = (value: number | null, control: number | null) => value === null || control === null ? null : value - control;

/** Each cell beside the control cell of the same batch, model and judge. */
export function comparisons(all: readonly Cell[]): Comparison[] {
  return all.map((cell) => {
    const controlCell = all.find((other) => other.batchId === cell.batchId && other.setup === cell.control && other.model === cell.model && other.judge === cell.judge);
    return { cell, controlCell,
      coverageDelta: delta(cell.coverage.mean, controlCell?.coverage.mean ?? null), judgeDelta: delta(cell.judgeScore.mean, controlCell?.judgeScore.mean ?? null),
      tokenRatio: ratio(cell.tokens.mean, controlCell?.tokens.mean ?? null), minuteRatio: ratio(cell.minutes.mean, controlCell?.minutes.mean ?? null) };
  });
}

export function filtered<T extends { readonly cell: Cell } | Cell>(items: readonly T[], filters: Filters): T[] {
  return items.filter((item) => {
    const cell: Cell = "cell" in item ? item.cell : item;
    return (filters.model === All || cell.model === filters.model) && (filters.judge === All || cell.judge === filters.judge)
      && (filters.startingPoint === All || cell.startingPoint === filters.startingPoint);
  });
}

const average = (values: readonly (number | null)[]) => measure(values).mean;
const geometric = (values: readonly (number | null)[]) => {
  const logs = values.filter((value): value is number => value !== null && value > 0).map(Math.log);
  return logs.length === 0 ? null : Math.exp(logs.reduce((sum, value) => sum + value, 0) / logs.length);
};

/**
 * Each setup against its control. Judge rows are averaged first, so every starting point, case and model counts once,
 * whatever its number of runs or judges.
 */
export function setupSummaries(items: readonly Comparison[]): SetupSummary[] {
  const setups = [...new Set(items.map((item) => item.cell.setup))];
  return setups.map((setup) => {
    const groups = new Map<string, Comparison[]>();
    for (const item of items.filter((entry) => entry.cell.setup === setup)) {
      const key = JSON.stringify([item.cell.batchId, item.cell.model]);
      groups.set(key, [...groups.get(key) ?? [], item]);
    }
    const perGroup = [...groups.values()].map((group) => ({
      runs: group[0]?.cell.runs ?? 0,
      coverageDelta: average(group.map((item) => item.coverageDelta)), judgeDelta: average(group.map((item) => item.judgeDelta)),
      tokenRatio: geometric(group.map((item) => item.tokenRatio)),
      coverage: average(group.map((item) => item.cell.coverage.mean)), judgeScore: average(group.map((item) => item.cell.judgeScore.mean)),
      tokens: average(group.map((item) => item.cell.tokens.mean)),
    }));
    const compared = perGroup.filter((group) => group.coverageDelta !== null || group.tokenRatio !== null || group.judgeDelta !== null);
    return { setup, groups: compared.length, runs: perGroup.reduce((sum, group) => sum + group.runs, 0),
      coverageDelta: average(compared.map((group) => group.coverageDelta)), judgeDelta: average(compared.map((group) => group.judgeDelta)),
      tokenRatio: geometric(compared.map((group) => group.tokenRatio)),
      coverage: average(perGroup.map((group) => group.coverage)), judgeScore: average(perGroup.map((group) => group.judgeScore)), tokens: average(perGroup.map((group) => group.tokens)) };
  });
}

/** The models and judges present, for the filter menus. */
export function options(all: readonly Cell[]): { readonly models: readonly { key: string; label: string }[]; readonly judges: readonly { key: string; label: string }[] } {
  const unique = (entries: readonly { key: string | null; label: string | null }[]) =>
    [...new Map(entries.flatMap((entry) => entry.key === null || entry.label === null ? [] : [[entry.key, { key: entry.key, label: entry.label }] as const])).values()];
  return { models: unique(all.map((cell) => ({ key: cell.model, label: cell.modelLabel }))), judges: unique(all.map((cell) => ({ key: cell.judge, label: cell.judgeLabel }))) };
}
