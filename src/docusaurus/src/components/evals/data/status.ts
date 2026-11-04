import type { Batch, GradingVersions } from "../records/records.types.ts";
import { agentVersion, type Cell } from "./cells.ts";
import { countedRuns } from "./run-measures.ts";

/** A batch's status once every planned run has finished. */
export const DoneStatus = "done";

/** Why the shown numbers can still change. */
export const PreliminaryReason = { running: "running", unchecked: "unchecked", recheck: "recheck", unjudged: "unjudged" } as const;
export type PreliminaryReason = (typeof PreliminaryReason)[keyof typeof PreliminaryReason];

/** The batches the shown cells come from, in the order they were created. */
export function shownBatches(batches: readonly Batch[], shown: readonly Cell[]): Batch[] {
  const ids = new Set(shown.map((cell) => cell.batchId));
  return batches.filter((batch) => ids.has(batch.experiment.id)).sort((a, b) => a.experiment.createdAt.localeCompare(b.experiment.createdAt));
}

/**
 * The view is preliminary while a counted batch isn't done, or while a counted cell has fewer checked or judged runs than
 * implemented runs. A cell without any judge counts as unjudged.
 */
export function preliminaryReasons(batches: readonly Batch[], shown: readonly Cell[]): PreliminaryReason[] {
  const reasons: PreliminaryReason[] = [];
  if (shownBatches(batches, shown).some((batch) => batch.experiment.status !== DoneStatus)) reasons.push(PreliminaryReason.running);
  if (shown.some((cell) => cell.checked + cell.recheck < cell.runs)) reasons.push(PreliminaryReason.unchecked);
  if (shown.some((cell) => cell.recheck > 0)) reasons.push(PreliminaryReason.recheck);
  if (shown.some((cell) => !cell.judgedLater && cell.judged < cell.runs)) reasons.push(PreliminaryReason.unjudged);
  return reasons;
}

/** What the data status says when nothing can still change, which doesn't claim judges where a plan leaves them out. */
export function settledText(shown: readonly Cell[]): string {
  if (shown.length === 0) return "No run counts in these filters.";
  return shown.some((cell) => cell.judged < cell.runs) ? "Every counted run is checked. Some have no judge, because their plans leave judging for later." : "Every counted run is checked and judged.";
}

export const MixedModelsText = "Not every scenario here ran on the same models, so totals and means side by side compare different models. Choose one model in the filters to compare scenarios directly.";

/** Scenarios that didn't all run on the same models, so totals and means beside each other compare different models. */
export function mixedModels(shown: readonly Cell[]): boolean {
  const models = new Map<string, Set<string>>();
  for (const cell of shown) if (cell.runs > 0) models.set(cell.setup, new Set([...models.get(cell.setup) ?? [], cell.model]));
  const groups = new Set([...models.values()].map((set) => [...set].sort().join("|")));
  return groups.size > 1;
}

/** Finished runs of older batches that newer runs of the same work replaced. They show on the History page. */
export function replacedRuns(batches: readonly Batch[], all: readonly Cell[]): number {
  const shown = new Set(all.flatMap((cell) => cell.runIds));
  return batches.filter((batch) => batch.experiment.config.layer !== undefined && batch.experiment.reference === undefined)
    .reduce((sum, batch) => sum + countedRuns(batch).filter((run) => !shown.has(run.id)).length, 0);
}

/** The agent programs a batch ran, for its row in the data status. */
export function agentVersions(batch: Batch): string {
  return [...new Set(batch.experiment.config.agents.map((agent) => agentVersion(batch, agent.tool) ?? "Not recorded"))].join(", ");
}

/** The distinct grading versions a batch's checks and judgments used. */
export function gradingVersions(batch: Batch): GradingVersions[] {
  const versions = new Map<string, GradingVersions>();
  for (const item of batch.assessments) if (item.kind !== "pick") versions.set(JSON.stringify(item.versions), item.versions);
  return [...versions.values()];
}

/** Batches with a starting point that no newest cell comes from. They show on the History page. */
export function olderBatches(batches: readonly Batch[], all: readonly Cell[]): Batch[] {
  const counted = new Set(all.map((cell) => cell.batchId));
  return batches.filter((batch) => batch.experiment.config.layer !== undefined && batch.experiment.reference === undefined && !counted.has(batch.experiment.id));
}
