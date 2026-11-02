import { useMemo, useState } from "react";
import { CartesianGrid, ReferenceLine, ResponsiveContainer, Scatter, ScatterChart, Tooltip, XAxis, YAxis, ZAxis, type ScatterShapeProps, type TooltipContentProps } from "recharts";
import type { Comparison } from "../data/aggregate.ts";
import { niceAxis, PlotDefaults, plotMeasure, PlotMode, plotText, plotTick } from "../data/plot.ts";
import { runPoints, setupPoints, type Channels, type PlotPoint } from "../data/plot-points.ts";
import { ChartFigure } from "./ChartFigure.tsx";
import { ChartStyle, TooltipCard } from "./marks.tsx";
import { ColorBy, markStyle, PlotLegend, PlotMark } from "./plot-marks.tsx";
import { NoSize, PlotControls, type PlotChoice } from "./PlotControls.tsx";
import { PlotTable } from "./PlotTable.tsx";

const Height = 380;
const Dot = 6;
/** Bubble areas in square pixels, from the smallest value to the largest. */
const SizeRange = [50, 700] as const;
const LabelGap = 6;
/** Past this many points, names would crowd each other, so the open table below names them instead. */
const MostLabels = 8;
/** A setup's label goes under its mark when an earlier mark sits this close, as a share of each axis. */
const Near = { x: 0.25, y: 0.12 } as const;

const Defaults: PlotChoice = { x: PlotDefaults.x, y: PlotDefaults.y, size: PlotDefaults.size, colorBy: ColorBy.openForge, runs: false };

function pointKey(value: unknown): string | null {
  return typeof value === "object" && value !== null && "key" in value && typeof value.key === "string" ? value.key : null;
}

/** The reader picks a measure for each channel. Each point is a setup at the filtered level, or one run when one model and starting point are kept. */
export function SuperChart({ items, canShowRuns }: { readonly items: readonly Comparison[]; readonly canShowRuns: boolean }) {
  const [choice, setChoice] = useState<PlotChoice>(Defaults);
  const channels: Channels = { x: plotMeasure(choice.x), y: plotMeasure(choice.y), size: choice.size === NoSize ? null : plotMeasure(choice.size) };
  const runs = canShowRuns && choice.runs;
  const points = useMemo(() => runs ? runPoints(items, channels) : setupPoints(items, channels, choice.colorBy === ColorBy.startingPoint), [items, choice, runs]);
  const xAxis = niceAxis(points.map((point) => point.x), channels.x.mode === PlotMode.change);
  const yAxis = niceAxis(points.map((point) => point.y), channels.y.mode === PlotMode.change);
  const span = { x: xAxis.domain[1] - xAxis.domain[0], y: yAxis.domain[1] - yAxis.domain[0] };
  const below = points.map((point, index) => points.slice(0, index).some((other) => Math.abs(other.x - point.x) < span.x * Near.x && Math.abs(other.y - point.y) < span.y * Near.y));
  const byKey = new Map(points.map((point) => [point.key, point]));
  const labelled = !runs && points.length <= MostLabels;
  const startingPoints = [...new Set(items.map((item) => item.cell.startingPoint))];
  const tooltip = ({ active, payload }: TooltipContentProps) => {
    const point = byKey.get(pointKey(payload[0]?.payload) ?? "");
    return active !== true || point === undefined ? null : <TooltipCard title={point.label} lines={[
      `${channels.x.title}: ${plotText(channels.x, point.raw.x)}`, `${channels.y.title}: ${plotText(channels.y, point.raw.y)}`,
      ...channels.size === null ? [] : [`${channels.size.title}: ${plotText(channels.size, point.raw.size)}`]]} />;
  };
  const shape = (props: ScatterShapeProps) => {
    const point = points[props.index];
    if (props.cx === undefined || props.cy === undefined || point === undefined) return <g />;
    const radius = channels.size === null ? Dot : Math.max(4, props.width / 2);
    return <g>
      <PlotMark cx={props.cx} cy={props.cy} r={radius} style={markStyle(choice.colorBy, point.openForge, point.startingPoint)} />
      {!labelled ? null : <text className="eval-point-label" x={props.cx} y={below[props.index] === true ? props.cy + radius + LabelGap + ChartStyle.fontSize : props.cy - radius - LabelGap} textAnchor="middle"
        fontSize={ChartStyle.fontSize} fill={ChartStyle.text} stroke={ChartStyle.surface} strokeWidth={3} paintOrder="stroke">{point.name}</text>}
    </g>;
  };
  const tickStyle = { fontSize: ChartStyle.fontSize, fill: ChartStyle.axis };
  const plot = () => <ResponsiveContainer width="100%" height="100%">
    <ScatterChart margin={{ top: 24, right: 24, bottom: 8, left: 8 }} accessibilityLayer={false}>
      <CartesianGrid stroke={ChartStyle.grid} />
      <XAxis type="number" dataKey="x" domain={[...xAxis.domain]} ticks={[...xAxis.ticks]} tickFormatter={(value: number) => plotTick(channels.x, value)} stroke={ChartStyle.grid} tick={tickStyle} tickLine={false} />
      <YAxis type="number" dataKey="y" domain={[...yAxis.domain]} ticks={[...yAxis.ticks]} tickFormatter={(value: number) => plotTick(channels.y, value)} width={52} stroke={ChartStyle.grid} tick={tickStyle} tickLine={false} />
      <ZAxis type="number" dataKey="z" range={channels.size === null ? [Dot * Dot, Dot * Dot] : [...SizeRange]} />
      {channels.x.mode === PlotMode.change ? <ReferenceLine x={0} stroke={ChartStyle.axis} /> : null}
      {channels.y.mode === PlotMode.change ? <ReferenceLine y={0} stroke={ChartStyle.axis} /> : null}
      <Tooltip cursor={false} content={tooltip} isAnimationActive={false} />
      <Scatter data={points.map((point) => ({ ...point }))} shape={shape} isAnimationActive={false} />
    </ScatterChart>
  </ResponsiveContainer>;
  return <ChartFigure wide dataOpen height={points.length === 0 ? 48 : Height} title="Compare any two measures" description={summary(points, channels, runs)}
  controls={<PlotControls choice={choice} onChange={setChoice} canShowRuns={canShowRuns} />} legend={<PlotLegend colorBy={choice.colorBy} startingPoints={startingPoints} />}
  plot={points.length === 0 ? () => <p className="eval-muted">No point has both measures in these filters.</p> : plot}
  data={<PlotTable points={points} channels={channels} />} />;
}

/** One sentence that says what the chart shows, for readers who can't see it. */
function summary(points: readonly PlotPoint[], channels: Channels, runs: boolean): string {
  const top = [...points].sort((a, b) => b.y - a.y)[0];
  const count = `${points.length} ${runs ? (points.length === 1 ? "run" : "runs") : (points.length === 1 ? "point" : "points")}`;
  return top === undefined ? `${count}.` : `${count}, ${channels.x.title.toLowerCase()} across and ${channels.y.title.toLowerCase()} up. The highest is ${top.label} at ${plotText(channels.y, top.raw.y)}.`;
}
