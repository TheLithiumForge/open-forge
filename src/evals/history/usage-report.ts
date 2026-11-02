import { readFile } from "node:fs/promises";
import { existsSync } from "node:fs";
import { join } from "node:path";
import { ResultFiles } from "../paths.ts";
import type { RunView } from "./records.types.ts";
import type { BatchRecord } from "./results.types.ts";
import { FailureKind, RunStatus } from "./statuses.ts";
import type { HistoryStore } from "./store.ts";
import { ActionKind, eventUsage, ReadKind, type EventUsage } from "./usage-analysis.ts";

const mean = (values: readonly number[]): number | null => values.length === 0 ? null : values.reduce((sum, value) => sum + value, 0) / values.length;
const thousands = (value: number | null): string => value === null ? "-" : `${Math.round(value / 1000)}k`;
const share = (value: number | null): string => value === null ? "-" : `${Math.round(value * 100)}%`;
const whole = (value: number | null): string => value === null ? "-" : value.toFixed(value < 10 ? 1 : 0);

function table(header: readonly string[], rows: readonly (readonly string[])[]): string[] {
  const widths = header.map((title, column) => Math.max(title.length, ...rows.map((row) => row[column]?.length ?? 0)));
  const line = (cells: readonly string[]) => cells.map((cell, column) => cell.padEnd(widths[column] ?? 0)).join("  ").trimEnd();
  return [line(header), ...rows.map(line)];
}

/** Each setup's mean usage in a batch, from telemetry and saved events, over runs whose agent finished. */
export async function usageLines(batch: BatchRecord, runs: readonly RunView[], store: Pick<HistoryStore, "runFolder">): Promise<string[]> {
  const replaced = new Set(runs.flatMap((run) => run.replacesRunId === null ? [] : [run.replacesRunId]));
  const counted = runs.filter((run) => run.agentSeconds !== null && run.status !== RunStatus.limited && !(run.failureKind === FailureKind.infrastructure && replaced.has(run.id)));
  const usage = new Map<string, EventUsage>();
  for (const run of counted) {
    const path = join(store.runFolder(batch.id, run.id), ResultFiles.events);
    if (existsSync(path)) usage.set(run.id, eventUsage(run.agent.tool, await readFile(path, "utf8")));
  }
  const groups = batch.config.setups.map((setup) => {
    const group = counted.filter((run) => run.setup === setup);
    return { setup, group, tokens: group.flatMap((run) => run.telemetry?.tokens == null ? [] : [run.telemetry.tokens]), events: group.flatMap((run) => usage.get(run.id) ?? []) };
  });
  const tokenRows = groups.map(({ setup, group, tokens, events }) => {
    // Output comes from each turn's result where the events report it, since older Claude telemetry undercounted it.
    const output = mean(group.flatMap((run) => { const fromEvents = usage.get(run.id)?.output; return fromEvents != null ? [fromEvents] : run.telemetry?.tokens == null ? [] : [run.telemetry.tokens.output]; }));
    // Claude reports newly processed text as cache writes, so new input counts them too. Codex writes no cache.
    const fresh = (item: (typeof tokens)[number]) => item.input + item.cacheWrite;
    const hit = mean(tokens.flatMap((item) => fresh(item) + item.cachedInput === 0 ? [] : [item.cachedInput / (fresh(item) + item.cachedInput)]));
    const analysis = mean(events.flatMap((item) => {
      if (item.phases === null) return [];
      const total = (split: typeof item.phases.analysis) => split.newInput + split.cachedInput + split.cacheWrite;
      const all = total(item.phases.analysis) + total(item.phases.implementation);
      return all === 0 ? [] : [total(item.phases.analysis) / all];
    }));
    return [setup, String(group.length), thousands(mean(tokens.map(fresh))), thousands(mean(tokens.map((item) => item.cachedInput))), share(hit),
      thousands(output), share(analysis), thousands(mean(events.flatMap((item) => item.peakContext === null ? [] : [item.peakContext])))];
  });
  const processRows = groups.map(({ setup, events }) => {
    const read = (kind: ReadKind) => thousands(mean(events.map((item) => item.reads[kind].characters)));
    const action = (kind: ActionKind) => whole(mean(events.map((item) => item.actions[kind])));
    const before = mean(events.flatMap((item) => item.stepsBeforeEdit === null || item.steps === 0 ? [] : [item.stepsBeforeEdit / item.steps]));
    return [setup, read(ReadKind.docs), read(ReadKind.framework), read(ReadKind.code), whole(mean(events.map((item) => item.repeatedReads))),
      action(ActionKind.read), action(ActionKind.edit), action(ActionKind.run), share(before)];
  });
  return [`Layer ${batch.config.layer ?? "?"} ${batch.suite}, batch ${batch.id}`,
    "Means per run. Cache hit is cached input among all input. Analysis is the share of input tokens before the first edit, which only Claude reports. Peak context is the largest single request.",
    ...table(["setup", "runs", "new in", "cached in", "cache hit", "output", "analysis", "peak context"], tokenRows), "",
    "Reads count characters seen from each kind of file. Runs are test, build and type check commands.",
    ...table(["setup", "read docs", "read framework", "read code", "repeated reads", "reads", "edits", "runs", "steps before edit"], processRows), ""];
}
