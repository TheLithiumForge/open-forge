import { createHash } from "node:crypto";
import type { IntentItem } from "../../benchmarks/suite.types.ts";

export const ProtocolVersion = 1;
export const QuestionRounds = 3;
export const DefaultReply = "Your call, within the supplied requirements.";
export const ConversationFile = "conversation.json";
export interface BankEntry { readonly topicId: string; readonly answer: string }
export interface FrozenProtocol {
  readonly version: typeof ProtocolVersion;
  readonly bank: readonly BankEntry[];
  readonly documents: readonly string[];
  readonly hash: string;
}

export function answerBank(intent: readonly IntentItem[]): readonly BankEntry[] {
  const bank = intent.map((item) => Object.freeze({ topicId: item.id, answer: item.text }));
  if (new Set(bank.map((entry) => entry.topicId)).size !== bank.length || bank.some((entry) => !entry.topicId || !entry.answer)) throw new Error("Answer bank topics must be unique and have literal answers.");
  return Object.freeze(bank);
}

export function freezeProtocol(bank: readonly BankEntry[], documents: readonly string[]): FrozenProtocol {
  const content: Omit<FrozenProtocol, "hash"> = { version: ProtocolVersion, bank: Object.freeze(bank.map((entry) => Object.freeze({ ...entry }))), documents: Object.freeze([...documents]) };
  return Object.freeze({ ...content, hash: createHash("sha256").update(JSON.stringify(content)).digest("hex") });
}
