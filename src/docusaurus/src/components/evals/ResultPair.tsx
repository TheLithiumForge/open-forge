import type { ResultGroup } from "./data/views.ts";
import { Bars } from "./Charts.tsx";
import { Metric } from "./Metric.tsx";
import { SetupTable } from "./SetupTable.tsx";
export function ResultPair({ group }: {
  readonly group: ResultGroup;
}) {
  const { rows, batch } = group;
  const first = rows[0];
  if (first === undefined)
    return null;
  const points = (key: "coverage" | "rules" | "code" | "intentJudge") => batch.experiment.config.setups.map((setup) => {
    const value = rows.find((row) => row.setup === setup)?.[key];
    return { label: setup, value: value?.mean ?? null, low: value?.low ?? null, high: value?.high ?? null };
  });
  return <section className="eval-panel">
    <h3>{first.tool} {first.model} with {first.judge}</h3>
    <p>CLI {first.version}. Batch <code>{first.batchId}</code>. {first.scoring ? "Container attempts" : "Not for scoring. Local isolation"}.</p>
    <details><summary>Comparison and grading versions</summary><p className="eval-key">{first.scope}</p><p className="eval-key">{first.grading}</p></details>
    <SetupTable rows={rows} setups={batch.experiment.config.setups} />
    <details><summary>Usage and picks</summary><div className="eval-scroll" tabIndex={0} role="region" aria-label="Usage and picks"><table><thead><tr>{["Setup", "Tool calls", "Failed calls", "Peak context", "Blind picks", "Revealed picks", "Unknown reveal"].map((name) => <th key={name}>{name}</th>)}</tr></thead>
      <tbody>{rows.map((row) => <tr key={row.setup}><th scope="row">{row.setup}</th><td><Metric value={row.tools} share={false} /></td><td><Metric value={row.failedTools} share={false} /></td><td><Metric value={row.context} share={false} /></td><td><Metric value={row.picks} /></td><td><Metric value={row.revealedPicks} /></td><td>{row.unknownPicks}</td></tr>)}</tbody>
    </table></div></details>
    <div className="eval-charts"><Bars title="Coverage by setup" metric="coverage" points={points("coverage")} /><Bars title="Judge rules by setup" metric="rules" points={points("rules")} /><Bars title="Judge code by setup" metric="code" points={points("code")} /><Bars title="Judge wishes by setup" metric="wishes" points={points("intentJudge")} /></div>
  </section>;
}
