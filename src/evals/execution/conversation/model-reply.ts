import { Tool } from "../../config/choices.ts";
import type { TokenCounts } from "../../history/results.types.ts";
import type { AssessorTools } from "../containers/assessor.ts";

export interface SimulatorUsage {
  readonly tokens: TokenCounts | null;
  readonly model: string | null;
  readonly tools: (typeof AssessorTools)[keyof typeof AssessorTools];
  readonly seconds: number;
  readonly error?: string;
}
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

export function parseAssessorReply(stdout: string, tool: Tool): { value: unknown; usage: Pick<SimulatorUsage, "tokens" | "model"> } {
  if (tool === Tool.claude) {
    const result = record(JSON.parse(stdout));
    if (result["is_error"] === true) throw new Error("The simulated user model returned an error.");
    const value: unknown = result["structured_output"] ?? JSON.parse(typeof result["result"] === "string" ? result["result"] : "null");
    return { value, usage: { tokens: tokens(result["usage"]), model: typeof result["model"] === "string" ? result["model"] : null } };
  }
  const events = stdout.split("\n").filter((line) => line.trim()).map((line) => record(JSON.parse(line)));
  if (events.some((event) => ["turn.failed", "error"].includes(String(event["type"])))) throw new Error("The simulated user model returned an error.");
  const text = events.map((event) => record(event["item"])).filter((item) => item["type"] === "agent_message").at(-1)?.["text"];
  return { value: JSON.parse(typeof text === "string" ? text : "null"), usage: { tokens: tokens(events.findLast((event) => event["type"] === "turn.completed")?.["usage"]), model: null } };
}
