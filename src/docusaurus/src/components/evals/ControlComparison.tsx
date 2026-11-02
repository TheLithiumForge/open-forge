import type { Interval } from "./data/analysis.ts";
import type { Summary } from "./data/summaries.ts";
import { number, percent } from "./data/format.ts";

type Measure = "coverage" | "rules" | "code" | "minutes" | "tokens";
// Shares compare in points, and usage compares as a ratio to the control. Tokens measure the agent's effort. Time mostly
// reflects the provider's speed, so it comes last and compares setups within one model only.
const Measures: readonly { readonly key: Measure; readonly title: string; readonly share: boolean }[] = [
  { key: "coverage", title: "Check coverage", share: true }, { key: "rules", title: "Judge rules", share: true },
  { key: "code", title: "Judge code", share: true }, { key: "tokens", title: "Tokens", share: false }, { key: "minutes", title: "Minutes, same model only", share: false },
];

function difference(value: Interval, control: Interval, share: boolean): string {
  if (value.mean === null || control.mean === null) return "Missing";
  if (share) {
    const points = (value.mean - control.mean) * 100;
    return `${points > 0 ? "+" : ""}${points.toFixed(1)} points`;
  }
  if (control.mean === 0) return "Missing";
  const ratio = (value.mean / control.mean - 1) * 100;
  return `${ratio > 0 ? "+" : ""}${ratio.toFixed(0)}%`;
}

/** Each setup against the plan's first setup, the control: no setup on Layer 1, plain docs on later layers. */
export function ControlComparison({ rows, setups }: { readonly rows: readonly Summary[]; readonly setups: readonly string[] }) {
  const controlName = setups[0];
  const control = rows.find((row) => row.setup === controlName);
  if (control === undefined || rows.length < 2) return null;
  return <div className="eval-control">
    <h4>Compared with the control, {controlName}</h4>
    <p>Means over this batch's runs. Tokens are the cost measure. Minutes mostly reflect the provider's speed, so compare them between setups of one model, not between models. Small samples make these differences screening signals, not claims. The intervals are in the table below.</p>
    <div className="eval-scroll" tabIndex={0} role="region" aria-label={`Setups compared with ${controlName}`}><table>
      <thead><tr><th>Setup</th><th>Runs</th>{Measures.map((measure) => <th key={measure.key}>{measure.title}</th>)}</tr></thead>
      <tbody>{setups.flatMap((setup) => {
        const row = rows.find((item) => item.setup === setup);
        return row === undefined ? [] : [<tr key={setup} className={setup === controlName ? "eval-control-row" : undefined}>
          <th scope="row">{setup}{setup === controlName ? " (control)" : ""}</th><td>{row.runs}</td>
          {Measures.map((measure) => <td key={measure.key}>
            <strong>{measure.share ? percent(row[measure.key].mean) : number(row[measure.key].mean)}</strong>
            {setup === controlName ? null : <div className="eval-muted">{difference(row[measure.key], control[measure.key], measure.share)}</div>}
          </td>)}
        </tr>];
      })}</tbody>
    </table></div>
  </div>;
}
