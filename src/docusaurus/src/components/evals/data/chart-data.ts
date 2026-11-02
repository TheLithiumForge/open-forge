import type { SetupSummary } from "./aggregate.ts";
import { OpenForgeSetups, type Cell } from "./cells.ts";
import type { MeasureSpec } from "./measures.ts";
import { setupName, setupRank } from "./names.ts";

/** One setup on a chart. Open Forge setups carry the accent, the others stay gray. */
export interface ChartRow {
  readonly setup: string;
  readonly name: string;
  readonly openForge: boolean;
  readonly value: number | null;
}

export interface SpreadRow extends Omit<ChartRow, "value"> {
  /** Every run's value. Judge scores hold one value per run and judge. */
  readonly values: readonly number[];
  readonly mean: number | null;
}

const OpenForge: readonly string[] = OpenForgeSetups;
export const bySetup = <T extends { readonly setup: string }>(a: T, b: T) => setupRank(a.setup) - setupRank(b.setup);
/** A setup's id, reader name and whether it installs Open Forge. */
export const identity = (setup: string) => ({ setup, name: setupName(setup), openForge: OpenForge.includes(setup) });

/** Setups that only ever appear as a control. Their change is zero by definition, so charts draw them as the zero line. */
export function controlSetups(all: readonly Cell[]): Set<string> {
  const compared = new Set(all.filter((cell) => cell.setup !== cell.control).map((cell) => cell.setup));
  return new Set(all.filter((cell) => cell.setup === cell.control && !compared.has(cell.setup)).map((cell) => cell.setup));
}

/** Each compared setup's value from its summary, in reading order. */
export function summaryRows(summaries: readonly SetupSummary[], controls: ReadonlySet<string>, pick: (summary: SetupSummary) => number | null): ChartRow[] {
  return summaries.filter((summary) => !controls.has(summary.setup)).map((summary) => ({ ...identity(summary.setup), value: pick(summary) })).sort(bySetup);
}

/** Every run's value of one measure by setup, for one model and case. Each judge adds its own judge scores. */
export function spreadRows(caseCells: readonly Cell[], spec: MeasureSpec): SpreadRow[] {
  const setups = [...new Set(caseCells.map((cell) => cell.setup))];
  return setups.map((setup) => {
    const matching = caseCells.filter((cell) => cell.setup === setup);
    const used = spec.perJudge ? matching : matching.slice(0, 1);
    const values = used.flatMap((cell) => spec.value(cell).values);
    return { ...identity(setup), values, mean: values.length === 0 ? null : values.reduce((sum, value) => sum + value, 0) / values.length };
  }).sort(bySetup);
}

/** A setup's mean tokens per run by class. The classes add up to the total. */
export interface TokenClassRow extends Omit<ChartRow, "value"> {
  readonly newInput: number | null;
  readonly output: number | null;
  readonly cacheWrites: number | null;
  readonly cachedInput: number | null;
}

/** Each setup's token classes, averaged over its cases and models so each counts once whatever its number of judges. */
export function tokenClassRows(all: readonly Cell[]): TokenClassRow[] {
  const setups = [...new Set(all.map((cell) => cell.setup))];
  return setups.map((setup) => {
    const groups = [...new Map(all.filter((cell) => cell.setup === setup).map((cell) => [JSON.stringify([cell.batchId, cell.model]), cell])).values()];
    const mean = (pick: (cell: Cell) => number | null) => {
      const values = groups.map(pick).filter((value): value is number => value !== null);
      return values.length === 0 ? null : values.reduce((sum, value) => sum + value, 0) / values.length;
    };
    return { ...identity(setup), newInput: mean((cell) => cell.newInput.mean), output: mean((cell) => cell.outputTokens.mean),
      cacheWrites: mean((cell) => cell.cacheWrites.mean), cachedInput: mean((cell) => cell.cachedInput.mean) };
  }).sort(bySetup);
}

/** The largest absolute value, rounded up to a step, so a chart centred on zero has room on both sides. */
export function symmetricLimit(values: readonly (number | null)[], step: number): number {
  const largest = Math.max(0, ...values.filter((value): value is number => value !== null).map(Math.abs));
  return Math.max(step, Math.ceil(largest / step) * step);
}
