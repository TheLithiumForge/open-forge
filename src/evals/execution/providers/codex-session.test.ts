import assert from "node:assert/strict";
import { test } from "node:test";
import { CodexOrchestrators, ImplementerPresets, PresetName } from "../../config/presets.ts";
import { CodexMultiAgent, teamArguments } from "./codex-session.ts";

test("a solo implementer switches Codex's default subagents off, and a team gets one level of them on the team's model and effort", () => {
  const [solo] = ImplementerPresets[PresetName.codex], [sol, luna] = CodexOrchestrators;
  assert.deepEqual(teamArguments(solo), ["--disable", CodexMultiAgent]);
  for (const team of [sol, luna]) {
    const args = teamArguments(team);
    assert.deepEqual(args.slice(0, 2), ["--enable", CodexMultiAgent]);
    for (const setting of ["agents.max_depth=1", "agents.max_concurrent_threads_per_session=1",
      `agents.default_subagent_model="${team.team.model}"`, `agents.default_subagent_reasoning_effort="${team.team.effort}"`]) assert.ok(args.includes(setting), setting);
  }
});
