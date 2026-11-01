import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import type { JudgeDifference, PairedScore } from "../../history/summary-matrix.ts";

function delta(score: PairedScore): string {
  return score.difference === null ? `No paired score (${score.missing} missing)`
    : `${score.difference >= 0 ? "+" : ""}${(score.difference * 100).toFixed(1)} pp (${score.samples} paired, ${score.missing} missing)`;
}

export function JudgeDifferences({ rows }: { rows: readonly JudgeDifference[] }) {
  if (!rows.length) return null;
  return <Card>
    <CardHeader>
      <CardTitle>Judge differences on the same attempts</CardTitle>
      <CardDescription>Second judge minus first judge, in percentage points. Each score uses matching attempts and grading versions. Missing judgments and errors stay counted.</CardDescription>
    </CardHeader>
    <CardContent className="overflow-x-auto">
      <Table>
        <TableHeader><TableRow>
          {["Implementer", "Setup", "First judge", "Second judge", "Rules", "Code", "Intent", "Missing", "Errors", "Different grading"].map((name) => <TableHead key={name}>{name}</TableHead>)}
        </TableRow></TableHeader>
        <TableBody>{rows.map((row) => <TableRow key={JSON.stringify([row.implementer, row.reportedModel, row.setup, row.grading, row.firstJudge, row.secondJudge])}>
          <TableCell>{row.implementer}<span className="block text-xs text-muted-foreground">Reported: {row.reportedModel ?? "unreported"}</span></TableCell><TableCell>{row.setup}</TableCell>
          <TableCell>{row.firstJudge}</TableCell><TableCell>{row.secondJudge}</TableCell>
          <TableCell>{delta(row.rules)}</TableCell><TableCell>{delta(row.code)}</TableCell><TableCell>{delta(row.intent)}</TableCell>
          <TableCell>{row.missing}</TableCell><TableCell>{row.errors}</TableCell><TableCell>{row.incompatible}</TableCell>
        </TableRow>)}</TableBody>
      </Table>
    </CardContent>
  </Card>;
}
