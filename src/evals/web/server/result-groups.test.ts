import assert from "node:assert/strict";
import { test } from "node:test";
import { v2Fixture, FixtureVersions } from "../../history/test-fixtures.ts";
import { checksAssessment } from "../../history/assessments.ts";
import { resultGroups } from "./result-groups.ts";

test("different grading versions never share a local summary", () => {
  const { run } = v2Fixture();
  const first = checksAssessment(run, run.checks, FixtureVersions);
  const other = { ...run, id: `${run.id}-other` };
  const next = checksAssessment(other, other.checks, { ...FixtureVersions, checker: `${FixtureVersions.checker}-next` });
  const rows = resultGroups([{ ...run, checksAssessmentId: first.id }, { ...other, checksAssessmentId: next.id }], [], [first, next]);
  assert.equal(rows.length, 2);
  assert.deepEqual(rows.map((group) => group.runs.length), [1, 1]);
});
