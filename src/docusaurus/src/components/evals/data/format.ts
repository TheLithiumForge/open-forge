import type { Interval } from "./analysis.ts";
export const percent = (value: number | null): string => value === null ? "Missing" : `${(value * 100).toFixed(1)}%`;
export const number = (value: number | null): string => value === null ? "Missing" : value.toLocaleString("en", { maximumFractionDigits: 1 });
export function intervalText(value: Pick<Interval, "low" | "high">, share = true): string | null {
  const format = share ? percent : number;
  return value.low === null || value.high === null ? null : `${format(value.low)} to ${format(value.high)}`;
}
export function metricTitle(value: Interval, share = true): string {
  const format = share ? percent : number;
  const range = intervalText(value, share);
  return `${format(value.mean)}. ${range === null ? "Interval unavailable" : `95% interval ${range}`}. ${value.samples} measured${value.missing === 0 ? "" : `, ${value.missing} missing`}.`;
}
