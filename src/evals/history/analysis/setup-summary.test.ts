import assert from "node:assert/strict";
import { test } from "node:test";
import type { RunAnalysis } from "./analysis.types.ts";
import { AnalysisMethod, ContextClass, ToolKind } from "./classes.ts";
import { setupUsage } from "./setup-summary.ts";

const Control = "plain-docs", Scoped = "full-core-scoped";
const calls = (read: number) => ({ [ToolKind.read]: read, [ToolKind.search]: 0, [ToolKind.edit]: 1, [ToolKind.run]: 1, [ToolKind.other]: 0 });
function analysis(spec: number, read: number, split = true): RunAnalysis {
  return { method: split ? AnalysisMethod.rollout : AnalysisMethod.none, sources: [], tokens: { newInput: 100, cachedInput: 900 + spec, cacheWrite: 0, output: 10, reasoning: null },
    totalsAgree: true, requests: 4, peakContext: 400, compactions: 0, beforeEdit: { requests: 1, input: 250, fresh: 100 },
    parts: split ? [{ class: ContextClass.upfront, fileType: null, input: 1000, fresh: 100, peak: 300 }, { class: ContextClass.spec, fileType: "md", input: spec, fresh: 0, peak: 100 }] : [],
    process: { calls: calls(read), failed: 0, steps: read + 2, stepsBeforeEdit: read, repeatedReads: 0, filesRead: read, filesEdited: 1 }, turns: [], threads: [] };
}

test("each setup's means cover its runs, and runs without a split still count for totals and tool calls", () => {
  const [control, scoped] = setupUsage([Control, Scoped], [
    { setup: Control, analysis: analysis(400, 6) }, { setup: Control, analysis: analysis(200, 4) },
    { setup: Scoped, analysis: analysis(100, 2) }, { setup: Scoped, analysis: analysis(0, 4, false) },
  ]);
  assert.equal(control?.runs, 2);
  assert.equal(control?.classes[ContextClass.spec], 300);
  assert.equal(control?.fileTypes["md"], 300);
  assert.equal(control?.calls[ToolKind.read], 5);
  assert.equal(control?.beforeEdit, (250 / 1400 + 250 / 1200) / 2);
  assert.equal(scoped?.classes[ContextClass.spec], 100);
  assert.equal(scoped?.calls[ToolKind.read], 3);
  assert.equal(scoped?.cachedInput, (1000 + 900) / 2);
});
