import { useState } from "react";
import { usePluginData } from "@docusaurus/useGlobalData";
import Tabs from "@theme/Tabs";
import TabItem from "@theme/TabItem";
import type { EvalsData } from "./data/snapshot.types.ts";
import { fetchLatest } from "./data/load.ts";
import { AnalysisVersion } from "./data/analysis.ts";
import { Layers } from "./data/observations.ts";
import { Results } from "./Results.tsx";
import { History } from "./History.tsx";
import "./evals.css";
export default function Evals({ history = false }: {
  readonly history?: boolean;
}) {
  // Docusaurus exposes plugin data as an untyped JSON object. loadContent validates this payload.
  const data = usePluginData("open-forge-evals") as EvalsData;
  const [snapshot, setSnapshot] = useState(data.snapshot);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  async function refresh() {
    setBusy(true);
    setError(null);
    try {
      setSnapshot(await fetchLatest(data));
    }
    catch (reason: unknown) {
      setError(reason instanceof Error ? reason.message : String(reason));
    }
    finally {
      setBusy(false);
    }
  }
  const layers: readonly (number | null)[] = snapshot.batches.some((batch) => batch.experiment.config.layer === undefined) ? [...Layers, null] : Layers;
  return <div className="evals">
    <div className="alert alert--secondary eval-source">
      <p>Data commit: <code>{snapshot.commit}</code>. Loaded: <time dateTime={snapshot.fetchedAt}>{snapshot.fetchedAt}</time>. Analysis: {AnalysisVersion}.</p>
      <button type="button" className="button button--secondary" disabled={busy} onClick={() => void refresh()}>{busy ? "Fetching results" : "Fetch latest results"}</button>
      <span role="status" aria-live="polite">{busy ? " Loading and validating every record." : " All displayed records passed validation."}</span>
      {error === null ? null : <p role="alert">{error}. The previous validated results are still shown.</p>}
    </div>
    {snapshot.batches.length === 0 ? <p>No batches are published yet.</p> : null}
    <Tabs defaultValue="1" groupId="eval-layers" queryString="layer" lazy>
      {layers.map((layer) => <TabItem key={layer ?? "unknown"} value={String(layer ?? "unknown")} label={layer === null ? "Unreported layer" : `Layer ${layer}`}>
        {history ? <History batches={snapshot.batches} layer={layer} /> : <Results batches={snapshot.batches} layer={layer} />}
      </TabItem>)}
    </Tabs>
  </div>;
}
