import { useState } from "react";
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis, type TooltipContentProps } from "recharts";
import type { TokenClassRow } from "../data/chart-data.ts";
import { compact } from "../data/format.ts";
import { Term } from "../info/Term.tsx";
import { AxisBand, ChartFigure, RowHeight } from "./ChartFigure.tsx";
import { ChartStyle, TooltipCard } from "./marks.tsx";
import { TokenNote } from "../info/notes.ts";

/** The token classes from the baseline out, in legend order: the small classes a setup changes first, then the reused context. */
const Classes = [
  { key: "newInput", title: "New input", color: "var(--eval-token-new)" },
  { key: "output", title: "Output", color: "var(--eval-token-output)" },
  { key: "cacheWrites", title: "Cache writes", color: "var(--eval-token-writes)" },
  { key: "cachedInput", title: "Cached input", color: "var(--eval-token-cached)" },
] as const satisfies readonly { readonly key: keyof TokenClassRow; readonly title: string; readonly color: string }[];

const CachedKey = "cachedInput";
const NameAxis = "name";
const NameWidth = 118;
const TotalWidth = 52;
type TokenClass = (typeof Classes)[number];
const total = (row: TokenClassRow, classes: readonly TokenClass[] = Classes) =>
  classes.every((item) => row[item.key] === null) ? null : classes.reduce((sum, item) => sum + (row[item.key] ?? 0), 0);

/** Mean tokens per run by class for every setup, the control included. Most of a total is cached input, since every request sends the whole context again. */
export function TokenClassChart({ rows }: { readonly rows: readonly TokenClassRow[] }) {
  // Cached input fills most of every bar, so a reader can leave it out to compare the classes a setup changes most.
  const [withCached, setWithCached] = useState(true);
  const shown = withCached ? Classes : Classes.filter((item) => item.key !== CachedKey);
  const byName = new Map(rows.map((row) => [row.name, row]));
  const tooltip = ({ active, label }: TooltipContentProps) => {
    const row = byName.get(String(label));
    return active !== true || row === undefined ? null
      : <TooltipCard title={row.name} lines={[`${compact(total(row))} tokens`, ...Classes.map((item) => `${item.title}: ${compact(row[item.key])}`)]} />;
  };
  const plot = () => <ResponsiveContainer width="100%" height="100%">
    <BarChart data={[...rows]} layout="vertical" margin={{ top: 4, right: 4, bottom: 0, left: 0 }} accessibilityLayer={false}>
      <CartesianGrid horizontal={false} stroke={ChartStyle.grid} />
      <XAxis type="number" tickFormatter={(value: number) => compact(value)} tick={{ fontSize: ChartStyle.fontSize, fill: ChartStyle.axis }} stroke={ChartStyle.grid} tickLine={false} />
      <YAxis yAxisId={NameAxis} type="category" dataKey="name" width={NameWidth} tickLine={false} axisLine={false} tick={{ fontSize: ChartStyle.fontSize + 1, fill: ChartStyle.text }} />
      <YAxis yAxisId="total" orientation="right" type="category" dataKey="name" width={TotalWidth} tickLine={false} axisLine={false}
        tickFormatter={(name: unknown) => { const row = byName.get(String(name)); return row === undefined ? "" : compact(total(row, shown)); }} tick={{ fontSize: ChartStyle.fontSize, fill: ChartStyle.text }} />
      <Tooltip cursor={{ fill: ChartStyle.hover }} content={tooltip} isAnimationActive={false} />
      {shown.map((item) => <Bar key={item.key} yAxisId={NameAxis} dataKey={item.key} stackId="tokens" barSize={16} fill={item.color} stroke={ChartStyle.surface} strokeWidth={1.5} isAnimationActive={false} />)}
    </BarChart>
  </ResponsiveContainer>;
  return <ChartFigure height={rows.length * RowHeight + AxisBand} plot={plot}
    title={<><Term id="tokens" /> by class</>}
    description={<>Mean tokens per run, with the total of the classes shown on the right. Compare setups within one starting point, since starting points differ in size. {TokenNote}</>}
    data={<table>
      <thead><tr><th scope="col">Scenario</th>{Classes.map((item) => <th key={item.key} scope="col">{item.title}</th>)}<th scope="col">Total</th></tr></thead>
      <tbody>{rows.map((row) => <tr key={row.setup}>
        <th scope="row"><Term id={row.setup}>{row.name}</Term></th>{Classes.map((item) => <td key={item.key}>{compact(row[item.key])}</td>)}<td>{compact(total(row))}</td>
      </tr>)}</tbody>
    </table>} controls={<div className="eval-segmented" role="group" aria-label="Token classes shown">
      <button type="button" aria-pressed={withCached} onClick={() => setWithCached(true)}>All classes</button>
      <button type="button" aria-pressed={!withCached} onClick={() => setWithCached(false)}>Without cached input</button>
    </div>} legend={<TokenClassLegend classes={shown} />} />;
}

/** The class colors. */
function TokenClassLegend({ classes }: { readonly classes: readonly TokenClass[] }) {
  return <ul className="eval-legend" aria-label="Token classes">
    {classes.map((item) => <li key={item.key}><span className="eval-swatch" style={{ background: item.color }} aria-hidden="true" />{item.title}</li>)}
  </ul>;
}
