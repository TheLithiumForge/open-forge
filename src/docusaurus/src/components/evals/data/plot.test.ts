import assert from "node:assert/strict";
import { test } from "node:test";
import { previewBatch } from "../../../../scripts/evals/preview-fixture.ts";
import { comparisons, setupSummaries } from "./aggregate.ts";
import { cells } from "./cells.ts";
import type { Batch } from "../records/records.types.ts";
import { CellMeasures } from "./measures.ts";
import { niceAxis, PlotDefaults, PlotMeasures, plotMeasure, PlotMode, RunsKey, SizeMeasures } from "./plot.ts";
import { runPoints, setupPoints, type Channels } from "./plot-points.ts";

/** The preview batch with token counts on every run, scaled by each run's time so setups differ. */
function withTokens(batch: Batch): Batch {
  return { ...batch, runs: batch.runs.map((run) => ({ ...run, telemetry: { toolCalls: 0, failedToolCalls: 0, peakContext: null,
    tokens: { input: run.agentSeconds ?? 0, cachedInput: (run.agentSeconds ?? 0) * 10, cacheWrite: 0, output: run.repeat } } })) };
}

const items = comparisons(cells([withTokens(previewBatch(1))]));
const channels = (x: string, y: string, size: string | null): Channels => ({ x: plotMeasure(x), y: plotMeasure(y), size: size === null ? null : plotMeasure(size) });

test("every cell measure is offered as its value and its change, and size offers no changes", () => {
  assert.equal(PlotMeasures.filter((measure) => measure.spec !== null).length, CellMeasures.length * 2);
  assert.ok(SizeMeasures.every((measure) => measure.mode === PlotMode.value));
  assert.ok(SizeMeasures.some((measure) => measure.key === RunsKey));
});

test("the default setup points match the setup summaries the cards and the tradeoff table use", () => {
  const points = setupPoints(items, channels(PlotDefaults.x, PlotDefaults.y, PlotDefaults.size), false);
  const summaries = setupSummaries(items);
  assert.ok(points.length > 0);
  for (const point of points) {
    const summary = summaries.find((item) => item.setup === point.setup);
    assert.ok(summary && summary.tokenRatio !== null && summary.coverageDelta !== null);
    assert.ok(Math.abs(point.x - (summary.tokenRatio - 1) * 100) < 1e-9);
    assert.ok(Math.abs(point.y - summary.coverageDelta * 100) < 1e-9);
    assert.equal(point.raw.size, summary.runs);
  }
  assert.ok(points.every((point) => point.setup !== previewBatch(1).experiment.config.setups[0]));
});

test("splitting by starting point keeps one point per setup and starting point", () => {
  const values = channels(`${CellMeasures[0]?.id}-${PlotMode.value}`, RunsKey, null);
  const split = setupPoints(items, values, true);
  assert.equal(new Set(split.map((point) => point.key)).size, split.length);
  assert.ok(split.every((point) => point.startingPoint !== null && point.z === null));
});

test("every run is a point once, or once per judge when a channel shows judge scores", () => {
  const model = items[0]?.cell.model;
  const ofModel = items.filter((item) => item.cell.model === model);
  const coverage = `${CellMeasures[0]?.id}-${PlotMode.value}`, judge = `${CellMeasures[1]?.id}-${PlotMode.value}`;
  const runs = new Set(ofModel.flatMap((item) => item.cell.runIds)).size;
  const judges = new Set(ofModel.map((item) => item.cell.judge)).size;
  assert.equal(runPoints(ofModel, channels(coverage, RunsKey, null)).length, runs);
  assert.equal(runPoints(ofModel, channels(coverage, judge, null)).length, runs * judges);
});

test("axes get round ticks around the values, and change axes always show zero", () => {
  const { domain, ticks } = niceAxis([12, 47], true);
  assert.ok(domain[0] <= 0 && domain[1] >= 47);
  assert.ok(ticks.includes(0));
  assert.ok(ticks.every((tick, index) => index === 0 || tick > (ticks[index - 1] ?? -Infinity)));
  const amounts = niceAxis([1_200_000, 4_800_000], false);
  assert.ok(amounts.domain[0] >= 0 && amounts.domain[0] <= 1_200_000 && amounts.domain[1] >= 4_800_000);
  assert.deepEqual(niceAxis([], false).domain.length, 2);
});
