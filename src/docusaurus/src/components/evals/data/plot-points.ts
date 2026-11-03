import type { Comparison } from "./aggregate.ts";
import type { StartingPointId } from "./cells.ts";
import { caseLabel, startingPointTitle } from "./cell-labels.ts";
import { bySetup, controlSetups, identity } from "./chart-data.ts";
import { aggregateMeasure, plotted, PlotMode, runMeasure, type PlotMeasure } from "./plot.ts";

/** The measures on each channel. Without a size measure every point has the same size. */
export interface Channels {
  readonly x: PlotMeasure;
  readonly y: PlotMeasure;
  readonly size: PlotMeasure | null;
}

export interface Raw {
  readonly x: number | null;
  readonly y: number | null;
  readonly size: number | null;
}

/** One plotted point: a setup, a setup on one starting point, or one run. */
export interface PlotPoint {
  readonly key: string;
  readonly setup: string;
  readonly name: string;
  readonly openForge: boolean;
  readonly startingPoint: StartingPointId | null;
  /** What the point stands for, in reader terms. */
  readonly label: string;
  readonly raw: Raw;
  readonly x: number;
  readonly y: number;
  readonly z: number | null;
}

type Base = Omit<PlotPoint, "raw" | "x" | "y" | "z">;

function point(base: Base, channels: Channels, raw: Raw): PlotPoint[] {
  const x = plotted(channels.x, raw.x), y = plotted(channels.y, raw.y);
  return x === null || y === null ? [] : [{ ...base, raw, x, y, z: channels.size === null ? null : plotted(channels.size, raw.size) }];
}

function groupBy<T>(items: readonly T[], key: (item: T) => string): Map<string, T[]> {
  const groups = new Map<string, T[]>();
  for (const item of items) groups.set(key(item), [...groups.get(key(item)) ?? [], item]);
  return groups;
}

const usesChange = ({ x, y }: Channels) => x.mode === PlotMode.change || y.mode === PlotMode.change;

/** One point per setup, or per setup and starting point. A change channel leaves the control out, since it sits at zero by definition. */
export function setupPoints(items: readonly Comparison[], channels: Channels, byStartingPoint: boolean): PlotPoint[] {
  const controls = usesChange(channels) ? controlSetups(items.map((item) => item.cell)) : new Set<string>();
  const sets = groupBy(items.filter((item) => !controls.has(item.cell.setup)), (item) => JSON.stringify([item.cell.setup, byStartingPoint ? item.cell.startingPoint : null]));
  return [...sets].flatMap(([key, members]) => {
    const first = members[0];
    if (first === undefined) return [];
    const groups = [...groupBy(members, (item) => JSON.stringify([item.cell.caseId, item.cell.model])).values()];
    const read = (measure: PlotMeasure | null) => measure === null ? null : aggregateMeasure(measure, groups);
    const own = identity(first.cell.setup), startingPoint = byStartingPoint ? first.cell.startingPoint : null;
    const label = startingPoint === null ? own.name : `${own.name}, ${startingPointTitle(startingPoint).toLowerCase()}`;
    return point({ key, ...own, startingPoint, label }, channels, { x: read(channels.x), y: read(channels.y), size: read(channels.size) });
  }).sort(bySetup);
}

/** Every run of the filtered rows. A judge score channel gives one point per run and judge, since each judge scores the run apart. */
export function runPoints(items: readonly Comparison[], channels: Channels): PlotPoint[] {
  const perJudge = [channels.x, channels.y, channels.size].some((measure) => measure?.spec?.perJudge === true);
  const used = perJudge ? items : [...groupBy(items, (item) => JSON.stringify([item.cell.batchId, item.cell.model, item.cell.setup])).values()].flatMap((group) => group.slice(0, 1));
  const cases = new Set(items.map((item) => item.cell.caseId)).size;
  return used.flatMap((item) => item.cell.runValues.flatMap((values, index) => {
    const read = (measure: PlotMeasure | null) => measure === null ? null : runMeasure(measure, item, index);
    const own = identity(item.cell.setup);
    const parts = [`${own.name}, run ${values.repeat}`, cases > 1 ? caseLabel(item.cell) : null, perJudge && item.cell.judgeLabel !== null ? `judged by ${item.cell.judgeLabel}` : null];
    return point({ key: JSON.stringify([item.cell.batchId, values.runId, item.cell.judge]), ...own, startingPoint: item.cell.startingPoint,
      label: parts.filter((part): part is string => part !== null).join(", ") }, channels, { x: read(channels.x), y: read(channels.y), size: read(channels.size) });
  })).sort(bySetup);
}
