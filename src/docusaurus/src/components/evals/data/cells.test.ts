import assert from "node:assert/strict";
import { test } from "node:test";
import { previewBatch } from "../../../../scripts/evals/preview-fixture.ts";
import type { Batch } from "../records/records.types.ts";
import { agentVersion, cells, newestCells } from "./cells.ts";
import { choiceKey } from "./observations.ts";

const GraderError = "error";

/** The preview batch with its first run's checks ending in a grader error. */
function withGraderError(batch: Batch): { readonly batch: Batch; readonly runId: string } {
  const runId = batch.runs[0]?.id ?? "";
  return { runId, batch: { ...batch, assessments: batch.assessments.map((item) => item.kind === "checks" && item.runId === runId
    ? { ...item, outcomes: item.outcomes.map((outcome) => ({ ...outcome, status: GraderError })) } : item) } };
}

test("every counted run is checked and judged once its grading finishes", () => {
  for (const cell of cells([previewBatch(1)])) {
    assert.equal(cell.checked, cell.runs);
    assert.equal(cell.recheck, 0);
    assert.equal(cell.judged, cell.runs);
    assert.equal(cell.coverage.values.length, cell.runs);
  }
});

test("a run with a grader error waits on a recheck and stays out of coverage", () => {
  const { batch, runId } = withGraderError(previewBatch(1));
  const affected = cells([batch]).filter((cell) => cell.runIds.includes(runId));
  assert.ok(affected.length > 0);
  for (const cell of affected) {
    assert.equal(cell.recheck, 1);
    assert.equal(cell.checked, cell.runs - 1);
    assert.equal(cell.coverage.values.length, cell.checked);
  }
});

test("a judge without judgments leaves its cells unjudged", () => {
  const batch = previewBatch(1);
  const judged = batch.assessments.find((item) => item.kind === "judge");
  assert.ok(judged?.kind === "judge" && judged.requestedModel !== null);
  const dropped = judged.requestedModel;
  const partial = { ...batch, assessments: batch.assessments.filter((item) => item.kind !== "judge" || item.requestedModel?.model !== dropped.model || item.runId !== judged.runId) };
  const cell = cells([partial]).find((item) => item.runIds.includes(judged.runId) && item.judge === choiceKey(dropped));
  assert.ok(cell);
  assert.equal(cell.judged, cell.runs - 1);
});

test("a newer batch with no finished run of a setup hides nothing, and one with runs replaces the older runs", () => {
  const older = cells([previewBatch(1)])[0];
  assert.ok(older);
  const newer = { ...older, batchId: "newer", createdAt: `${older.createdAt}z` };
  const pick = (...items: (typeof older)[]) => newestCells(items.map((cell) => ({ cell }))).map(({ cell }) => cell.batchId);
  assert.deepEqual(pick(older, { ...newer, runs: 0 }), [older.batchId]);
  assert.deepEqual(pick({ ...newer, runs: 0 }, older), [older.batchId]);
  assert.deepEqual(pick(older, newer), [newer.batchId]);
  assert.deepEqual(pick({ ...older, runs: 0 }, { ...newer, runs: 0 }), [newer.batchId]);
});

test("a cell names the agent program's version when its batch recorded one", () => {
  const batch = previewBatch(1), tool = "codex", version = "1.2.3";
  assert.equal(agentVersion(batch, tool), null);
  const recorded = { ...batch, experiment: { ...batch.experiment, pinnedTools: { "@openai/codex": version } } };
  assert.ok(agentVersion(recorded, tool)?.endsWith(version));
  assert.equal(agentVersion(recorded, "another tool"), null);
});
