import assert from "node:assert/strict";
import { test } from "node:test";
import { Flag } from "./commands.ts";
import { rejudgeOptions, runPresetOptions } from "./preset-options.ts";
import { JudgePresets, PresetName, UserPresets } from "./presets.ts";

test("rejudge accepts a judge preset around its operands and keeps no-option batches unchanged", () => {
  const first = JudgePresets[PresetName.both][0];
  for (const args of [["batch", "attempt", Flag.judges, PresetName.both], [Flag.judges, PresetName.both, "batch", "attempt"]]) {
    assert.deepEqual(rejudgeOptions(args), { batchId: "batch", onlyRun: "attempt", overrides: { judge: first, judges: JudgePresets[PresetName.both] } });
  }
  assert.deepEqual(rejudgeOptions(["batch"]), { batchId: "batch", onlyRun: undefined, overrides: {} });
  assert.deepEqual(runPresetOptions([Flag.user, PresetName.codex]), { simulatedUser: UserPresets[PresetName.codex] });
  for (const args of [[], ["batch", Flag.judges], ["batch", Flag.plan, "plan"], ["batch", Flag.user, PresetName.claude], ["batch", "attempt", "extra"]]) assert.throws(() => rejudgeOptions(args));
});
