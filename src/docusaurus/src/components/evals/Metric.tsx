import type { Interval } from "./data/analysis.ts";
import { intervalText, metricTitle, number, percent } from "./data/format.ts";
export function Metric({ value, share = true }: {
  readonly value: Interval;
  readonly share?: boolean;
}) {
  const range = intervalText(value, share);
  return <span className="eval-stat" title={metricTitle(value, share)}>
    <strong>{(share ? percent : number)(value.mean)}</strong>
    {range === null ? null : <small className="eval-muted">{range}</small>}
    <small className="eval-muted">n={value.samples}{value.missing === 0 ? "" : `, ${value.missing} missing`}</small>
  </span>;
}
