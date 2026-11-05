import assert from "node:assert/strict";
import { test } from "node:test";
import { previewBatch } from "../../../../scripts/evals/preview-fixture.ts";
import { comparisons, setupSummaries } from "./aggregate.ts";
import { cells } from "./cells.ts";
import { controlSetups, spreadRows, summaryRows, symmetricLimit, tokenClassRows } from "./chart-data.ts";
import { CellMeasures, measureChange } from "./measures.ts";
import { setupRank } from "./names.ts";

test("charts leave the control out and list the other setups in reading order", () => {
  const batch = previewBatch(1);
  const all = cells([batch]);
  const controls = controlSetups(all);
  assert.deepEqual([...controls], [batch.experiment.config.setups[0]]);
  const rows = summaryRows(setupSummaries(comparisons(all)), controls, (summary) => summary.coverageDelta);
  assert.equal(rows.length, batch.experiment.config.setups.length - 1);
  assert.ok(rows.every((row, index) => index === 0 || setupRank(rows[index - 1]?.setup ?? "") <= setupRank(row.setup)));
});

test("run spreads count each run once, and each judge once per run for judge scores", () => {
  const batch = previewBatch(1);
  const model = cells([batch])[0]?.model;
  const caseCells = cells([batch]).filter((cell) => cell.model === model);
  const judges = new Set(caseCells.map((cell) => cell.judge)).size;
  for (const spec of CellMeasures) for (const row of spreadRows(caseCells, spec)) {
    const runs = caseCells.find((cell) => cell.setup === row.setup)?.runs ?? 0;
    assert.ok(row.values.length <= (spec.perJudge ? runs * judges : runs));
    if (spec.perJudge) assert.equal(row.values.length, runs * judges);
  }
});

test("a chart centred on zero is wide enough for the largest change", () => {
  assert.equal(symmetricLimit([-12, 3, null], 5), 15);
  assert.equal(symmetricLimit([null], 5), 5);
});

test("token classes add up to the total for every setup", () => {
  const all = cells([previewBatch(1)]);
  for (const row of tokenClassRows(all)) {
    const parts = [row.newInput, row.output, row.cacheWrites, row.cachedInput];
    const total = all.find((cell) => cell.setup === row.setup)?.tokens.mean ?? null;
    if (total === null) assert.ok(parts.every((part) => part === null));
    else assert.equal(parts.reduce<number>((sum, part) => sum + (part ?? 0), 0), total);
  }
});

test("each measure's change from the control matches the comparison's own change", () => {
  const items = comparisons(cells([previewBatch(1)]));
  const [coverage, judge, , , tokens] = CellMeasures;
  assert.ok(coverage && judge && tokens);
  for (const item of items) {
    assert.equal(measureChange(coverage, item), item.coverageDelta);
    assert.equal(measureChange(judge, item), item.judgeDelta);
    assert.equal(measureChange(tokens, item), item.tokenRatio);
  }
});
