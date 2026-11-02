import { useEffect, useState } from "react";
import { usePluginData } from "@docusaurus/useGlobalData";
import { useLocation } from "@docusaurus/router";
import type { EvalsData } from "./data/snapshot.types.ts";
import { fetchLatest } from "./data/load.ts";
import { commitSource, folderFiles, loadFolder, loadLocal, localPort, SourceKind, type Loaded } from "./data/sources.ts";

export interface Sources {
  readonly shown: Loaded;
  /** What is loading right now, or null. */
  readonly busy: string | null;
  readonly error: string | null;
  /** The local data server's port when the site command opened this page. */
  readonly port: number | null;
  readonly published: () => void;
  readonly latest: () => void;
  readonly local: () => void;
  readonly folder: (picked: FileList) => void;
}

/** Where the shown records come from. Every source validates each record before it shows. */
export function useSources(): Sources {
  // Docusaurus exposes plugin data as an untyped JSON object. loadContent validates this payload.
  const data = usePluginData("open-forge-evals") as EvalsData;
  const initial: Loaded = { snapshot: data.snapshot, source: commitSource(SourceKind.published, "Published copy from this site's build", data.snapshot.commit) };
  const [shown, setShown] = useState<Loaded>(initial);
  const [busy, setBusy] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const port = localPort(useLocation().search);

  async function load(task: string, read: () => Promise<Loaded>) {
    setBusy(task);
    setError(null);
    try { setShown(await read()); }
    catch (reason: unknown) { setError(reason instanceof Error ? reason.message : String(reason)); }
    finally { setBusy(null); }
  }
  const local = () => { if (port !== null) void load("Reading local runs", () => loadLocal(data, port)); };
  // The site command opens this page with the local data server's port, so local runs show first.
  useEffect(local, [port]);

  return {
    shown, busy, error, port, local,
    published: () => { setError(null); setShown(initial); },
    latest: () => void load("Fetching the latest published results", async () => {
      const snapshot = await fetchLatest(data);
      return { snapshot, source: commitSource(SourceKind.latest, "Latest published results", snapshot.commit) };
    }),
    folder: (picked) => {
      const files = Array.from(picked);
      if (files.length === 0) return;
      void load("Reading the folder", () => loadFolder(data, folderFiles(files), files[0]?.webkitRelativePath.split("/")[0] ?? "folder"));
    },
  };
}
