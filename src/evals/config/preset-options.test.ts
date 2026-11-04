import assert from "node:assert/strict";
import { test } from "node:test";
import { Flag } from "./commands.ts";
import { judgeOptions, runPresetOptions } from "./preset-options.ts";
import { JudgePresets, PresetName, UserPresets } from "./presets.ts";

test("judge accepts a judge preset around its operands and keeps no-option batches unchanged", () => {
  const first = JudgePresets[PresetName.both][0];
  for (const args of [["batch", "attempt", Flag.judges, PresetName.both], [Flag.judges, PresetName.both, "batch", "attempt"]]) {
    assert.deepEqual(judgeOptions(args), { batchId: "batch", onlyRun: "attempt", overrides: { judge: first, judges: JudgePresets[PresetName.both] }, missingOnly: false });
  }
  assert.deepEqual(judgeOptions(["batch"]), { batchId: "batch", onlyRun: undefined, overrides: {}, missingOnly: false });
  assert.equal(judgeOptions([Flag.missing, "batch"]).missingOnly, true);
  const parallel = 20;
  assert.deepEqual(judgeOptions(["batch", Flag.parallel, String(parallel), Flag.missing]), { batchId: "batch", onlyRun: undefined, overrides: { parallelRuns: parallel }, missingOnly: true });
  assert.throws(() => judgeOptions(["batch", Flag.parallel, "0"]));
  assert.deepEqual(runPresetOptions([Flag.user, PresetName.codex]), { simulatedUser: UserPresets[PresetName.codex] });
  for (const args of [[], ["batch", Flag.judges], ["batch", Flag.plan, "plan"], ["batch", Flag.user, PresetName.claude], ["batch", "attempt", "extra"], ["batch", Flag.missing, Flag.missing]]) assert.throws(() => judgeOptions(args));
});
