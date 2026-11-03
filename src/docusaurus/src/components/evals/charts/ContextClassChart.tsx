import { useState } from "react";
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis, type TooltipContentProps } from "recharts";
import { ContextClasses, type ObjectiveRow } from "../data/objective.ts";
import { compact, NoValue, signedCompact } from "../data/format.ts";
import { Term } from "../info/Term.tsx";
import { AxisBand, ChartFigure, RowHeight } from "./ChartFigure.tsx";
import { ChartStyle, TooltipCard } from "./marks.tsx";

const UpfrontKey = "upfront";
const NameAxis = "name";
const NameWidth = 118;
const classColor = (key: string) => `var(--eval-class-${key})`;
type ContextClass = (typeof ContextClasses)[number];

/** A token difference from the control, such as +12K, or nothing for the control itself. */
export function tokenDelta(value: number | null | undefined, control: number | null | undefined): string {
  if (value == null || control == null) return "";
  return ` (${signedCompact(Math.round(value - control))})`;
}

/** Mean input tokens per run by context class for every setup. The up-front text fills most bars, so a reader can leave it out. */
export function ContextClassChart({ rows }: { readonly rows: readonly ObjectiveRow[] }) {
  const [withUpfront, setWithUpfront] = useState(true);
  const present = ContextClasses.filter((item) => rows.some((row) => (row.measures.classes[item.key] ?? 0) > 0));
  const shown = withUpfront ? present : present.filter((item) => item.key !== UpfrontKey);
  const data = rows.map((row) => ({ name: row.name, ...Object.fromEntries(present.map((item) => [item.key, row.measures.classes[item.key] ?? 0])) }));
  const byName = new Map(rows.map((row) => [row.name, row]));
  const tooltip = ({ active, label }: TooltipContentProps) => {
    const row = byName.get(String(label));
    return active !== true || row === undefined ? null
      : <TooltipCard title={row.name} lines={present.map((item) => `${item.title}: ${compact(row.measures.classes[item.key] ?? null)}`)} />;
  };
  const plot = () => <ResponsiveContainer width="100%" height="100%">
    <BarChart data={data} layout="vertical" margin={{ top: 4, right: 4, bottom: 0, left: 0 }} accessibilityLayer={false}>
      <CartesianGrid horizontal={false} stroke={ChartStyle.grid} />
      <XAxis type="number" tickFormatter={(value: number) => compact(value)} tick={{ fontSize: ChartStyle.fontSize, fill: ChartStyle.axis }} stroke={ChartStyle.grid} tickLine={false} />
      <YAxis yAxisId={NameAxis} type="category" dataKey="name" width={NameWidth} tickLine={false} axisLine={false} tick={{ fontSize: ChartStyle.fontSize + 1, fill: ChartStyle.text }} />
      <Tooltip cursor={{ fill: ChartStyle.hover }} content={tooltip} isAnimationActive={false} />
      {shown.map((item) => <Bar key={item.key} yAxisId={NameAxis} dataKey={item.key} stackId="classes" barSize={16} fill={classColor(item.key)} stroke={ChartStyle.surface} strokeWidth={1.5} isAnimationActive={false} />)}
    </BarChart>
  </ResponsiveContainer>;
  return <ChartFigure height={rows.length * RowHeight + AxisBand} plot={plot} wide
    title={<>Input <Term id="tokens">tokens</Term> by where they came from</>}
    description="Mean input tokens per run by class. Every request sends the whole context again, so a class costs its size times the requests that carry it. The split is estimated, and each run's classes add up to its exact input. In the data, each value shows its change from the control in brackets."
    data={<table>
      <thead><tr><th scope="col">Setup</th>{present.map((item) => <th key={item.key} scope="col">{item.title}</th>)}</tr></thead>
      <tbody>{rows.map((row) => <tr key={row.setup}>
        <th scope="row"><Term id={row.setup}>{row.name}</Term></th>
        {present.map((item) => <td key={item.key}>{cell(row, item)}</td>)}
      </tr>)}</tbody>
    </table>} controls={<div className="eval-segmented" role="group" aria-label="Classes shown">
      <button type="button" aria-pressed={withUpfront} onClick={() => setWithUpfront(true)}>All classes</button>
      <button type="button" aria-pressed={!withUpfront} onClick={() => setWithUpfront(false)}>Without the up-front text</button>
    </div>} legend={<ul className="eval-legend" aria-label="Context classes">
      {shown.map((item) => <li key={item.key}><span className="eval-swatch" style={{ background: classColor(item.key) }} aria-hidden="true" />{item.title}</li>)}
    </ul>} />;
}

function cell(row: ObjectiveRow, item: ContextClass): string {
  const value = row.measures.classes[item.key];
  return value == null ? NoValue : `${compact(value)}${tokenDelta(value, row.control?.classes[item.key])}`;
}
