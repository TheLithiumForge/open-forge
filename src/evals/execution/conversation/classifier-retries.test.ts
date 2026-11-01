import assert from "node:assert/strict";
import { test } from "node:test";
import { MatrixJudges } from "../../config/matrix.ts";
import { Tool } from "../../config/choices.ts";
import { AssessorTools } from "../containers/assessor.ts";
import { ReplyEvidenceBytes } from "../reply-json.ts";
import { answerBank } from "./answer-bank.ts";
import { classifyQuestions, ClassifierFailure, ClassifierFormatRetries, type ClassifierInput } from "./model-classifier.ts";
import { Question, TestSuite, Topic } from "./test-fixtures.ts";

const valid = JSON.stringify({ questions: [{ text: Question.sound, topicIds: [Topic.sound], documented: false }] });
const InvalidClassification = JSON.stringify({ questions: [{ text: "Invented question?", topicIds: [], documented: false }] });

function result(tool: Tool, reply: string) {
  const usage = { input_tokens: 10, output_tokens: 2 };
  return { code: 0, timedOut: false, stderr: "", tools: AssessorTools.off, stdout: tool === Tool.codex
    ? [JSON.stringify({ type: "item.completed", item: { type: "agent_message", text: reply } }), JSON.stringify({ type: "turn.completed", usage })].join("\n")
    : JSON.stringify({ result: reply, usage }) };
}

test("classifiers accept fenced and prose replies and retry invalid classifications with unchanged prompts in fresh homes", async () => {
  for (const choice of MatrixJudges) for (const wrapped of [`\`\`\`json\n${valid}\n\`\`\``, `Classification:\n${valid}\nComplete.`]) {
    const prompts: string[] = [], homes: string[] = [];
    const input: ClassifierInput = { choice, message: Question.sound, bank: answerBank(TestSuite.intent), documents: [TestSuite.prompt], options: { timeoutMs: 1000 } };
    const classified = await classifyQuestions(input, async (space, _choice, prompt) => {
      prompts.push(prompt); homes.push(space.home);
      return result(choice.tool, prompts.length === 1 ? InvalidClassification : wrapped);
    });
    assert.equal(prompts.length, 2);
    assert.equal(new Set(homes).size, homes.length);
    assert.ok(prompts.every((prompt) => prompt === prompts[0]));
    assert.equal(classified.questions[0]?.text, Question.sound);
    assert.equal(classified.calls?.length, prompts.length);
    assert.ok(classified.calls?.every((call) => call.tokens !== null));
    assert.equal(classified.calls?.[0]?.error !== undefined, true);
    assert.equal(classified.usage.error, undefined);
  }
});

test("an exhausted classifier keeps its final reply capped as UTF-8 data and every failed call", async () => {
  const replies: string[] = [];
  const input: ClassifierInput = { choice: MatrixJudges[0], message: Question.sound, bank: answerBank(TestSuite.intent), documents: [], options: { timeoutMs: 1000 } };
  await assert.rejects(classifyQuestions(input, async () => {
    const reply = `${InvalidClassification}\n<script>${replies.length}${"💡".repeat(ReplyEvidenceBytes)}</script>`;
    replies.push(reply); return result(input.choice.tool, reply);
  }), (failure: unknown) => {
    assert.ok(failure instanceof ClassifierFailure);
    assert.equal(replies.length, ClassifierFormatRetries + 1);
    assert.equal(failure.calls.length, replies.length);
    const last = failure.calls.at(-1);
    assert.ok(last?.error);
    assert.ok(last.reply);
    assert.ok(Buffer.byteLength(last.reply) <= ReplyEvidenceBytes);
    assert.ok(replies.at(-1)?.startsWith(last.reply));
    assert.ok(!last.reply.endsWith("�"));
    return true;
  });
});
