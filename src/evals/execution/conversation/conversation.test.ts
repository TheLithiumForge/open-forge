import assert from "node:assert/strict";
import { test } from "node:test";
import { readFile } from "node:fs/promises";
import { join } from "node:path";
import { converse } from "./conversation.ts";
import { ConversationFile, DefaultReply, QuestionRounds } from "./answer-bank.ts";
import { ConversationRole, SimulatorFailure, type ConversationReport } from "./report.ts";
import { Answer, classification, conversationFixture, Question, TestConfig, TestSuite, TestUsage } from "./test-fixtures.ts";

test("bundled decisions get only literal bank answers and count separately", async () => {
  const fixture = await conversationFixture([`${Question.color} ${Question.sound}`, Question.done]);
  try {
    const result = await converse(fixture.context, TestSuite, TestConfig, { open: () => fixture.session, classify: async ({ message }) => classification(message) });
    assert.deepEqual(fixture.sent, [TestSuite.prompt, `${Answer.color}\n${Answer.sound}`]);
    assert.equal(result.questions.length, 2);
    assert.equal(result.questions[0]?.round, 1);
    assert.equal(result.questions[1]?.round, 1);
    assert.equal(result.questions[0]?.documented, true);
    assert.equal(result.questions[0]?.answerLength, Answer.color.length);
    const report: ConversationReport = JSON.parse(await readFile(join(fixture.context.space.root, ConversationFile), "utf8"));
    assert.equal(report.questionCount, 2);
    assert.equal(report.rounds, 1);
    assert.equal(report.documented, 1);
    assert.deepEqual(report.calls, [TestUsage, TestUsage]);
    assert.equal(fixture.finished(), true);
  } finally { await fixture.dispose(); }
});

test("uncovered choices receive the fallback and are unanswered", async () => {
  const fixture = await conversationFixture([Question.other, Question.done]);
  try {
    const result = await converse(fixture.context, TestSuite, TestConfig, { open: () => fixture.session, classify: async ({ message }) => classification(message) });
    assert.equal(fixture.sent[1], DefaultReply);
    assert.equal(result.questions[0]?.answered, false);
  } finally { await fixture.dispose(); }
});

test("questions after the third bank answer get one terminal fallback", async () => {
  const fixture = await conversationFixture([...Array.from({ length: QuestionRounds + 1 }, () => Question.sound), Question.done]);
  try {
    const result = await converse(fixture.context, TestSuite, TestConfig, { open: () => fixture.session, classify: async ({ message }) => classification(message) });
    assert.equal(result.questions.filter((question) => question.answered).length, QuestionRounds);
    assert.equal(result.questions.at(-1)?.answer, DefaultReply);
    assert.equal(result.questions.at(-1)?.answered, false);
    assert.equal(fixture.sent.at(-1), DefaultReply);
    assert.equal(result.questions.filter((question) => question.repeated).length, QuestionRounds);
    assert.equal(fixture.sent.length, QuestionRounds + 2);
  } finally { await fixture.dispose(); }
});

test("simulator failure rejects as infrastructure evidence and finishes the agent", async () => {
  const fixture = await conversationFixture([Question.sound]);
  try {
    const failure = new Error("Classifier unavailable.");
    await assert.rejects(converse(fixture.context, TestSuite, TestConfig, { open: () => fixture.session, classify: async () => { throw failure; } }), SimulatorFailure);
    const report: ConversationReport = JSON.parse(await readFile(join(fixture.context.space.root, ConversationFile), "utf8"));
    assert.equal(report.error, failure.message);
    assert.equal(report.calls.length, 1);
    assert.equal(report.calls[0]?.tokens, null);
    assert.equal(report.messages?.findLast((message) => message.role === ConversationRole.agent)?.text, Question.sound);
    assert.equal(fixture.finished(), true);
  } finally { await fixture.dispose(); }
});

test("agent cleanup errors can't discard the conversation or replace the classifier failure", async () => {
  const fixture = await conversationFixture([Question.sound]);
  try {
    await assert.rejects(converse(fixture.context, TestSuite, TestConfig, {
      open: () => ({ ...fixture.session, finish: async () => { throw new Error("Fixture cleanup failure."); } }),
      classify: async () => { throw new Error("Fixture classifier failure."); },
    }), SimulatorFailure);
    const report: ConversationReport = JSON.parse(await readFile(join(fixture.context.space.root, ConversationFile), "utf8"));
    assert.equal(report.messages?.at(-1)?.text, Question.sound);
    assert.ok(report.calls[0]?.error);
    assert.equal(report.error, report.calls[0]?.error);
  } finally { await fixture.dispose(); }
});
