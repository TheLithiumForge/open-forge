import { useId, useMemo, useState } from "react";
import type { Batch } from "./records/records.types.ts";
import { comparisonRows, pairKey, pairOptions, resultGroups } from "./data/views.ts";
import { differences } from "./data/differences.ts";
import { choiceLabel, isReference } from "./data/observations.ts";
import { Bars } from "./Charts.tsx";
import { Metric } from "./Metric.tsx";
import { Attempt } from "./Attempt.tsx";
import { PairSelector } from "./PairSelector.tsx";
import { ResultPair } from "./ResultPair.tsx";
import { Progress } from "./Progress.tsx";
export function Results({ batches, layer }: {
  readonly batches: readonly Batch[];
  readonly layer: number | null;
}) {
  const selected = useMemo(() => batches.filter((batch) => (batch.experiment.config.layer ?? null) === layer), [batches, layer]);
  const groups = useMemo(() => resultGroups(selected), [selected]);
  const pairs = pairOptions(groups.flatMap((group) => group.rows));
  const [chosenPair, choosePair] = useState("");
  const [chosenGroup, chooseGroup] = useState("");
  // Without a choice, show the implementer and judge pair with the most runs.
  const runCount = (key: string) => groups.flatMap((group) => group.rows).filter((row) => pairKey(row) === key).reduce((sum, row) => sum + row.runs, 0);
  const pair = pairs.find((item) => item.key === chosenPair) ?? [...pairs].sort((a, b) => runCount(b.key) - runCount(a.key))[0];
  const candidates = groups.filter((group) => group.rows.some((row) => pairKey(row) === pair?.key));
  const active = candidates.find((group) => group.key === chosenGroup) ?? candidates[0];
  const row = active?.rows[0];
  const batchId = useId();
  const runs = active === undefined || row === undefined ? [] : active.batch.runs.filter((run) => !isReference(run) && run.agent.tool === row.tool
    && (run.reportedModel ?? run.requestedModel ?? "unreported model") === row.model && run.agent.effort === row.effort);
  const referenceRuns = selected.flatMap((batch) => batch.runs.filter(isReference).map((run) => ({ run, batch })));
  const deltas = active === undefined || runs[0] === undefined ? [] : differences(active.batch).filter((item) => item.implementer === choiceLabel(runs[0]?.agent));
  return <>
    <h2>{layer === null ? "Unreported layer" : `Layer ${layer}`}</h2>
    {selected.length === 0 ? <p>No batches are published for this layer.</p> : <Progress batches={selected} />}
    {pair === undefined ? null : <PairSelector pairs={pairs} value={pair.key} onChange={choosePair} />}
    {active === undefined ? null : <>
      <div className="eval-field"><label htmlFor={batchId}>Batch and grading</label><select id={batchId} value={active.key} onChange={(event) => chooseGroup(event.target.value)}>
        {candidates.map((group, index) => <option key={group.key} value={group.key}>CLI {group.batch.experiment.cliVersion}. Batch {group.batch.experiment.id}. Grading group {index + 1}.</option>)}
      </select></div>
      <section className="eval-panel"><p>Pair comparison on {row?.setup}. CLI {row?.version}. Matching grading versions only.</p>
        <Bars title="Judge rules by pair" metric="rules" points={comparisonRows(groups, active).map((item) => ({ label: `${item.tool} ${item.model} with ${item.judge}`, value: item.rules.mean, low: item.rules.low, high: item.rules.high }))} />
      </section>
      <ResultPair group={active} />
    </>}
    {deltas.length === 0 ? null : <details className="eval-panel"><summary>Judge differences</summary><p>Second judge minus first judge on the same attempts and grading versions. Missing judgments, errors and different grading stay apart.</p>
      <div className="eval-scroll" tabIndex={0} role="region" aria-label="Judge differences"><table><thead><tr>{["Setup", "Implementer", "First judge", "Second judge", "Rules", "Code", "Wishes", "Missing", "Errors", "Different grading"].map((name) => <th key={name}>{name}</th>)}</tr></thead>
        <tbody>{deltas.map((item, index) => <tr key={`${item.key}-${index}`}><th scope="row">{item.setup}</th><td>{item.implementer}</td><td>{item.first}</td><td>{item.second}</td>{item.scores.map((score) => <td key={score.area}><Metric value={score.value} /></td>)}<td>{item.missing}</td><td>{item.errors}</td><td>{item.incompatible}</td></tr>)}</tbody>
      </table></div></details>}
    <h2>Runs</h2><p>Open a run to see its saved files, checks and judge answers. Replaced runs stay listed. Reference runs stay out of setup comparisons.</p>
    {active === undefined ? null : runs.map((run) => <details className="eval-panel" key={run.id}>
      <summary>{run.setup}. {choiceLabel(run.agent)}. Repeat {run.repeat}. {run.pipelineStatus}.</summary><Attempt batch={active.batch} run={run} />
    </details>)}
    {referenceRuns.map(({ run, batch }) => <details className="eval-panel" key={run.id}><summary>Reference. CLI {batch.experiment.cliVersion}. Repeat {run.repeat}.</summary><Attempt batch={batch} run={run} /></details>)}
  </>;
}
