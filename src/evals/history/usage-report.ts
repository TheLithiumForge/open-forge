import type { RunView } from "./records.types.ts";
import type { BatchRecord } from "./results.types.ts";
import { FailureKind, RunStatus } from "./statuses.ts";
import type { HistoryStore } from "./store.ts";
import { ContextClass, ToolKind } from "./analysis/classes.ts";
import { analyzeSavedRun } from "./analysis/saved-run.ts";
import { setupUsage, type SetupUsage } from "./analysis/setup-summary.ts";

/** How many file types the report shows, largest first. The rest add up in the last column. */
const ShownFileTypes = 6;
const thousands = (value: number | null | undefined): string => value == null ? "-" : `${Math.round(value / 1000)}k`;
const share = (value: number | null): string => value === null ? "-" : `${Math.round(value * 100)}%`;
const whole = (value: number | null | undefined): string => value == null ? "-" : value.toFixed(value < 10 ? 1 : 0);

function table(header: readonly string[], rows: readonly (readonly string[])[]): string[] {
  const widths = header.map((title, column) => Math.max(title.length, ...rows.map((row) => row[column]?.length ?? 0)));
  const line = (cells: readonly string[]) => cells.map((cell, column) => cell.padEnd(widths[column] ?? 0)).join("  ").trimEnd();
  return [line(header), ...rows.map(line)];
}

function cacheHit(row: SetupUsage): number | null {
  if (row.newInput === null || row.cachedInput === null) return null;
  const all = row.newInput + row.cachedInput + (row.cacheWrite ?? 0);
  return all === 0 ? null : row.cachedInput / all;
}

/** The report's tables from the setups' means. */
export function usageTables(rows: readonly SetupUsage[]): string[] {
  const classes = Object.values(ContextClass).filter((type) => rows.some((row) => (row.classes[type] ?? 0) > 0));
  const totals = new Map<string, number>();
  for (const row of rows) for (const [type, value] of Object.entries(row.fileTypes)) totals.set(type, (totals.get(type) ?? 0) + value);
  const types = [...totals].sort((a, b) => b[1] - a[1]).slice(0, ShownFileTypes).map(([type]) => type);
  const otherTypes = (row: SetupUsage) => Object.entries(row.fileTypes).filter(([type]) => !types.includes(type)).reduce((sum, [, value]) => sum + value, 0);
  return [
    "Means per run, exact. New input includes Claude's cache writes. Cache hit is cached input among all input. Before edit is the share of input sent before the first edit's result, where the logs report each request.",
    ...table(["setup", "runs", "new in", "cached in", "cache hit", "output", "requests", "peak context", "before edit"],
      rows.map((row) => [row.setup, String(row.runs), thousands(row.newInput === null ? null : row.newInput + (row.cacheWrite ?? 0)), thousands(row.cachedInput), share(cacheHit(row)),
        thousands(row.output), whole(row.requests), thousands(row.peakContext), share(row.beforeEdit)])), "",
    "Input tokens by class, mean per run. Estimated from the logs, and each run's classes sum to its exact input.",
    ...table(["setup", ...classes], rows.map((row) => [row.setup, ...classes.map((type) => thousands(row.classes[type]))])), "",
    "Input tokens by file type, mean per run, estimated. Only file content and search results have a type.",
    ...table(["setup", ...types, "other"], rows.map((row) => [row.setup, ...types.map((type) => thousands(row.fileTypes[type] ?? 0)), thousands(otherTypes(row))])), "",
    "Tool calls by kind, mean per run. Runs are test, build and type check commands.",
    ...table(["setup", ...Object.values(ToolKind), "failed", "steps before edit", "repeated reads", "files read"],
      rows.map((row) => [row.setup, ...Object.values(ToolKind).map((kind) => whole(row.calls[kind])), whole(row.failed), whole(row.stepsBeforeEdit), whole(row.repeatedReads), whole(row.filesRead)])), "",
  ];
}

/** Each setup's mean usage in a batch over runs whose agent finished, analyzed from their saved logs without saving a record. */
export async function usageLines(batch: BatchRecord, runs: readonly RunView[], store: Pick<HistoryStore, "runFolder" | "readSetups">): Promise<string[]> {
  const replaced = new Set(runs.flatMap((run) => run.replacesRunId === null ? [] : [run.replacesRunId]));
  const counted = runs.filter((run) => run.agentSeconds !== null && run.status !== RunStatus.limited && !(run.failureKind === FailureKind.infrastructure && replaced.has(run.id)));
  const setups = await store.readSetups(batch);
  const analyses = await Promise.all(counted.map(async (run) => ({ setup: run.setup, analysis: await analyzeSavedRun(store, batch, run, setups, null) })));
  return [`Layer ${batch.config.layer ?? "?"} ${batch.suite}, batch ${batch.id}`, ...usageTables(setupUsage(batch.config.setups, analyses))];
}
