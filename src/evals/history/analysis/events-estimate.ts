import type { ContextPart, TokenTotals } from "./analysis.types.ts";
import { CharactersPerToken, ContextClass } from "./classes.ts";
import { commandFacts } from "./command-facts.ts";
import { partKey } from "./ledger.ts";
import { list, logLines, record, string, text } from "./log-values.ts";
import { contextParts } from "./parts.ts";
import { outputSegments, type KnownFiles, type Segment } from "./segments.ts";

/**
 * A rougher split for Codex runs saved before the session log was kept. Their events give every tool output in order
 * but only one total per turn. Each output is assumed to stay in the context for the rest of the run, and the up-front
 * size and one scale for the outputs are fitted so the estimate meets the run's exact total input and its largest request.
 */

/** Scales outside this range mean the fit failed, and the outputs keep their character estimate. */
const ScaleRange = { lowest: 0.2, highest: 2 } as const;

/** What joined the context between turns and tool calls, in order. Turns after the first add the user's answer. */
export function codexEventItems(events: string, answers: readonly string[], files: KnownFiles): Segment[][] {
  const items: Segment[][] = [];
  let turns = 0;
  for (const line of logLines(events)) {
    const item = record(line["item"]);
    if (line["type"] === "turn.started") {
      turns += 1;
      if (turns > 1) items.push([{ class: ContextClass.user, fileType: null, chars: (answers[turns - 2] ?? "").length }]);
    }
    if (line["type"] !== "item.completed" || item === null) continue;
    const type = string(item["type"]);
    if (type === "command_execution") items.push(outputSegments(commandFacts(string(item["command"]) ?? ""), string(item["aggregated_output"]) ?? "", files));
    if (type === "file_change") items.push([{ class: ContextClass.command, fileType: null, chars: list(item["changes"]).map((change) => string(record(change)?.["path"]) ?? "").join("\n").length }]);
    if (type === "mcp_tool_call") items.push([{ class: ContextClass.command, fileType: null, chars: text(record(item["result"])?.["content"]).length }]);
  }
  return items;
}

export interface EventsTotals { readonly tokens: TokenTotals; readonly requests: number | null; readonly peak: number | null }

/** The fitted split. It sums exactly to the run's input, its fresh input and, when known, its largest request. */
export function eventsEstimate(items: readonly (readonly Segment[])[], totals: EventsTotals): ContextPart[] {
  const { tokens } = totals;
  const count = items.length, requests = Math.max(1, totals.requests ?? count + 1);
  const sizes = items.map((segments) => segments.reduce((sum, part) => sum + part.chars, 0) / CharactersPerToken);
  // An output joined after its place in the run, so it is sent with the requests that follow it.
  const resends = items.map((_, index) => ((count - index) * requests) / (count + 1));
  const input = tokens.newInput + tokens.cachedInput + tokens.cacheWrite, fresh = tokens.newInput + tokens.cacheWrite, output = tokens.output;
  const agent = (output * (requests - 1)) / 2;
  const size = sizes.reduce((sum, value) => sum + value, 0), resent = sizes.reduce((sum, value, index) => sum + value * (resends[index] ?? 0), 0);
  let scale = 1, upfront = Math.max(0, (input - agent - resent) / requests);
  if (totals.peak !== null && resent !== requests * size) {
    const fitted = (input - agent - requests * (totals.peak - output)) / (resent - requests * size);
    const fittedUpfront = totals.peak - output - fitted * size;
    if (Number.isFinite(fitted) && fitted >= ScaleRange.lowest && fitted <= ScaleRange.highest && fittedUpfront >= 0) { scale = fitted; upfront = fittedUpfront; }
  }
  const maps = { input: new Map<string, number>(), fresh: new Map<string, number>(), peak: new Map<string, number>() };
  const add = (target: Map<string, number>, key: string, value: number) => target.set(key, (target.get(key) ?? 0) + value);
  const upfrontKey = partKey(ContextClass.upfront, null), agentKey = partKey(ContextClass.agent, null);
  add(maps.input, upfrontKey, upfront * requests); add(maps.peak, upfrontKey, upfront);
  add(maps.input, agentKey, agent); add(maps.peak, agentKey, output); add(maps.fresh, agentKey, output);
  items.forEach((segments, index) => {
    for (const part of segments) {
      const tokensOf = (scale * part.chars) / CharactersPerToken, key = partKey(part.class, part.fileType);
      add(maps.input, key, tokensOf * (resends[index] ?? 0)); add(maps.peak, key, tokensOf); add(maps.fresh, key, tokensOf);
    }
  });
  add(maps.fresh, upfrontKey, Math.max(0, fresh - output - scale * size));
  return contextParts(maps, { input, fresh, peak: totals.peak });
}
