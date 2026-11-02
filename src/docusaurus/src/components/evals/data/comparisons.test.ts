import assert from "node:assert/strict";
import { test } from "node:test";
import { previewBatch } from "../../../../scripts/evals/preview-fixture.ts";
import { summaries } from "./summaries.ts";
import { differences } from "./differences.ts";
import { referenceTrends } from "./reference-trend.ts";
test("both judges and implementers have separate rows without doubling implementation usage", () => {
  const batch = previewBatch(1);
  const rows = summaries(batch);
  assert.equal(rows.length, 16);
  assert.equal(new Set(rows.map((row) => row.judge)).size, 2);
  assert.ok(rows.every((row) => row.runs === 2));
  assert.ok(rows.some((row) => row.tokens.mean === 160 && row.tokens.missing === 1));
  assert.ok(rows.some((row) => row.tokens.mean === null && row.tokens.missing === 2));
  const delta = differences(batch);
  assert.ok(delta.every((row) => row.missing === 0 && row.incompatible === 0 && row.scores.every((score) => Math.abs((score.value.mean ?? 0) + 1 / 3) < 0.001)));
});
test("recheck versions, missing judges and judge failures stay separate", () => {
  const batch = previewBatch(1), first = batch.assessments.find((item) => item.kind === "checks");
  if (first === undefined)
    throw new Error("Missing check");
  const changed = { ...first, id: `${first.id}-new`, versions: { ...first.versions, checker: "new-checker" } };
  const next = { ...batch, assessments: [...batch.assessments, changed] };
  assert.ok(summaries(next).some((row) => row.grading.includes("new-checker") && row.judgeMissing > 0));
  assert.ok(summaries(next).some((row) => !row.grading.includes("new-checker") && row.judgeMissing === 0));
  const broken = { ...batch, assessments: batch.assessments.map((item) => item.kind === "judge" ? { ...item, error: "Fixture grader failure" } : item) };
  assert.ok(summaries(broken).every((row) => row.rules.mean === null && row.judgeErrors > 0 && row.coverage.mean !== null));
  const missing = { ...batch, assessments: batch.assessments.filter((item) => item.kind !== "judge" || item.requestedModel?.model !== "fixture-beta") };
  assert.ok(summaries(missing).some((row) => row.judgeMissing === row.runs));
});
test("reference history keeps every assessment, source and grading apart from model comparisons", () => {
  const first = previewBatch(1, true), second = previewBatch(2, true);
  assert.equal(summaries(first).length, 0);
  const groups = referenceTrends([first, second]);
  assert.equal(groups.length, 3);
  assert.ok(groups.every((group) => group.points.length === 4));
  const shifted = { ...second, assessments: second.assessments.map((item) => ({ ...item, versions: { ...item.versions, spec: "changed-spec" } })) };
  assert.equal(referenceTrends([first, shifted]).length, 6);
});
test("judge differences never average attempts from different judge prompt versions", () => {
  const batch = previewBatch(1);
  const changedRuns = new Set(batch.runs.filter((run) => run.repeat === 2).map((run) => run.id));
  const shifted = {
    ...batch, assessments: batch.assessments.map((item) => item.kind === "judge" && changedRuns.has(item.runId)
      ? { ...item, versions: { ...item.versions, judge: "fixture-prompt-v2" } } : item)
  };
  const rows = differences(shifted);
  assert.equal(rows.length, 16);
  assert.ok(rows.every((row) => row.scores.every((score) => score.value.samples === 1 && score.value.low === null)));
});
