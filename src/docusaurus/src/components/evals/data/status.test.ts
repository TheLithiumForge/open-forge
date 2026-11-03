import assert from "node:assert/strict";
import { test } from "node:test";
import { previewBatch } from "../../../../scripts/evals/preview-fixture.ts";
import type { Batch } from "../records/records.types.ts";
import { Judging } from "../records/schema-values.ts";
import { comparisons } from "./aggregate.ts";
import { cells, newestCells } from "./cells.ts";
import { DoneStatus, olderBatches, preliminaryReasons, PreliminaryReason } from "./status.ts";

const RunningStatus = "running";

test("a done batch with every run checked and judged is final", () => {
  const batch = previewBatch(1);
  const done = { ...batch, experiment: { ...batch.experiment, status: DoneStatus } };
  assert.deepEqual(preliminaryReasons([done], cells([done])), []);
});

test("a running batch and runs waiting on grading make the view preliminary", () => {
  const batch = previewBatch(1);
  const running = { ...batch, experiment: { ...batch.experiment, status: RunningStatus } };
  assert.deepEqual(preliminaryReasons([running], cells([running])), [PreliminaryReason.running]);
  const done = { ...batch, experiment: { ...batch.experiment, status: DoneStatus } };
  const shown = cells([done]);
  const first = shown[0];
  assert.ok(first);
  const waiting = [{ ...first, checked: first.runs - 1 }, ...shown.slice(1)];
  assert.deepEqual(preliminaryReasons([done], waiting), [PreliminaryReason.unchecked]);
  const recheck = [{ ...first, checked: first.runs - 1, recheck: 1 }, ...shown.slice(1)];
  assert.deepEqual(preliminaryReasons([done], recheck), [PreliminaryReason.recheck]);
  const unjudged = [{ ...first, judged: 0 }, ...shown.slice(1)];
  assert.deepEqual(preliminaryReasons([done], unjudged), [PreliminaryReason.unjudged]);
});

test("a batch that leaves judging for later is final without judgments", () => {
  const batch = previewBatch(1);
  const later = { ...batch, experiment: { ...batch.experiment, status: DoneStatus, config: { ...batch.experiment.config, judging: Judging.later } },
    assessments: batch.assessments.filter((item) => item.kind !== "judge") };
  const shown = cells([later]);
  assert.ok(shown.every((cell) => cell.judgedLater && cell.judged < cell.runs));
  assert.deepEqual(preliminaryReasons([later], shown), []);
});

test("only the newest runs of each setup count, so a later batch can add a setup beside its control", () => {
  const older = previewBatch(1), newer = previewBatch(2);
  const shown = (batches: readonly Batch[]) => newestCells(comparisons(cells(batches))).map((item) => item.cell);
  assert.deepEqual(olderBatches([older, newer], shown([older, newer])).map((batch) => batch.experiment.id), [older.experiment.id]);
  const [control, added] = newer.experiment.config.setups;
  assert.ok(control !== undefined && added !== undefined);
  const partial = { ...newer, experiment: { ...newer.experiment, config: { ...newer.experiment.config, setups: [control, added] } },
    runs: newer.runs.filter((run) => run.setup === control || run.setup === added) };
  const mixed = newestCells(comparisons(cells([older, partial])));
  assert.deepEqual(olderBatches([older, partial], mixed.map((item) => item.cell)), []);
  assert.deepEqual(new Set(mixed.map((item) => item.cell.setup)), new Set(older.experiment.config.setups));
  for (const item of mixed) {
    assert.equal(item.cell.batchId, [control, added].includes(item.cell.setup) ? partial.experiment.id : older.experiment.id);
    assert.equal(item.controlCell?.batchId, item.cell.batchId, "Each setup meets its own batch's control.");
  }
});
