import assert from "node:assert/strict";
import { test } from "node:test";
import { v2Fixture, FixtureVersions } from "../../history/test-fixtures.ts";
import { assessRun, checksAssessment, judgeAssessment } from "../../history/assessments.ts";
import { resultGroups } from "./result-groups.ts";
import { MatrixAgents, MatrixJudges } from "../../config/matrix.ts";
import { batchView } from "./routes.ts";
import type { AssessmentRecord } from "../../history/records.types.ts";

test("different grading versions never share a local summary", () => {
  const { run } = v2Fixture();
  const first = checksAssessment(run, run.checks, FixtureVersions);
  const other = { ...run, id: `${run.id}-other` };
  const next = checksAssessment(other, other.checks, { ...FixtureVersions, checker: `${FixtureVersions.checker}-next` });
  const rows = resultGroups([{ ...run, checksAssessmentId: first.id }, { ...other, checksAssessmentId: next.id }], [], [first, next]);
  assert.equal(rows.length, 2);
  assert.deepEqual(rows.map((group) => group.runs.length), [1, 1]);
});

test("the results API shows four pairs after reveal and hides their assessments before it", async () => {
  const { batch: original, run } = v2Fixture();
  const config = { ...original.config, layer: 1, agents: MatrixAgents, judge: MatrixJudges[0], judges: MatrixJudges };
  const runs = MatrixAgents.map((agent, index) => ({ ...run, id: `${run.id}-${index}`, agent, build: null }));
  const records: AssessmentRecord[] = [];
  for (const attempt of runs) {
    records.push(checksAssessment(attempt, attempt.checks, FixtureVersions));
    for (const choice of MatrixJudges) records.push({ ...judgeAssessment(attempt, [], FixtureVersions), requestedModel: choice });
  }
  let revealed = false;
  const history = { readBatch: async () => ({ ...original, config, runs: runs.map((run) => run.id), revealed }), readRuns: async () => runs.map((run) => assessRun(run, records)),
    readPreferences: async () => [], readAssessments: async () => records };
  const hidden = await batchView(original.id, history);
  assert.equal(hidden.summary, null);
  assert.equal(hidden.runs, null);
  assert.deepEqual(hidden.gradingGroups, []);
  assert.deepEqual(hidden.judgeDifferences, []);
  revealed = true;
  const shown = await batchView(original.id, history);
  assert.equal(shown.gradingGroups?.length, MatrixAgents.length * MatrixJudges.length);
  assert.equal(shown.implementationAttempts, runs.length);
  assert.equal(shown.judgeDifferences?.length, MatrixAgents.length);
});
