import assert from "node:assert/strict";
import { test } from "node:test";
import { previewBatch } from "../../../../scripts/evals/preview-fixture.ts";
import { interval, proportionInterval } from "./analysis.ts";
import { intervalText, metricTitle } from "./format.ts";
import { comparisonRows, pairKey, pairOptions, resultGroups } from "./views.ts";
test("metric descriptions keep intervals, measured counts and nonzero missing counts", () => {
  const value = proportionInterval([true, false]);
  assert.match(metricTitle(value), /50.0%/);
  assert.match(metricTitle(value), /95% interval/);
  assert.match(metricTitle(value), /2 measured/);
  assert.doesNotMatch(metricTitle(value), /missing/);
  assert.match(metricTitle(interval([null, 40]), false), /1 measured, 1 missing/);
  assert.equal(intervalText(interval([null])), null);
});
test("pair choices span CLI versions while result cards keep batches and grading separate", () => {
  const groups = resultGroups([previewBatch(1), previewBatch(2)]);
  const pairs = pairOptions(groups.flatMap((group) => group.rows));
  assert.equal(pairs.length, 4);
  const first = pairs[0];
  assert.ok(first);
  const selected = groups.filter((group) => group.rows.some((row) => pairKey(row) === first.key));
  assert.equal(selected.length, 2);
  assert.notEqual(selected[0]?.key, selected[1]?.key);
});
test("pair comparison chart fixes the batch, setup and grading versions", () => {
  const groups = resultGroups([previewBatch(1), previewBatch(2)]);
  const selected = groups[0];
  assert.ok(selected);
  const rows = comparisonRows(groups, selected);
  assert.equal(rows.length, 4);
  assert.ok(rows.every((row) => row.batchId === selected.batch.experiment.id && row.setup === selected.rows[0]?.setup && row.grading === selected.rows[0]?.grading));
  const changed = { ...selected, rows: selected.rows.map((row) => ({ ...row, grading: "different-grading" })) };
  assert.deepEqual(comparisonRows(groups, changed), []);
});
