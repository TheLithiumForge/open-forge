import { ChartColumn, Gamepad2, RotateCcw } from "lucide-react";
import { useCallback, useEffect, useState } from "react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { cn } from "@/lib/utils";
import type { ProgressMessage } from "../src/batch/progress.ts";
import { BatchStatus, RunStatus } from "../src/results/statuses.ts";
import type { BatchResponse } from "../src/server/api.types.ts";
import { api } from "./api.ts";
import { BlindCompare } from "./BlindCompare.tsx";
import { ResultsView } from "./ResultsView.tsx";

const RefreshMs = 5000;
const Tab = { compare: "compare", results: "results" } as const;
const runColors: Readonly<Record<string, string>> = {
  [RunStatus.done]: "bg-success",
  [RunStatus.failed]: "bg-destructive",
  [RunStatus.limited]: "bg-destructive",
  [RunStatus.pending]: "bg-muted",
};

interface BatchViewProps {
  readonly batchId: string;
  readonly onChange: () => void;
}

export function BatchView({ batchId, onChange }: BatchViewProps) {
  const [view, setView] = useState<BatchResponse | null>(null);
  const [log, setLog] = useState<ProgressMessage[]>([]);
  const refresh = useCallback(() => {
    api.batch(batchId).then((next) => {
      setView(next);
      setLog((current) => (current.length === 0 ? [...next.progress] : current));
    }, () => undefined);
  }, [batchId]);

  useEffect(() => {
    refresh();
    const events = new EventSource(`/api/events?batch=${batchId}`);
    events.onmessage = (event: MessageEvent<string>) => {
      const message: ProgressMessage = JSON.parse(event.data);
      setLog((current) => [...current, message]);
      refresh();
    };
    const timer = setInterval(refresh, RefreshMs);
    return () => {
      events.close();
      clearInterval(timer);
    };
  }, [batchId, refresh]);

  if (view === null) return <p className="text-sm text-muted-foreground">Loading the batch…</p>;
  const { batch } = view;
  const finished = view.hidden.filter((run) => run.status === RunStatus.done).length;
  const resumable = batch.status === BatchStatus.paused || batch.status === BatchStatus.failed;
  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center gap-2">
        <Badge variant={batch.status === BatchStatus.failed ? "destructive" : batch.status === BatchStatus.done ? "default" : "secondary"}>{batch.status}</Badge>
        <span className="text-sm text-muted-foreground">
          {finished} of {view.hidden.length} runs done · eval <code>{batch.fingerprint}</code>
        </span>
        {resumable ? (
          <Button size="sm" variant="outline" className="ml-auto" onClick={() => api.resume(batchId).then(() => (refresh(), onChange()))}>
            <RotateCcw /> Resume
          </Button>
        ) : null}
      </div>
      <Progress value={(finished / Math.max(1, view.hidden.length)) * 100} />
      <div className="flex flex-wrap gap-1.5">
        {view.hidden.map((run, index) => (
          <span key={run.id} title={`Run ${index + 1}: ${run.status}`} className={cn("flex size-7 items-center justify-center rounded-md text-xs font-medium", runColors[run.status] ?? "animate-pulse bg-chart-1 text-white")}>
            {index + 1}
          </span>
        ))}
      </div>
      <ScrollArea className="h-36 rounded-md border bg-muted/30">
        <div className="space-y-0.5 p-3 font-mono text-xs text-muted-foreground">
          {log.length === 0 ? <div>No progress messages since the eval server started.</div> : null}
          {log.slice(-60).map((message, index) => (
            <div key={index}>
              <span className="text-foreground/60">{new Date(message.at).toLocaleTimeString()}</span> {message.text}
            </div>
          ))}
        </div>
      </ScrollArea>
      <Tabs defaultValue={Tab.compare}>
        <TabsList>
          <TabsTrigger value={Tab.compare}>
            <Gamepad2 /> Play and compare
          </TabsTrigger>
          <TabsTrigger value={Tab.results}>
            <ChartColumn /> Results
          </TabsTrigger>
        </TabsList>
        <TabsContent value={Tab.compare}>
          <BlindCompare view={view} onChange={setView} />
        </TabsContent>
        <TabsContent value={Tab.results}>
          <ResultsView view={view} onChange={setView} />
        </TabsContent>
      </Tabs>
    </div>
  );
}
