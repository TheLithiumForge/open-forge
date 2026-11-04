import assert from "node:assert/strict";
import { test } from "node:test";
import { previewBatch } from "../../../../scripts/evals/preview-fixture.ts";
import { comparisons, setupSummaries, type SetupSummary } from "./aggregate.ts";
import { cells } from "./cells.ts";
import { controlSetups } from "./chart-data.ts";
import { SameTokens, TokenStep, TokenTrend, tradeoffRows, trendOf } from "./tradeoff.ts";

const setup = "essentials";
const summary = (tokenRatio: number | null, coverageDelta: number | null): SetupSummary => ({
  setup, groups: 1, coverageGroups: 1, judgeGroups: 0, tokenGroups: 1, runs: 3, coverageDelta, judgeDelta: null, tokenRatio, newInputRatio: null, outputRatio: null, coverage: null, judgeScore: null, tokens: null,
});

test("points per 10% more tokens divide the quality change by the token change", () => {
  const [row] = tradeoffRows([summary(1.3, 0.2)], new Set());
  assert.ok(row);
  assert.equal(row.tokenTrend, TokenTrend.more);
  assert.ok(row.coveragePerStep !== null && row.tokenChange !== null && row.coverageChange !== null);
  assert.ok(Math.abs(row.coveragePerStep * (row.tokenChange / TokenStep) - row.coverageChange) < 1e-9);
  assert.equal(row.judgePerStep, null);
});

test("a setup that saves tokens or keeps them about the same has no price per point", () => {
  for (const ratio of [0.8, 1 + SameTokens / 200]) {
    const [row] = tradeoffRows([summary(ratio, 0.1)], new Set());
    assert.equal(row?.coveragePerStep, null);
    assert.notEqual(row?.tokenTrend, TokenTrend.more);
  }
  assert.equal(trendOf(-SameTokens * 2), TokenTrend.fewer);
  assert.equal(trendOf(null), null);
});

test("the control has no row of its own", () => {
  const all = cells([previewBatch(1)]);
  const controls = controlSetups(all);
  const rows = tradeoffRows(setupSummaries(comparisons(all)), controls);
  assert.ok(rows.length > 0);
  assert.ok(rows.every((row) => !controls.has(row.setup)));
});
