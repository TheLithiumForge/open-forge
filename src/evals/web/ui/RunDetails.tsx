import { CircleCheck, CircleHelp, CircleX, ExternalLink } from "lucide-react";
import { AccordionContent, AccordionItem, AccordionTrigger } from "@/components/ui/accordion";
import { Badge } from "@/components/ui/badge";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import type { RunRecord } from "../../history/results.types.ts";
import { CheckStatus, RunStatus } from "../../history/statuses.ts";
import { number, shotUrl } from "./api.ts";

function CheckIcon({ status }: { status: CheckStatus }) {
  if (status === CheckStatus.blocked || status === CheckStatus.error) return <CircleHelp className="size-4 shrink-0 text-warning" />;
  return status === CheckStatus.pass ? <CircleCheck className="size-4 shrink-0 text-success" /> : <CircleX className="size-4 shrink-0 text-destructive" />;
}

function Stat({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-md border px-3 py-2">
      <div className="text-xs text-muted-foreground">{label}</div>
      <div className="font-medium tabular-nums">{value}</div>
    </div>
  );
}

/** Everything one run recorded: numbers, questions, checks, judge answers and screenshots. */
export function RunDetails({ run }: { run: RunRecord }) {
  const telemetry = run.telemetry;
  const tokens = telemetry?.tokens;
  const passed = run.functionalResult?.coverage.passed ?? 0;
  const applicable = run.functionalResult?.coverage.applicable ?? 0;
  return (
    <AccordionItem value={run.id}>
      <AccordionTrigger>
        <span className="flex flex-wrap items-center gap-2">
          <Badge variant="outline">{run.setup}</Badge>
          <Badge variant={run.status === RunStatus.done ? "secondary" : "destructive"}>{run.status}</Badge>
          <span className="text-muted-foreground">{passed}/{applicable} checks</span>
        </span>
      </AccordionTrigger>
      <AccordionContent className="space-y-4">
        {run.error === null ? null : <p className="text-sm text-destructive">{run.error.slice(0, 400)}</p>}
        {run.checksError === null ? null : <p className="text-sm text-destructive">Checks: {run.checksError}</p>}
        {run.judgeError === null ? null : <p className="text-sm text-destructive">Judge: {run.judgeError}</p>}
        <div className="grid grid-cols-2 gap-2 sm:grid-cols-4 xl:grid-cols-7">
          <Stat label="Tokens" value={number(tokens === null || tokens === undefined ? null : tokens.input + tokens.cachedInput + tokens.cacheWrite + tokens.output)} />
          <Stat label="Peak context" value={number(telemetry?.peakContext ?? null)} />
          <Stat label="Compactions" value={number(telemetry?.compactions ?? null)} />
          <Stat label="Tool calls" value={String(telemetry?.toolCalls ?? "–")} />
          <Stat label="Failed tool calls" value={String(telemetry?.failedToolCalls ?? "–")} />
          <Stat label="Open Forge reads" value={String(telemetry?.openForgeReads ?? "–")} />
          <Stat label="Agent" value={`${number(run.agentSeconds === null ? null : run.agentSeconds / 60, 1)} min`} />
          <Stat label="Build" value={run.build === null ? "–" : `${run.build.passed ? "passed" : "failed"} · ${run.build.seconds.toFixed(0)} s`} />
        </div>
        {run.questions.length === 0 ? null : (
          <div className="space-y-2">
            <p className="text-sm font-medium">Questions</p>
            {run.questions.map((round, index) => (
              <div key={index} className="rounded-md border p-3 text-sm">
                <p className="whitespace-pre-wrap text-muted-foreground">{round.question.slice(0, 600)}</p>
                <p className="mt-2">↳ {round.answer}</p>
              </div>
            ))}
          </div>
        )}
        <div className="space-y-2">
          <p className="text-sm font-medium">Hidden checks</p>
          <ul className="space-y-1.5 text-sm">
            {run.checkOutcomes.map((check) => (
              <li key={check.id} className="flex gap-2">
                <CheckIcon status={check.status} />
                <span>
                  <span className="font-medium">{check.area} ({check.status}):</span> <span className="text-muted-foreground">{check.evidence}</span>
                </span>
              </li>
            ))}
          </ul>
        </div>
        {run.judge.length === 0 ? null : (
          <div className="overflow-x-auto rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Judge question</TableHead>
                  <TableHead className="text-right">Yes</TableHead>
                  <TableHead className="text-right">No</TableHead>
                  <TableHead className="text-right">Unsure</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {run.judge.map((answer) => (
                  <TableRow key={answer.id} title={answer.evidence.join("\n\n")}>
                    <TableCell className="whitespace-normal">{answer.question}</TableCell>
                    <TableCell className="text-right">{answer.yes}</TableCell>
                    <TableCell className="text-right">{answer.no}</TableCell>
                    <TableCell className="text-right">{answer.unsure}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        )}
        <div className="flex flex-wrap gap-2">
          {run.shots.map((shot) => (
            <a key={shot} href={shotUrl(run.batchId, run.id, shot)} target="_blank" rel="noreferrer">
              <img src={shotUrl(run.batchId, run.id, shot)} alt={shot} className="h-40 rounded-md border" />
            </a>
          ))}
        </div>
        <a className="inline-flex items-center gap-1 text-sm text-chart-2 hover:underline" href={`http://${run.id}.localhost:${window.location.port}/`} target="_blank" rel="noreferrer">
          <ExternalLink className="size-4" /> Open this game
        </a>
      </AccordionContent>
    </AccordionItem>
  );
}
