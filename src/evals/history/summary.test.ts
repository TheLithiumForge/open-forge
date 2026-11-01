import assert from "node:assert/strict";
import { test } from "node:test";
import { assessRun, checksAssessment } from "./assessments.ts";
import { replacementRun } from "./run-records.ts";
import { CheckStatus, FailureKind, RunStatus } from "./statuses.ts";
import { summarize } from "./summary.ts";
import { FixtureVersions, v2Fixture } from "./test-fixtures.ts";

test("a judge failure keeps coverage and failed attempts keep missing token counts", () => {
  const { run } = v2Fixture();
  const assessment = checksAssessment(run, run.checks, FixtureVersions);
  const passed = assessRun({ ...run, pipelineStatus: RunStatus.failed, failureKind: FailureKind.grading }, [assessment]);
  const errored = { ...assessment, outcomes: assessment.outcomes.map((check) => ({ ...check, status: CheckStatus.error })) };
  const failed = assessRun({ ...run, id: `${run.id}-failed`, pipelineStatus: RunStatus.failed }, [{ ...errored, runId: `${run.id}-failed` }]);
  const row = summarize([passed, failed], [])[0];
  assert.equal(row?.coverage, 0.5);
  assert.equal(row?.statedSuccess, 0.5);
  assert.equal(row?.unresolvedErrors, 1);
  assert.equal(row?.tokenMissing, 2);
  assert.equal(row?.attempts, 2);
  assert.equal(row?.tokens, null);
});

test("a replaced infrastructure failure stays recorded without counting twice", () => {
  const { run } = v2Fixture();
  const failed = { ...run, status: RunStatus.failed, failureKind: FailureKind.infrastructure };
  const replacement = { ...replacementRun(failed, `${run.id}-replacement`, "retry"), status: RunStatus.done, startedAt: run.startedAt };
  const row = summarize([failed, replacement], [])[0];
  assert.equal(row?.runs, 2);
  assert.equal(row?.attempts, 1);
  assert.equal(row?.tokenMissing, 2);
  assert.equal(failed.replacesRunId, null);
});
