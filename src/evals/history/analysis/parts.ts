import type { ContextPart } from "./analysis.types.ts";
import { partOf, type LedgerTotals } from "./ledger.ts";

/**
 * Whole numbers proportional to the values that sum exactly to the total, by the largest remainder. Values that are all
 * zero share the total equally, so an exact total is never lost.
 */
export function apportion(values: readonly number[], total: number): number[] {
  if (values.length === 0) return [];
  const sum = values.reduce((all, value) => all + Math.max(0, value), 0);
  const exact = values.map((value) => (sum === 0 ? total / values.length : (Math.max(0, value) * total) / sum));
  const whole = exact.map(Math.floor);
  let left = total - whole.reduce((all, value) => all + value, 0);
  const order = exact.map((value, index) => ({ index, rest: value - Math.floor(value) })).sort((a, b) => b.rest - a.rest || a.index - b.index);
  for (const { index } of order) {
    if (left <= 0) break;
    whole[index] = (whole[index] ?? 0) + 1;
    left -= 1;
  }
  return whole;
}

/** The exact totals the parts must add up to. Peak is null when the run's largest request is unknown. */
export interface PartTotals { readonly input: number; readonly fresh: number; readonly peak: number | null }

/** The ledger's estimates as whole tokens that sum to the exact totals, largest input first. */
export function contextParts(ledger: LedgerTotals, totals: PartTotals): ContextPart[] {
  const keys = [...new Set([...ledger.input.keys(), ...ledger.fresh.keys(), ...ledger.peak.keys()])];
  const values = (tokens: ReadonlyMap<string, number>) => keys.map((key) => tokens.get(key) ?? 0);
  const input = apportion(values(ledger.input), totals.input);
  const fresh = apportion(values(ledger.fresh), totals.fresh);
  const peakTotal = totals.peak ?? Math.round(values(ledger.peak).reduce((sum, value) => sum + value, 0));
  const peak = apportion(values(ledger.peak), peakTotal);
  return keys.map((key, index) => {
    const { type, fileType } = partOf(key);
    return { class: type, fileType, input: input[index] ?? 0, fresh: fresh[index] ?? 0, peak: peak[index] ?? 0 };
  }).filter((part) => part.input > 0 || part.fresh > 0 || part.peak > 0).sort((a, b) => b.input - a.input);
}
