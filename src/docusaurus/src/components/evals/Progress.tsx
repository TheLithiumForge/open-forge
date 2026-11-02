import type { Batch } from "./records/records.types.ts";
import { batchProgress } from "./data/progress.ts";
import { modelLabel } from "./data/names.ts";
import { startingPointOf } from "./data/cells.ts";
import { startingPointTitle } from "./data/cell-labels.ts";
import { Term } from "./info/Term.tsx";

const share = (part: number, whole: number) => whole === 0 ? "" : ` (${Math.round(part / whole * 100)}%)`;

/** Each batch's completion, so a running batch's preliminary scores can be read with how much of it they cover. */
export function Progress({ batches }: { readonly batches: readonly Batch[] }) {
  if (batches.length === 0) return <p>No batches match the filters.</p>;
  return <div className="eval-scroll" tabIndex={0} role="region" aria-label="Batch progress">
    <table className="eval-table">
      <thead><tr>{["Batch", "Starting point", "Models", "Status", "Finished runs", "Checked", "Judged"].map((name) => <th key={name} scope="col">{name}</th>)}</tr></thead>
      <tbody>{batches.map((batch) => {
        const progress = batchProgress(batch), point = startingPointOf(batch);
        return <tr key={batch.experiment.id}>
          <th scope="row"><code>{batch.experiment.id}</code></th>
          <td>{point === null ? "Not reported" : <Term id={point} batch={batch}>{startingPointTitle(point)}</Term>}<div className="eval-muted">{batch.experiment.suite}</div></td>
          <td>{[...new Set(batch.experiment.config.agents.map((agent) => modelLabel(agent)))].join(", ")}</td>
          <td>{batch.experiment.status}</td>
          <td><progress value={progress.implemented} max={Math.max(1, progress.planned)} /> {progress.implemented} of {progress.planned}{share(progress.implemented, progress.planned)}</td>
          <td>{progress.checked} of {progress.implemented}</td>
          <td>{progress.judges.length === 0 ? "None yet" : progress.judges.map((judge) => <div key={judge.judge}>{judge.judge}: {judge.judged}</div>)}</td>
        </tr>;
      })}</tbody>
    </table>
  </div>;
}
