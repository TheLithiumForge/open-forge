import assert from "node:assert/strict";
import { test } from "node:test";
import type { Run, ThreadTelemetry } from "../records/records.types.ts";
import { orchestratorCostShare, priceOf, PriceVersions, runCost, threadCost, type ModelPrice } from "./prices.ts";

const Lead = "gpt-6.1-sol", Team = "gpt-6-luna";
const price = (model: string): ModelPrice => {
  const found = priceOf(model);
  assert.ok(found !== null);
  return found;
};
const tokens = { input: 1_000_000, cachedInput: 2_000_000, cacheWrite: 3_000_000, output: 4_000_000 };
const expected = (model: string, counts = tokens) => {
  const { input, cachedInput, cacheWrite, output } = price(model);
  return counts.input / 1e6 * input + counts.cachedInput / 1e6 * cachedInput + counts.cacheWrite / 1e6 * cacheWrite + counts.output / 1e6 * output;
};
const run = (agent: Run["agent"], telemetry: Partial<NonNullable<Run["telemetry"]>> | null, reportedModel: string | null = null) => ({
  agent, reportedModel, telemetry: telemetry === null ? null : { tokens, toolCalls: 0, failedToolCalls: 0, peakContext: null, ...telemetry },
}) as Run;
const thread = (role: ThreadTelemetry["role"], model?: string): ThreadTelemetry => ({
  id: role, parentId: null, role, requests: 1, peakContext: null, toolCalls: 0, ...(model === undefined ? {} : { model }),
  tokens: { newInput: tokens.input, cachedInput: tokens.cachedInput, cacheWrite: tokens.cacheWrite, output: tokens.output, reasoning: null },
});
const close = (actual: number | null, wanted: number) => assert.ok(actual !== null && Math.abs(actual - wanted) < 1e-9, `${actual} is not ${wanted}`);

test("every price version names where it was read and prices each token class", () => {
  for (const version of PriceVersions) {
    assert.ok(version.sources.length > 0);
    for (const model of Object.values(version.models)) for (const value of [model.input, model.cachedInput, model.cacheWrite, model.output]) assert.ok(value > 0);
  }
});

test("a run without subagents costs its tokens at its reported model's prices, and a short name reads the current model", () => {
  close(runCost(run({ tool: "codex", model: "unknown", effort: null }, {}, Lead)), expected(Lead));
  close(runCost(run({ tool: "claude", model: "opus", effort: null }, {})), expected("claude-opus-5-5"));
});

test("a led run adds each thread at its own model: the lead's, its team's, or the one the thread names", () => {
  const agent = { tool: "codex", model: Lead, effort: null, team: { model: Team, effort: null } };
  const led = run(agent, { threads: [thread("main"), thread("subagent")] });
  close(runCost(led), expected(Lead) + expected(Team));
  close(orchestratorCostShare(led), expected(Lead) / (expected(Lead) + expected(Team)));
  const named = run(agent, { threads: [thread("main"), thread("subagent", Lead)] });
  close(runCost(named), expected(Lead) * 2);
  close(threadCost(named, thread("subagent", Lead)), expected(Lead));
});

test("a run costs nothing known without tokens or a price, and has no orchestrator share without subagents", () => {
  assert.equal(runCost(run({ tool: "codex", model: Lead, effort: null }, null)), null);
  assert.equal(runCost(run({ tool: "mock", model: null, effort: null }, {})), null);
  assert.equal(runCost(run({ tool: "codex", model: "unlisted", effort: null }, { threads: [thread("main")] })), null);
  assert.equal(orchestratorCostShare(run({ tool: "codex", model: Lead, effort: null }, { threads: [thread("main")] })), null);
  assert.equal(priceOf(null), null);
});
