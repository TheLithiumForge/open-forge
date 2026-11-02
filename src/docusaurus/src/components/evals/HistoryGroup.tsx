import type { Summary } from "./data/summaries.ts";
import { historyStatus } from "./data/summaries.ts";
import { Trend } from "./Trend.tsx";
import { setupName } from "./data/names.ts";
import { Term } from "./info/Term.tsx";
import { Metric } from "./Metric.tsx";
export function HistoryGroup({ group }: {
  readonly group: readonly Summary[];
}) {
  const rows = [...group].sort((a, b) => a.createdAt.localeCompare(b.createdAt));
  const first = rows[0];
  if (first === undefined)
    return null;
  return <section className="eval-panel">
    <h3><Term id={first.setup}>{setupName(first.setup)}</Term></h3>
    <p className="eval-muted">{first.modelLabel}, judged by {first.judgeLabel ?? "no judge yet"}.</p>
    <details><summary>Comparison and grading versions</summary><p className="eval-key">{first.scope}</p><p className="eval-key">{first.grading}</p></details>
    <div className="eval-trends"><Trend title="Coverage across CLI versions" metric="coverage" points={rows.map((row) => ({ label: row.version, value: row.coverage.mean, low: row.coverage.low, high: row.coverage.high }))} /><Trend title="Judge rules across CLI versions" metric="rules" points={rows.map((row) => ({ label: row.version, value: row.rules.mean, low: row.rules.low, high: row.rules.high }))} /></div>
    <div className="eval-scroll" tabIndex={0} role="region" aria-label={`${setupName(first.setup)} CLI history`}><table><caption>Each row keeps the same grading and model choices. Intervals describe screening variation.</caption><thead><tr>{["Setup", "Point", "CLI", "Graded", "Batch", "Coverage", "Working gate", "Judge code", "Wishes", "Minutes", "Tokens", "Screening result"].map((name) => <th key={name}>{name}</th>)}</tr></thead>
      <tbody>{rows.map((row, index) => <tr key={`${row.batchId}-${index}`}><th scope="row">{setupName(row.setup)}</th><td>{index + 1}</td><td>{row.version}</td><td>{row.createdAt}</td><td><code>{row.batchId}</code></td>{[row.coverage, row.working, row.code, row.intent].map((value, i) => <td key={i}><Metric value={value} /></td>)}<td><Metric value={row.minutes} share={false} /></td><td><Metric value={row.tokens} share={false} /></td><td>{historyStatus(rows[index - 1], row)}</td></tr>)}</tbody>
    </table></div>
  </section>;
}
