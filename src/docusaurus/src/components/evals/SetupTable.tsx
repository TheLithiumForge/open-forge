import type { Summary } from "./data/summaries.ts";
import { Metric } from "./Metric.tsx";
const columns = ["Setup", "Attempts", "Coverage", "Working gate", "Stated success", "Wishes", "Judge rules", "Judge code", "Judge wishes", "Judge missing", "Judge errors", "Unsure ballots", "Check errors", "Minutes", "Tokens"] as const;
export function SetupTable({ rows, setups }: {
  readonly rows: readonly Summary[];
  readonly setups: readonly string[];
}) {
  return <div className="eval-scroll" tabIndex={0} role="region" aria-label="Setup results"><table>
    <caption>Saved attempts by setup. Values show 95% intervals and measured counts. Intervals describe screening variation.</caption>
    <thead><tr>{columns.map((name) => <th key={name} scope="col" title={name === "Coverage" ? "Applicable checks passed. Each cell gives a 95% interval, n measured attempts and any missing values." : undefined}>{name}</th>)}</tr></thead>
    <tbody>{setups.map((setup) => {
      const row = rows.find((item) => item.setup === setup);
      return <tr key={setup}><th scope="row">{setup}</th>{row === undefined ? <td colSpan={14}>Missing</td> : <>
        <td>{row.runs}</td>{[row.coverage, row.working, row.stated, row.intent, row.rules, row.code, row.intentJudge].map((value, index) => <td key={index}><Metric value={value} /></td>)}
        <td>{row.judgeMissing}</td><td>{row.judgeErrors}</td><td>{row.unsure}</td><td>{row.errors}</td><td><Metric value={row.minutes} share={false} /></td><td><Metric value={row.tokens} share={false} /></td>
      </>}</tr>;
    })}</tbody>
  </table></div>;
}
