import assert from "node:assert/strict";
import { test } from "node:test";
import { MatrixJudges } from "../../config/matrix.ts";
import { judgeRun, type JudgeAssessor } from "./judge.ts";
import { answerObject } from "./reply.ts";
import { BallotCount } from "./rubric.ts";
import { assessorResult, ballotFixture, BallotTimeoutMs } from "./test-fixture.ts";

test("both assessors accept the ballot alone, in a fence and with prose", async (t) => {
  const fixture = await ballotFixture(t);
  const replies = [fixture.reply, `\`\`\`json\n${fixture.reply}\n\`\`\``, `Here is the ballot.\n${fixture.reply}\nReview complete.`];
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
  for (const invalid of ["No answer.", "{} {}", "```json\n{}\n```\n```json\n{}\n```", '[{"answers":[]}]', '{"answers": [}', '{"answers":[]', `{} ${reply}`]) assert.throws(() => answerObject(invalid));
});
