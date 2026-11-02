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

/** Shown where a value has no measured runs yet. */
export const NoValue = "No data";
const Minus = "−";
const compactFormat = new Intl.NumberFormat("en", { notation: "compact", maximumFractionDigits: 1 });

/** Large counts such as tokens, as 1.9M. */
export const compact = (value: number | null): string => value === null ? NoValue : compactFormat.format(value);
/** A signed number with a real minus sign, and no sign at zero. */
export function signed(value: number, digits: number): string {
  const text = Math.abs(value).toFixed(digits);
  return Number(text) === 0 ? text : `${value > 0 ? "+" : Minus}${text}`;
}
/** A difference of two shares, in percentage points. */
export const points = (delta: number | null): string => delta === null ? NoValue : `${signed(delta * 100, 1)} points`;
/** A ratio to the control, as a signed percent change. */
export const change = (ratio: number | null): string => ratio === null ? NoValue : `${signed((ratio - 1) * 100, 0)}%`;
/** A token ratio to the control, in words. */
export function tokenChange(ratio: number | null): string {
  if (ratio === null) return NoValue;
  const shift = Math.round((ratio - 1) * 100);
  return shift === 0 ? "About the same" : `${Math.abs(shift)}% ${shift > 0 ? "more" : "fewer"}`;
}
