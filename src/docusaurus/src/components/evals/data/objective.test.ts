import assert from "node:assert/strict";
import { test } from "node:test";
import { previewBatch } from "../../../../scripts/evals/preview-fixture.ts";
import type { Batch } from "../records/records.types.ts";
import { comparisons } from "./aggregate.ts";
import { cells, newestCells, WorkModeIds } from "./cells.ts";
import { latestAnalyses, objectiveRows } from "./objective.ts";
import { milestoneRows, packetRows } from "./work-views.ts";

const Chain = "fixture-chain";
const byId = (batches: readonly Batch[]) => new Map(batches.map((batch) => [batch.experiment.id, batch]));
const specOf = (batch: Batch, runId: string) => latestAnalyses(batch).get(runId)?.parts.filter((part) => part.class === "spec").reduce((sum, part) => sum + part.input, 0) ?? 0;

/** The preview batch as a later milestone of a chain, under its own id. */
function chained(batch: Batch): Batch {
  const id = `${batch.experiment.id}-chained`;
  return { ...batch, experiment: { ...batch.experiment, id, createdAt: "2026-02-01T00:00:00.000Z", chain: { id: Chain, milestone: 2, parentBatchId: batch.experiment.id } },
    runs: batch.runs.map((run) => ({ ...run, batchId: id })), setups: batch.setups.map((setup) => ({ ...setup, experimentId: id })),
    assessments: batch.assessments.map((item) => ({ ...item, experimentId: id })), analyses: (batch.analyses ?? []).map((item) => ({ ...item, experimentId: id })) };
}

test("each setup's objective measures average its runs' analyses and sit beside the control's", () => {
  const batch = previewBatch(1);
  const rows = objectiveRows(comparisons(cells([batch])), byId([batch]));
  const control = batch.experiment.config.setups[0], compared = batch.experiment.config.setups[1];
  const runsOf = (setup: string | undefined) => batch.runs.filter((run) => run.setup === setup).map((run) => specOf(batch, run.id));
  const mean = (values: readonly number[]) => values.reduce((sum, value) => sum + value, 0) / values.length;
  const controlRow = rows.find((row) => row.setup === control), comparedRow = rows.find((row) => row.setup === compared);
  assert.equal(controlRow?.control, null);
  assert.equal(controlRow?.measures.classes["spec"], mean(runsOf(control)));
  assert.equal(comparedRow?.measures.classes["spec"], mean(runsOf(compared)));
  assert.equal(comparedRow?.control?.classes["spec"], mean(runsOf(control)));
  assert.equal(comparedRow?.measures.rough, 0);
});

test("a chained milestone keeps its own cells beside the standalone runs of the same task and meets them as its twin", () => {
  const standalone = previewBatch(1), milestone = chained(standalone);
  const items = newestCells(comparisons(cells([standalone, milestone])));
  assert.ok(items.some((item) => item.cell.chain === null) && items.some((item) => item.cell.chain?.id === Chain));
  const rows = milestoneRows(items);
  assert.ok(rows.length > 0);
  assert.ok(rows.every((row) => row.chain === Chain && row.milestone === 2 && row.tokens === row.standaloneTokens));
});

test("a task-by-task packet adds its tasks up into one row per scenario, beside the same packet handed over whole", () => {
  const base = previewBatch(1), packet = "l4-p1-four-bugs";
  const handed = (id: string, workMode: string, chain?: { readonly id: string; readonly milestone: number }): Batch =>
    ({ ...base, experiment: { ...base.experiment, id, config: { ...base.experiment.config, packet, workMode, ...(chain === undefined ? {} : { chain }) } } });
  const batches = [handed("step-1", WorkModeIds.taskByTask, { id: packet, milestone: 1 }), handed("step-2", WorkModeIds.taskByTask, { id: packet, milestone: 2 }), handed("whole", WorkModeIds.wholePacket)];
  const rows = packetRows(comparisons(cells(batches)), byId(batches));
  const stepped = rows.filter((row) => row.workMode === WorkModeIds.taskByTask), whole = rows.filter((row) => row.workMode === WorkModeIds.wholePacket);
  assert.ok(stepped.length > 0 && stepped.length === whole.length);
  for (const row of stepped) {
    const twin = whole.find((other) => other.setup === row.setup && other.modelLabel === row.modelLabel);
    assert.equal(row.steps, 2);
    assert.equal(row.tokens, twin?.tokens === null || twin?.tokens === undefined ? null : twin.tokens * 2);
  }
  assert.ok(cells(batches).every((cell) => cell.workMode !== WorkModeIds.singleTask));
  assert.ok(cells([base]).every((cell) => cell.workMode === WorkModeIds.singleTask));
});
