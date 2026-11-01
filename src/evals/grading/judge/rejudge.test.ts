import assert from "node:assert/strict";
import { mkdir, mkdtemp, readFile, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { MatrixJudges } from "../../config/matrix.ts";
import { executeRun, type RunServices } from "../../execution/run.ts";
import { TurnOutcome } from "../../execution/providers/agent-session.ts";
import { HistoryFiles } from "../../history/artifacts.ts";
import { FixtureCheckId, FixtureVersions, v2Fixture } from "../../history/test-fixtures.ts";
import { AssessmentKind, CheckStatus, FailureKind, RunStatus } from "../../history/statuses.ts";
import { plannedRun } from "../../history/run-records.ts";
import { ResultFiles } from "../../paths.ts";
import { rejudgeRuns } from "../rejudge.ts";
import { judgeRun, type JudgeAssessor } from "./judge.ts";
import { JudgeFormatRetries } from "./report.ts";
import { BallotCount } from "./rubric.ts";
import { assessorResult, ballotFixture, BallotSource, BallotSourceText } from "./test-fixture.ts";

const Telemetry = { tokens: null, requests: null, peakContext: null, contextWindow: null, compactions: null, toolCalls: 0, failedToolCalls: 0,
  openForgeReads: 0, openForgeBytes: null, reportedCostUsd: null, model: null, sessionId: null } as const;

test("a grading failure keeps the last ballot reply and whole-batch rejudge appends both judges without changing checks or the run", async (t) => {
  const fixture = await ballotFixture(t), original = v2Fixture();
  const config = { ...original.batch.config, judge: MatrixJudges[0], judges: MatrixJudges };
  const batch = { ...original.batch, config };
  const run = plannedRun(original.run.id, batch.id, original.run.setup, original.run.agent, original.run.repeat);
  await fixture.store.saveBatch(batch); await fixture.store.saveRun(run);
  const failedReply = JSON.stringify({ answers: fixture.answers.slice(1), note: "<script>Untrusted reply data.</script>" });
  const prompts: string[] = [];
  const failing: JudgeAssessor = { supportsImages: true, run: async (_space, choice, prompt) => {
    prompts.push(prompt); return assessorResult(choice.tool, failedReply);
  } };
  const scratch = () => mkdtemp(join(fixture.root, "judge-"));
  const services: RunServices = { store: fixture.store, stop: async () => {}, scratch,
    createSpace: async (project) => {
      const root = join(fixture.root, "attempt"), work = join(root, project), home = join(root, "home");
      await mkdir(work, { recursive: true }); await mkdir(home);
      await writeFile(join(work, BallotSource), BallotSourceText);
      return { root, work, home, project };
    },
    conversation: async () => ({ outcome: TurnOutcome.answered, finalMessage: "Fixture complete.", questions: [], telemetry: Telemetry, error: null, seconds: 0 }),
    build: async () => ({ passed: true, typecheck: true, seconds: 0, packages: 0, html: null, bytes: null, log: "Fixture verifier." }),
    checks: async () => ({ checks: [{ id: FixtureCheckId, area: "play", status: CheckStatus.pass, evidence: "Fixture check evidence." }], shots: [], performance: null }),
    judge: (suite, choice, folder, home, timeout, options) => judgeRun(suite, choice, folder, home, timeout, { ...options, assessor: failing }),
  };
  await executeRun({ run, suite: fixture.suite, config, snapshot: "", position: "fixture", services });
  const failed = await fixture.store.readRun(batch.id, run.id);
  assert.equal(failed.status, RunStatus.failed);
  assert.equal(failed.failureKind, FailureKind.grading);
  const before = await fixture.store.readAssessments(batch.id);
  const firstJudgments = before.filter((item) => item.kind === AssessmentKind.judge);
  assert.equal(firstJudgments.length, MatrixJudges.length);
  assert.ok(firstJudgments.every((item) => item.error !== null && item.formatRetries === JudgeFormatRetries
    && item.ballotFailures?.length === 1 && item.ballotFailures[0]?.reply === failedReply));
  const runPath = join(fixture.store.runFolder(batch.id, run.id), ResultFiles.run);
  const checksPath = join(fixture.store.batchFolder(batch.id), HistoryFiles.assessments, `${AssessmentKind.checks}.jsonl`);
  const runBytes = await readFile(runPath), checkBytes = await readFile(checksPath);
  const calls = new Map<string, number>();
  const recovered: JudgeAssessor = { supportsImages: true, run: async (_space, choice, prompt) => {
    prompts.push(prompt);
    const count = (calls.get(choice.tool) ?? 0) + 1; calls.set(choice.tool, count);
    return assessorResult(choice.tool, count === 1 ? "Unreadable ballot." : fixture.reply);
  } };
  await rejudgeRuns(fixture.suite, await fixture.store.readRuns(batch), config, FixtureVersions,
    { store: fixture.store, scratch, options: { assessor: recovered } });
  const after = await fixture.store.readAssessments(batch.id);
  assert.deepEqual(after.slice(0, before.length), before);
  const latest = after.slice(before.length).filter((item) => item.kind === AssessmentKind.judge);
  assert.equal(latest.length, MatrixJudges.length);
  assert.deepEqual(latest.map((item) => item.requestedModel), MatrixJudges);
  assert.deepEqual(latest.map((item) => item.replacesAssessmentId), firstJudgments.map((item) => item.id));
  assert.ok(latest.every((item) => item.error === null && item.formatRetries === 1 && item.ballotFailures?.length === 0));
  assert.ok([...calls.values()].every((count) => count === BallotCount + 1));
  assert.ok(prompts.every((prompt) => prompt === prompts[0]));
  assert.deepEqual(await readFile(runPath), runBytes);
  assert.deepEqual(await readFile(checksPath), checkBytes);
  assert.equal((await fixture.store.readRun(batch.id, run.id)).functionalResult?.coverage.share, 1);
});
