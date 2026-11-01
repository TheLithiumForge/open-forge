import { dirname, join } from "node:path";
import { writeFile } from "node:fs/promises";
import { openSession, TurnOutcome, type AgentSession, type SessionContext, type TurnResult } from "../providers/agent-session.ts";
import type { EvalConfig } from "../../config/config.types.ts";
import type { AgentTelemetry } from "../../history/results.types.ts";
import type { Suite } from "../../benchmarks/suite.types.ts";
import { replyAsUser } from "./simulated-user.ts";
import { answerBank, ConversationFile, freezeProtocol, QuestionRounds } from "./answer-bank.ts";
import { literalAnswers, type QuestionEntry } from "./replies.ts";
import { emptyConversation, ConversationRole, SimulatorFailure, type ConversationMessage, type ConversationReport } from "./report.ts";
import type { SimulatorUsage } from "./model-reply.ts";
import { ClassifierFailure, type Classify } from "./model-classifier.ts";
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
  let session: AgentSession | null = null;
  const questions: QuestionEntry[] = [];
  const calls: SimulatorUsage[] = [];
  const messages: ConversationMessage[] = [];
  const seen = new Set<string>();
  let rounds = 0;
  let error: string | null = null;
  let last: TurnResult = { outcome: TurnOutcome.answered, message: "", error: null };
  let telemetry: AgentTelemetry | undefined;
  const report = (): ConversationReport => ({ ...emptyConversation(config, frozen.hash),
    questions, calls, messages, rounds, questionCount: questions.length, repeats: questions.filter((question) => question.repeated).length,
    documented: questions.filter((question) => question.documented).length, unanswered: questions.filter((question) => !question.answered).length, error });
  const save = services.save ?? (async (value) => { await writeFile(join(dirname(context.eventsPath), ConversationFile), `${JSON.stringify(value, null, 2)}\n`); });
  const turn = async (message: string): Promise<TurnResult> => {
    if (session === null) throw new Error("The agent session hasn't started.");
    messages.push({ role: ConversationRole.user, text: message });
    await save(report());
    const result = await session.turn(message, Math.max(1, deadline - Date.now()));
    messages.push({ role: ConversationRole.agent, text: result.message });
    await save(report());
    return result;
  };
  try {
    session = (services.open ?? openSession)(context);
    let message = suite.prompt;
    while (Date.now() < deadline) {
      last = await turn(message);
      if (last.outcome !== TurnOutcome.answered) break;
      let classified;
      const callStarted = Date.now();
      try {
        classified = await replyAsUser({ choice: config.simulatedUser, request: suite.prompt, intent: suite.intent, bank: frozen.bank, documents: frozen.documents,
          agentMessage: last.message, home: join(context.space.root, "user"), timeoutMs: Math.min(UserTimeoutMs, Math.max(1, deadline - Date.now())),
          ...(services.options === undefined ? {} : { options: services.options }) }, services.classify);
      } catch (failure) {
        error = failure instanceof Error ? failure.message : String(failure);
        calls.push(...(failure instanceof ClassifierFailure ? failure.calls : [{ tokens: null, model: null, tools: config.simulatedUser.tool === Tool.codex ? AssessorTools.restricted : AssessorTools.off, seconds: (Date.now() - callStarted) / 1000, error }]));
        throw new SimulatorFailure(report());
      }
      calls.push(...(classified.calls ?? [classified.usage]));
      if (classified.questions.length === 0) break;
      rounds += 1;
      const entries = literalAnswers(classified.questions, frozen.bank, rounds, rounds <= QuestionRounds, seen);
      questions.push(...entries);
      message = entries.map((entry) => entry.answer).join("\n");
      if (rounds > QuestionRounds) {
        // Deliver the terminal fallback once, then freeze the attempt at its question limit.
        last = await turn(message);
        break;
      }
    }
  } catch (failure) {
    error = failure instanceof Error ? failure.message : String(failure);
    throw failure;
  } finally {
    try {
      if (session !== null) telemetry = await session.finish();
    } catch (failure) {
      if (error === null) { error = failure instanceof Error ? failure.message : String(failure); throw failure; }
    } finally { await save(report()); }
  }
  if (telemetry === undefined) throw new Error("The agent returned no telemetry.");
  return { outcome: last.outcome, finalMessage: last.message, questions, telemetry, error: last.error, seconds: (Date.now() - started) / 1000 };
}
