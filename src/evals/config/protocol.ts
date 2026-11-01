import { answerBank, freezeProtocol, ProtocolVersion, type FrozenProtocol } from "../execution/conversation/answer-bank.ts";
import type { Suite } from "../benchmarks/suite.types.ts";
import { object, text } from "./validation.ts";

export function readProtocol(value: unknown): FrozenProtocol {
  const source = object(value);
  if (source["version"] !== ProtocolVersion || !Array.isArray(source["bank"]) || !Array.isArray(source["documents"])) throw new Error("Invalid frozen protocol.");
  const bank = source["bank"].map((entry: unknown) => {
    const item = object(entry);
    return { topicId: text(item["topicId"]), answer: text(item["answer"]) };
  });
  if (new Set(bank.map((entry) => entry.topicId)).size !== bank.length) throw new Error("Duplicate answer bank topics.");
  const documents = source["documents"].map(text);
  const frozen = freezeProtocol(bank, documents);
  if (frozen.hash !== source["hash"]) throw new Error("The frozen answer bank changed.");
  return frozen;
}

export function suiteProtocol(suite: Suite): FrozenProtocol { return freezeProtocol(answerBank(suite.intent), [suite.prompt]); }
