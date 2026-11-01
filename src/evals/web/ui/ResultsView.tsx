import { Eye, EyeOff } from "lucide-react";
import { Accordion } from "@/components/ui/accordion";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { agentLabel } from "../../history/summary.ts";
import type { BatchResponse } from "../server/api.types.ts";
import { api } from "./api.ts";
import { QualityChart, TokensChart } from "./Charts.tsx";
import { RunDetails } from "./RunDetails.tsx";
import { SummaryTable } from "./SummaryTable.tsx";

interface ResultsViewProps {
  readonly view: BatchResponse;
  readonly onChange: (view: BatchResponse) => void;
}

/** One table and two charts per tool and model, since setups are only compared within one. */
export function ResultsView({ view, onChange }: ResultsViewProps) {
  if (view.summary === null || view.runs === null) {
    return (
      <Alert className="mt-2">
        <EyeOff />
        <AlertTitle>Results are hidden</AlertTitle>
        <AlertDescription>
          <p>They stay hidden until you reveal the setups, so they can't sway the blind comparison.</p>
          <Button className="mt-2" size="sm" onClick={() => api.reveal(view.batch.id).then(onChange)}>
            <Eye /> Reveal setups and results
          </Button>
        </AlertDescription>
      </Alert>
    );
  }
  const agents = [...new Set(view.summary.map((row) => row.agent))];
  return (
    <div className="space-y-4 pt-2">
      {agents.map((agent) => {
        const rows = (view.summary ?? []).filter((row) => row.agent === agent);
        const runs = (view.runs ?? []).filter((run) => agentLabel(run.agent) === agent);
        return (
          <Card key={agent}>
            <CardHeader>
              <CardTitle>{agent}</CardTitle>
              <CardDescription>
                Coverage counts every applicable check. The working gate and stated success use their declared checks. Intent, Rules, Code and blind picks stay separate. Missing tokens are reported beside the measured mean.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <SummaryTable rows={rows} />
              <div className="grid gap-4 2xl:grid-cols-2">
                <QualityChart rows={rows} />
                <TokensChart runs={runs} />
              </div>
              <Accordion type="multiple">
                {runs.map((run) => (
                  <RunDetails key={run.id} run={run} />
                ))}
              </Accordion>
            </CardContent>
          </Card>
        );
      })}
    </div>
  );
}
