import { Eye, EyeOff } from "lucide-react";
import { Accordion } from "@/components/ui/accordion";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { agentLabel } from "../../history/summary.ts";
import type { BatchResponse } from "../server/api.types.ts";
import { api } from "./api.ts";
import { RunDetails } from "./RunDetails.tsx";
import { ResultPanel, SummaryTable } from "./SummaryTable.tsx";

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
  const groups = view.gradingGroups ?? [{ grading: "Historical grading", reportedModel: null, summary: view.summary, runs: view.runs }];
  return (
    <div className="space-y-4 pt-2">
      {groups.flatMap((group) => [...new Set(group.summary.map((row) => row.agent))].map((agent) => {
        const rows = group.summary.filter((row) => row.agent === agent);
        const runs = group.runs.filter((run) => agentLabel(run.agent) === agent);
        return (
          <Card key={`${group.grading}/${group.reportedModel}/${agent}`}>
            <CardHeader>
              <CardTitle>{agent}. Reported model: {group.reportedModel ?? "unreported"}</CardTitle>
              <CardDescription>
                Coverage counts every applicable check. The working gate and stated success use their declared checks. Intent, Rules, Code and blind picks stay separate. Missing tokens are reported beside the measured mean.
              </CardDescription>
              <p className="break-all text-xs text-muted-foreground">Grading versions: {group.grading}</p>
            </CardHeader>
            <CardContent className="space-y-6">
              {Object.values(ResultPanel).map((panel) => <Card key={panel}>
                <CardHeader><CardTitle className="capitalize">{panel}</CardTitle></CardHeader>
                <CardContent><SummaryTable rows={rows} panel={panel} /></CardContent>
              </Card>)}
              <Accordion type="multiple">
                {runs.map((run) => (
                  <RunDetails key={run.id} run={run} />
                ))}
              </Accordion>
            </CardContent>
          </Card>
        );
      }))}
    </div>
  );
}
