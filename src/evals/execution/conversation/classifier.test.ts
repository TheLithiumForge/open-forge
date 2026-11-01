import assert from "node:assert/strict";
import { test } from "node:test";
import { answerBank, DefaultReply } from "./answer-bank.ts";
import { ClassifierPrompt, classifierPrompt, readClassification } from "./classifier.ts";
import { replyAsUser } from "./simulated-user.ts";
import { literalAnswers } from "./replies.ts";
import { Answer, classification, MockChoice, Question, TestSuite, Topic } from "./test-fixtures.ts";
import { parseAssessorReply } from "./model-reply.ts";
import { Tool } from "../../config/choices.ts";

test("the classifier fake maps decisions to topic IDs before the runner chooses literal answers", async () => {
  const bank = answerBank(TestSuite.intent);
  const result = await replyAsUser({ choice: MockChoice, request: TestSuite.prompt, intent: TestSuite.intent, agentMessage: Question.sound, home: "unused", timeoutMs: 1 }, async (input) => {
    assert.deepEqual(input.bank, bank);
    return { ...classification(input.message), questions: readClassification(classification(input.message), input.message, bank) };
  });
  const replies = literalAnswers(result.questions, bank, 1, true, new Set());
  assert.equal(replies[0]?.answer, Answer.sound);
  assert.equal(replies[0]?.answered, true);
  assert.ok(classifierPrompt(Question.sound, bank, [TestSuite.prompt]).startsWith(ClassifierPrompt));
});

test("unknown classifier topics and invented questions fail validation", () => {
  const bank = answerBank(TestSuite.intent);
  assert.throws(() => readClassification({ questions: [{ text: Question.sound, topicIds: ["unknown"], documented: false }] }, Question.sound, bank));
  assert.throws(() => readClassification({ questions: [{ text: Question.other, topicIds: [Topic.sound], documented: false }] }, Question.sound, bank));
  assert.equal(literalAnswers([{ text: Question.other, topicIds: [], documented: false }], bank, 1, true, new Set())[0]?.answer, DefaultReply);
});

test("both assessor output formats yield a checked classifier result and separate usage", () => {
  const value = { questions: [] };
  const usage = { input_tokens: 10, cached_input_tokens: 3, output_tokens: 2 };
  const claude = parseAssessorReply(JSON.stringify({ result: JSON.stringify(value), usage }), Tool.claude);
  const codex = parseAssessorReply([JSON.stringify({ type: "item.completed", item: { type: "agent_message", text: JSON.stringify(value) } }), JSON.stringify({ type: "turn.completed", usage })].join("\n"), Tool.codex);
  assert.deepEqual(claude.value, value);
  assert.deepEqual(codex.value, value);
  assert.equal(codex.usage.tokens?.input, 7);
  assert.throws(() => parseAssessorReply(JSON.stringify({ is_error: true }), Tool.claude));
});
