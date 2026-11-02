import type { Batch } from "./records/records.types.ts";
import type { Cell } from "./data/cells.ts";
import { DoneStatus, gradingVersions, olderBatches, shownBatches, type PreliminaryReason } from "./data/status.ts";
import { ReasonText } from "./Headline.tsx";
import { Progress } from "./Progress.tsx";

const ShortHash = 12;
const short = (value: string | null | undefined) => value == null ? "Not reported" : value.slice(0, ShortHash);

/** How complete the shown data is, which batches count and which grading produced the scores. Collapsed by default. */
export function DataStatus({ batches, all, shown, reasons }: {
  readonly batches: readonly Batch[];
  readonly all: readonly Cell[];
  readonly shown: readonly Cell[];
  readonly reasons: readonly PreliminaryReason[];
}) {
  const counted = shownBatches(batches, shown);
  const running = counted.filter((batch) => batch.experiment.status !== DoneStatus).length;
  const older = olderBatches(batches, all);
  return <section className="eval-section" aria-labelledby="eval-status">
    <h2 id="eval-status">Data status</h2>
    <p>{counted.length} {counted.length === 1 ? "batch counts" : "batches count"} in these filters, and {running} {running === 1 ? "is" : "are"} still running. {reasons.length === 0 ? "Every counted run is checked and judged." : reasons.map((reason) => ReasonText[reason]).join(" ")}</p>
    <details className="eval-details">
      <summary>Batch progress, counting rules and grading versions</summary>
      <h3>Batch progress</h3>
      <p>A run counts as finished once its agent stops. Checks and judges grade it afterwards, from its saved files.</p>
      <Progress batches={counted} />
      <h3>Which batches count</h3>
      <p>Only the newest batch for each case and model counts, because older batches used earlier inputs or grading. Every setup is compared with the control from the same batch, so both sides share the CLI version and the grading.</p>
      {older.length === 0 ? null : <p>Older batches on the History page: {older.map((batch) => batch.experiment.id).join(", ")}.</p>}
      <h3>Grading versions</h3>
      <p>Each version is a hash of the checker, the judge rubric and the judge prompt. A changed version means the grading changed, so it stays apart from earlier scores.</p>
      <div className="eval-scroll" tabIndex={0} role="region" aria-label="Grading versions">
        <table className="eval-table">
          <thead><tr>{["Batch", "Checker", "Rubric", "Judge prompt"].map((name) => <th key={name} scope="col">{name}</th>)}</tr></thead>
          <tbody>{counted.flatMap((batch) => gradingVersions(batch).map((versions, index) => <tr key={`${batch.experiment.id}-${index}`}>
            <th scope="row"><code>{batch.experiment.id}</code></th>
            <td><code>{short(versions.checker)}</code></td><td><code>{short(versions.rubric)}</code></td><td><code>{short(versions.judge)}</code></td>
          </tr>))}</tbody>
        </table>
      </div>
    </details>
  </section>;
}
