import { FlaskConical } from "lucide-react";
import { useCallback, useEffect, useState } from "react";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import type { StateResponse } from "../server/api.types.ts";
import { api } from "./api.ts";
import { BatchPanel } from "./BatchPanel.tsx";
import { SetupPanel } from "./SetupPanel.tsx";
import { StatusBadges } from "./StatusBadges.tsx";
import { ReferenceHistoryPanel } from "./ReferenceHistory.tsx";

export function App() {
  const [state, setState] = useState<StateResponse | null>(null);
  const [selected, setSelected] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const refresh = useCallback(() => {
    api.state().then(setState, (reason: unknown) => setError(String(reason)));
  }, []);
  useEffect(refresh, [refresh]);

  async function run() {
    try {
      setError(null);
      const batch = await api.startBatch();
      setSelected(batch.id);
      refresh();
    } catch (reason) {
      setError(String(reason));
    }
  }

  const current = selected ?? state?.running ?? state?.batches[0]?.id ?? null;
  return (
    <div className="min-h-screen">
      <header className="border-b">
        <div className="mx-auto flex max-w-[1600px] flex-wrap items-center gap-3 px-6 py-4">
          <FlaskConical className="size-5 text-chart-2" />
          <h1 className="text-lg font-semibold">Open Forge Evals</h1>
          <span className="text-muted-foreground">{state?.suite.title ?? ""}</span>
          <div className="ml-auto">{state === null ? null : <StatusBadges state={state} />}</div>
        </div>
      </header>
      <main className="mx-auto grid max-w-[1600px] items-start gap-6 p-6 lg:grid-cols-[420px_minmax(0,1fr)]">
        {error === null ? null : (
          <Alert variant="destructive" className="lg:col-span-2">
            <AlertTitle>Something went wrong</AlertTitle>
            <AlertDescription>{error}</AlertDescription>
          </Alert>
        )}
        {state === null ? (
          <p className="text-muted-foreground">Loading…</p>
        ) : (
          <>
            <SetupPanel state={state} onChange={setState} onRun={run} />
            <BatchPanel state={state} current={current} onSelect={setSelected} onChange={refresh} />
            <ReferenceHistoryPanel refreshKey={JSON.stringify(state.batches.map((batch) => [batch.id, batch.status]))} />
          </>
        )}
      </main>
    </div>
  );
}
