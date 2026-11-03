import type { Batch, GradingVersions } from "../records/records.types.ts";
import type { Cell } from "./cells.ts";

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
