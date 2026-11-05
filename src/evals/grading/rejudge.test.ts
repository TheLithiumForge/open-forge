import assert from "node:assert/strict";
import { existsSync } from "node:fs";
import { mkdir, readFile, writeFile } from "node:fs/promises";
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
import { DocumentScaleCase } from "../materialize/brownfield-case.ts";
import { assessmentEvidenceFolder } from "../history/store.ts";
import { PlaythroughFiles, savedPlaythrough, type Playthrough } from "./judge/playthrough.ts";
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

test("every judge of a case with a playthrough reads one capture, and each judgment keeps what its judge saw", async (t) => {
  const history = await temporaryHistory(); t.after(history.cleanup);
  const { batch, run } = v2Fixture();
  await history.store.saveBatch(batch); await history.store.saveRun(run);
  const folder = history.store.runFolder(batch.id, run.id), Image = "shot.png", Trace = "[]\n";
  await writeNew(join(folder, ResultFiles.files), "game.ts", "export const game = true;\n");
  await writeFile(join(folder, ResultFiles.game), "<html></html>\n");
  let captures = 0;
  const capture = async (_suite: unknown, _html: string, target: string): Promise<Playthrough> => {
    captures += 1;
    await mkdir(target, { recursive: true });
    await writeFile(join(target, PlaythroughFiles.trace), Trace);
    await writeFile(join(target, Image), "image");
    const saved = await savedPlaythrough(target);
    assert.ok(saved !== null);
    return saved;
  };
  const seen: { trace: string; images: readonly string[] }[] = [];
  const judge = async (_suite: unknown, _choice: unknown, project: string, _home: string, _timeout: number, options?: { images?: readonly string[] }) => {
    seen.push({ trace: await readFile(join(project, PlaythroughFiles.folder, PlaythroughFiles.trace), "utf8"), images: options?.images ?? [] });
    return [];
  };
  const services = { store: history.store, capture, judge } as unknown as Parameters<typeof judgeSavedRun>[4];
  const suite = { ...FixtureSuite, id: DocumentScaleCase };
  const first = await judgeSavedRun(suite, run, MatrixJudges[0], FixtureVersions, services);
  const second = await judgeSavedRun(suite, run, MatrixJudges[1], FixtureVersions, services);
  assert.equal(captures, 1);
  assert.deepEqual(seen.map((item) => item.trace), [Trace, Trace]);
  assert.deepEqual(seen[0]?.images, seen[1]?.images);
  for (const judgment of [first, second]) assert.ok(existsSync(join(assessmentEvidenceFolder(batch.id, judgment.id), PlaythroughFiles.folder, Image)));
});

test("a case with a playthrough leaves out a run that never built, and setups can narrow the runs", async (t) => {
  const history = await temporaryHistory(); t.after(history.cleanup);
  const { batch, run } = v2Fixture();
  await history.store.saveBatch(batch); await history.store.saveRun(run);
  await writeNew(join(history.store.runFolder(batch.id, run.id), ResultFiles.files), "game.ts", "export const game = true;\n");
  let judged = 0;
  const services = { store: history.store, judge: async () => { judged += 1; return []; } };
  await rejudgeRuns({ ...FixtureSuite, id: DocumentScaleCase }, [run], batch.config, FixtureVersions, services);
  await rejudgeRuns(FixtureSuite, [run], batch.config, FixtureVersions, services, undefined, false, [`not-${run.setup}`]);
  assert.equal(judged, 0);
  await rejudgeRuns(FixtureSuite, [run], batch.config, FixtureVersions, services, undefined, false, [run.setup]);
  assert.ok(judged > 0);
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
