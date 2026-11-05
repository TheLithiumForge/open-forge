import type { Judge, JudgeScore } from "../records/records.types.ts";

export const JudgeHeadline = { quality: "quality", adherence: "adherence", upkeep: "upkeep" } as const;
export type JudgeHeadline = (typeof JudgeHeadline)[keyof typeof JudgeHeadline];
/** Judges score each dimension out of this. Headlines read as a share of it, like the page's other shares. */
export const ScoreScale = 10;
/** Two judges this far apart on a headline, as a share, are reported as disagreeing. */
export const JudgeDisputeGap = 0.2;

export function median(values: readonly number[]): number | null {
  const sorted = [...values].sort((a, b) => a - b);
  if (sorted.length === 0) return null;
  const middle = Math.floor(sorted.length / 2);
  return sorted.length % 2 === 1 ? sorted[middle] ?? null : ((sorted[middle - 1] ?? 0) + (sorted[middle] ?? 0)) / 2;
}
const mean = (values: readonly number[]): number | null => values.length === 0 ? null : values.reduce((sum, value) => sum + value, 0) / values.length;

/** A dimension's score across its ballots. The middle ballot keeps one odd reading from moving it. */
export const dimensionScore = (score: JudgeScore): number | null => median(score.ballots);

/** The dimensions' mean within each group, then the groups' mean, so play and code weigh the same whatever their counts. */
export function headlineShare(judgment: Judge, headline: JudgeHeadline): number | null {
  const groups = new Map<string, number[]>();
  for (const score of judgment.scores ?? []) {
    const value = dimensionScore(score);
    if (score.headline === headline && value !== null) groups.set(score.group, [...groups.get(score.group) ?? [], value]);
  }
  const groupMean = mean([...groups.values()].flatMap((values) => mean(values) ?? []));
  return groupMean === null ? null : groupMean / ScoreScale;
}

/** The share of pages that described the game before the work and still do after, read from the files. */
export function currentPagesShare(judgment: Judge): number | null {
  const pages = judgment.staleDocs ?? [];
  return pages.length === 0 ? null : pages.filter((page) => page.current).length / pages.length;
}

/** One group's share, such as play alone, to read beside the headline it feeds. */
export function groupShare(judgment: Judge, group: string): number | null {
  const value = mean((judgment.scores ?? []).filter((score) => score.group === group).flatMap((score) => dimensionScore(score) ?? []));
  return value === null ? null : value / ScoreScale;
}
