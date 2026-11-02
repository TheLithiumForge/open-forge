import { useRef } from "react";
import type { Batch } from "./records/records.types.ts";
import { ArchiveUrl, batchDownload, batchPath, SourceKind } from "./data/sources.ts";
import { startingPointOf } from "./data/cells.ts";
import { startingPointTitle } from "./data/cell-labels.ts";
import { JsonType, save } from "./download.ts";
import type { Sources } from "./use-sources.ts";
import { useSource } from "./source-context.ts";

/** A folder picker. Browsers read this attribute, but React's input types don't list it. */
const PickFolder = { webkitdirectory: "" };

function BatchDownloads({ batches }: { readonly batches: readonly Batch[] }) {
  const source = useSource();
  return <div className="eval-scroll" tabIndex={0} role="region" aria-label="Batch downloads">
    <table className="eval-table">
      <thead><tr>{["Batch", "Starting point", "Records", "Files"].map((name) => <th key={name} scope="col">{name}</th>)}</tr></thead>
      <tbody>{batches.map((batch) => {
        const point = startingPointOf(batch), folder = source.folder(batchPath(batch));
        return <tr key={batch.experiment.id}>
          <th scope="row"><code>{batch.experiment.id}</code></th>
          <td>{point === null ? "Not reported" : startingPointTitle(point)}</td>
          <td><button type="button" className="eval-link-button" onClick={() => save(`${batch.experiment.id}.json`, batchDownload(batch), JsonType)}>Download JSON</button></td>
          <td>{folder === null ? "Not published" : <a href={folder}>Browse files</a>}</td>
        </tr>;
      })}</tbody>
    </table>
  </div>;
}

/** Where the records come from, and the downloads of them. */
export function DataSources({ sources }: { readonly sources: Sources }) {
  const picker = useRef<HTMLInputElement>(null);
  const { shown, busy, error, port } = sources;
  const kind = shown.source.kind;
  const choice = (label: string, active: boolean, pick: () => void) =>
    <button type="button" aria-pressed={active} disabled={busy !== null} onClick={pick}>{label}</button>;
  return <section className="eval-section" aria-labelledby="eval-data">
    <h2 id="eval-data">Data and downloads</h2>
    <p>Choose where the records come from. Each record is checked against the published JSON Schemas before it shows.</p>
    <div className="eval-segmented" role="group" aria-label="Results source">
      {choice("Published", kind === SourceKind.published, sources.published)}
      {choice("Latest", kind === SourceKind.latest, sources.latest)}
      {port === null ? null : choice("Local", kind === SourceKind.local, sources.local)}
      {choice("Open a folder", kind === SourceKind.folder, () => picker.current?.click())}
    </div>
    <input ref={picker} type="file" hidden multiple onChange={(event) => { if (event.target.files !== null) sources.folder(event.target.files); event.target.value = ""; }} {...PickFolder} />
    <ul className="eval-notes">
      <li>Published - the results repository at the commit this site was built from.</li>
      <li>Latest - the results repository at its newest commit, fetched now.</li>
      {port === null ? null : <li>Local - your own runs from the data server on port {port}, including batches that are still running. Select it again to refresh.</li>}
      <li>Open a folder - a results repository on your computer, such as an unzipped copy of the full download.</li>
    </ul>
    <p role="status" className="eval-muted">{busy === null ? "" : `${busy}.`}</p>
    {error === null ? null : <p role="alert" className="eval-error">{error}. The previous results still show.</p>}
    <h3>Downloads</h3>
    <ul className="eval-notes">
      <li><a href={ArchiveUrl}>Download all</a> - a zip of the results repository with every batch's records, its evidence files and the JSON Schemas.</li>
      <li>Download JSON - one batch's experiment, setups, runs, assessments and artifact list in one file. Its evidence files stay in the repository, under Browse files.</li>
      <li>Download CSV - in All results, the table rows shown, with raw numbers.</li>
    </ul>
    <details className="eval-details">
      <summary>Batch downloads, {shown.snapshot.batches.length} {shown.snapshot.batches.length === 1 ? "batch" : "batches"}</summary>
      <BatchDownloads batches={shown.snapshot.batches} />
    </details>
  </section>;
}
