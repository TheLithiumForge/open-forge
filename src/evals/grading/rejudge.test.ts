import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { FixtureSuite } from "../config/test-fixtures.ts";
import { MatrixJudges } from "../config/matrix.ts";
import { judgeOptions } from "../config/preset-options.ts";
import { JudgePresets, PresetName } from "../config/presets.ts";
import { Flag } from "../config/commands.ts";
import { AssessmentKind, FailureKind } from "../history/statuses.ts";
import { FixtureVersions, temporaryHistory, v2Fixture } from "../history/test-fixtures.ts";
import { ResultFiles } from "../paths.ts";
import { writeNew } from "../materialize/files.ts";
import { judgeSavedRun, rejudgeRuns } from "./rejudge.ts";

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

test("rejudge judge presets append the chosen judges without altering the batch config", async (t) => {
  const history = await temporaryHistory(); t.after(history.cleanup);
  const { batch, run } = v2Fixture();
  await history.store.saveBatch(batch); await history.store.saveRun(run);
  await writeNew(join(history.store.runFolder(batch.id, run.id), ResultFiles.files), "game.ts", "export const game = true;\n");
  const request = judgeOptions([batch.id, Flag.judges, PresetName.both]);
  await rejudgeRuns(FixtureSuite, [run], { ...batch.config, ...request.overrides }, FixtureVersions,
    { store: history.store, judge: async () => [] }, request.onlyRun);
  const judgments = await history.store.readAssessments(batch.id);
  assert.deepEqual(judgments.map((assessment) => assessment.kind === AssessmentKind.judge ? assessment.requestedModel : null), JudgePresets[PresetName.both]);
  assert.deepEqual((await history.store.readBatch(batch.id)).config, batch.config);
});

test("rejudge skips an attempt that stopped on an infrastructure failure and kept no files", async (t) => {
  const history = await temporaryHistory(); t.after(history.cleanup);
  const { batch, run } = v2Fixture();
  const stopped = { ...run, failureKind: FailureKind.infrastructure };
  await history.store.saveBatch(batch); await history.store.saveRun(stopped);
  let judged = 0;
  await rejudgeRuns(FixtureSuite, [stopped], batch.config, FixtureVersions, { store: history.store, judge: async () => { judged += 1; return []; } });
  assert.equal(judged, 0);
  assert.deepEqual(await history.store.readAssessments(batch.id), []);
});

test("judging only missing runs skips a run that judge already judged and asks again after an error", async (t) => {
  const history = await temporaryHistory(); t.after(history.cleanup);
  const { batch, run } = v2Fixture();
  await history.store.saveBatch(batch); await history.store.saveRun(run);
  await writeNew(join(history.store.runFolder(batch.id, run.id), ResultFiles.files), "game.ts", "export const game = true;\n");
  const request = judgeOptions([batch.id, Flag.judges, PresetName.codex, Flag.missing]);
  const config = { ...batch.config, ...request.overrides };
  let judged = 0;
  const failing = { store: history.store, judge: async () => { judged += 1; throw new Error("Fixture judge failure."); } };
  const working = { store: history.store, judge: async () => { judged += 1; return []; } };
  await rejudgeRuns(FixtureSuite, [run], config, FixtureVersions, failing, request.onlyRun, request.missingOnly);
  await rejudgeRuns(FixtureSuite, [run], config, FixtureVersions, working, request.onlyRun, request.missingOnly);
  await rejudgeRuns(FixtureSuite, [run], config, FixtureVersions, working, request.onlyRun, request.missingOnly);
  assert.equal(judged, 2);
  assert.deepEqual((await history.store.readAssessments(batch.id)).map((assessment) => assessment.error === null), [false, true]);
});
