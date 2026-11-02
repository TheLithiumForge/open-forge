import type { Batch } from "./records/records.types.ts";
import { referenceTrends } from "./data/reference-trend.ts";
import { Trend } from "./Charts.tsx";
import { percent } from "./data/format.ts";
export function ReferenceHistory({ batches }: {
  readonly batches: readonly Batch[];
}) {
  const references = referenceTrends(batches);
  return <>
    <h2>Reference over time</h2>
    <p>Each point is a saved reference assessment. Source, spec, grading, judge model and ballot count define a group. The reference stays separate from implementer comparisons.</p>
    {references.length === 0 ? <p>No reference assessments are published for this layer.</p> : null}
    {references.map((group) => <section className="eval-panel" key={group.key}>
      <h3>Layer {group.layer ?? "Missing"} reference</h3>
      <details><summary>Source and grading versions</summary><p className="eval-key">{group.key}</p></details>
      <div className="eval-charts"><Trend title="Reference coverage" metric="coverage" points={group.points.map((point) => ({ label: point.cliVersion ?? "Unknown CLI", value: point.coverage }))} /><Trend title="Reference judge code" metric="code" points={group.points.map((point) => ({ label: point.cliVersion ?? "Unknown CLI", value: point.code }))} /></div>
      <div className="eval-scroll" tabIndex={0} role="region" aria-label="Reference assessments"><table><thead><tr>{["Point", "Graded", "CLI", "Reference commit", "Coverage", "Rules", "Code", "Wishes", "Error"].map((name) => <th key={name}>{name}</th>)}</tr></thead>
        <tbody>{group.points.map((point, index) => <tr key={point.id}><th scope="row">{index + 1}</th><td>{point.at ?? "Missing"}</td><td>{point.cliVersion ?? "Missing"}</td><td><code>{point.commit}</code></td>{[point.coverage, point.rules, point.code, point.wishes].map((value, i) => <td key={i}><strong>{percent(value)}</strong></td>)}<td>{point.error ?? "None"}</td></tr>)}</tbody>
      </table></div>
    </section>)}
  </>;
}
