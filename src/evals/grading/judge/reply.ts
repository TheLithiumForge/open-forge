import { Tool } from "../../config/choices.ts";
import type { JudgeChoice } from "../../config/config.types.ts";
import type { ProcessResult } from "../../execution/process.ts";
import { record } from "../../benchmarks/neon-pairs/checks/layer.ts";
import { replyJson } from "../../execution/reply-json.ts";

export class JudgeAssessorFailure extends Error {}

export function assessorReply(result: ProcessResult, choice: JudgeChoice): string {
  if (result.code !== 0 || result.timedOut || result.truncated) throw new JudgeAssessorFailure("Judge assessor failed or timed out.");
  if (choice.tool === Tool.codex) {
    const events: unknown[] = result.stdout.split("\n").filter((line) => line.trim()).map((line) => JSON.parse(line));
    if (events.some((event) => record(event) && ["error", "turn.failed"].includes(String(event["type"])))) throw new JudgeAssessorFailure("Judge assessor returned an error.");
    const messages = events.flatMap((event) => record(event) && record(event["item"]) && event["item"]["type"] === "agent_message" && typeof event["item"]["text"] === "string" ? [event["item"]["text"]] : []);
    const message = messages.at(-1);
    if (message === undefined) throw new Error("Judge assessor returned no answer JSON.");
    return message;
  }
  const envelope: unknown = JSON.parse(result.stdout);
  if (!record(envelope) || envelope["is_error"] === true) throw new JudgeAssessorFailure("Judge assessor returned an error.");
  if ("structured_output" in envelope) return JSON.stringify(envelope["structured_output"]);
  if (typeof envelope["result"] !== "string") throw new Error("Judge assessor returned no answer JSON.");
  return envelope["result"];
}

export function answerObject(reply: string): unknown {
  const value = replyJson(reply);
  return Array.isArray(value) ? { answers: value } : value;
}
