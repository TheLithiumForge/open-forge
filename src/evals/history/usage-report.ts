import { readFile } from "node:fs/promises";
import { existsSync } from "node:fs";
import { join } from "node:path";
import { ResultFiles } from "../paths.ts";
import type { RunView } from "./records.types.ts";
import type { BatchRecord } from "./results.types.ts";
import { FailureKind, RunStatus } from "./statuses.ts";
import type { HistoryStore } from "./store.ts";
import { eventUsage, ReadKind, type EventUsage } from "./usage-analysis.ts";

const mean = (values: readonly number[]): number | null => values.length === 0 ? null : values.reduce((sum, value) => sum + value, 0) / values.length;
const thousands = (value: number | null): string => value === null ? "-" : `${Math.round(value / 1000)}k`;
const share = (value: number | null): string => value === null ? "-" : `${Math.round(value * 100)}%`;

/** Each setup's mean usage in a batch, from telemetry and saved events, over runs whose agent finished. */
export async function usageLines(batch: BatchRecord, runs: readonly RunView[], store: Pick<HistoryStore, "runFolder">): Promise<string[]> {
  const replaced = new Set(runs.flatMap((run) => run.replacesRunId === null ? [] : [run.replacesRunId]));
  const counted = runs.filter((run) => run.agentSeconds !== null && run.status !== RunStatus.limited && !(run.failureKind === FailureKind.infrastructure && replaced.has(run.id)));
  const usage = new Map<string, EventUsage>();
  for (const run of counted) {
    const path = join(store.runFolder(batch.id, run.id), ResultFiles.events);
    if (existsSync(path)) usage.set(run.id, eventUsage(run.agent.tool, await readFile(path, "utf8")));
  }
  const header = ["setup", "runs", "new in", "cached in", "output", "read docs", "read framework", "read code", "steps before edit", "analysis tokens"];
  const rows = batch.config.setups.map((setup) => {
    const group = counted.filter((run) => run.setup === setup);
    const tokens = group.flatMap((run) => run.telemetry?.tokens == null ? [] : [run.telemetry.tokens]);
    const events = group.flatMap((run) => usage.get(run.id) ?? []);
    const read = (kind: ReadKind) => thousands(mean(events.map((item) => item.reads[kind].characters)));
    const before = mean(events.flatMap((item) => item.stepsBeforeEdit === null || item.steps === 0 ? [] : [item.stepsBeforeEdit / item.steps]));
    const analysis = mean(events.flatMap((item) => {
      if (item.phases === null) return [];
      const total = (split: typeof item.phases.analysis) => split.newInput + split.cachedInput + split.cacheWrite;
      const all = total(item.phases.analysis) + total(item.phases.implementation);
      return all === 0 ? [] : [total(item.phases.analysis) / all];
    }));
    return [setup, String(group.length), thousands(mean(tokens.map((item) => item.input))), thousands(mean(tokens.map((item) => item.cachedInput))),
      thousands(mean(group.flatMap((run) => { const fromEvents = usage.get(run.id)?.output; return fromEvents != null ? [fromEvents] : run.telemetry?.tokens == null ? [] : [run.telemetry.tokens.output]; }))), read(ReadKind.docs), read(ReadKind.framework), read(ReadKind.code), share(before), share(analysis)];
  });
  const widths = header.map((title, column) => Math.max(title.length, ...rows.map((row) => row[column]?.length ?? 0)));
  const line = (cells: readonly string[]) => cells.map((cell, column) => cell.padEnd(widths[column] ?? 0)).join("  ").trimEnd();
  return [`Layer ${batch.config.layer ?? "?"} ${batch.suite}, batch ${batch.id}`,
    "Means per run. Reads count characters the agent saw from each kind of file. Output comes from each turn's result where the events report it. Analysis is the share of input tokens before the first edit, which only Claude reports.",
    line(header), ...rows.map(line), ""];
}
