import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis, type TooltipContentProps } from "recharts";
import type { TokenClassRow } from "../data/chart-data.ts";
import { compact } from "../data/format.ts";
import { Term } from "../info/Term.tsx";
import { AxisBand, ChartFigure, RowHeight } from "./ChartFigure.tsx";
import { ChartStyle, TooltipCard } from "./marks.tsx";
import { TokenNote } from "../info/notes.ts";

/** The token classes from the baseline out: the small classes a setup changes first, then the reused context. */
const Classes = [
  { key: "newInput", title: "New input", color: "var(--eval-token-new)" },
  { key: "output", title: "Output", color: "var(--eval-token-output)" },
  { key: "cacheWrites", title: "Cache writes", color: "var(--eval-token-writes)" },
  { key: "cachedInput", title: "Cached input", color: "var(--eval-token-cached)" },
] as const satisfies readonly { readonly key: keyof TokenClassRow; readonly title: string; readonly color: string }[];

const NameAxis = "name";
const NameWidth = 118;
const TotalWidth = 52;
const total = (row: TokenClassRow) => Classes.every((item) => row[item.key] === null) ? null : Classes.reduce((sum, item) => sum + (row[item.key] ?? 0), 0);

/** Mean tokens per run by class for every setup, the control included. Most of a total is cached input, since every request sends the whole context again. */
export function TokenClassChart({ rows }: { readonly rows: readonly TokenClassRow[] }) {
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
        tickFormatter={(name: unknown) => { const row = byName.get(String(name)); return row === undefined ? "" : compact(total(row)); }} tick={{ fontSize: ChartStyle.fontSize, fill: ChartStyle.text }} />
      <Tooltip cursor={{ fill: ChartStyle.hover }} content={tooltip} isAnimationActive={false} />
      {Classes.map((item) => <Bar key={item.key} yAxisId={NameAxis} dataKey={item.key} stackId="tokens" barSize={16} fill={item.color} stroke={ChartStyle.surface} strokeWidth={1} isAnimationActive={false} />)}
    </BarChart>
  </ResponsiveContainer>;
  return <ChartFigure height={rows.length * RowHeight + AxisBand} plot={plot}
    title={<><Term id="tokens" /> by class</>}
    description={<>Mean tokens per run, with the total on the right. Compare setups within one starting point, since starting points differ in size. {TokenNote}</>}
    data={<table>
      <thead><tr><th scope="col">Setup</th>{Classes.map((item) => <th key={item.key} scope="col">{item.title}</th>)}<th scope="col">Total</th></tr></thead>
      <tbody>{rows.map((row) => <tr key={row.setup}>
        <th scope="row"><Term id={row.setup}>{row.name}</Term></th>{Classes.map((item) => <td key={item.key}>{compact(row[item.key])}</td>)}<td>{compact(total(row))}</td>
      </tr>)}</tbody>
    </table>} legend={<TokenClassLegend />} />;
}

/** The class colors. */
function TokenClassLegend() {
  return <ul className="eval-legend" aria-label="Token classes">
    {Classes.map((item) => <li key={item.key}><span className="eval-swatch" style={{ background: item.color }} aria-hidden="true" />{item.title}</li>)}
  </ul>;
}
