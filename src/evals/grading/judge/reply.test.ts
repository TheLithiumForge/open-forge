import assert from "node:assert/strict";
import { test } from "node:test";
import { MatrixJudges } from "../../config/matrix.ts";
import { judgeRun, type JudgeAssessor } from "./judge.ts";
import { answerObject, assessorReply, JudgeAssessorFailure, JudgeLimited } from "./reply.ts";
import { Tool } from "../../config/choices.ts";
import type { ProcessResult } from "../../execution/process.ts";
import { BallotCount } from "./rubric.ts";
import { assessorResult, ballotFixture, BallotTimeoutMs } from "./test-fixture.ts";

test("both assessors accept the ballot object or answers array alone, in a fence and with prose", async (t) => {
  const fixture = await ballotFixture(t);
  const replies = [fixture.reply, JSON.stringify(fixture.answers)].flatMap((reply) => [reply, `\`\`\`json\n${reply}\n\`\`\``, `Here is the ballot.\n${reply}\nReview complete.`]);
  for (const choice of MatrixJudges) for (const reply of replies) {
    let calls = 0;
    const assessor: JudgeAssessor = { supportsImages: true, run: async () => { calls += 1; return assessorResult(choice.tool, reply); } };
    const answers = await judgeRun(fixture.suite, choice, fixture.folder, fixture.home, BallotTimeoutMs, { assessor });
    assert.equal(calls, BallotCount);
    assert.ok(answers.every((answer) => answer.unsure === BallotCount));
  }
});

test("only one outer object can be read and quoted delimiters don't create candidates", () => {
  const value = { answers: [{ evidence: 'A brace } and an escaped " quote {' }] };
  const reply = JSON.stringify(value);
  assert.deepEqual(answerObject(`Result:\n${reply}\nDone.`), value);
  for (const invalid of ["No answer.", "{} {}", "```json\n{}\n```\n```json\n{}\n```", '[] {}', '{"answers": [}', '{"answers":[]', `{} ${reply}`]) assert.throws(() => answerObject(invalid));
});

test("a usage limit is told apart from other assessor failures and from a judge quoting limits", () => {
  const LimitText = "You've hit your 5-hour limit";
  const [claude, codex] = [MatrixJudges.find((choice) => choice.tool === Tool.claude), MatrixJudges.find((choice) => choice.tool === Tool.codex)];
  assert.ok(claude && codex);
  const failed = (stdout: string): ProcessResult => ({ code: 1, timedOut: false, stderr: "", stdout });
  assert.throws(() => assessorReply(failed(JSON.stringify({ type: "result", is_error: true, api_error_status: 429, result: LimitText })), claude), JudgeLimited);
  assert.throws(() => assessorReply({ ...failed(JSON.stringify({ type: "result", is_error: true, result: LimitText })), code: 0 }, claude), JudgeLimited);
  assert.throws(() => assessorReply(failed(JSON.stringify({ type: "error", message: LimitText })), codex), JudgeLimited);
  for (const [result, choice] of [[failed(JSON.stringify({ type: "result", is_error: true, result: "Fixture failure." })), claude], [failed(JSON.stringify({ type: "turn.failed" })), codex]] as const) {
    assert.throws(() => assessorReply(result, choice), (error: unknown) => error instanceof JudgeAssessorFailure && !(error instanceof JudgeLimited));
  }
  const quoted = JSON.stringify({ answers: [{ evidence: `The game shows ${LimitText} and a 429 retry.` }] });
  for (const choice of [claude, codex]) assert.equal(assessorReply(assessorResult(choice.tool, quoted), choice), quoted);
});
