import type { Batch } from "../records/records.types.ts";
import { summaries, type Summary } from "./summaries.ts";
export interface PairOption {
  readonly key: string;
  readonly label: string;
}
export interface ResultGroup {
  readonly key: string;
  readonly batch: Batch;
  readonly rows: readonly Summary[];
}
export function pairKey(row: Summary): string { return JSON.stringify([row.tool, row.model, row.effort, row.judge]); }
export function pairOptions(rows: readonly Summary[]): PairOption[] {
  return [...new Map(rows.map((row) => [pairKey(row), { key: pairKey(row), label: `${row.tool} ${row.model} (${row.effort ?? "unreported effort"}) with ${row.judge}` }])).values()];
}
export function resultGroups(batches: readonly Batch[]): ResultGroup[] {
  return batches.flatMap((batch) => {
    const groups = new Map<string, Summary[]>();
    for (const row of summaries(batch))
      groups.set(row.scope, [...groups.get(row.scope) ?? [], row]);
    return [...groups].map(([scope, rows]) => ({ key: JSON.stringify([batch.experiment.id, scope]), batch, rows }));
  });
}
export function comparisonRows(groups: readonly ResultGroup[], selected: ResultGroup): Summary[] {
  const first = selected.rows[0];
  if (first === undefined)
    return [];
  return groups.filter((group) => group.batch.experiment.id === selected.batch.experiment.id)
    .flatMap((group) => group.rows.filter((row) => row.setup === first.setup && row.grading === first.grading));
}
