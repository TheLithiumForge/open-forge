import assert from "node:assert/strict";
import { test } from "node:test";
import { WorkMode } from "../../config/choices.ts";
import { CodexMultiAgent, workModeArguments } from "./codex-session.ts";

test("one agent switches Codex's default subagents off, and an orchestrator gets one level of them on the implementer's model and effort", () => {
  assert.deepEqual(workModeArguments(WorkMode.oneAgent, "gpt-6.1-sol", "high"), ["--disable", CodexMultiAgent]);
  const orchestrated = workModeArguments(WorkMode.orchestrated, "gpt-6.1-sol", "high");
  assert.deepEqual(orchestrated.slice(0, 2), ["--enable", CodexMultiAgent]);
  for (const setting of ["agents.max_depth=1", "agents.max_concurrent_threads_per_session=1", 'agents.default_subagent_model="gpt-6.1-sol"', 'agents.default_subagent_reasoning_effort="high"']) {
    assert.ok(orchestrated.includes(setting), setting);
  }
});
