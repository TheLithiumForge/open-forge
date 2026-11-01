import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { FixtureConfig, FixtureSuite } from "../config/test-fixtures.ts";
import { MatrixJudges, PlanJudges } from "../config/matrix.ts";
import { FixtureVersions, temporaryHistory, v2Fixture } from "../history/test-fixtures.ts";
import { ResultFiles } from "../paths.ts";
import { writeNew } from "../materialize/files.ts";
import { judgeSavedRun, withPlanJudges } from "./rejudge.ts";

test("rejudging replaces only a prior assessment from the same judge and keeps every earlier record", async (t) => {
  const history = await temporaryHistory(); t.after(history.cleanup);
  const { batch, run } = v2Fixture();
  await history.store.saveBatch(batch); await history.store.saveRun(run);
  await writeNew(join(history.store.runFolder(batch.id, run.id), ResultFiles.files), "game.ts", "export const game = true;\n");
  const path = join(history.store.runFolder(batch.id, run.id), ResultFiles.run), before = await readFile(path);
  const dependencies = { store: history.store, judge: async () => [] };
  const first = await judgeSavedRun(FixtureSuite, run, MatrixJudges[0], FixtureVersions, dependencies);
  const second = await judgeSavedRun(FixtureSuite, run, MatrixJudges[1], FixtureVersions, dependencies);
  const repeated = await judgeSavedRun(FixtureSuite, run, MatrixJudges[0], FixtureVersions, dependencies);
  assert.ok(first && second && repeated);
  assert.equal(first.replacesAssessmentId, null);
  assert.equal(second.replacesAssessmentId, null);
  assert.equal(repeated.replacesAssessmentId, first.id);
  assert.deepEqual((await history.store.readAssessments(batch.id)).map((record) => record.id), [first.id, second.id, repeated.id]);
  assert.deepEqual(await readFile(path), before);
});

test("a plan's judges replace the batch's saved judges and keep the rest of its config", () => {
  const { batch } = v2Fixture();
  const saved = { ...batch.config, judge: MatrixJudges[0], judges: MatrixJudges };
  const plan = { ...FixtureConfig, judge: PlanJudges[0], judges: PlanJudges };
  const config = withPlanJudges(saved, plan);
  assert.deepEqual(config.judges, PlanJudges);
  assert.deepEqual(config.judge, PlanJudges[0]);
  assert.deepEqual({ ...config, judge: saved.judge, judges: saved.judges }, saved);
});
