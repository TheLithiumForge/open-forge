import assert from "node:assert/strict";
import { test } from "node:test";
import { previewBatch } from "../../../../scripts/evals/preview-fixture.ts";
import { All, comparisons, filtered, options, setupSummaries, type FilterOption } from "./aggregate.ts";
import { cells, type Cell } from "./cells.ts";

test("every setup compares with the plan's first setup on the same batch, model and judge", () => {
  const batch = previewBatch(1);
  const all = cells([batch, previewBatch(2, true)]);
  assert.ok(all.length > 0);
  assert.ok(all.every((cell) => cell.batchId === batch.experiment.id && cell.control === batch.experiment.config.setups[0]));
  for (const item of comparisons(all).filter((entry) => entry.cell.setup === entry.cell.control)) {
    assert.equal(item.controlCell, item.cell);
    if (item.coverageDelta !== null) assert.equal(item.coverageDelta, 0);
    if (item.tokenRatio !== null) assert.equal(item.tokenRatio, 1);
  }
});

test("summaries count each batch and model once and filters narrow by model, judge and starting point", () => {
  const all = comparisons(cells([previewBatch(1)]));
  const { models, judges } = options(all.map((item) => item.cell));
  assert.ok(models.length > 0 && judges.length > 0);
  const control = setupSummaries(all).find((summary) => summary.setup === previewBatch(1).experiment.config.setups[0]);
  assert.ok(control);
  if (control.tokenRatio !== null) assert.equal(control.tokenRatio, 1);
  const first = models[0];
  assert.ok(first);
  const narrowed = filtered(all, { model: first.key, judge: All, startingPoint: All });
  assert.ok(narrowed.every((item) => item.cell.model === first.key));
  assert.equal(filtered(all, { model: All, judge: All, startingPoint: "existing-app" }).length, 0);
  for (const summary of setupSummaries(narrowed)) assert.ok(summary.groups <= 1);
});

test("filter options take a model's label from a cell with runs", () => {
  const all = cells([previewBatch(1)]);
  const withRuns = all.find((cell) => cell.runs > 0);
  assert.ok(withRuns);
  const empty: Cell = { ...withRuns, runs: 0, runIds: [], modelLabel: `${withRuns.modelLabel}, without runs` };
  const orders: readonly (readonly Cell[])[] = [[empty, withRuns], [withRuns, empty]];
  for (const order of orders) {
    const chosen: FilterOption | undefined = options(order).models.find((option) => option.key === withRuns.model);
    assert.equal(chosen?.label, withRuns.modelLabel);
  }
});
