import assert from "node:assert/strict";
import { test } from "node:test";
import { Tool } from "../../config/choices.ts";
import type { AgentTelemetry } from "../results.types.ts";
import type { RunAnalysis } from "./analysis.types.ts";
import { analyzeRun } from "./analyze.ts";
import { AnalysisMethod, ContextClass, ToolKind } from "./classes.ts";

const lines = (items: readonly object[]) => items.map((item) => JSON.stringify(item)).join("\n");
const command = (text: string, output: string, exit = 0) => ({ type: "item.completed", item: { type: "command_execution", command: `/bin/sh -lc '${text}'`, aggregated_output: output, exit_code: exit } });
const Turn = { input_tokens: 4000, cached_input_tokens: 3000, output_tokens: 100, reasoning_output_tokens: 20 };
const Events = lines([
  { type: "turn.started" }, command("cat docs/rules.md", "r".repeat(800)), command("rg -n best src", "src/a.ts:1:best", 1),
  { type: "item.completed", item: { type: "file_change", changes: [{ path: "/work/src/a.ts", kind: "update" }] } }, command("npm test", "ok"),
  { type: "turn.completed", usage: Turn },
]);
const Rollout = lines([
  { type: "session_meta", payload: { id: "t", source: "exec" } },
  { type: "response_item", payload: { type: "function_call", name: "exec_command", call_id: "c", arguments: JSON.stringify({ cmd: "cat docs/rules.md" }) } },
  { type: "token_usage_record", payload: { thread_id: "t", usage: { input_tokens: 1500, cached_input_tokens: 1000, output_tokens: 50, reasoning_output_tokens: 10 } } },
  { type: "response_item", payload: { type: "function_call_output", call_id: "c", output: "r".repeat(800) } },
  { type: "token_usage_record", payload: { thread_id: "t", usage: { input_tokens: 2500, cached_input_tokens: 2000, output_tokens: 50, reasoning_output_tokens: 10 } } },
]);
const telemetry = (requests: number | null, peak: number | null): AgentTelemetry => ({
  tokens: { input: 1000, cachedInput: 3000, cacheWrite: 0, output: 100, reasoning: 20 }, requests, peakContext: peak, contextWindow: null, compactions: 0,
  toolCalls: 4, failedToolCalls: 1, openForgeReads: 0, openForgeBytes: 0, reportedCostUsd: null, model: null, sessionId: null,
});
const inputs = { tool: Tool.codex, answers: [], files: new Map([["docs/rules.md", 800]]) };
const partSum = (analysis: RunAnalysis, pick: (part: RunAnalysis["parts"][number]) => number) => analysis.parts.reduce((sum, part) => sum + pick(part), 0);

test("a run with a session log takes exact totals from its requests, and its parts sum back to them", () => {
  const analysis = analyzeRun({ ...inputs, telemetry: telemetry(2, 2500), events: Events, rollout: Rollout });
  assert.equal(analysis.method, AnalysisMethod.rollout);
  assert.deepEqual(analysis.tokens, { newInput: 1000, cachedInput: 3000, cacheWrite: 0, output: 100, reasoning: 20 });
  assert.equal(analysis.totalsAgree, true);
  assert.equal(analysis.requests, 2);
  assert.equal(partSum(analysis, (part) => part.input), 4000);
  assert.equal(partSum(analysis, (part) => part.fresh), 1000);
  assert.equal(partSum(analysis, (part) => part.peak), 2500);
  assert.ok(analysis.parts.some((part) => part.class === ContextClass.spec && part.fileType === "md"));
});

test("a run saved without a session log gets the events estimate, which still sums to its exact totals and peak", () => {
  const analysis = analyzeRun({ ...inputs, telemetry: telemetry(5, 1200), events: Events, rollout: null });
  assert.equal(analysis.method, AnalysisMethod.events);
  assert.equal(analysis.totalsAgree, null);
  assert.equal(partSum(analysis, (part) => part.input), 4000);
  assert.equal(partSum(analysis, (part) => part.fresh), 1000);
  assert.equal(partSum(analysis, (part) => part.peak), 1200);
  assert.ok(analysis.parts.some((part) => part.class === ContextClass.spec && part.fileType === "md"));
});

test("tool calls, failures, the first edit and each turn's exact tokens come from the events", () => {
  const analysis = analyzeRun({ ...inputs, telemetry: telemetry(5, 1200), events: Events, rollout: null });
  assert.deepEqual(analysis.process?.calls, { [ToolKind.read]: 1, [ToolKind.search]: 1, [ToolKind.edit]: 1, [ToolKind.run]: 1, [ToolKind.other]: 0 });
  assert.equal(analysis.process?.failed, 1);
  assert.equal(analysis.process?.stepsBeforeEdit, 2);
  assert.equal(analysis.process?.filesEdited, 1);
  assert.deepEqual(analysis.turns.map((turn) => turn.tokens.newInput), [1000]);
});

test("a run without telemetry or logs keeps its totals unknown and no split", () => {
  const analysis = analyzeRun({ ...inputs, tool: Tool.mock, telemetry: null, events: null, rollout: null });
  assert.equal(analysis.method, AnalysisMethod.none);
  assert.equal(analysis.tokens, null);
  assert.deepEqual(analysis.parts, []);
  assert.equal(analysis.process, null);
});
