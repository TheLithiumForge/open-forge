import type { Batch } from "../records/records.types.ts";
import type { Comparison } from "./aggregate.ts";
import { OneAgent, type Cell } from "./cells.ts";
import { bySetup, identity, type ChartRow } from "./chart-data.ts";
import type { ThreadRoles } from "../records/schema-values.ts";
import { latestAnalyses } from "./objective.ts";

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

/** The plan names an orchestrated setup after its one-agent twin, such as plain-docs-orchestrated. */
export const OrchestratedSuffix = "-orchestrated";

/** An orchestrated setup's system tokens against its one-agent twin in the same batch, with the orchestrator's share. */
export interface OrchestrationRow extends Omit<ChartRow, "value"> {
  readonly caseId: string;
  readonly tokens: number | null;
  readonly oneAgentTokens: number | null;
  /** The orchestrator thread's share of the system's input. */
  readonly orchestratorShare: number | null;
  readonly subagents: number | null;
  readonly coverage: number | null;
  readonly oneAgentCoverage: number | null;
}

export function orchestrationRows(items: readonly Comparison[], batches: ReadonlyMap<string, Batch>): OrchestrationRow[] {
  const cells = unique(items);
  return cells.filter((cell) => cell.workMode !== OneAgent).map((cell) => {
    const twin = cells.find((other) => other.batchId === cell.batchId && other.model === cell.model && other.setup === cell.setup.replace(OrchestratedSuffix, "") && other.workMode === OneAgent);
    const batch = batches.get(cell.batchId);
    const analyses = cell.runIds.flatMap((id) => (batch === undefined ? [] : latestAnalyses(batch).get(id) ?? []));
    const input = (tokens: { readonly newInput: number; readonly cachedInput: number; readonly cacheWrite: number }) => tokens.newInput + tokens.cachedInput + tokens.cacheWrite;
    const share = mean(analyses.map((analysis) => {
      const main = analysis.threads.find((thread) => thread.role === MainThread), all = analysis.threads.reduce((sum, thread) => sum + input(thread.tokens), 0);
      return main === undefined || all === 0 ? null : input(main.tokens) / all;
    }));
    return { ...identity(cell.setup), caseId: cell.caseId, tokens: cell.tokens.mean, oneAgentTokens: twin?.tokens.mean ?? null, orchestratorShare: share,
      subagents: mean(analyses.map((analysis) => analysis.threads.filter((thread) => thread.role !== MainThread).length)), coverage: cell.coverage.mean, oneAgentCoverage: twin?.coverage.mean ?? null };
  }).sort(bySetup);
}
