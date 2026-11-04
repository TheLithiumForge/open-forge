import type { Batch } from "../records/records.types.ts";
import type { Comparison } from "./aggregate.ts";
import { WorkModeIds, type Cell, type WorkModeId } from "./cells.ts";
import { bySetup, identity, type ChartRow } from "./chart-data.ts";
import type { ThreadRoles } from "../records/schema-values.ts";
import { isCaseId, PacketTasks } from "./names.ts";
import { latestAnalyses } from "./objective.ts";
import { orchestratorCostShare } from "./prices.ts";

/** Tokens per task, milestone views and orchestration comparisons, each from the cells that have such data. */

const mean = (values: readonly (number | null | undefined)[]): number | null => {
  const known = values.filter((value): value is number => typeof value === "number");
  return known.length === 0 ? null : known.reduce((sum, value) => sum + value, 0) / known.length;
};
/** Judge rows repeat a cell's runs, so each batch, setup and model counts once. */
const unique = (items: readonly Comparison[]) => [...new Map(items.map((item) => [JSON.stringify([item.cell.batchId, item.cell.setup, item.cell.model]), item.cell])).values()];

/** One task, a Layer 4 case or a starting point's suite, with each setup's mean tokens per run. */
export interface TaskRow {
  readonly caseId: string;
  readonly tokens: ReadonlyMap<string, number | null>;
}

export function taskRows(items: readonly Comparison[]): TaskRow[] {
  const cells = unique(items);
  return [...new Set(cells.map((cell) => cell.caseId))].sort().map((caseId) => {
    const own = cells.filter((cell) => cell.caseId === caseId);
    return { caseId, tokens: new Map([...new Set(own.map((cell) => cell.setup))].map((setup) => [setup, mean(own.filter((cell) => cell.setup === setup).map((cell) => cell.tokens.mean))])) };
  });
}

/** A chained milestone beside the same task run on its own, from the shared start. */
export interface MilestoneRow extends Omit<ChartRow, "value"> {
  readonly chain: string;
  readonly milestone: number;
  readonly caseId: string;
  readonly tokens: number | null;
  readonly coverage: number | null;
  readonly standaloneTokens: number | null;
  readonly standaloneCoverage: number | null;
}

export function milestoneRows(items: readonly Comparison[]): MilestoneRow[] {
  const cells = unique(items);
  const standalone = (cell: Cell) => cells.filter((other) => other.chain === null && other.caseId === cell.caseId && other.setup === cell.setup && other.model === cell.model);
  return cells.flatMap((cell) => cell.chain === null ? [] : [{
    ...identity(cell.setup), chain: cell.chain.id, milestone: cell.chain.milestone, caseId: cell.caseId, tokens: cell.tokens.mean, coverage: cell.coverage.mean,
    standaloneTokens: mean(standalone(cell).map((other) => other.tokens.mean)), standaloneCoverage: mean(standalone(cell).map((other) => other.coverage.mean)),
  }]).sort((a, b) => a.chain.localeCompare(b.chain) || a.milestone - b.milestone || bySetup(a, b));
}

const MainThread: (typeof ThreadRoles)[number] = "main";
const ModeOrder: readonly WorkModeId[] = Object.values(WorkModeIds);

/** One scenario's work on a packet in one work mode by one implementer. Task by task adds up its tasks. */
export interface PacketRow extends Omit<ChartRow, "value"> {
  readonly packet: string;
  readonly workMode: WorkModeId;
  readonly modelLabel: string;
  /** The tasks this row adds up: one request for a whole or orchestrated packet, one per task otherwise. */
  readonly steps: number;
  /** The tasks the packet holds, whichever way it was handed over. */
  readonly tasks: number;
  readonly tokens: number | null;
  readonly cost: number | null;
  readonly costPerTask: number | null;
  readonly coverage: number | null;
  /** The orchestrator thread's share of the system's input, for orchestrated work. */
  readonly orchestratorShare: number | null;
  /** The orchestrator thread's share of the system's cost, for orchestrated work. */
  readonly orchestratorCostShare: number | null;
  readonly subagents: number | null;
}

export function packetRows(items: readonly Comparison[], batches: ReadonlyMap<string, Batch>): PacketRow[] {
  const cells = unique(items);
  const threads = (cell: Cell) => {
    if (cell.workMode !== WorkModeIds.orchestrated) return { orchestratorShare: null, orchestratorCostShare: null, subagents: null };
    const batch = batches.get(cell.batchId);
    const analyses = cell.runIds.flatMap((id) => (batch === undefined ? [] : latestAnalyses(batch).get(id) ?? []));
    const input = (tokens: { readonly newInput: number; readonly cachedInput: number; readonly cacheWrite: number }) => tokens.newInput + tokens.cachedInput + tokens.cacheWrite;
    return {
      orchestratorShare: mean(analyses.map((analysis) => {
        const main = analysis.threads.find((thread) => thread.role === MainThread), all = analysis.threads.reduce((sum, thread) => sum + input(thread.tokens), 0);
        return main === undefined || all === 0 ? null : input(main.tokens) / all;
      })),
      orchestratorCostShare: mean((batch?.runs ?? []).filter((run) => cell.runIds.includes(run.id)).map(orchestratorCostShare)),
      subagents: mean(analyses.map((analysis) => analysis.threads.filter((thread) => thread.role !== MainThread).length)),
    };
  };
  const perTask = (cost: number | null, tasks: number) => cost === null ? null : cost / tasks;
  const whole = cells.filter((cell) => cell.workMode === WorkModeIds.wholePacket || cell.workMode === WorkModeIds.orchestrated).map((cell): PacketRow => {
    const tasks = (isCaseId(cell.caseId) ? PacketTasks[cell.caseId] : undefined) ?? 1;
    return { ...identity(cell.setup), packet: cell.caseId, workMode: cell.workMode, modelLabel: cell.modelLabel, steps: 1, tasks, tokens: cell.tokens.mean,
      cost: cell.cost.mean, costPerTask: perTask(cell.cost.mean, tasks), coverage: cell.coverage.mean, ...threads(cell) };
  });
  const groups = new Map<string, Cell[]>();
  for (const cell of cells) {
    if (cell.workMode !== WorkModeIds.taskByTask || cell.chain === null) continue;
    const key = JSON.stringify([cell.chain.id, cell.setup, cell.model]);
    groups.set(key, [...groups.get(key) ?? [], cell]);
  }
  const stepped = [...groups.values()].flatMap((group): PacketRow[] => {
    const [first] = group;
    if (first?.chain == null) return [];
    const total = (values: readonly (number | null)[]) => values.some((value) => value === null) ? null : values.reduce<number>((sum, value) => sum + (value ?? 0), 0);
    const cost = total(group.map((cell) => cell.cost.mean));
    return [{ ...identity(first.setup), packet: first.chain.id, workMode: WorkModeIds.taskByTask, modelLabel: first.modelLabel, steps: group.length, tasks: group.length,
      tokens: total(group.map((cell) => cell.tokens.mean)), cost, costPerTask: perTask(cost, group.length),
      coverage: mean(group.map((cell) => cell.coverage.mean)), orchestratorShare: null, orchestratorCostShare: null, subagents: null }];
  });
  return [...whole, ...stepped].sort((a, b) => a.packet.localeCompare(b.packet) || bySetup(a, b) || ModeOrder.indexOf(a.workMode) - ModeOrder.indexOf(b.workMode) || a.modelLabel.localeCompare(b.modelLabel));
}
