import { useEffect, useState } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import type { ReferenceTrend } from "../../history/reference-trend.ts";
import { agentLabel } from "../../history/summary.ts";
import { api, percent } from "./api.ts";

export function ReferenceHistory({ groups }: { readonly groups: readonly ReferenceTrend[] }) {
  return <div className="space-y-4">
    {groups.length === 0 ? <p className="text-sm text-muted-foreground">No reference assessments yet.</p> : null}
    {groups.map(({ key, grading, points }) => {
      const areas = [...new Set(points.flatMap((point) => Object.keys(point.scores)))];
      return <Card key={key}>
      <CardHeader>
        <CardTitle>Reference. Layer {grading.layer}. {grading.model === null ? "Checks" : agentLabel(grading.model)}</CardTitle>
        <CardDescription>Each row is a saved assessment. Matching source, spec and grading versions share a group.</CardDescription>
        <details className="break-all text-xs text-muted-foreground">
          <summary>Source and grading versions</summary>
          <p>Reference: {grading.referenceHash}</p><p>Spec {grading.specVersion}: {grading.specHash}</p>
          <p>Checker: {grading.checker}</p><p>Rubric: {grading.rubric}</p><p>Judge prompt: {grading.judge}</p>
          <p>Reported judge: {grading.reportedModel ?? "unreported"}. Ballots: {grading.ballots ?? "unreported"}.</p>
        </details>
      </CardHeader>
      <CardContent className="overflow-x-auto">
        <Table>
          <TableHeader><TableRow>{["Graded", "Commit", "Coverage", ...areas, "Error"].map((label) => <TableHead key={label}>{label}</TableHead>)}</TableRow></TableHeader>
          <TableBody>{points.map((point) => <TableRow key={point.assessmentId}>
            <TableCell>{point.at === null ? "Unreported" : new Date(point.at).toLocaleString()}</TableCell>
            <TableCell title={point.commit}><code>{point.commit.slice(0, 12)}</code></TableCell>
            <TableCell>{percent(point.coverage)}</TableCell>{areas.map((area) => <TableCell key={area}>{percent(point.scores[area] ?? null)}</TableCell>)}
            <TableCell className="whitespace-normal">{point.error ?? "None"}</TableCell>
          </TableRow>)}</TableBody>
        </Table>
      </CardContent>
    </Card>;
    })}
  </div>;
}

export function ReferenceHistoryPanel({ refreshKey }: { readonly refreshKey: string }) {
  const [groups, setGroups] = useState<readonly ReferenceTrend[]>([]);
  const [error, setError] = useState<string | null>(null);
  useEffect(() => {
    let active = true;
    api.referenceHistory().then((data) => {
      if (active) { setGroups(data); setError(null); }
    }).catch((reason: unknown) => { if (active) setError(String(reason)); });
    return () => { active = false; };
  }, [refreshKey]);
  return <Card className="min-w-0 lg:col-span-2">
    <CardHeader><CardTitle>Reference over time</CardTitle><CardDescription>Reference grades stay separate from model setup comparisons.</CardDescription></CardHeader>
    <CardContent>{error === null ? <ReferenceHistory groups={groups} /> : <p role="alert">{error}</p>}</CardContent>
  </Card>;
}
