import type { ReactNode } from "react";
import { CartesianGrid, ResponsiveContainer, Scatter, ScatterChart, Tooltip, XAxis, YAxis, type ScatterShapeProps, type TooltipContentProps } from "recharts";
import type { SpreadRow } from "../data/chart-data.ts";
import { NoValue } from "../data/format.ts";
import { Term } from "../info/Term.tsx";
import { AxisBand, ChartFigure, RowHeight } from "./ChartFigure.tsx";
import { ChartStyle, markFill, setupOf, TooltipCard } from "./marks.tsx";

interface Point { readonly setup: string; readonly x: number; readonly y: number; readonly openForge: boolean }

/** Runs with equal values would hide each other, so each row spreads its dots a little up and down. */
const Jitter = 0.36;
const NameWidth = 118;
const Dot = 4;
const MeanHalf = 9;

function xOf(value: unknown): number | null {
  return typeof value === "object" && value !== null && "x" in value && typeof value.x === "number" ? value.x : null;
}

/** Every run's value by setup, with each setup's mean as a short line. Shown when the filters keep one model and one starting point. */
export function SpreadChart({ title, description, rows, format, domain }: {
  readonly title: ReactNode;
  readonly description: ReactNode;
  readonly rows: readonly SpreadRow[];
  readonly format: (value: number | null) => string;
  readonly domain: readonly [number, number | "auto"];
}) {
  const runs: Point[] = rows.flatMap((row, index) => row.values.map((value, run) => ({
    setup: row.setup, openForge: row.openForge, x: value, y: index + (row.values.length === 1 ? 0 : (run / (row.values.length - 1) - 0.5) * Jitter),
  })));
  const means: Point[] = rows.flatMap((row, index) => row.mean === null ? [] : [{ setup: row.setup, openForge: row.openForge, x: row.mean, y: index }]);
  const names = new Map(rows.map((row) => [row.setup, row.name]));
  const tooltip = ({ active, payload }: TooltipContentProps) => {
    const entry: unknown = payload[0]?.payload;
    const setup = setupOf(entry), value = xOf(entry);
    return active !== true || setup === null ? null : <TooltipCard title={names.get(setup) ?? setup} lines={[format(value)]} />;
  };
  const dot = (props: ScatterShapeProps) => {
    const point = runs[props.index];
    return props.cx === undefined || props.cy === undefined || point === undefined ? <g />
      : <circle cx={props.cx} cy={props.cy} r={Dot} fill={markFill(point.openForge)} fillOpacity={0.75} stroke={ChartStyle.surface} strokeWidth={1.5} />;
  };
  const mean = (props: ScatterShapeProps) => props.cx === undefined || props.cy === undefined ? <g />
    : <line x1={props.cx} x2={props.cx} y1={props.cy - MeanHalf} y2={props.cy + MeanHalf} stroke={ChartStyle.text} strokeWidth={2} strokeLinecap="round" />;
  const plot = () => <ResponsiveContainer width="100%" height="100%">
    <ScatterChart margin={{ top: 4, right: 16, bottom: 0, left: 0 }} accessibilityLayer={false}>
      <CartesianGrid horizontal={false} stroke={ChartStyle.grid} />
      <XAxis type="number" dataKey="x" domain={[...domain]} tickFormatter={(value: number) => format(value)} tickCount={5} stroke={ChartStyle.grid}
        tick={{ fontSize: ChartStyle.fontSize, fill: ChartStyle.axis }} tickLine={false} />
      <YAxis type="number" dataKey="y" domain={[-0.5, rows.length - 0.5]} ticks={rows.map((_, index) => index)} reversed width={NameWidth}
        tickFormatter={(index: number) => rows[index]?.name ?? ""} axisLine={false} tickLine={false} tick={{ fontSize: ChartStyle.fontSize + 1, fill: ChartStyle.text }} />
      <Tooltip cursor={false} content={tooltip} isAnimationActive={false} />
      <Scatter data={runs} shape={dot} isAnimationActive={false} />
      <Scatter data={means} shape={mean} isAnimationActive={false} />
    </ScatterChart>
  </ResponsiveContainer>;
  return <ChartFigure title={title} description={description} height={rows.length * RowHeight + AxisBand} plot={plot}
    data={<table>
      <thead><tr><th scope="col">Setup</th><th scope="col">Runs</th><th scope="col">Mean</th></tr></thead>
      <tbody>{rows.map((row) => <tr key={row.setup}>
        <th scope="row"><Term id={row.setup}>{row.name}</Term></th>
        <td>{row.values.length === 0 ? NoValue : row.values.map((value) => format(value)).join(", ")}</td><td>{format(row.mean)}</td>
      </tr>)}</tbody>
    </table>} />;
}
