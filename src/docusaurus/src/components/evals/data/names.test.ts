import assert from "node:assert/strict";
import { test } from "node:test";
import { modelLabel, setupRank, SetupNames } from "./names.ts";

test("model labels name the reported model and its reasoning effort", () => {
  assert.equal(modelLabel({ tool: "claude", model: "opus", effort: "high" }, "claude-opus-5-5"), "Claude Opus 5.5, high reasoning");
  assert.equal(modelLabel({ tool: "codex", model: "gpt-6.1-sol", effort: "xhigh" }), "GPT-6.1 Sol, xhigh reasoning");
  assert.equal(modelLabel({ tool: "codex", model: "future-model", effort: null }), "future-model");
});

test("setups sort in reading order with unknown setups last", () => {
  const known = Object.keys(SetupNames);
  const shuffled = [...known].reverse();
  assert.deepEqual(shuffled.sort((a, b) => setupRank(a) - setupRank(b)), known);
  const unknown = `${known[0] ?? ""}-unknown`;
  assert.ok(known.every((id) => setupRank(id) < setupRank(unknown)));
});
