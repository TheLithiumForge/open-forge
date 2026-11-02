import assert from "node:assert/strict";
import { test } from "node:test";
import { previewBatch } from "../../../../scripts/evals/preview-fixture.ts";
import { cells } from "./cells.ts";
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

test("only the newest batch for a case and model counts", () => {
  const older = previewBatch(1), newer = previewBatch(2);
  const all = cells([older, newer]);
  assert.deepEqual(olderBatches([older, newer], all).map((batch) => batch.experiment.id), [older.experiment.id]);
});
