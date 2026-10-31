import { join } from "node:path";
import { openSession, TurnOutcome, type SessionContext, type TurnResult } from "../agents/agent-session.ts";
import type { EvalConfig } from "../config/config.types.ts";
import type { AgentTelemetry, QuestionRound } from "../results/results.types.ts";
import type { Suite } from "../suite/suite.types.ts";
import { replyAsUser } from "../user/simulated-user.ts";

export interface Conversation {
  readonly outcome: TurnOutcome;
  readonly finalMessage: string;
  readonly questions: readonly QuestionRound[];
  readonly telemetry: AgentTelemetry;
  readonly error: string | null;
  readonly seconds: number;
}

const UserTimeoutMs = 3 * 60_000;

/**
 * Sends the prompt, then lets the simulated user answer the agent's questions until the agent
 * stops asking, the question rounds run out, or the run's time is up.
 */
export async function converse(context: SessionContext, suite: Suite, config: EvalConfig): Promise<Conversation> {
  const started = Date.now();
  const deadline = started + config.limits.runMinutes * 60_000;
  const session = openSession(context);
  const questions: QuestionRound[] = [];
  let message = suite.prompt;
  let last: TurnResult = { outcome: TurnOutcome.answered, message: "", error: null };
  for (let round = 0; round <= config.limits.questionRounds; round += 1) {
    last = await session.turn(message, Math.max(1, deadline - Date.now()));
    if (last.outcome !== TurnOutcome.answered || round === config.limits.questionRounds) break;
    const reply = await replyAsUser({
      choice: config.simulatedUser,
      request: suite.prompt,
      intent: suite.intent,
      agentMessage: last.message,
      home: join(context.space.root, "user"),
      timeoutMs: UserTimeoutMs,
    });
    if (!reply.asking) break;
    questions.push({ question: last.message, answer: reply.reply });
    message = reply.reply;
  }
  const telemetry = await session.finish();
  return { outcome: last.outcome, finalMessage: last.message, questions, telemetry, error: last.error, seconds: (Date.now() - started) / 1000 };
}
