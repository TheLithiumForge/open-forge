import type { Batch, Run } from "../records/records.types.ts";
import type { Cell } from "../data/cells.ts";
import { tokenTotal } from "../data/analysis.ts";
import { compact, dollars, NoValue, percent } from "../data/format.ts";
import { choiceKey, isReference } from "../data/observations.ts";
import { runCost } from "../data/prices.ts";
import { JudgeHeadline } from "../data/judge-scores.ts";
import { checkState, CheckState, coverageOf, judgeChoices, judgeHeadlineOf, judgeScoreOf } from "../data/run-measures.ts";
import { Term } from "../info/Term.tsx";

const CheckText: Readonly<Record<CheckState, string | null>> = {
  [CheckState.checked]: null, [CheckState.recheck]: "Waits on a recheck", [CheckState.unchecked]: "Not checked yet",
};

function RunRow({ run, batch, cell, open }: { readonly run: Run; readonly batch: Batch; readonly cell: Cell; readonly open: (id: string) => void }) {
  const judge = cell.judge === null ? undefined : judgeChoices(batch).get(cell.judge);
  const state = checkState(run, batch.assessments);
  const score = judge === undefined ? null : judgeScoreOf(run, judge, batch.assessments);
  const headline = (name: JudgeHeadline) => { const value = judge === undefined ? null : judgeHeadlineOf(run, judge, batch.assessments, name); return value === null ? NoValue : percent(value); };
  return <tr>
    <th scope="row"><button type="button" className="eval-link-button" onClick={() => open(run.id)}>Run {run.repeat}</button></th>
    <td>{CheckText[state] ?? percent(coverageOf(run, batch.assessments))}</td>
    <td>{score === null ? NoValue : percent(score)}</td>
    <td>{headline(JudgeHeadline.quality)}</td>
    <td>{headline(JudgeHeadline.adherence)}</td>
    <td>{compact(tokenTotal(run))}</td>
    <td>{dollars(runCost(run))}</td>
    <td>{run.agentSeconds === null ? NoValue : (run.agentSeconds / 60).toFixed(1)}</td>
  </tr>;
}

/** A cell's counted runs, then the runs of the same setup and model that don't count, so none disappears silently. */
export function RunList({ cell, batch, open }: { readonly cell: Cell; readonly batch: Batch; readonly open: (id: string) => void }) {
  const counted = batch.runs.filter((run) => cell.runIds.includes(run.id)).sort((a, b) => a.repeat - b.repeat);
  const others = batch.runs.filter((run) => !isReference(run) && run.setup === cell.setup && choiceKey(run.agent) === cell.model && !cell.runIds.includes(run.id));
  return <>
    <p>Open a run to see its screenshots, built game, conversation, event log, code, checks and judge answers.</p>
    <div className="eval-scroll" tabIndex={0} role="region" aria-label="Runs">
      <table className="eval-table">
        <thead><tr>
          <th scope="col">Run</th><th scope="col"><Term id="check-coverage" /></th><th scope="col"><Term id="judge-score" /></th>
          <th scope="col"><Term id="judge-quality" /></th><th scope="col"><Term id="judge-adherence" /></th>
          <th scope="col"><Term id="tokens" /></th><th scope="col"><Term id="cost" /></th><th scope="col"><Term id="time">Minutes</Term></th>
        </tr></thead>
        <tbody>{counted.length === 0 ? <tr><td colSpan={8}>No finished runs yet.</td></tr>
          : counted.map((run) => <RunRow key={run.id} run={run} batch={batch} cell={cell} open={open} />)}</tbody>
      </table>
    </div>
    {others.length === 0 ? null : <details>
      <summary>{others.length} more {others.length === 1 ? "run doesn't" : "runs don't"} count</summary>
      <p>These runs are still working, wait to be redone after a usage limit, or were replaced after an infrastructure failure.</p>
      <ul>{others.map((run) => <li key={run.id}>
        <button type="button" className="eval-link-button" onClick={() => open(run.id)}>Run {run.repeat}</button> {run.pipelineStatus}{run.failureKind === null ? "" : `, ${run.failureKind}`}
      </li>)}</ul>
    </details>}
  </>;
}
