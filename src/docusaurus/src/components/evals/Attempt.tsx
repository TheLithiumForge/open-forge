import type { Batch, Run, Checks, Judge } from "./records/records.types.ts";
import { latestRun, tokenTotal } from "./data/analysis.ts";
import { modelLabel } from "./data/names.ts";
import { number, percent } from "./data/format.ts";
import { Evidence } from "./Evidence.tsx";
import { JudgeDetails } from "./JudgeDetails.tsx";
import { RunTokens } from "./RunTokens.tsx";
export function Attempt({ batch, run }: {
  readonly batch: Batch;
  readonly run: Run;
}) {
  const view = latestRun(run, batch.assessments);
  const records = batch.assessments.filter((item): item is Checks | Judge => item.kind !== "pick" && item.runId === run.id);
  const json = `data:application/json;charset=utf-8,${encodeURIComponent(JSON.stringify({ run, assessments: records, artifacts: batch.artifacts.filter((item) => item.runId === run.id) }, null, 2))}`;
  return <div className="eval-attempt">
    <h3>Run</h3>
    <p>Batch <code>{batch.experiment.id}</code>. Run <code>{run.id}</code>.</p>
    <p>Requested model: {run.requestedModel ?? "Missing"}. Reported model: {run.reportedModel ?? "Missing"}.</p>
    <p>Latest check coverage: {percent(view.result?.coverage.share ?? null)}. Time: {number(run.agentSeconds)} seconds. Tokens: {number(tokenTotal(run))}.</p>
    <RunTokens run={run} />
    <p>Failure kind: {run.failureKind ?? "None"}. {run.replacesRunId === null ? "" : `Replaces ${run.replacesRunId}. ${run.replacementReason ?? "Missing reason"}`}</p>
    {run.error === null ? null : <pre>{run.error}</pre>}
    <a href={json} download={`${run.id}.json`}>Download run records</a>
    <h3>Saved files</h3>
    <Evidence batch={batch} artifacts={batch.artifacts.filter((item) => item.runId === run.id && item.assessmentId === null)} />
    {records.length === 0 ? <p>Check and judge assessments are missing.</p> : null}
    {records.map((record) => <section key={record.id}>
      <h3>{record.kind === "checks" ? "Checks" : record.requestedModel === null ? "Judge" : `Judge ${modelLabel(record.requestedModel)}`}</h3>
      <p>Assessment <code>{record.id}</code>. {record.createdAt ?? "Missing time"}.</p>
      <p className="eval-key">Grading: {JSON.stringify(record.versions)}</p>
      {record.error === null ? null : <p role="alert">Grader error: {record.error}</p>}
      {record.kind === "checks" ? <>
        <p>Applicable: {record.selection.applicable.join(", ") || "None"}. Working gate: {record.selection.working.join(", ") || "Missing"}. Stated success: {record.selection.stated.join(", ") || "Missing"}.</p>
        {batch.artifacts.some((item) => item.assessmentId === record.id) ? <Evidence batch={batch} artifacts={batch.artifacts.filter((item) => item.assessmentId === record.id)} /> : null}
        <div className="eval-scroll" tabIndex={0} role="region" aria-label="Attempt checks"><table><thead><tr><th>Check</th><th>Area</th><th>Status</th><th>Evidence</th></tr></thead>
          <tbody>{record.outcomes.map((check) => <tr key={check.id}><td>{check.id}</td><td>{check.area}</td><td>{check.status}</td><td className="eval-evidence">{check.evidence}</td></tr>)}{record.selection.applicable.filter((id) => !record.outcomes.some((item) => item.id === id)).map((id) => <tr key={id}><td>{id}</td><td>Missing</td><td>Missing</td><td>Missing</td></tr>)}</tbody>
        </table></div>
      </> : <>
        <p>Reported judge: {record.reportedModel ?? "Missing"}. Ballots: {record.ballots ?? "Missing"}.</p>
        <JudgeDetails record={record} />
        {batch.artifacts.some((item) => item.assessmentId === record.id) ? <Evidence batch={batch} artifacts={batch.artifacts.filter((item) => item.assessmentId === record.id)} /> : null}
        {record.answers.length === 0 ? <p>Judge answers are missing.</p> : null}
        {record.answers.map((answer) => <details key={answer.id}><summary>{answer.area}: {answer.question}. {answer.yes} yes, {answer.no} no, {answer.unsure} unsure.</summary>
          <p>Evidence:</p><ul>{answer.evidence.map((evidence, index) => <li className="eval-evidence" key={index}>{evidence}</li>)}</ul>
        </details>)}
      </>}
    </section>)}
    {run.finalMessage === null ? null : <details><summary>Final message</summary><pre>{run.finalMessage}</pre></details>}
  </div>;
}
