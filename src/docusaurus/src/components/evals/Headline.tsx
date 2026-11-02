import type { SetupSummary } from "./data/aggregate.ts";
import { OpenForgeSetups } from "./data/cells.ts";
import { change, NoValue, signed } from "./data/format.ts";
import { PreliminaryReason } from "./data/status.ts";
import { Term } from "./info/Term.tsx";

/** Plain words for why the numbers can still change. */
export const ReasonText: Readonly<Record<PreliminaryReason, string>> = {
  [PreliminaryReason.running]: "Some batches are still running.",
  [PreliminaryReason.unchecked]: "Some finished runs aren't checked yet.",
  [PreliminaryReason.recheck]: "Some runs wait on a recheck after a grader error.",
  [PreliminaryReason.unjudged]: "Some finished runs aren't judged yet.",
};

const plural = (count: number, one: string, many: string) => `${count} ${count === 1 ? one : many}`;

function Figure({ id, value, unit }: { readonly id: string; readonly value: string; readonly unit: string }) {
  return <div className="eval-figure">
    <dt><Term id={id} /></dt>
    <dd><span className="eval-figure-value">{value}</span> {value === NoValue ? null : <span className="eval-muted">{unit}</span>}</dd>
  </div>;
}

function Card({ setup, summary }: { readonly setup: string; readonly summary: SetupSummary | undefined }) {
  const shift = (delta: number | null) => delta === null ? NoValue : signed(delta * 100, 1);
  return <article className="eval-card">
    <h3><Term id={setup} /></h3>
    {summary === undefined || summary.groups === 0 ? <p className="eval-muted">No comparison with the control in these filters.</p> : <>
      <dl>
        <Figure id="check-coverage" value={shift(summary.coverageDelta)} unit="points" />
        <Figure id="judge-score" value={shift(summary.judgeDelta)} unit="points" />
        <Figure id="tokens" value={change(summary.tokenRatio)} unit="tokens" />
      </dl>
      <p className="eval-muted eval-card-classes">New input {change(summary.newInputRatio)}, output {change(summary.outputRatio)}.</p>
      <p className="eval-muted eval-card-basis">From {plural(summary.groups, "case and model", "cases and models")}, {plural(summary.runs, "run", "runs")}.</p>
    </>}
  </article>;
}

/** Each Open Forge setup against the control, averaged over the filtered starting points, cases and models. */
export function Headline({ summaries, reasons }: {
  readonly summaries: readonly SetupSummary[];
  readonly reasons: readonly PreliminaryReason[];
}) {
  return <section className="eval-section" aria-labelledby="eval-headline">
    <h2 id="eval-headline">Compared with the control</h2>
    <p>Each card shows how a setup with Open Forge differs from the control on the same starting point, case and model, averaged over everything the filters keep. Coverage and judge scores change in percentage points, tokens in percent. The samples are small, so read these as screening signals, not claims.</p>
    {reasons.length === 0 ? null : <p className="eval-preliminary">
      <span className="badge badge--warning">Preliminary</span> {reasons.map((reason) => ReasonText[reason]).join(" ")} These numbers can still change.
    </p>}
    <div className="eval-cards">
      {OpenForgeSetups.map((setup) => <Card key={setup} setup={setup} summary={summaries.find((summary) => summary.setup === setup)} />)}
    </div>
  </section>;
}
