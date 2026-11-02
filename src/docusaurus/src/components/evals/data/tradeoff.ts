import type { SetupSummary } from "./aggregate.ts";
import { bySetup, identity, type ChartRow } from "./chart-data.ts";

/** Token changes smaller than this, in percent, count as about the same tokens, since points per 10% would grow without bound. */
export const SameTokens = 1;
/** The token change the derived columns are measured against, in percent. */
export const TokenStep = 10;

export const TokenTrend = { more: "more", fewer: "fewer", same: "same" } as const;
export type TokenTrend = (typeof TokenTrend)[keyof typeof TokenTrend];

/** What a setup's extra tokens buy against its control: changes in percent and points, and points per 10% more tokens. */
export interface TradeoffRow extends Omit<ChartRow, "value"> {
  readonly tokenChange: number | null;
  readonly coverageChange: number | null;
  readonly judgeChange: number | null;
  readonly tokenTrend: TokenTrend | null;
  /** Only when tokens rose. A setup that saves tokens has no price per point. */
  readonly coveragePerStep: number | null;
  readonly judgePerStep: number | null;
  readonly groups: number;
  readonly runs: number;
}

export function trendOf(tokenChange: number | null): TokenTrend | null {
  if (tokenChange === null) return null;
  if (tokenChange >= SameTokens) return TokenTrend.more;
  return tokenChange <= -SameTokens ? TokenTrend.fewer : TokenTrend.same;
}

const points = (delta: number | null) => delta === null ? null : delta * 100;

/** One row per setup that has a control to compare with, in reading order. */
export function tradeoffRows(summaries: readonly SetupSummary[], controls: ReadonlySet<string>): TradeoffRow[] {
  return summaries.filter((summary) => !controls.has(summary.setup) && summary.groups > 0).map((summary) => {
    const tokenChange = summary.tokenRatio === null ? null : (summary.tokenRatio - 1) * 100;
    const tokenTrend = trendOf(tokenChange);
    const perStep = (change: number | null) => tokenTrend !== TokenTrend.more || change === null || tokenChange === null ? null : change / (tokenChange / TokenStep);
    const coverageChange = points(summary.coverageDelta), judgeChange = points(summary.judgeDelta);
    return { ...identity(summary.setup), tokenChange, coverageChange, judgeChange, tokenTrend,
      coveragePerStep: perStep(coverageChange), judgePerStep: perStep(judgeChange), groups: summary.groups, runs: summary.runs };
  }).sort(bySetup);
}
