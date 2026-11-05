import type { Comparison } from "./aggregate.ts";
import type { Cell, Measure } from "./cells.ts";
import { change, compact, dollars, NoValue, percent, points } from "./format.ts";
import type { MeasureId } from "./names.ts";
import type { RunValues } from "./run-measures.ts";

/** How a measure differs from its control: shares in percentage points, amounts as a ratio. */
export const ChangeKind = { points: "points", ratio: "ratio" } as const;
export type ChangeKind = (typeof ChangeKind)[keyof typeof ChangeKind];

/**
 * One measure of a cell, with its comparison to the control. The table, the CSV and the run charts list these in order,
 * so a new measure is one more entry here plus the value it reads from a cell.
 */
export interface MeasureSpec {
  readonly id: string;
  /** The definition snippet that explains the measure. */
  readonly definition: MeasureId;
  readonly title: string;
  readonly changeKind: ChangeKind;
  /** Time mostly reflects the provider's speed, so it compares setups of one model only. */
  readonly sameModelOnly: boolean;
  /** Each judge gives its own value. Other measures repeat the same runs on every judge's row. */
  readonly perJudge: boolean;
  /** Whether the results table shows it. The CSV holds every measure. */
  readonly inTable: boolean;
  /** Whether the run charts show it, when the filters keep one model and one starting point. */
  readonly inSpread: boolean;
  readonly value: (cell: Cell) => Measure;
  /** The same measure for one run. */
  readonly run: (values: RunValues) => number | null;
  readonly format: (value: number | null) => string;
  /** The value as a CSV number: percent for shares, the plain amount otherwise. */
  readonly csv: (value: number | null) => number | null;
}

const minutes = (value: number | null) => value === null ? NoValue : value.toFixed(1);
const share = (value: number | null) => value === null ? null : Math.round(value * 1000) / 10;
const amount = (value: number | null) => value === null ? null : Math.round(value * 10) / 10;
const money = (value: number | null) => value === null ? null : Math.round(value * 1000) / 1000;
const shareText = (value: number | null) => value === null ? NoValue : percent(value);
const base = { sameModelOnly: false, perJudge: false, inTable: true, inSpread: false } as const;
const tokenMeasure = { ...base, definition: "tokens", changeKind: ChangeKind.ratio, format: compact, csv: amount } as const;

export const CellMeasures: readonly MeasureSpec[] = [
  { ...base, id: "check-coverage", definition: "check-coverage", title: "Check coverage", changeKind: ChangeKind.points, inSpread: true, value: (cell) => cell.coverage, run: (values) => values.coverage, format: shareText, csv: share },
  { ...base, id: "judge-score", definition: "judge-score", title: "Judge score", changeKind: ChangeKind.points, perJudge: true, inSpread: true, value: (cell) => cell.judgeScore, run: (values) => values.judgeScore, format: shareText, csv: share },
  { ...base, id: "judge-quality", definition: "judge-quality", title: "Judge quality", changeKind: ChangeKind.points, perJudge: true, inSpread: true, value: (cell) => cell.judgeQuality, run: (values) => values.judgeQuality, format: shareText, csv: share },
  { ...base, id: "judge-adherence", definition: "judge-adherence", title: "Judge adherence", changeKind: ChangeKind.points, perJudge: true, inSpread: true, value: (cell) => cell.judgeAdherence, run: (values) => values.judgeAdherence, format: shareText, csv: share },
  { ...tokenMeasure, id: "tokens", title: "Tokens", inSpread: true, value: (cell) => cell.tokens, run: (values) => values.tokens },
  { ...base, id: "cost", definition: "cost", title: "Cost", changeKind: ChangeKind.ratio, inSpread: true, value: (cell) => cell.cost, run: (values) => values.cost, format: dollars, csv: money },
  { ...tokenMeasure, id: "new-input", title: "New input", value: (cell) => cell.newInput, run: (values) => values.newInput },
  { ...tokenMeasure, id: "cached-input", title: "Cached input", value: (cell) => cell.cachedInput, run: (values) => values.cachedInput },
  { ...tokenMeasure, id: "output", title: "Output", value: (cell) => cell.outputTokens, run: (values) => values.output },
  { ...base, id: "cache-hit-rate", definition: "tokens", title: "Cache hit rate", changeKind: ChangeKind.points, inTable: false, value: (cell) => cell.cacheHitRate, run: (values) => values.cacheHitRate, format: shareText, csv: share },
  { ...base, id: "time", definition: "time", title: "Minutes", changeKind: ChangeKind.ratio, sameModelOnly: true, inSpread: true, value: (cell) => cell.minutes, run: (values) => values.minutes, format: minutes, csv: amount },
];

/** The change from the control: a difference for shares, a ratio for amounts. Null without both values. */
export function measureChange(spec: MeasureSpec, item: Comparison): number | null {
  const value = spec.value(item.cell).mean, control = item.controlCell === undefined ? null : spec.value(item.controlCell).mean;
  if (value === null || control === null) return null;
  if (spec.changeKind === ChangeKind.points) return value - control;
  return control === 0 ? null : value / control;
}

export function formatChange(spec: MeasureSpec, value: number | null): string {
  return spec.changeKind === ChangeKind.points ? points(value) : change(value);
}

/** The change as a CSV number: points for shares, percent for ratios. */
export function csvChange(spec: MeasureSpec, value: number | null): number | null {
  if (value === null) return null;
  return Math.round((spec.changeKind === ChangeKind.points ? value * 100 : (value - 1) * 100) * 10) / 10;
}
