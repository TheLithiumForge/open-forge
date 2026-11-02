import type { Batch } from "./records/records.types.ts";
import { referenceTrends } from "./data/reference-trend.ts";
import { Trend } from "./Trend.tsx";
import { StartingPoints } from "./data/cells.ts";
import { percent } from "./data/format.ts";
export function ReferenceHistory({ batches }: {
  readonly batches: readonly Batch[];
}) {
  const references = referenceTrends(batches);
  return <>
    <h2>Reference over time</h2>
    <p>Each point grades the reviewed reference solution with the same checks and judges. The source, spec, grading, judge model and ballot count define a line. The reference stays apart from the setup comparisons.</p>
    {references.length === 0 ? <p>No reference assessments are published for this starting point.</p> : null}
    {references.map((group) => <section className="eval-panel" key={group.key}>
      <h3>Reference for {StartingPoints.find((point) => point.layer === group.layer)?.title.toLowerCase() ?? "an unreported starting point"}</h3>
      <details><summary>Source and grading versions</summary><p className="eval-key">{group.key}</p></details>
      <div className="eval-trends"><Trend title="Reference coverage" metric="coverage" points={group.points.map((point) => ({ label: point.cliVersion ?? "Unknown CLI", value: point.coverage }))} /><Trend title="Reference judge code" metric="code" points={group.points.map((point) => ({ label: point.cliVersion ?? "Unknown CLI", value: point.code }))} /></div>
      <div className="eval-scroll" tabIndex={0} role="region" aria-label="Reference assessments"><table><thead><tr>{["Point", "Graded", "CLI", "Reference commit", "Coverage", "Rules", "Code", "Wishes", "Error"].map((name) => <th key={name}>{name}</th>)}</tr></thead>
        <tbody>{group.points.map((point, index) => <tr key={point.id}><th scope="row">{index + 1}</th><td>{point.at ?? "Missing"}</td><td>{point.cliVersion ?? "Missing"}</td><td><code>{point.commit}</code></td>{[point.coverage, point.rules, point.code, point.wishes].map((value, i) => <td key={i}><strong>{percent(value)}</strong></td>)}<td>{point.error ?? "None"}</td></tr>)}</tbody>
      </table></div>
    </section>)}
  </>;
}
