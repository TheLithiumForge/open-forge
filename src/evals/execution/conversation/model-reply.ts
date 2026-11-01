import { Tool } from "../../config/choices.ts";
import type { TokenCounts } from "../../history/results.types.ts";
import type { AssessorTools } from "../containers/assessor.ts";
import { replyJson } from "../reply-json.ts";

export interface SimulatorUsage {
  readonly tokens: TokenCounts | null;
  readonly model: string | null;
  readonly tools: (typeof AssessorTools)[keyof typeof AssessorTools];
  readonly seconds: number;
  readonly error?: string;
  readonly reply?: string;
}
export class SimulatorModelFailure extends Error {}
function record(value: unknown): Record<string, unknown> {
  return typeof value === "object" && value !== null && !Array.isArray(value) ? Object.fromEntries(Object.entries(value)) : {};
}
function count(value: unknown): number {
  if (value === undefined) return 0;
  if (typeof value !== "number" || !Number.isSafeInteger(value) || value < 0) throw new Error("The assessor reported invalid token usage.");
  return value;
}
function tokens(value: unknown): TokenCounts | null {
  const usage = record(value);
  if (typeof usage["input_tokens"] !== "number" || typeof usage["output_tokens"] !== "number") return null;
  const cachedInput = count(usage["cached_input_tokens"] ?? usage["cache_read_input_tokens"]);
  return { input: Math.max(0, count(usage["input_tokens"]) - ("cached_input_tokens" in usage ? cachedInput : 0)), cachedInput,
    cacheWrite: count(usage["cache_creation_input_tokens"]), output: count(usage["output_tokens"]), reasoning: count(usage["reasoning_output_tokens"]) };
}

export function readAssessorMessage(stdout: string, tool: Tool): { reply: string; usage: Pick<SimulatorUsage, "tokens" | "model"> } {
  if (tool === Tool.claude) {
    const result = record(JSON.parse(stdout));
    if (result["is_error"] === true) throw new SimulatorModelFailure("The simulated user model returned an error.");
    const reply = result["structured_output"] === undefined ? typeof result["result"] === "string" ? result["result"] : "null" : JSON.stringify(result["structured_output"]);
    return { reply, usage: { tokens: tokens(result["usage"]), model: typeof result["model"] === "string" ? result["model"] : null } };
  }
  const events = stdout.split("\n").filter((line) => line.trim()).map((line) => record(JSON.parse(line)));
  if (events.some((event) => ["turn.failed", "error"].includes(String(event["type"])))) throw new SimulatorModelFailure("The simulated user model returned an error.");
  const text = events.map((event) => record(event["item"])).filter((item) => item["type"] === "agent_message").at(-1)?.["text"];
  return { reply: typeof text === "string" ? text : "null", usage: { tokens: tokens(events.findLast((event) => event["type"] === "turn.completed")?.["usage"]), model: null } };
}

export function parseAssessorReply(stdout: string, tool: Tool): { value: unknown; usage: Pick<SimulatorUsage, "tokens" | "model"> } {
  const parsed = readAssessorMessage(stdout, tool);
  return { value: replyJson(parsed.reply), usage: parsed.usage };
}
