import { mkdtemp, rm } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import type { Suite } from "../../benchmarks/suite.types.ts";
import { Isolation, Tool } from "../../config/choices.ts";
import type { EvalConfig } from "../../config/config.types.ts";
import { JudgedBy } from "../../benchmarks/suite-values.ts";
import { TurnOutcome, type AgentSession, type SessionContext } from "../providers/agent-session.ts";
import { AssessorTools } from "../containers/assessor.ts";
import { answerBank, freezeProtocol, QuestionRounds } from "./answer-bank.ts";
import type { Classification } from "./model-classifier.ts";

export const Topic = { color: "color", sound: "sound" } as const;
export const Question = { color: "Which colors?", sound: "Should it have sound?", other: "Should there be a menu?", done: "The work is complete." } as const;
export const Answer = { color: "Cyan and magenta.", sound: "No sound." } as const;
export const MockChoice = { tool: Tool.mock, model: null, effort: null } as const;
export const TestSuite: Suite = {
  id: "conversation-case", title: "Conversation case", project: "project", baseline: "no-setup", setups: { "no-setup": { label: "No setup", commands: [] } },
  prompt: "Build a game.", intent: [{ id: Topic.color, text: Answer.color, judgedBy: JudgedBy.judge }, { id: Topic.sound, text: Answer.sound, judgedBy: JudgedBy.checks }],
  rubric: [], fingerprint: "fixture", intentEdited: false,
};
export const TestConfig: EvalConfig = { suite: TestSuite.id, runsPerSetup: 1, parallelRuns: 1, setups: [TestSuite.baseline], agents: [MockChoice],
  judge: { ...MockChoice, repeats: 1 }, simulatedUser: MockChoice, isolation: Isolation.local, limits: { runMinutes: 1, checkMinutes: 1, questionRounds: QuestionRounds },
  protocol: freezeProtocol(answerBank(TestSuite.intent), [TestSuite.prompt]) };
export const TestTelemetry = { tokens: null, requests: null, peakContext: null, contextWindow: null, compactions: null, toolCalls: 0, failedToolCalls: 0, openForgeReads: 0, openForgeBytes: null, reportedCostUsd: null, model: null, sessionId: null } as const;
export const TestUsage = { tokens: { input: 4, output: 2, cachedInput: 0, cacheWrite: 0, reasoning: 0 }, model: "classifier-fixture", tools: AssessorTools.off, seconds: 1 } as const;

export function classification(message: string): Classification {
  const questions = [];
  if (message.includes(Question.color)) questions.push({ text: Question.color, topicIds: [Topic.color], documented: true });
  if (message.includes(Question.sound)) questions.push({ text: Question.sound, topicIds: [Topic.sound], documented: false });
  if (message.includes(Question.other)) questions.push({ text: Question.other, topicIds: [], documented: false });
  return { questions, usage: TestUsage };
}

export async function conversationFixture(messages: readonly string[]) {
  const root = await mkdtemp(join(tmpdir(), "eval-conversation-"));
  const sent: string[] = [];
  let finished = false;
  const session: AgentSession = {
    async turn(message) {
      const response = messages[sent.length];
      sent.push(message);
      if (response === undefined) throw new Error("The scripted conversation ran too long.");
      return { outcome: TurnOutcome.answered, message: response, error: null };
    },
    async finish() { finished = true; return TestTelemetry; },
  };
  const context: SessionContext = { isolation: Isolation.local, choice: MockChoice,
    space: { root, work: join(root, "project"), home: join(root, "home"), project: TestSuite.project }, eventsPath: join(root, "events.jsonl") };
  return { context, session, sent, finished: () => finished, dispose: () => rm(root, { recursive: true, force: true }) };
}
