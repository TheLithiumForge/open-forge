import assert from "node:assert/strict";
import { mkdir, mkdtemp, readFile, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { test, type TestContext } from "node:test";
import { FixtureSuite } from "../config/test-fixtures.ts";
import { MatrixJudges } from "../config/matrix.ts";
import { modelKey } from "../config/judges.ts";
import { temporaryHistory, v2Fixture } from "../history/test-fixtures.ts";
import { AssessmentKind, CheckStatus, RunStatus } from "../history/statuses.ts";
import { ResultFiles } from "../paths.ts";
import { plannedRun } from "../history/run-records.ts";
import { TurnOutcome } from "./providers/agent-session.ts";
import { completeSavedJudgments } from "../grading/rejudge.ts";
import { executeRun, type RunServices } from "./run.ts";
import { JudgeLimited } from "../grading/judge/reply.ts";
import { Judging } from "../config/choices.ts";

const SourceFile = "game.ts", SourceText = "export const game = 'fixture';\n", BuiltPage = "dist/index.html";
const Telemetry = { tokens: null, requests: null, peakContext: null, contextWindow: null, compactions: null, toolCalls: 0, failedToolCalls: 0,
  openForgeReads: 0, openForgeBytes: null, reportedCostUsd: null, model: null, sessionId: null } as const;

async function matrixRun(t: TestContext, failFirst = false) {
  const history = await temporaryHistory(); t.after(history.cleanup);
  const { batch: original, run: template } = v2Fixture();
  const run = plannedRun(template.id, original.id, template.setup, template.agent, template.repeat);
  const config = { ...original.config, judge: MatrixJudges[0], judges: MatrixJudges };
  const batch = { ...original, config };
  await history.store.saveBatch(batch); await history.store.saveRun(run);
  const calls: string[] = [], sources: string[] = [], images: (readonly string[])[] = [], snapshots: string[] = [];
  let conversations = 0, builds = 0, checks = 0, beforeSecond: Buffer | undefined;
  const scratch = () => mkdtemp(join(history.root, "judge-"));
  // Real providers and candidate execution are replaced at the run boundary.
  const services: RunServices = { store: history.store, stop: async () => {}, scratch,
    createSpace: async (project, snapshot) => {
      snapshots.push(snapshot);
      const root = join(history.root, "attempt"), work = join(root, project), home = join(root, "home");
      await mkdir(work, { recursive: true }); await mkdir(home);
      await writeFile(join(work, SourceFile), SourceText);
      return { root, work, home, project };
    },
    conversation: async () => { conversations += 1; return { outcome: TurnOutcome.answered, finalMessage: "Fixture complete.", questions: [], telemetry: Telemetry, error: null, seconds: 0 }; },
    build: async (_isolation, space) => {
      builds += 1; await mkdir(join(space.work, "dist")); await writeFile(join(space.work, BuiltPage), "<html>Fixture</html>");
      return { passed: true, typecheck: true, seconds: 0, packages: 0, html: BuiltPage, bytes: 1, log: "Fixture verifier." };
    },
    checks: async () => { checks += 1; return { checks: [{ id: "fixture-check", area: "play", status: CheckStatus.pass, evidence: "Fixture check evidence." }], shots: [], performance: null }; },
    judge: async (_suite, choice, folder, _home, _timeout, options) => {
      calls.push(modelKey(choice)); sources.push(await readFile(join(folder, SourceFile), "utf8")); images.push(options?.images ?? []);
      if (calls.length === 2) beforeSecond = await readFile(join(history.store.runFolder(batch.id, run.id), ResultFiles.run));
      if (failFirst && calls.length === 1) throw new Error("Fixture first judge unavailable.");
      return [{ id: "fixture-question", area: "rules", question: "Fixture rule", yes: 3, no: 0, unsure: 0, evidence: ["Fixture source evidence."] }];
    },
  };
  return { ...history, batch, config, run, services, calls, sources, images, snapshots, counters: () => ({ conversations, builds, checks }), beforeSecond: () => beforeSecond };
}

test("one implementation is judged in order and the second assessment leaves the saved run unchanged", async (t) => {
  const fixture = await matrixRun(t);
  const output = await executeRun({ run: fixture.run, suite: FixtureSuite, config: fixture.config, snapshot: "", position: "fixture", services: fixture.services });
  assert.equal(output.status, RunStatus.done);
  assert.deepEqual(fixture.calls, MatrixJudges.map(modelKey));
  assert.deepEqual(fixture.sources, [SourceText, SourceText]);
  assert.deepEqual(fixture.images[0], fixture.images[1]);
  assert.deepEqual(fixture.counters(), { conversations: 1, builds: 1, checks: 1 });
  const judgments = (await fixture.store.readAssessments(fixture.batch.id)).filter((item) => item.kind === AssessmentKind.judge);
  assert.equal(judgments.length, MatrixJudges.length);
  assert.ok(judgments.every((item) => item.replacesAssessmentId === null));
  assert.deepEqual(judgments.map((item) => item.requestedModel), MatrixJudges);
  assert.deepEqual(await readFile(join(fixture.store.runFolder(fixture.batch.id, fixture.run.id), ResultFiles.run)), fixture.beforeSecond());
});

test("a first judge error stays recorded and does not block the second judge", async (t) => {
  const fixture = await matrixRun(t, true);
  await executeRun({ run: fixture.run, suite: FixtureSuite, config: fixture.config, snapshot: "", position: "fixture", services: fixture.services });
  const judgments = (await fixture.store.readAssessments(fixture.batch.id)).filter((item) => item.kind === AssessmentKind.judge);
  assert.deepEqual(fixture.calls, MatrixJudges.map(modelKey));
  assert.notEqual(judgments[0]?.error, null);
  assert.equal(judgments[1]?.error, null);
  assert.equal((await fixture.store.readRun(fixture.batch.id, fixture.run.id)).functionalResult?.coverage.share, 1);
});

test("a plan that judges later finishes each run after its checks and calls no judge", async (t) => {
  const fixture = await matrixRun(t);
  const output = await executeRun({ run: fixture.run, suite: FixtureSuite, config: { ...fixture.config, judging: Judging.later }, snapshot: "", position: "fixture", services: fixture.services });
  assert.equal(output.status, RunStatus.done);
  assert.deepEqual(fixture.calls, []);
  assert.deepEqual(fixture.counters(), { conversations: 1, builds: 1, checks: 1 });
  const saved = await fixture.store.readRun(fixture.batch.id, fixture.run.id);
  assert.equal(saved.judgeAssessmentId, null);
  assert.notEqual(saved.checksAssessmentId, null);
  assert.equal(saved.functionalResult?.coverage.share, 1);
  assert.deepEqual((await fixture.store.readAssessments(fixture.batch.id)).map((item) => item.kind), [AssessmentKind.checks]);
});

test("resume fills only missing judgments and never starts the implementer again", async (t) => {
  const fixture = await matrixRun(t);
  await executeRun({ run: fixture.run, suite: FixtureSuite, config: { ...fixture.config, judges: [MatrixJudges[0]] }, snapshot: "", position: "fixture", services: fixture.services });
  const saved = await fixture.store.readRun(fixture.batch.id, fixture.run.id);
  const first = (await fixture.store.readAssessments(fixture.batch.id)).find((item) => item.kind === AssessmentKind.judge); assert.ok(first);
  const dependencies = { store: fixture.store, judge: fixture.services.judge, scratch: fixture.services.scratch };
  const judge = dependencies.judge, scratch = dependencies.scratch; assert.ok(judge && scratch);
  await completeSavedJudgments(FixtureSuite, saved, fixture.config, first.versions, { store: fixture.store, judge, scratch });
  await completeSavedJudgments(FixtureSuite, saved, fixture.config, first.versions, { store: fixture.store, judge, scratch });
  assert.deepEqual(fixture.calls, MatrixJudges.map(modelKey));
  assert.deepEqual(fixture.counters(), { conversations: 1, builds: 1, checks: 1 });
});

test("grading saved files builds, checks and judges again without the agent and keeps the attempt's conversation", async (t) => {
  const fixture = await matrixRun(t);
  const config = { ...fixture.config, judges: [MatrixJudges[0]] };
  await executeRun({ run: fixture.run, suite: FixtureSuite, config, snapshot: "", position: "fixture", services: fixture.services });
  const folder = fixture.store.runFolder(fixture.batch.id, fixture.run.id);
  const finished = await fixture.store.readRun(fixture.batch.id, fixture.run.id);
  const source = await readFile(join(folder, ResultFiles.files, SourceFile));
  const interrupted = { ...finished, status: RunStatus.checking, finishedAt: null };
  await fixture.store.saveRun(interrupted);
  const before = await readFile(join(folder, "conversation.json"));
  const output = await executeRun({ run: interrupted, suite: FixtureSuite, config, snapshot: "", position: "fixture", services: fixture.services, savedFiles: true });
  assert.equal(output.status, RunStatus.done);
  assert.equal(output.startedAt, finished.startedAt);
  assert.deepEqual(fixture.counters(), { conversations: 1, builds: 2, checks: 2 });
  assert.equal(fixture.snapshots.at(-1), join(folder, ResultFiles.files));
  assert.deepEqual(await readFile(join(folder, "conversation.json")), before);
  assert.deepEqual(await readFile(join(folder, ResultFiles.files, SourceFile)), source);
});

test("a judge usage limit keeps the checked attempt without a judgment and resume adds it", async (t) => {
  const fixture = await matrixRun(t);
  const judge = fixture.services.judge, scratch = fixture.services.scratch; assert.ok(judge && scratch);
  let limited = true;
  const services: RunServices = { ...fixture.services, judge: async (...args) => {
    if (limited) { limited = false; throw new JudgeLimited(); }
    return judge(...args);
  } };
  await assert.rejects(executeRun({ run: fixture.run, suite: FixtureSuite, config: fixture.config, snapshot: "", position: "fixture", services }), JudgeLimited);
  const saved = await fixture.store.readRun(fixture.batch.id, fixture.run.id);
  assert.equal(saved.status, RunStatus.done);
  assert.equal(saved.judgeAssessmentId, null);
  const assessments = await fixture.store.readAssessments(fixture.batch.id);
  assert.deepEqual(assessments.filter((item) => item.kind === AssessmentKind.judge), []);
  const checked = assessments.find((item) => item.kind === AssessmentKind.checks); assert.ok(checked);
  await completeSavedJudgments(FixtureSuite, saved, fixture.config, checked.versions, { store: fixture.store, judge, scratch });
  const judgments = (await fixture.store.readAssessments(fixture.batch.id)).filter((item) => item.kind === AssessmentKind.judge);
  assert.deepEqual(judgments.map((item) => item.requestedModel), MatrixJudges);
  assert.ok(judgments.every((item) => item.error === null));
  assert.deepEqual(fixture.counters(), { conversations: 1, builds: 1, checks: 1 });
});

test("a verifier or checker fault still sends the saved implementation to both judges", async (t) => {
  for (const stage of ["build", "checks"] as const) {
    const fixture = await matrixRun(t);
    const fail = async (): Promise<never> => { throw new Error("Fixture grading stage fault."); };
    const services = stage === "build" ? { ...fixture.services, build: fail } : { ...fixture.services, checks: fail };
    const result = await executeRun({ run: fixture.run, suite: FixtureSuite, config: fixture.config, snapshot: "", position: "fixture", services });
    assert.equal(result.status, RunStatus.failed);
    assert.deepEqual(fixture.calls, MatrixJudges.map(modelKey));
    assert.deepEqual(fixture.sources, [SourceText, SourceText]);
    assert.equal(fixture.counters().conversations, 1);
  }
});
