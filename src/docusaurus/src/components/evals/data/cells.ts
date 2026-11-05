import type { Batch } from "../records/records.types.ts";
import { Judging } from "../records/schema-values.ts";
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
export const OpenForgeSetups = ["minimal-proper", "essentials-proper", "full-core-proper", "minimal", "minimal-scoped", "essentials", "essentials-scoped", "full-core", "full-core-scoped", "full-core-scoped-memory"] as const;
const inputsScope = (batch: Batch): string => {
  const fingerprints: unknown = Reflect.get(batch.experiment, "fingerprints");
  return JSON.stringify([typeof fingerprints === "object" && fingerprints !== null ? [Reflect.get(fingerprints, "benchmarkInputs"), Reflect.get(fingerprints, "tooling")] : null, batch.experiment.suite]);
};
/** A case whose control ran in an earlier batch, so a batch of only new setups still has something to compare with. */
export const CaseControls: Readonly<Record<string, string>> = { "l4-f2-tournament": "organized-docs" };

export interface Measure {
  readonly mean: number | null;
  /** Every run's value, so a chart can show the spread. */
  readonly values: readonly number[];
}

/** One starting point, case, setup, implementer and judge. Checks and tokens don't depend on the judge, so judge rows repeat them. */
export interface Cell {
  readonly startingPoint: StartingPointId;
  /** The Layer 4 case, such as l4-f1-name-saves, or the suite for earlier starting points. */
  readonly caseId: string;
  readonly batchId: string;
  /** When the batch started, so the newest runs of a setup win. */
  readonly createdAt: string;
  readonly cliVersion: string | null;
  readonly setup: string;
  readonly control: string;
  /** The benchmark inputs and tooling the batch used. Only cells with the same ones compare across batches. */
  readonly inputsScope: string;
  /** The milestone this cell's batch belongs to, or null for a standalone batch. */
  readonly chain: { readonly id: string; readonly milestone: number } | null;
  readonly workMode: WorkModeId;
  readonly model: string;
  readonly modelLabel: string;
  /** The agent's own program and version, such as Codex 0.166.0, when the batch recorded it. */
  readonly agentVersion: string | null;
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
  /** The batch leaves judging for later, so runs without a judgment don't make the view preliminary. */
  readonly judgedLater: boolean;
  readonly coverage: Measure;
  readonly pagesCurrent: Measure;
  readonly judgeScore: Measure;
  readonly judgeQuality: Measure;
  readonly judgeAdherence: Measure;
  readonly judgeUpkeep: Measure;
  readonly tokens: Measure;
  readonly cost: Measure;
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

function chainOf(batch: Batch): Cell["chain"] {
  const position = batch.experiment.chain ?? batch.experiment.config.chain;
  return position === undefined ? null : { id: position.id, milestone: position.milestone };
}

/** A chained milestone is its own measurement, so it never replaces the same case's standalone runs, nor they it. */
const setupKey = (cell: Cell) => JSON.stringify([cell.caseId, cell.model, cell.setup, cell.chain?.id ?? null, cell.chain?.milestone ?? null]);
/** How a cell's work was handed over: one task alone, or a work packet whole, task by task or to an orchestrator. */
export const WorkModeIds = { singleTask: "single-task", wholePacket: "whole-packet", taskByTask: "task-by-task", orchestrated: "orchestrated" } as const;
export type WorkModeId = (typeof WorkModeIds)[keyof typeof WorkModeIds];
const workModes: readonly string[] = Object.values(WorkModeIds);

/** A plan names its work mode. Earlier batches ran single tasks, and a team led its runs as an orchestrator. */
export function workModeOf(config: Batch["experiment"]["config"], agent: Batch["experiment"]["config"]["agents"][number]): WorkModeId {
  if (config.workMode !== undefined && workModes.includes(config.workMode)) return config.workMode as WorkModeId;
  return agent.team === undefined ? WorkModeIds.singleTask : WorkModeIds.orchestrated;
}

/**
 * Each setup's newest runs for a case and implementer. A later batch can add a setup beside its own control without hiding
 * the setups an earlier batch ran. Older runs of a setup used earlier inputs or grading, so they stay in history only.
 * A batch with no finished run of a setup hides nothing, so an empty cell shows only while no batch has runs for it.
 */
export function newestCells<T extends { readonly cell: Cell }>(items: readonly T[]): T[] {
  const newest = new Map<string, { readonly createdAt: string; readonly hasRuns: boolean }>();
  for (const { cell } of items) {
    const current = newest.get(setupKey(cell)), hasRuns = cell.runs > 0;
    if (current === undefined || (hasRuns && !current.hasRuns) || (hasRuns === current.hasRuns && current.createdAt < cell.createdAt)) newest.set(setupKey(cell), { createdAt: cell.createdAt, hasRuns });
  }
  return items.filter(({ cell }) => newest.get(setupKey(cell))?.createdAt === cell.createdAt);
}

const AgentPrograms: Readonly<Record<string, { readonly name: string; readonly tool: string }>> = {
  codex: { name: "Codex", tool: "@openai/codex" },
  claude: { name: "Claude Code", tool: "@anthropic-ai/claude-code" },
};
/** The agent program's version, which the batch record carries but its synced type doesn't list. */
export function agentVersion(batch: Batch, tool: string): string | null {
  const program = AgentPrograms[tool], pinned: unknown = Reflect.get(batch.experiment, "pinnedTools");
  if (program === undefined || typeof pinned !== "object" || pinned === null) return null;
  const version: unknown = Reflect.get(pinned, program.tool);
  return typeof version === "string" ? `${program.name} ${version}` : null;
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
    const control = CaseControls[batch.experiment.suite] ?? batch.experiment.config.setups[0] ?? "";
    const agents = batch.experiment.config.agents;
    return agents.flatMap((agent) => batch.experiment.config.setups.flatMap((setup): Cell[] => {
      const group = runs.filter((run) => run.setup === setup && choiceKey(run.agent) === choiceKey(agent));
      const reported = group.find((run) => run.reportedModel !== null)?.reportedModel ?? null;
      const states = group.map((run) => checkState(run, batch.assessments));
      const values = group.map((run) => runValues(run, batch.assessments, null));
      const pick = (read: (value: RunValues) => number | null) => measure(values.map(read));
      const base = {
        startingPoint, caseId: batch.experiment.suite, inputsScope: inputsScope(batch), batchId: batch.experiment.id, createdAt: batch.experiment.createdAt, cliVersion: batch.experiment.cliVersion, setup, control,
        // A task-by-task packet's first task starts fresh, so only its plan names the chain.
        chain: chainOf(batch),
        workMode: workModeOf(batch.experiment.config, agent),
        judgedLater: batch.experiment.config.judging === Judging.later,
        model: choiceKey(agent), modelLabel: modelLabel(agent, reported), agentVersion: agentVersion(batch, agent.tool), runs: group.length, runIds: group.map((run) => run.id),
        checked: states.filter((state) => state === CheckState.checked).length, recheck: states.filter((state) => state === CheckState.recheck).length,
        coverage: pick((value) => value.coverage), pagesCurrent: pick((value) => value.pagesCurrent), tokens: pick((value) => value.tokens), cost: pick((value) => value.cost), newInput: pick((value) => value.newInput),
        cacheWrites: pick((value) => value.cacheWrites), cachedInput: pick((value) => value.cachedInput), outputTokens: pick((value) => value.output),
        cacheHitRate: pick((value) => value.cacheHitRate), minutes: pick((value) => value.minutes),
      };
      if (judges.size === 0) return [{ ...base, judge: null, judgeLabel: null, judged: 0, judgeScore: measure([]), judgeQuality: measure([]), judgeAdherence: measure([]), judgeUpkeep: measure([]), runValues: values }];
      return [...judges.values()].map((choice) => {
        const judged = group.map((run) => runValues(run, batch.assessments, choice));
        const scores = judged.map((value) => value.judgeScore);
        return { ...base, judge: choiceKey(choice), judgeLabel: modelLabel(choice), judged: scores.filter((score) => score !== null).length, judgeScore: measure(scores), judgeQuality: measure(judged.map((value) => value.judgeQuality)), judgeAdherence: measure(judged.map((value) => value.judgeAdherence)), judgeUpkeep: measure(judged.map((value) => value.judgeUpkeep)), runValues: judged };
      });
    }));
  });
}
