import type { Batch } from "../records/records.types.ts";
import { choiceKey } from "./observations.ts";
import { modelLabel } from "./names.ts";
import { checkState, CheckState, countedRuns, judgeChoices, runValues, type RunValues } from "./run-measures.ts";

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
/** Setups that install Open Forge, from smallest to largest, then the scoped variant. */
export const OpenForgeSetups = ["minimal", "essentials", "full-core", "full-core-scoped"] as const;

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
  /** When the batch started, so the newest runs of a setup win. */
  readonly createdAt: string;
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
  /** Runs with a coverage value. The others wait on their checks or on a recheck after a grader error. */
  readonly checked: number;
  /** Runs whose checks ended in a grader error. They stay out of coverage until a recheck succeeds. */
  readonly recheck: number;
  /** Runs this cell's judge scored. */
  readonly judged: number;
  readonly coverage: Measure;
  readonly judgeScore: Measure;
  readonly tokens: Measure;
  /** Token classes. They add up to the total. */
  readonly newInput: Measure;
  readonly cacheWrites: Measure;
  readonly cachedInput: Measure;
  readonly outputTokens: Measure;
  readonly cacheHitRate: Measure;
  readonly minutes: Measure;
  /** Every run's measures, in run order, with this cell's judge. */
  readonly runValues: readonly RunValues[];
}

export function measure(values: readonly (number | null)[]): Measure {
  const present = values.filter((value): value is number => value !== null && Number.isFinite(value));
  return { mean: present.length === 0 ? null : present.reduce((sum, value) => sum + value, 0) / present.length, values: present };
}

export function startingPointOf(batch: Batch): StartingPointId | null {
  return StartingPoints.find((point) => point.layer === batch.experiment.config.layer)?.id ?? null;
}

const setupKey = (cell: Cell) => JSON.stringify([cell.caseId, cell.model, cell.setup]);

/**
 * Each setup's newest runs for a case and implementer. A later batch can add a setup beside its own control without hiding
 * the setups an earlier batch ran. Older runs of a setup used earlier inputs or grading, so they stay in history only.
 */
export function newestCells<T extends { readonly cell: Cell }>(items: readonly T[]): T[] {
  const newest = new Map<string, string>();
  for (const { cell } of items) {
    const current = newest.get(setupKey(cell));
    if (current === undefined || current < cell.createdAt) newest.set(setupKey(cell), cell.createdAt);
  }
  return items.filter(({ cell }) => newest.get(setupKey(cell)) === cell.createdAt);
}

/**
 * Every cell of every batch, so each one can meet its own batch's control. Pick the shown ones with newestCells. Batches without a
 * known starting point, and reference grading, stay out.
 */
export function cells(batches: readonly Batch[]): Cell[] {
  const comparable = batches.filter((batch) => startingPointOf(batch) !== null && batch.experiment.reference === undefined);
  return comparable.flatMap((batch) => {
    const startingPoint = startingPointOf(batch);
    if (startingPoint === null) return [];
    const runs = countedRuns(batch);
    const judges = judgeChoices(batch);
    const control = batch.experiment.config.setups[0] ?? "";
    const agents = batch.experiment.config.agents;
    return agents.flatMap((agent) => batch.experiment.config.setups.flatMap((setup): Cell[] => {
      const group = runs.filter((run) => run.setup === setup && choiceKey(run.agent) === choiceKey(agent));
      const reported = group.find((run) => run.reportedModel !== null)?.reportedModel ?? null;
      const states = group.map((run) => checkState(run, batch.assessments));
      const values = group.map((run) => runValues(run, batch.assessments, null));
      const pick = (read: (value: RunValues) => number | null) => measure(values.map(read));
      const base = {
        startingPoint, caseId: batch.experiment.suite, batchId: batch.experiment.id, createdAt: batch.experiment.createdAt, cliVersion: batch.experiment.cliVersion, setup, control,
        model: choiceKey(agent), modelLabel: modelLabel(agent, reported), runs: group.length, runIds: group.map((run) => run.id),
        checked: states.filter((state) => state === CheckState.checked).length, recheck: states.filter((state) => state === CheckState.recheck).length,
        coverage: pick((value) => value.coverage), tokens: pick((value) => value.tokens), newInput: pick((value) => value.newInput),
        cacheWrites: pick((value) => value.cacheWrites), cachedInput: pick((value) => value.cachedInput), outputTokens: pick((value) => value.output),
        cacheHitRate: pick((value) => value.cacheHitRate), minutes: pick((value) => value.minutes),
      };
      if (judges.size === 0) return [{ ...base, judge: null, judgeLabel: null, judged: 0, judgeScore: measure([]), runValues: values }];
      return [...judges.values()].map((choice) => {
        const judged = group.map((run) => runValues(run, batch.assessments, choice));
        const scores = judged.map((value) => value.judgeScore);
        return { ...base, judge: choiceKey(choice), judgeLabel: modelLabel(choice), judged: scores.filter((score) => score !== null).length, judgeScore: measure(scores), runValues: judged };
      });
    }));
  });
}
