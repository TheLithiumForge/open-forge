import { Card, CardAction, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import type { StateResponse } from "../src/server/api.types.ts";
import { BatchView } from "./BatchView.tsx";

interface BatchPanelProps {
  readonly state: StateResponse;
  readonly current: string | null;
  readonly onSelect: (batchId: string) => void;
  readonly onChange: () => void;
}

/** Every batch run so far, newest first, and the chosen one's progress, games and results. */
export function BatchPanel({ state, current, onSelect, onChange }: BatchPanelProps) {
  return (
    <Card className="min-w-0">
      <CardHeader>
        <CardTitle>Batches</CardTitle>
        <CardDescription>Each batch runs every chosen setup on every agent, in random order.</CardDescription>
        <CardAction>
          {state.batches.length === 0 ? null : (
            <Select value={current ?? ""} onValueChange={onSelect}>
              <SelectTrigger className="w-72">
                <SelectValue placeholder="Pick a batch" />
              </SelectTrigger>
              <SelectContent>
                {state.batches.map((batch) => (
                  <SelectItem key={batch.id} value={batch.id}>
                    {new Date(batch.createdAt).toLocaleString()} · {batch.config.agents.map((agent) => agent.model ?? agent.tool).join(", ")} · {batch.status}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          )}
        </CardAction>
      </CardHeader>
      <CardContent>
        {current === null ? (
          <p className="text-sm text-muted-foreground">No batches yet. Check the setup, then press Run.</p>
        ) : (
          <BatchView key={current} batchId={current} onChange={onChange} />
        )}
      </CardContent>
    </Card>
  );
}
