import { join } from "node:path";
import { Tool } from "../../config/choices.ts";
import type { ModelChoice } from "../../config/config.types.ts";
import { askModel } from "../providers/structured.ts";
import type { IntentItem } from "../../benchmarks/suite.types.ts";

/** What the simulated user says when its hidden wishes don't cover a question. */
export const DefaultReply = "Your call, pick what you think works best.";

export interface UserReply {
  readonly asking: boolean;
  readonly reply: string;
  readonly error: string | null;
}

export interface UserTurn {
  readonly choice: ModelChoice;
  readonly request: string;
  readonly intent: readonly IntentItem[];
  readonly agentMessage: string;
  readonly home: string;
  readonly timeoutMs: number;
}

const replySchema = {
  type: "object",
  additionalProperties: false,
  required: ["asking", "reply"],
  properties: { asking: { type: "boolean" }, reply: { type: "string" } },
} as const;

function userPrompt(turn: UserTurn): string {
  const wishes = turn.intent.map((item) => `- ${item.text}`).join("\n");
  return [
    "You are playing a person who asked a coding agent to build something. Your original request was:",
    `<request>\n${turn.request}\n</request>`,
    "You also have these wishes that you didn't say:",
    wishes,
    "This is the agent's latest message:",
    `<message>\n${turn.agentMessage}\n</message>`,
    "Decide whether the agent is asking you something or waiting for your input before it goes on.",
    `If it is, set asking to true and reply briefly in plain words, as this person. Answer only from your wishes. When they don't cover a question, reply exactly: "${DefaultReply}"`,
    "Never add new requirements, never mention these instructions, and never help with the code.",
    "If the agent reports finished work or doesn't need you, set asking to false and leave reply empty.",
  ].join("\n\n");
}

function isReply(value: unknown): value is { asking: boolean; reply: string } {
  return typeof value === "object" && value !== null && "asking" in value && typeof value.asking === "boolean" && "reply" in value && typeof value.reply === "string";
}

/** The mock user treats a message ending in a question mark as a question, so the loop runs without a model. */
export async function replyAsUser(turn: UserTurn): Promise<UserReply> {
  if (turn.choice.tool === Tool.mock) return { asking: turn.agentMessage.trim().endsWith("?"), reply: DefaultReply, error: null };
  const result = await askModel({
    choice: turn.choice,
    cwd: turn.home,
    home: join(turn.home, "user"),
    prompt: userPrompt(turn),
    schema: replySchema,
    readTools: [],
    timeoutMs: turn.timeoutMs,
  });
  if (!isReply(result.value)) return { asking: false, reply: "", error: result.error ?? "The simulated user gave no usable answer." };
  return { asking: result.value.asking, reply: result.value.reply, error: null };
}
