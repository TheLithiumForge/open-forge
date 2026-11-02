import { useId } from "react";
import { percent } from "./data/format.ts";

export interface TrendPoint {
  readonly label: string;
  readonly value: number | null;
  readonly low?: number | null;
  readonly high?: number | null;
}

const AxisTicks = [0, 0.25, 0.5, 0.75, 1] as const;
const Scale = { width: 640, height: 240, left: 56, right: 584, top: 32, bottom: 174 } as const;
const LongLabel = 17;

function TrendData({ points }: { readonly points: readonly TrendPoint[] }) {
  return <details className="eval-chart-data"><summary>Chart data</summary><ul>{points.map((point, index) => <li key={`${point.label}-${index}`}>
    {point.label}: {percent(point.value)}{point.low == null || point.high == null ? "" : `. 95% interval ${percent(point.low)} to ${percent(point.high)}`}.
  </li>)}</ul></details>;
}

/** A share across CLI versions, with 95% intervals where a version has more than one run. Missing values leave gaps. */
export function Trend({ title, points, metric }: {
  readonly title: string;
  readonly points: readonly TrendPoint[];
  readonly metric: "coverage" | "rules" | "code";
}) {
  const id = useId();
  const x = (index: number) => Scale.left + index * (Scale.right - Scale.left) / Math.max(1, points.length - 1);
  const y = (value: number) => Scale.bottom - value * (Scale.bottom - Scale.top);
  const tickStep = Math.max(1, Math.ceil((points.length - 1) / 3));
  return <figure className={`eval-trend eval-metric-${metric}`} aria-labelledby={id}>
    <figcaption id={id}>{title}</figcaption>
    <svg viewBox={`0 0 ${Scale.width} ${Scale.height}`} role="img" aria-label={`${title}. Axis from 0 to 100 percent. Missing values have gaps.`}>
      <title>{`${title}. The chart data lists every value and interval.`}</title>
      {AxisTicks.map((value) => <g key={value}><line x1={Scale.left} x2={Scale.right} y1={y(value)} y2={y(value)} className="eval-grid" /><text x="0" y={y(value) + 4} fontSize="18">{value * 100}%</text></g>)}
      {points.map((point, index) => {
        const previous = points[index - 1];
        return <g key={`${point.label}-${index}`}>
          {index % tickStep !== 0 && index !== points.length - 1 ? null : <text x={x(index)} y="202" textAnchor="middle" fontSize="17"><title>{point.label}</title>{point.label.length > LongLabel ? `${point.label.slice(0, LongLabel - 3)}...` : point.label}</text>}
          {point.value === null ? <text x={x(index)} y={Scale.bottom - 8} textAnchor="middle" fontSize="18">Missing</text> : <>
            {previous?.value == null ? null : <line x1={x(index - 1)} x2={x(index)} y1={y(previous.value)} y2={y(point.value)} className="eval-line" />}
            {point.low == null || point.high == null ? null : <g className="eval-interval">
              <line x1={x(index)} x2={x(index)} y1={y(point.low)} y2={y(point.high)} />
              <line x1={x(index) - 5} x2={x(index) + 5} y1={y(point.low)} y2={y(point.low)} /><line x1={x(index) - 5} x2={x(index) + 5} y1={y(point.high)} y2={y(point.high)} />
            </g>}
            <circle cx={x(index)} cy={y(point.value)} r="4" className="eval-fill" />
            <text x={x(index)} y={y(point.value) - 11} textAnchor="middle" fontSize="18">{Math.round(point.value * 100)}%</text>
          </>}
        </g>;
      })}
    </svg>
    <TrendData points={points} />
  </figure>;
}
