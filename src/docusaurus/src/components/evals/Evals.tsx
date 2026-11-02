import { useEffect, useRef, useState, type ChangeEvent } from "react";
import { usePluginData } from "@docusaurus/useGlobalData";
import { useLocation } from "@docusaurus/router";
import Tabs from "@theme/Tabs";
import TabItem from "@theme/TabItem";
import type { EvalsData } from "./data/snapshot.types.ts";
import { fetchLatest } from "./data/load.ts";
import { AnalysisVersion } from "./data/analysis.ts";
import { Layers } from "./data/observations.ts";
import { ArchiveUrl, commitSource, folderFiles, loadFolder, loadLocal, localPort, SourceKind, type Loaded } from "./data/sources.ts";
import { SourceContext } from "./source-context.ts";
import { Results } from "./Results.tsx";
import { History } from "./History.tsx";
import "./evals.css";

export default function Evals({ history = false }: {
  readonly history?: boolean;
}) {
  // Docusaurus exposes plugin data as an untyped JSON object. loadContent validates this payload.
  const data = usePluginData("open-forge-evals") as EvalsData;
  const published: Loaded = { snapshot: data.snapshot, source: commitSource(SourceKind.published, "Published copy from this site's build", data.snapshot.commit) };
  const [shown, setShown] = useState<Loaded>(published);
  const [busy, setBusy] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const port = localPort(useLocation().search);
  const folderInput = useRef<HTMLInputElement>(null);

  async function load(task: string, read: () => Promise<Loaded>) {
    setBusy(task);
    setError(null);
    try { setShown(await read()); }
    catch (reason: unknown) { setError(reason instanceof Error ? reason.message : String(reason)); }
    finally { setBusy(null); }
  }
  const latest = () => load("Fetching the latest published results", async () => {
    const snapshot = await fetchLatest(data);
    return { snapshot, source: commitSource(SourceKind.latest, "Latest published results", snapshot.commit) };
  });
  const local = (value: number) => load("Reading local runs", () => loadLocal(data, value));
  function openFolder(event: ChangeEvent<HTMLInputElement>) {
    const picked = event.target.files;
    if (picked === null || picked.length === 0) return;
    void load("Reading the folder", async () => {
      const files = folderFiles(Array.from(picked));
      return loadFolder(data, files, picked[0]?.webkitRelativePath.split("/")[0] ?? "folder");
    });
    event.target.value = "";
  }
  // The site command opens this page with the local data server's port, so local runs show first.
  useEffect(() => { if (port !== null) void local(port); }, [port]);

  const { snapshot, source } = shown;
  const layers: readonly (number | null)[] = snapshot.batches.some((batch) => batch.experiment.config.layer === undefined) ? [...Layers, null] : Layers;
  return <SourceContext.Provider value={source}>
    <div className="evals">
      <div className="alert alert--secondary eval-source">
        <p><strong>{source.label}.</strong> {source.kind === SourceKind.local || source.kind === SourceKind.folder ? "Running and unpublished batches are included." : <>Data commit <code>{snapshot.commit}</code>.</>} Loaded <time dateTime={snapshot.fetchedAt}>{snapshot.fetchedAt}</time>. Analysis {AnalysisVersion}.</p>
        <div className="eval-actions">
          <button type="button" className="button button--sm button--secondary" disabled={busy !== null || source.kind === SourceKind.published} onClick={() => { setError(null); setShown(published); }}>Published copy</button>
          <button type="button" className="button button--sm button--secondary" disabled={busy !== null} onClick={() => void latest()}>Fetch latest results</button>
          {port === null ? null : <button type="button" className="button button--sm button--primary" disabled={busy !== null} onClick={() => void local(port)}>{source.kind === SourceKind.local ? "Refresh local runs" : "Local runs"}</button>}
          <button type="button" className="button button--sm button--secondary" disabled={busy !== null} onClick={() => folderInput.current?.click()}>Open a results folder</button>
          <a className="button button--sm button--link" href={ArchiveUrl}>Download all results</a>
          <input ref={folderInput} type="file" hidden multiple onChange={openFolder} {...{ webkitdirectory: "" }} />
        </div>
        <p role="status" aria-live="polite">{busy === null ? "Every shown record passed validation." : `${busy}. Every record is validated before it shows.`}</p>
        {error === null ? null : <p role="alert">{error}. The previous results are still shown.</p>}
      </div>
      {snapshot.batches.length === 0 ? <p>No batches are published yet.</p> : null}
      <Tabs defaultValue="1" groupId="eval-layers" queryString="layer" lazy>
        {layers.map((layer) => <TabItem key={layer ?? "unknown"} value={String(layer ?? "unknown")} label={layer === null ? "Unreported layer" : `Layer ${layer}`}>
          {history ? <History batches={snapshot.batches} layer={layer} /> : <Results batches={snapshot.batches} layer={layer} />}
        </TabItem>)}
      </Tabs>
    </div>
  </SourceContext.Provider>;
}
