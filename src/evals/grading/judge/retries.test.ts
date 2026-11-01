import assert from "node:assert/strict";
import { test } from "node:test";
import { MatrixJudges } from "../../config/matrix.ts";
import type { JudgeEvidence } from "./evidence.ts";
import { judgeRun, type JudgeAssessor } from "./judge.ts";
import { JudgeFormatRetries, JudgeFailureReplyBytes, type JudgeReport } from "./report.ts";
import { BallotCount } from "./rubric.ts";
import { assessorResult, ballotFixture, BallotTimeoutMs } from "./test-fixture.ts";

test("format retries use the same prompt and evidence in fresh samples before completing all ballots", async (t) => {
  const fixture = await ballotFixture(t);
  for (const choice of MatrixJudges) {
    const calls: { prompt: string; home: string; evidence: JudgeEvidence }[] = [];
    const reports: JudgeReport[] = [];
    const assessor: JudgeAssessor = { supportsImages: true, run: async (space, _choice, prompt, _options, evidence) => {
      calls.push({ prompt, home: space.home, evidence });
      const reply = calls.length === 1 ? "Unreadable ballot." : calls.length === 2 ? JSON.stringify({ answers: fixture.answers.slice(1) }) : fixture.reply;
      return assessorResult(choice.tool, reply);
    } };
    const answers = await judgeRun(fixture.suite, choice, fixture.folder, fixture.home, BallotTimeoutMs, { assessor, onReport: (report) => reports.push(report) });
    assert.equal(calls.length, BallotCount + JudgeFormatRetries);
    assert.equal(new Set(calls.map((call) => call.home)).size, calls.length);
    assert.ok(calls.every((call) => call.prompt === calls[0]?.prompt && call.evidence === calls[0]?.evidence));
    assert.ok(answers.every((answer) => answer.unsure === BallotCount && answer.yes === 0 && answer.no === 0));
    assert.deepEqual(reports, [{ formatRetries: JudgeFormatRetries, failures: [] }]);
  }
});

test("an exhausted ballot keeps its last reply as capped UTF-8 data with its validation error", async (t) => {
  const fixture = await ballotFixture(t);
  const reports: JudgeReport[] = [], replies: string[] = [];
  const assessor: JudgeAssessor = { supportsImages: true, run: async (_space, choice) => {
    const reply = JSON.stringify({ answers: fixture.answers.slice(1), evidence: `<script>untrusted${replies.length}</script>${"💡".repeat(JudgeFailureReplyBytes)}` });
    replies.push(reply);
    return assessorResult(choice.tool, reply);
  } };
  await assert.rejects(judgeRun(fixture.suite, MatrixJudges[0], fixture.folder, fixture.home, BallotTimeoutMs, { assessor, onReport: (report) => reports.push(report) }));
  assert.equal(replies.length, JudgeFormatRetries + 1);
  const report = reports[0], failure = report?.failures[0];
  assert.ok(failure);
  assert.equal(report?.formatRetries, JudgeFormatRetries);
  assert.equal(failure.ballot, 1);
  assert.equal(failure.formatRetries, JudgeFormatRetries);
  assert.ok(failure.error.length > 0);
  assert.ok(Buffer.byteLength(failure.reply) <= JudgeFailureReplyBytes);
  assert.ok(replies.at(-1)?.startsWith(failure.reply));
  assert.ok(!failure.reply.endsWith("�"));
});

test("an assessor process failure keeps its output without spending format retries", async (t) => {
  const fixture = await ballotFixture(t);
  const reply = "Fixture provider failure.", reports: JudgeReport[] = [];
  let calls = 0;
  const assessor: JudgeAssessor = { supportsImages: true, run: async () => { calls += 1; return { code: 1, timedOut: false, stderr: "", stdout: reply }; } };
  await assert.rejects(judgeRun(fixture.suite, MatrixJudges[0], fixture.folder, fixture.home, BallotTimeoutMs, { assessor, onReport: (report) => reports.push(report) }));
  assert.equal(calls, 1);
  assert.equal(reports[0]?.formatRetries, 0);
  assert.equal(reports[0]?.failures[0]?.reply, reply);
});
