import { useId } from "react";
import { percent } from "./data/format.ts";
export interface ChartPoint {
  readonly label: string;
  readonly value: number | null;
  readonly low?: number | null;
  readonly high?: number | null;
}
interface ChartProps {
  readonly title: string;
  readonly points: readonly ChartPoint[];
  readonly metric: "coverage" | "rules" | "code" | "wishes";
}
const AxisTicks = [0, 0.25, 0.5, 0.75, 1] as const;
const BarScale = { start: 8, end: 268, label: 280, width: 340, height: 32 } as const;
const TrendScale = { width: 640, height: 240, left: 56, right: 584, top: 32, bottom: 174 } as const;
function ChartData({ points }: {
  readonly points: readonly ChartPoint[];
}) {
  return <details className="eval-chart-data"><summary>Chart data</summary><ul>{points.map((point, index) => <li key={`${point.label}-${index}`}>
    {point.label}: {percent(point.value)}{point.low == null || point.high == null ? "" : `. 95% interval ${percent(point.low)} to ${percent(point.high)}`}.
  </li>)}</ul></details>;
}
export function Bars({ title, points, metric }: ChartProps) {
  const id = useId();
  const x = (value: number) => BarScale.start + value * (BarScale.end - BarScale.start);
  return <figure className={`eval-chart eval-metric-${metric}`} aria-labelledby={id}>
    <figcaption id={id}>{title}</figcaption>
    {points.map((point, index) => <div className="eval-bar" key={`${point.label}-${index}`}>
      <span>{point.label}</span>
      <svg viewBox={`0 0 ${BarScale.width} ${BarScale.height}`} role="img" aria-label={`${point.label}: ${percent(point.value)}`}>
        <title>{`${point.label}: ${percent(point.value)}`}</title>
        {AxisTicks.map((tick) => <line key={tick} x1={x(tick)} x2={x(tick)} y1="2" y2="30" className="eval-grid" />)}
        {point.value === null ? <text x={BarScale.start} y="21" className="eval-value">Missing</text> : <>
          <rect x={x(0)} y="9" width={point.value * (BarScale.end - BarScale.start)} height="14" rx="2" className="eval-fill" />
          {point.low == null || point.high == null ? null : <g className="eval-interval">
            <line x1={x(point.low)} x2={x(point.high)} y1="16" y2="16" />
            <line x1={x(point.low)} x2={x(point.low)} y1="10" y2="22" /><line x1={x(point.high)} x2={x(point.high)} y1="10" y2="22" />
          </g>}
          <text x={BarScale.label} y="21" className="eval-value">{percent(point.value)}</text>
        </>}
      </svg>
    </div>)}
    <div className="eval-bar eval-bar-axis"><span aria-hidden="true" /><svg viewBox={`0 0 ${BarScale.width} 24`} aria-label="Axis from 0 to 100 percent" role="img">
      {AxisTicks.map((tick) => <text key={tick} x={x(tick)} y="14" fontSize="11" textAnchor={tick === 0 ? "start" : "middle"}>{tick * 100}%</text>)}
    </svg></div>
    <ChartData points={points} />
  </figure>;
}
export function Trend({ title, points, metric }: ChartProps) {
  const id = useId();
  const x = (index: number) => TrendScale.left + index * (TrendScale.right - TrendScale.left) / Math.max(1, points.length - 1);
  const y = (value: number) => TrendScale.bottom - value * (TrendScale.bottom - TrendScale.top);
  const tickStep = Math.max(1, Math.ceil((points.length - 1) / 3));
  return <figure className={`eval-chart eval-metric-${metric}`} aria-labelledby={id}>
    <figcaption id={id}>{title}</figcaption>
    <svg viewBox={`0 0 ${TrendScale.width} ${TrendScale.height}`} role="img" aria-label={`${title}. Axis from 0 to 100 percent. Missing values have gaps.`}>
      <title>{`${title}. The chart data lists every value and interval.`}</title>
      {AxisTicks.map((value) => <g key={value}><line x1={TrendScale.left} x2={TrendScale.right} y1={y(value)} y2={y(value)} className="eval-grid" /><text x="0" y={y(value) + 4} fontSize="18">{value * 100}%</text></g>)}
      {points.map((point, index) => {
        const previous = points[index - 1];
        return <g key={`${point.label}-${index}`}>
          {index % tickStep !== 0 && index !== points.length - 1 ? null : <text x={x(index)} y="202" textAnchor="middle" fontSize="17"><title>{point.label}</title>{point.label.length > 17 ? `${point.label.slice(0, 14)}...` : point.label}</text>}
          {point.value === null ? <text x={x(index)} y={TrendScale.bottom - 8} textAnchor="middle" fontSize="18">Missing</text> : <>
            {previous?.value == null ? null : <line x1={x(index - 1)} x2={x(index)} y1={y(previous.value)} y2={y(point.value)} className="eval-line" />}
            {point.low == null || point.high == null ? null : <g className="eval-interval">
              <line x1={x(index)} x2={x(index)} y1={y(point.low)} y2={y(point.high)} />
              <line x1={x(index) - 5} x2={x(index) + 5} y1={y(point.low)} y2={y(point.low)} /><line x1={x(index) - 5} x2={x(index) + 5} y1={y(point.high)} y2={y(point.high)} />
            </g>}
            <circle cx={x(index)} cy={y(point.value)} r="4" className="eval-fill" />
            <text x={x(index)} y={y(point.value) - 11} textAnchor="middle" fontSize="18" className="eval-trend-value">{Math.round(point.value * 100)}%</text>
          </>}
        </g>;
      })}
    </svg>
    <ChartData points={points} />
  </figure>;
}
