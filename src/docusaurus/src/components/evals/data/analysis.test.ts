import assert from "node:assert/strict";
import { test } from "node:test";
import { fixture, FixtureDigest } from "../records/test-fixtures.ts";
import { gradingScope, interval, latestRun, proportionInterval } from "./analysis.ts";
import { summaries, historyStatus } from "./summaries.ts";
test("intervals keep missing observations and never invent a single-attempt interval", () => {
  assert.deepEqual(interval([null, 1]), { mean: 1, low: null, high: null, samples: 1, missing: 1 });
  assert.deepEqual(interval([0, 1, null]), { mean: 0.5, low: 0, high: 1, samples: 2, missing: 1 });
});
test("binary intervals reflect uncertainty even when all five attempts pass", () => {
  const result = proportionInterval([true, true, true, true, true]);
  assert.equal(result.mean, 1);
  assert.ok(result.low !== null && result.low < 0.6);
  assert.ok(result.high !== null && result.high > 0.99);
  assert.equal(proportionInterval([true]).low, null);
});
test("latest assessments apply without changing pipeline status and grading versions partition comparisons", () => {
  const batch = fixture();
  const run = batch.runs[0];
  const initial = batch.assessments.find((item) => item.kind === "checks");
  if (run === undefined || initial?.kind !== "checks")
    throw new Error("Incomplete fixture.");
  const first = latestRun(run, batch.assessments);
  assert.equal(first.result?.coverage.share, 1);
  const changed = { ...initial, id: `${initial.id}-next`, versions: { ...initial.versions, checker: `${FixtureDigest}-next` }, outcomes: initial.outcomes.map((check) => ({ ...check, status: "blocked" })) };
  const next = latestRun(run, [...batch.assessments, changed]);
  assert.equal(next.result?.coverage.share, 0);
  assert.equal(next.pipelineStatus, run.pipelineStatus);
  assert.notEqual(gradingScope(batch, next), gradingScope(batch, first));
  const row = summaries(batch)[0];
  assert.equal(row?.tokens.missing, 1);
  assert.equal(row?.coverage.mean, 1);
  if (row !== undefined)
    assert.equal(historyStatus(undefined, row), "Inconclusive");
});
