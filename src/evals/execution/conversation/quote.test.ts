import assert from "node:assert/strict";
import { test } from "node:test";
import { answerBank } from "./answer-bank.ts";
import { readClassification } from "./classifier.ts";
import { originalQuote } from "./quote.ts";
import { TestSuite, Topic } from "./test-fixtures.ts";

test("classifier quote matching normalizes typography and whitespace while keeping the agent's exact text", () => {
  const original = "Should the “Neon”\n\tmode’s cards — and hints – use sound?";
  const folded = 'Should the "Neon" mode\'s cards - and hints - use sound?';
  const message = `I have a question.\n${original}\nPlease choose.`;
  const questions = readClassification({ questions: [{ text: folded, topicIds: [Topic.sound], documented: false }] }, message, answerBank(TestSuite.intent));
  assert.equal(questions[0]?.text, original);
  assert.equal(originalQuote(message, "Should it play music?"), null);
  assert.equal(originalQuote(message, " \n\t "), null);
});
