import { Tool } from "../../config/choices.ts";
import type { JudgeChoice } from "../../config/config.types.ts";
import type { ProcessResult } from "../../execution/process.ts";
import { record } from "../../benchmarks/neon-pairs/checks/layer.ts";
import { replyJson } from "../../execution/reply-json.ts";
import { providerRefused } from "../../execution/providers/agent-session.ts";
import { claudeLimitEvent } from "../../execution/providers/claude-telemetry.ts";

export class JudgeAssessorFailure extends Error {}
/** A usage limit or a rejected sign-in isn't a judge failure. The batch pauses, and resume asks the judge again. */
export class JudgeLimited extends JudgeAssessorFailure {}

function limited(result: ProcessResult): boolean {
  if (providerRefused(result.stderr)) return true;
  return result.stdout.split("\n").some((line) => {
    try {
      const event: unknown = JSON.parse(line);
      return record(event) && (claudeLimitEvent(event) || (["error", "turn.failed"].includes(String(event["type"])) && providerRefused(JSON.stringify(event)))
        || (event["is_error"] === true && typeof event["result"] === "string" && providerRefused(event["result"])));
    } catch {
      return false; // Not an event line.
    }
  });
}

// Only a failed or erroring assessor is read for a limit, so a judge quoting rate limit code is never mistaken for one.
function failure(result: ProcessResult, message: string): JudgeAssessorFailure {
  return limited(result) ? new JudgeLimited("The judge hit a usage limit.") : new JudgeAssessorFailure(message);
}

export function assessorReply(result: ProcessResult, choice: JudgeChoice): string {
  if (result.code !== 0 || result.timedOut || result.truncated) throw failure(result, "Judge assessor failed or timed out.");
  if (choice.tool === Tool.codex) {
    const events: unknown[] = result.stdout.split("\n").filter((line) => line.trim()).map((line) => JSON.parse(line));
    if (events.some((event) => record(event) && ["error", "turn.failed"].includes(String(event["type"])))) throw failure(result, "Judge assessor returned an error.");
    const messages = events.flatMap((event) => record(event) && record(event["item"]) && event["item"]["type"] === "agent_message" && typeof event["item"]["text"] === "string" ? [event["item"]["text"]] : []);
    const message = messages.at(-1);
    if (message === undefined) throw new Error("Judge assessor returned no answer JSON.");
    return message;
  }
  const envelope: unknown = JSON.parse(result.stdout);
  if (!record(envelope) || envelope["is_error"] === true) throw failure(result, "Judge assessor returned an error.");
  if ("structured_output" in envelope) return JSON.stringify(envelope["structured_output"]);
  if (typeof envelope["result"] !== "string") throw new Error("Judge assessor returned no answer JSON.");
  return envelope["result"];
}

export function answerObject(reply: string): unknown {
  const value = replyJson(reply);
  return Array.isArray(value) ? { answers: value } : value;
}
