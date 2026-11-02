import { SourceContext } from "./source-context.ts";
import { useSources } from "./use-sources.ts";
import { InfoProvider } from "./info/InfoProvider.tsx";
import { Results } from "./Results.tsx";
import { History } from "./History.tsx";
import { DataSources } from "./DataSources.tsx";
import "./evals.css";

/** UTC, so the server's render and the browser's agree. */
const LoadedFormat = new Intl.DateTimeFormat("en", { dateStyle: "medium", timeStyle: "short", timeZone: "UTC" });

/** The Results page, or with `history` the History page. Both read the same records and end with the source choice. */
export default function Evals({ history = false }: {
  readonly history?: boolean;
}) {
  const sources = useSources();
  const { snapshot, source } = sources.shown;
  const loaded = new Date(snapshot.fetchedAt);
  return <SourceContext.Provider value={source}>
    <div className="evals">
      <InfoProvider>
        <p className="eval-source-line">
          {source.label}. Loaded <time dateTime={snapshot.fetchedAt}>{Number.isNaN(loaded.getTime()) ? snapshot.fetchedAt : `${LoadedFormat.format(loaded)} UTC`}</time>.
          {" "}<a href="#eval-data">Change the source</a>{sources.busy === null ? null : <span className="eval-muted"> {sources.busy}.</span>}
        </p>
        {snapshot.batches.length === 0 ? <p>No batches are published yet.</p> : null}
        {history ? <History batches={snapshot.batches} /> : <Results batches={snapshot.batches} />}
        <DataSources sources={sources} />
      </InfoProvider>
    </div>
  </SourceContext.Provider>;
}
