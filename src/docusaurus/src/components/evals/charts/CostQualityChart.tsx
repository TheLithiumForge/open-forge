import { CartesianGrid, ReferenceLine, ResponsiveContainer, Scatter, ScatterChart, Tooltip, XAxis, YAxis, type ScatterShapeProps, type TooltipContentProps } from "recharts";
import { NoValue, signed } from "../data/format.ts";
import { symmetricLimit } from "../data/chart-data.ts";
import { Term } from "../info/Term.tsx";
import { ChartFigure } from "./ChartFigure.tsx";
import { ChartStyle, markFill, setupOf, TooltipCard } from "./marks.tsx";

/** One setup's average token change in percent and its quality change in points. */
export interface CostPoint {
  readonly setup: string;
  readonly name: string;
  readonly openForge: boolean;
  readonly tokens: number;
  readonly quality: number;
}

const Height = 320;
const EmptyHeight = 48;
const Step = 10;
const Dot = 5;
const LabelGap = 10;
/** How close two dots are, as a share of each axis, before their labels would collide. */
const Near = { x: 0.5, y: 0.25 } as const;
const percent = (value: number) => `${signed(value, 0)}%`;
const points = (value: number) => signed(value, 0);

/** Each setup placed by what it costs in tokens and what it changes in check coverage, both against the control at the centre. */
export function CostQualityChart({ points: data }: { readonly points: readonly CostPoint[] }) {
  const x = symmetricLimit(data.map((point) => point.tokens), Step * 2);
  const y = symmetricLimit(data.map((point) => point.quality), Step);
  const bySetup = new Map(data.map((point) => [point.setup, point]));
  // A label goes under its dot when an earlier dot sits close by, so two nearby labels don't overlap.
  const below = data.map((point, index) => data.slice(0, index).some((other) =>
    Math.abs(other.tokens - point.tokens) < x * Near.x && Math.abs(other.quality - point.quality) < y * Near.y));
  const tooltip = ({ active, payload }: TooltipContentProps) => {
    const point = bySetup.get(setupOf(payload[0]?.payload) ?? "");
    return active !== true || point === undefined ? null
      : <TooltipCard title={point.name} lines={[`${points(point.quality)} points coverage`, `${percent(point.tokens)} tokens`]} />;
  };
  const shape = (props: ScatterShapeProps) => {
    const point = data[props.index];
    return props.cx === undefined || props.cy === undefined || point === undefined ? <g />
      : <g>
        <circle cx={props.cx} cy={props.cy} r={Dot} fill={markFill(point.openForge)} stroke={ChartStyle.surface} strokeWidth={2} />
        <text x={props.cx} y={below[props.index] === true ? props.cy + LabelGap + ChartStyle.fontSize : props.cy - LabelGap} textAnchor="middle"
          fontSize={ChartStyle.fontSize} fill={ChartStyle.text} stroke={ChartStyle.surface} strokeWidth={3} paintOrder="stroke">{point.name}</text>
      </g>;
  };
  const plot = () => <ResponsiveContainer width="100%" height="100%">
    <ScatterChart margin={{ top: 20, right: 48, bottom: 8, left: 8 }} accessibilityLayer={false}>
      <CartesianGrid stroke={ChartStyle.grid} />
      <XAxis type="number" dataKey="tokens" domain={[-x, x]} ticks={[-x, -x / 2, 0, x / 2, x]} tickFormatter={percent} stroke={ChartStyle.grid}
        tick={{ fontSize: ChartStyle.fontSize, fill: ChartStyle.axis }} tickLine={false} />
      <YAxis type="number" dataKey="quality" domain={[-y, y]} ticks={[-y, -y / 2, 0, y / 2, y]} tickFormatter={points} width={40} stroke={ChartStyle.grid}
        tick={{ fontSize: ChartStyle.fontSize, fill: ChartStyle.axis }} tickLine={false} />
      <ReferenceLine x={0} stroke={ChartStyle.axis} />
      <ReferenceLine y={0} stroke={ChartStyle.axis} />
      <Tooltip cursor={false} content={tooltip} isAnimationActive={false} />
      <Scatter data={[...data]} shape={shape} isAnimationActive={false} />
    </ScatterChart>
  </ResponsiveContainer>;
  return <ChartFigure height={data.length === 0 ? EmptyHeight : Height} plot={data.length === 0 ? () => <p className="eval-muted">No setup has both a coverage change and a token change yet.</p> : plot}
    title={<>Cost against <Term id="check-coverage">check coverage</Term></>}
    description="Tokens left and right, coverage up and down, with the control where the lines cross. Up and to the left is more coverage for fewer tokens."
    data={<table>
      <thead><tr><th scope="col">Setup</th><th scope="col">Token change</th><th scope="col">Coverage change, points</th></tr></thead>
      <tbody>{data.length === 0 ? <tr><td colSpan={3}>{NoValue}</td></tr> : data.map((point) => <tr key={point.setup}>
        <th scope="row"><Term id={point.setup}>{point.name}</Term></th><td>{percent(point.tokens)}</td><td>{points(point.quality)}</td>
      </tr>)}</tbody>
    </table>} />;
}
