import { average, geometric, type Comparison } from "./aggregate.ts";
import { compact, signed } from "./format.ts";
import { CellMeasures, ChangeKind, formatChange, measureChange, type MeasureSpec } from "./measures.ts";
import type { MeasureId } from "./names.ts";

/** A measure as each setup's own value, or as its change from the control. */
export const PlotMode = { value: "value", change: "change" } as const;
export type PlotMode = (typeof PlotMode)[keyof typeof PlotMode];

/** A measure a chart channel can show. The number of runs has no cell measure behind it. */
export interface PlotMeasure {
  readonly key: string;
  readonly title: string;
  readonly definition: MeasureId | null;
  readonly spec: MeasureSpec | null;
  readonly mode: PlotMode;
}

export const RunsKey = "runs";
const changeKey = (spec: MeasureSpec) => `${spec.id}-${PlotMode.change}`;

export const PlotMeasures: readonly PlotMeasure[] = [
  ...CellMeasures.flatMap((spec): PlotMeasure[] => [
    { key: changeKey(spec), title: `${spec.title} change`, definition: spec.definition, spec, mode: PlotMode.change },
    { key: `${spec.id}-${PlotMode.value}`, title: spec.title, definition: spec.definition, spec, mode: PlotMode.value },
  ]),
  { key: RunsKey, title: "Runs", definition: null, spec: null, mode: PlotMode.value },
];

const [Coverage, , , , Tokens] = CellMeasures;
/** Token change across, check coverage change up, and runs as the bubble size. */
export const PlotDefaults = { x: Tokens === undefined ? RunsKey : changeKey(Tokens), y: Coverage === undefined ? RunsKey : changeKey(Coverage), size: RunsKey } as const;

export function plotMeasure(key: string): PlotMeasure {
  return PlotMeasures.find((measure) => measure.key === key) ?? PlotMeasures[0] ?? { key: RunsKey, title: "Runs", definition: null, spec: null, mode: PlotMode.value };
}

/** A size channel needs amounts that can't go below zero, so it offers values and the run count only. */
export const SizeMeasures = PlotMeasures.filter((measure) => measure.mode === PlotMode.value);

/** Where a raw value sits on an axis: shares in percent, share changes in points, ratios as percent changes, amounts as they are. */
export function plotted(measure: PlotMeasure, raw: number | null): number | null {
  if (raw === null || measure.spec === null) return raw;
  if (measure.mode === PlotMode.value) return measure.spec.changeKind === ChangeKind.points ? raw * 100 : raw;
  return measure.spec.changeKind === ChangeKind.points ? raw * 100 : (raw - 1) * 100;
}

/** A raw value as text, the way the table shows it. */
export function plotText(measure: PlotMeasure, raw: number | null): string {
  if (measure.spec === null) return raw === null ? compact(null) : String(raw);
  return measure.mode === PlotMode.value ? measure.spec.format(raw) : formatChange(measure.spec, raw);
}

/** The axis label for a plotted number, in the same units. */
export function plotTick(measure: PlotMeasure, value: number): string {
  if (measure.spec === null) return String(value);
  if (measure.mode === PlotMode.change) return `${signed(value, 0)}${measure.spec.changeKind === ChangeKind.points ? "" : "%"}`;
  return measure.spec.changeKind === ChangeKind.points ? `${Math.round(value)}%` : compact(value);
}

/**
 * One measure for a group of table rows: each case and model counts once, and judge rows average first. Changes in points
 * average, ratios take the geometric mean, and runs add up.
 */
export function aggregateMeasure(measure: PlotMeasure, groups: readonly (readonly Comparison[])[]): number | null {
  const { spec } = measure;
  if (spec === null) return groups.reduce((sum, group) => sum + (group[0]?.cell.runs ?? 0), 0);
  if (measure.mode === PlotMode.value) return average(groups.map((group) => average(group.map((item) => spec.value(item.cell).mean))));
  const mean = spec.changeKind === ChangeKind.points ? average : geometric;
  return mean(groups.map((group) => mean(group.map((item) => measureChange(spec, item)))));
}

/** One run's measure: its own value, or its change from the mean of the control's runs. */
export function runMeasure(measure: PlotMeasure, item: Comparison, index: number): number | null {
  const { spec } = measure;
  const values = item.cell.runValues[index];
  if (spec === null || values === undefined) return spec === null ? 1 : null;
  const own = spec.run(values);
  if (measure.mode === PlotMode.value || own === null) return own;
  const control = item.controlCell === undefined ? null : spec.value(item.controlCell).mean;
  if (control === null) return null;
  if (spec.changeKind === ChangeKind.points) return own - control;
  return control === 0 ? null : own / control;
}

/** Room around the outermost points, as a share of the span, so bubbles and labels stay inside the plot. */
const Padding = 0.1;
const TickCount = 4;

function niceStep(rough: number): number {
  const power = 10 ** Math.floor(Math.log10(rough));
  return [1, 2, 5, 10].map((factor) => factor * power).find((step) => step >= rough) ?? 10 * power;
}

/** An axis range with round ticks around the values. A change axis always shows zero, where the control sits. */
export function niceAxis(values: readonly number[], includeZero: boolean): { readonly domain: readonly [number, number]; readonly ticks: readonly number[] } {
  const known = values.filter(Number.isFinite);
  let low = Math.min(...known, ...(includeZero || known.length === 0 ? [0] : []));
  let high = Math.max(...known, ...(includeZero || known.length === 0 ? [0] : []));
  if (low === high) { low -= 1; high += 1; }
  const pad = (high - low) * Padding;
  const floor = low >= 0 ? 0 : -Infinity;
  low = Math.max(floor, low - pad);
  high += pad;
  const step = niceStep((high - low) / TickCount);
  const start = Math.floor(low / step) * step, end = Math.ceil(high / step) * step;
  const ticks = Array.from({ length: Math.round((end - start) / step) + 1 }, (_, index) => Math.round((start + index * step) * 1e6) / 1e6);
  return { domain: [start, end], ticks };
}
