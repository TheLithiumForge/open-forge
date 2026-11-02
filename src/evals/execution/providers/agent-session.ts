import { Tool, type Isolation } from "../../config/choices.ts";
import type { ModelChoice } from "../../config/config.types.ts";
import type { AgentTelemetry } from "../../history/results.types.ts";
import type { RunSpace } from "../containers/executor.ts";
import { ClaudeSession } from "./claude-session.ts";
import { CodexSession } from "./codex-session.ts";
import { MockSession } from "./mock-session.ts";

export const TurnOutcome = {
  answered: "answered",
  failed: "failed",
  // A usage or rate limit stopped the tool. The batch pauses instead of counting a failure.
  limited: "limited",
  timedOut: "timedOut",
} as const;
export type TurnOutcome = (typeof TurnOutcome)[keyof typeof TurnOutcome];

export interface TurnResult {
  readonly outcome: TurnOutcome;
  readonly message: string;
  readonly error: string | null;
}

/** One conversation with an agent. Each turn sends a user message and waits for the agent's reply. */
export interface AgentSession {
  turn(message: string, timeoutMs: number): Promise<TurnResult>;
  finish(): Promise<AgentTelemetry>;
}

export interface SessionContext {
  readonly isolation: Isolation;
  readonly space: RunSpace;
  readonly choice: ModelChoice;
  /** Every raw event the tool printed, kept for later checks of the numbers. */
  readonly eventsPath: string;
}

// Claude Code says "You've hit your 5-hour limit", which names neither usage nor rate.
const limitPattern = /usage limit|rate limit|limit reached|too many requests|\b429\b|quota|hit your [\w -]*limit/i;

export function limitReached(text: string): boolean {
  return limitPattern.test(text);
}

export function outcomeForError(text: string): TurnOutcome {
  return limitReached(text) ? TurnOutcome.limited : TurnOutcome.failed;
}

export function openSession(context: SessionContext): AgentSession {
  switch (context.choice.tool) {
    case Tool.claude:
      return new ClaudeSession(context);
    case Tool.codex:
      return new CodexSession(context);
    case Tool.mock:
      return new MockSession(context);
  }
}
