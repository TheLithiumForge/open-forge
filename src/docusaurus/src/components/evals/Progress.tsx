import type { Batch } from "./records/records.types.ts";
import { batchProgress } from "./data/progress.ts";
import { batchDownload, batchPath } from "./data/sources.ts";
import { choiceLabel } from "./data/observations.ts";
import { useSource } from "./source-context.ts";

/** Saves text as a file in the browser, for records too large for a link. */
function save(name: string, text: string): void {
  const url = URL.createObjectURL(new Blob([text], { type: "application/json" }));
  const link = Object.assign(document.createElement("a"), { href: url, download: name });
  link.click();
  URL.revokeObjectURL(url);
}

const share = (part: number, whole: number) => whole === 0 ? "" : ` (${Math.round(part / whole * 100)}%)`;

/** Each batch's completion, so a running batch's preliminary scores can be read with how much of it they cover. */
export function Progress({ batches }: { readonly batches: readonly Batch[] }) {
  const source = useSource();
  if (batches.length === 0) return null;
  return <section className="eval-panel">
    <h3>Batches</h3>
    <p>How far each batch has come. A run counts as implemented once its agent finished. Scores further down are preliminary until every run is checked and judged.</p>
    <div className="eval-scroll" tabIndex={0} role="region" aria-label="Batch progress"><table>
      <thead><tr>{["Batch", "Case", "Implementer", "Status", "Implemented", "Checked", "Judged", "Records"].map((name) => <th key={name}>{name}</th>)}</tr></thead>
      <tbody>{batches.map((batch) => {
        const progress = batchProgress(batch), folder = source.folder(batchPath(batch));
        return <tr key={batch.experiment.id}>
          <th scope="row"><code>{batch.experiment.id}</code></th>
          <td>{batch.experiment.suite}</td>
          <td>{[...new Set(batch.experiment.config.agents.map(choiceLabel))].join(", ")}</td>
          <td>{batch.experiment.status}</td>
          <td><progress value={progress.implemented} max={Math.max(1, progress.planned)} /> {progress.implemented} of {progress.planned}{share(progress.implemented, progress.planned)}</td>
          <td>{progress.checked} of {progress.implemented}</td>
          <td>{progress.judges.length === 0 ? "None yet" : progress.judges.map((judge) => <div key={judge.judge}>{judge.judge}: {judge.judged}</div>)}</td>
          <td><button type="button" className="button button--sm button--link" onClick={() => save(`${batch.experiment.id}.json`, batchDownload(batch))}>Download</button>
            {folder === null ? null : <a href={folder}>Browse files</a>}</td>
        </tr>;
      })}</tbody>
    </table></div>
  </section>;
}
