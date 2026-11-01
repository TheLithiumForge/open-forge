import { dirname, join } from "node:path";
import { writeFile } from "node:fs/promises";
import { openSession, TurnOutcome, type SessionContext, type TurnResult } from "../providers/agent-session.ts";
import type { EvalConfig } from "../../config/config.types.ts";
import type { AgentTelemetry } from "../../history/results.types.ts";
import type { Suite } from "../../benchmarks/suite.types.ts";
import { replyAsUser } from "./simulated-user.ts";
import { answerBank, ConversationFile, freezeProtocol, ProtocolVersion, QuestionRounds } from "./answer-bank.ts";
import { ClassifierVersion } from "./classifier.ts";
import { literalAnswers, type QuestionEntry } from "./replies.ts";
import { SimulatorFailure, type ConversationReport } from "./report.ts";
import type { SimulatorUsage } from "./model-reply.ts";
import type { Classify } from "./model-classifier.ts";
import type { StageOptions } from "../containers/stage.ts";
import { AssessorTools } from "../containers/assessor.ts";
import { Tool } from "../../config/choices.ts";
import { readProtocol } from "../../config/protocol.ts";

export interface Conversation {
  readonly outcome: TurnOutcome;
  readonly finalMessage: string;
  readonly questions: readonly QuestionEntry[];
  readonly telemetry: AgentTelemetry;
  readonly error: string | null;
  readonly seconds: number;
}
export interface ConversationServices {
  readonly open?: typeof openSession;
  readonly classify?: Classify;
  readonly save?: (report: ConversationReport) => Promise<void>;
  readonly options?: Omit<StageOptions, "timeoutMs">;
}
const UserTimeoutMs = 3 * 60_000;

export async function converse(context: SessionContext, suite: Suite, config: EvalConfig, services: ConversationServices = {}): Promise<Conversation> {
  const started = Date.now();
  const deadline = started + config.limits.runMinutes * 60_000;
  const frozen = config.protocol === undefined ? freezeProtocol(answerBank(suite.intent), [suite.prompt]) : readProtocol(config.protocol);
  const session = (services.open ?? openSession)(context);
  const questions: QuestionEntry[] = [];
  const calls: SimulatorUsage[] = [];
  const seen = new Set<string>();
  let rounds = 0;
  let error: string | null = null;
  let last: TurnResult = { outcome: TurnOutcome.answered, message: "", error: null };
  let telemetry: AgentTelemetry;
  const report = (): ConversationReport => ({ version: ProtocolVersion, classifier: ClassifierVersion, bankHash: frozen.hash, requestedModel: config.simulatedUser,
    questions, calls, rounds, questionCount: questions.length, repeats: questions.filter((question) => question.repeated).length,
    documented: questions.filter((question) => question.documented).length, unanswered: questions.filter((question) => !question.answered).length, error });
  try {
    let message = suite.prompt;
    while (Date.now() < deadline) {
      last = await session.turn(message, Math.max(1, deadline - Date.now()));
      if (last.outcome !== TurnOutcome.answered) break;
      let classified;
      const callStarted = Date.now();
      try {
        classified = await replyAsUser({ choice: config.simulatedUser, request: suite.prompt, intent: suite.intent, bank: frozen.bank, documents: frozen.documents,
          agentMessage: last.message, home: join(context.space.root, "user"), timeoutMs: Math.min(UserTimeoutMs, Math.max(1, deadline - Date.now())),
          ...(services.options === undefined ? {} : { options: services.options }) }, services.classify);
      } catch (failure) {
        error = failure instanceof Error ? failure.message : String(failure);
        calls.push({ tokens: null, model: null, tools: config.simulatedUser.tool === Tool.codex ? AssessorTools.restricted : AssessorTools.off, seconds: (Date.now() - callStarted) / 1000, error });
        throw new SimulatorFailure(report());
      }
      calls.push(classified.usage);
      if (classified.questions.length === 0) break;
      rounds += 1;
      const entries = literalAnswers(classified.questions, frozen.bank, rounds, rounds <= QuestionRounds, seen);
      questions.push(...entries);
      message = entries.map((entry) => entry.answer).join("\n");
      if (rounds > QuestionRounds) {
        // Deliver the terminal fallback once, then freeze the attempt at its question limit.
        last = await session.turn(message, Math.max(1, deadline - Date.now()));
        break;
      }
    }
  } finally {
    telemetry = await session.finish();
    const save = services.save ?? (async (value) => { await writeFile(join(dirname(context.eventsPath), ConversationFile), `${JSON.stringify(value, null, 2)}\n`); });
    await save(report());
  }
  return { outcome: last.outcome, finalMessage: last.message, questions, telemetry, error: last.error, seconds: (Date.now() - started) / 1000 };
}
