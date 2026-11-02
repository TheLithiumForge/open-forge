import type { ReactNode } from "react";
import { Bar, BarChart, CartesianGrid, ReferenceLine, ResponsiveContainer, Tooltip, XAxis, YAxis, type BarShapeProps, type TooltipContentProps } from "recharts";
import { NoValue } from "../data/format.ts";
import type { ChartRow } from "../data/chart-data.ts";
import { symmetricLimit } from "../data/chart-data.ts";
import { Term } from "../info/Term.tsx";
import { AxisBand, ChartFigure, RowHeight } from "./ChartFigure.tsx";
import { ChartStyle, DeltaBar, TooltipCard } from "./marks.tsx";

const NameAxis = "name";
const ValueAxis = "value";
const NameWidth = 118;
const ValueWidth = 70;

/**
 * Each setup's change from the control as a bar from the zero line. The control is the zero line, so it has no bar.
 * The values sit in a column on the right, so no label overlaps a bar.
 */
export function DeltaChart({ title, description, rows, format, tick, step }: {
  readonly title: ReactNode;
  readonly description: ReactNode;
  /** Values in display units: percentage points or percent. */
  readonly rows: readonly ChartRow[];
  readonly format: (value: number | null) => string;
  /** Short axis labels. */
  readonly tick: (value: number) => string;
  /** Axis ticks fall on multiples of this step. */
  readonly step: number;
}) {
  const limit = symmetricLimit(rows.map((row) => row.value), step);
  const data = rows.map((row) => ({ ...row, plotted: row.value ?? 0 }));
  const byName = new Map(rows.map((row) => [row.name, row]));
  const valueText = (name: unknown) => format(byName.get(String(name))?.value ?? null);
  const tooltip = ({ active, label }: TooltipContentProps) => {
    const row = byName.get(String(label));
    return active !== true || row === undefined ? null : <TooltipCard title={row.name} lines={[format(row.value)]} />;
  };
  const shape = (props: BarShapeProps) => {
    const row = rows[props.index];
    return row?.value == null ? <g /> : <DeltaBar x={props.x} y={props.y} width={props.width} height={props.height} positive={row.value >= 0} openForge={row.openForge} />;
  };
  const plot = () => <ResponsiveContainer width="100%" height="100%">
    <BarChart data={data} layout="vertical" margin={{ top: 4, right: 4, bottom: 0, left: 0 }} accessibilityLayer={false}>
      <CartesianGrid horizontal={false} stroke={ChartStyle.grid} />
      <XAxis type="number" dataKey="plotted" domain={[-limit, limit]} ticks={[-limit, -limit / 2, 0, limit / 2, limit]} tickFormatter={tick}
        tick={{ fontSize: ChartStyle.fontSize, fill: ChartStyle.axis }} stroke={ChartStyle.grid} tickLine={false} />
      <YAxis yAxisId={NameAxis} type="category" dataKey="name" width={NameWidth} tickLine={false} axisLine={false} tick={{ fontSize: ChartStyle.fontSize + 1, fill: ChartStyle.text }} />
      <YAxis yAxisId={ValueAxis} orientation="right" type="category" dataKey="name" width={ValueWidth} tickLine={false} axisLine={false}
        tickFormatter={valueText} tick={{ fontSize: ChartStyle.fontSize, fill: ChartStyle.text }} />
      <ReferenceLine x={0} yAxisId={NameAxis} stroke={ChartStyle.axis} />
      <Tooltip cursor={{ fill: ChartStyle.hover }} content={tooltip} isAnimationActive={false} />
      <Bar yAxisId={NameAxis} dataKey="plotted" barSize={16} shape={shape} isAnimationActive={false} />
    </BarChart>
  </ResponsiveContainer>;
  return <ChartFigure title={title} description={description} height={rows.length * RowHeight + AxisBand} plot={plot}
    data={<table>
      <thead><tr><th scope="col">Setup</th><th scope="col">Change from the control</th></tr></thead>
      <tbody>{rows.map((row) => <tr key={row.setup}><th scope="row"><Term id={row.setup}>{row.name}</Term></th><td>{row.value === null ? NoValue : format(row.value)}</td></tr>)}</tbody>
    </table>} />;
}
