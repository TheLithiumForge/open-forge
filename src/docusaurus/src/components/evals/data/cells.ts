import type { Batch, Choice } from "../records/records.types.ts";
import { tokenTotal } from "./analysis.ts";
import { choiceKey } from "./observations.ts";
import { modelLabel } from "./names.ts";
import { cacheHitRate, checkState, CheckState, countedRuns, coverageOf, judgeChoices, judgeScoreOf, tokenClass, TokenClass } from "./run-measures.ts";

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
}

export function measure(values: readonly (number | null)[]): Measure {
  const present = values.filter((value): value is number => value !== null && Number.isFinite(value));
  return { mean: present.length === 0 ? null : present.reduce((sum, value) => sum + value, 0) / present.length, values: present };
}

export function startingPointOf(batch: Batch): StartingPointId | null {
  return StartingPoints.find((point) => point.layer === batch.experiment.config.layer)?.id ?? null;
}

const newestKey = (batch: Batch, agent: Choice) => JSON.stringify([batch.experiment.suite, choiceKey(agent)]);

/** The newest batch for each case and implementer. Older batches used earlier inputs or grading, so they stay in history only. */
export function newestBatches(batches: readonly Batch[]): Map<string, string> {
  const newest = new Map<string, Batch>();
  for (const batch of batches) for (const agent of batch.experiment.config.agents) {
    const current = newest.get(newestKey(batch, agent));
    if (current === undefined || current.experiment.createdAt < batch.experiment.createdAt) newest.set(newestKey(batch, agent), batch);
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
    const judges = judgeChoices(batch);
    const control = batch.experiment.config.setups[0] ?? "";
    const agents = batch.experiment.config.agents.filter((agent) => newest.get(newestKey(batch, agent)) === batch.experiment.id);
    return agents.flatMap((agent) => batch.experiment.config.setups.flatMap((setup): Cell[] => {
      const group = runs.filter((run) => run.setup === setup && choiceKey(run.agent) === choiceKey(agent));
      const reported = group.find((run) => run.reportedModel !== null)?.reportedModel ?? null;
      const states = group.map((run) => checkState(run, batch.assessments));
      const base = {
        startingPoint, caseId: batch.experiment.suite, batchId: batch.experiment.id, cliVersion: batch.experiment.cliVersion, setup, control,
        model: choiceKey(agent), modelLabel: modelLabel(agent, reported), runs: group.length, runIds: group.map((run) => run.id),
        checked: states.filter((state) => state === CheckState.checked).length, recheck: states.filter((state) => state === CheckState.recheck).length,
        coverage: measure(group.map((run) => coverageOf(run, batch.assessments))),
        tokens: measure(group.map(tokenTotal)), newInput: measure(group.map((run) => tokenClass(run, TokenClass.newInput))),
        cacheWrites: measure(group.map((run) => tokenClass(run, TokenClass.cacheWrites))), cachedInput: measure(group.map((run) => tokenClass(run, TokenClass.cachedInput))),
        outputTokens: measure(group.map((run) => tokenClass(run, TokenClass.output))), cacheHitRate: measure(group.map(cacheHitRate)),
        minutes: measure(group.map((run) => run.agentSeconds === null ? null : run.agentSeconds / 60)),
      };
      if (judges.size === 0) return [{ ...base, judge: null, judgeLabel: null, judged: 0, judgeScore: measure([]) }];
      return [...judges.values()].map((choice) => {
        const scores = group.map((run) => judgeScoreOf(run, choice, batch.assessments));
        return { ...base, judge: choiceKey(choice), judgeLabel: modelLabel(choice), judged: scores.filter((score) => score !== null).length, judgeScore: measure(scores) };
      });
    }));
  });
}
