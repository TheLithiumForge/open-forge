import { Tool } from "../../config/choices.ts";
import type { JudgeChoice } from "../../config/config.types.ts";
import type { ProcessResult } from "../../execution/process.ts";
import { record } from "../../benchmarks/neon-pairs/checks/layer.ts";

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
  const candidates: string[] = [], stack: string[] = [];
  let start = 0, quoted = false, escaped = false;
  // Find outer objects only. JSON.parse still owns the JSON grammar and ballot validation is separate.
  for (let index = 0; index < reply.length; index += 1) {
    const char = reply[index];
    if (!stack.length) {
      if (char === "{" || char === "[") { start = index; stack.push(char); }
      continue;
    }
    if (quoted) {
      if (escaped) escaped = false;
      else if (char === "\\") escaped = true;
      else if (char === '"') quoted = false;
    } else if (char === '"') quoted = true;
    else if (char === "{" || char === "[") stack.push(char);
    else if (char === "}" || char === "]") {
      const opening = stack.pop();
      if (opening !== (char === "}" ? "{" : "[")) throw new Error("Judge reply has mismatched JSON delimiters.");
      if (!stack.length && reply[start] === "{") candidates.push(reply.slice(start, index + 1));
    }
  }
  const candidate = candidates[0];
  if (stack.length || candidates.length !== 1 || candidate === undefined) throw new Error("Judge reply needs exactly one top-level JSON object.");
  return JSON.parse(candidate);
}
