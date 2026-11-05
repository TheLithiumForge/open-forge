import { measure, type Cell, type StartingPointId } from "./cells.ts";
import { WorkModeNames } from "./names.ts";

/** A filter value that keeps every model, judge or starting point, so the views show averages. */
export const All = "all";

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
  readonly workMode: string;
}

/** One setup's comparison with the control, averaged over the starting points, cases and models that pass the filters. */
export interface SetupSummary {
  readonly setup: string;
  /** Starting point, case and model combinations that have both this setup and its control. Each counts once. */
  readonly groups: number;
  /** How many of those comparisons each figure rests on. */
  readonly coverageGroups: number;
  readonly judgeGroups: number;
  readonly tokenGroups: number;
  readonly runs: number;
  readonly coverageDelta: number | null;
  readonly judgeDelta: number | null;
  /** Geometric mean of the token ratios, so 20% more and 20% less don't cancel to a false zero. */
  readonly tokenRatio: number | null;
  /** The same for new input and output, the token classes a setup changes most directly. */
  readonly newInputRatio: number | null;
  readonly outputRatio: number | null;
  readonly coverage: number | null;
  readonly judgeScore: number | null;
  readonly tokens: number | null;
}

export interface FilterOption {
  readonly key: string;
  readonly label: string;
}

const ratio = (value: number | null, control: number | null) => value === null || control === null || control === 0 ? null : value / control;
const delta = (value: number | null, control: number | null) => value === null || control === null ? null : value - control;

/** Each cell beside the control cell of the same batch, model and judge, or the newest one from a batch with the same inputs. */
export function comparisons(all: readonly Cell[]): Comparison[] {
  return all.map((cell) => {
    const candidates = all.filter((other) => other.setup === cell.control && other.model === cell.model && other.judge === cell.judge);
    const controlCell = candidates.find((other) => other.batchId === cell.batchId)
      ?? candidates.filter((other) => other.inputsScope === cell.inputsScope).sort((a, b) => b.createdAt.localeCompare(a.createdAt))[0];
    return { cell, controlCell,
      coverageDelta: delta(cell.coverage.mean, controlCell?.coverage.mean ?? null), judgeDelta: delta(cell.judgeScore.mean, controlCell?.judgeScore.mean ?? null),
      tokenRatio: ratio(cell.tokens.mean, controlCell?.tokens.mean ?? null), minuteRatio: ratio(cell.minutes.mean, controlCell?.minutes.mean ?? null) };
  });
}

export function filtered<T extends { readonly cell: Cell } | Cell>(items: readonly T[], filters: Filters): T[] {
  return items.filter((item) => {
    const cell: Cell = "cell" in item ? item.cell : item;
    return (filters.model === All || cell.model === filters.model) && (filters.judge === All || cell.judge === filters.judge)
      && (filters.startingPoint === All || cell.startingPoint === filters.startingPoint) && (filters.workMode === All || cell.workMode === filters.workMode);
  });
}

/** The mean of the known values. */
export const average = (values: readonly (number | null)[]) => measure(values).mean;
/** The geometric mean of the known positive values, for ratios. */
export const geometric = (values: readonly (number | null)[]) => {
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
      const key = JSON.stringify([item.cell.caseId, item.cell.model]);
      groups.set(key, [...groups.get(key) ?? [], item]);
    }
    const perGroup = [...groups.values()].map((group) => ({
      runs: group[0]?.cell.runs ?? 0,
      coverageDelta: average(group.map((item) => item.coverageDelta)), judgeDelta: average(group.map((item) => item.judgeDelta)),
      tokenRatio: geometric(group.map((item) => item.tokenRatio)),
      newInputRatio: geometric(group.map((item) => ratio(item.cell.newInput.mean, item.controlCell?.newInput.mean ?? null))),
      outputRatio: geometric(group.map((item) => ratio(item.cell.outputTokens.mean, item.controlCell?.outputTokens.mean ?? null))),
      coverage: average(group.map((item) => item.cell.coverage.mean)), judgeScore: average(group.map((item) => item.cell.judgeScore.mean)),
      tokens: average(group.map((item) => item.cell.tokens.mean)),
    }));
    const compared = perGroup.filter((group) => group.coverageDelta !== null || group.tokenRatio !== null || group.judgeDelta !== null);
    const known = (read: (group: (typeof compared)[number]) => number | null) => compared.filter((group) => read(group) !== null).length;
    return { setup, groups: compared.length, coverageGroups: known((group) => group.coverageDelta), judgeGroups: known((group) => group.judgeDelta), tokenGroups: known((group) => group.tokenRatio),
      runs: compared.reduce((sum, group) => sum + group.runs, 0),
      coverageDelta: average(compared.map((group) => group.coverageDelta)), judgeDelta: average(compared.map((group) => group.judgeDelta)),
      tokenRatio: geometric(compared.map((group) => group.tokenRatio)),
      newInputRatio: geometric(compared.map((group) => group.newInputRatio)), outputRatio: geometric(compared.map((group) => group.outputRatio)),
      coverage: average(perGroup.map((group) => group.coverage)), judgeScore: average(perGroup.map((group) => group.judgeScore)), tokens: average(perGroup.map((group) => group.tokens)) };
  });
}

/** Unique options by key. A label from a cell with runs wins, since only those know the model the provider reported. */
export function filterOptions(entries: readonly { readonly key: string | null; readonly label: string | null; readonly runs: number }[]): FilterOption[] {
  const chosen = new Map<string, { readonly label: string; readonly runs: number }>();
  for (const { key, label, runs } of entries) {
    if (key === null || label === null) continue;
    const current = chosen.get(key);
    if (current === undefined || (current.runs === 0 && runs > 0)) chosen.set(key, { label, runs });
  }
  return [...chosen].map(([key, { label }]) => ({ key, label }));
}

/** The models, judges and work modes present, for the filter menus. */
export function options(all: readonly Cell[]): { readonly models: readonly FilterOption[]; readonly judges: readonly FilterOption[]; readonly workModes: readonly FilterOption[] } {
  const present = new Set(all.map((cell) => cell.workMode));
  return {
    models: filterOptions(all.map((cell) => ({ key: cell.model, label: cell.modelLabel, runs: cell.runs }))),
    judges: filterOptions(all.map((cell) => ({ key: cell.judge, label: cell.judgeLabel, runs: cell.runs }))),
    workModes: Object.entries(WorkModeNames).filter(([key]) => present.has(key as Cell["workMode"])).map(([key, label]) => ({ key, label })),
  };
}
