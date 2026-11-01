import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import type { SetupSummary } from "../../history/summary.ts";
import { number, percent } from "./api.ts";

interface Column {
  readonly label: string;
  readonly value: (row: SetupSummary) => string;
}

export const ResultPanel = { correctness: "correctness", rules: "rules", cost: "cost", wishes: "wishes", picks: "picks" } as const;
type ResultPanel = (typeof ResultPanel)[keyof typeof ResultPanel];
const panelLabels: Readonly<Record<ResultPanel, readonly string[]>> = {
  correctness: ["Runs", "Working gate", "Stated success", "Coverage", "Check errors"], rules: ["Rules", "Code"],
  wishes: ["Intent"], picks: ["Picked"],
  cost: ["Tokens", "Tool calls", "Failed tool calls", "Peak context", "Compactions", "OF reads", "Questions", "Minutes", "Cost", "KB", "Tap p95"],
};

const columns: readonly Column[] = [
  { label: "Runs", value: (row) => `${row.finished}/${row.runs}` },
  { label: "Working gate", value: (row) => percent(row.workingGate) },
  { label: "Stated success", value: (row) => percent(row.statedSuccess) },
  { label: "Coverage", value: (row) => percent(row.coverage) },
  { label: "Check errors", value: (row) => String(row.unresolvedErrors) },
  { label: "Intent", value: (row) => percent(row.intent) },
  { label: "Rules", value: (row) => percent(row.rules) },
  { label: "Code", value: (row) => percent(row.code) },
  { label: "Picked", value: (row) => percent(row.preference) },
  { label: "Tokens", value: (row) => `${number(row.tokens)} (${row.tokenMissing} missing)` },
  { label: "Tool calls", value: (row) => number(row.toolCalls, 1) },
  { label: "Failed tool calls", value: (row) => number(row.failedToolCalls, 1) },
  { label: "Peak context", value: (row) => number(row.peakContext) },
  { label: "Compactions", value: (row) => number(row.compactions, 1) },
  { label: "OF reads", value: (row) => number(row.openForgeReads, 1) },
  { label: "Questions", value: (row) => number(row.questions, 1) },
  { label: "Minutes", value: (row) => number(row.seconds === null ? null : row.seconds / 60, 1) },
  { label: "Cost", value: (row) => (row.costUsd === null ? "–" : `$${row.costUsd.toFixed(2)}`) },
  { label: "KB", value: (row) => number(row.bytes === null ? null : row.bytes / 1024) },
  { label: "Tap p95", value: (row) => (row.tapP95Ms === null ? "–" : `${row.tapP95Ms.toFixed(0)} ms`) },
];

/** One row per setup. Quality and cost stay in separate columns. */
export function SummaryTable({ rows, panel }: { rows: readonly SetupSummary[]; panel?: ResultPanel }) {
  const shown = panel === undefined ? columns : columns.filter((column) => panelLabels[panel].includes(column.label));
  return (
    <div className="overflow-x-auto rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Setup</TableHead>
            {shown.map((column) => (
              <TableHead key={column.label} className="text-right">
                {column.label}
              </TableHead>
            ))}
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.map((row) => (
            <TableRow key={row.setup}>
              <TableCell className="font-medium">{row.setup}</TableCell>
              {shown.map((column) => (
                <TableCell key={column.label} className="text-right tabular-nums">
                  {column.value(row)}
                </TableCell>
              ))}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
