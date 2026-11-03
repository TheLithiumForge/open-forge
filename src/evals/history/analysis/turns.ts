import { Tool } from "../../config/choices.ts";
import type { TokenTotals, TurnUsage } from "./analysis.types.ts";
import { count, logLines, record } from "./log-values.ts";

/** Codex counts cached input inside its input, so new input is the difference. */
function codexTurn(usage: Readonly<Record<string, unknown>>): TokenTotals {
  const input = count(usage["input_tokens"]), cached = count(usage["cached_input_tokens"]);
  return { newInput: input - cached, cachedInput: cached, cacheWrite: 0, output: count(usage["output_tokens"]), reasoning: count(usage["reasoning_output_tokens"]) };
}

/** Claude reports its real output, thinking included, in each turn's result. */
function claudeTurn(usage: Readonly<Record<string, unknown>>): TokenTotals {
  const thinking = record(usage["output_tokens_details"])?.["thinking_tokens"];
  return { newInput: count(usage["input_tokens"]), cachedInput: count(usage["cache_read_input_tokens"]), cacheWrite: count(usage["cache_creation_input_tokens"]),
    output: count(usage["output_tokens"]), reasoning: typeof thinking === "number" ? thinking : null };
}

/** Each user turn's exact tokens, from the total its tool reports when the turn ends. */
export function turnUsage(tool: string, events: string): TurnUsage[] {
  const ending = tool === Tool.claude ? "result" : "turn.completed";
  return logLines(events).flatMap((line) => {
    const usage = record(line["usage"]);
    return line["type"] !== ending || usage === null ? [] : [tool === Tool.claude ? claudeTurn(usage) : codexTurn(usage)];
  }).map((tokens, index) => ({ turn: index + 1, tokens }));
}

/** The sum of token totals. Reasoning stays null only when no total reports it. */
export function addTokens(totals: readonly TokenTotals[]): TokenTotals | null {
  if (totals.length === 0) return null;
  const sum = (pick: (item: TokenTotals) => number) => totals.reduce((all, item) => all + pick(item), 0);
  const reasoning = totals.some((item) => item.reasoning !== null) ? sum((item) => item.reasoning ?? 0) : null;
  return { newInput: sum((item) => item.newInput), cachedInput: sum((item) => item.cachedInput), cacheWrite: sum((item) => item.cacheWrite), output: sum((item) => item.output), reasoning };
}
