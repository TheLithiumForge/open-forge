import assert from "node:assert/strict";
import { test } from "node:test";
import { parseConfig } from "../../config/config.ts";
import { configuredJudges } from "../../config/judges.ts";
import { FixtureConfig } from "../../config/test-fixtures.ts";
import { CustomPreset, ImplementerPresets, JudgePresets, UserPresets, PresetName, matchImplementers, matchJudges, matchUser, presetChoices } from "../../config/presets.ts";

test("the UI identifies role presets and its resolved config passes the guarded API parser", () => {
  for (const implementers of Object.keys(ImplementerPresets)) for (const judges of Object.keys(JudgePresets)) for (const user of Object.keys(UserPresets)) {
    const config = parseConfig({ ...FixtureConfig, ...presetChoices({ implementers, judges, user }) });
    assert.equal(matchImplementers(config.agents), implementers);
    assert.equal(matchJudges(configuredJudges(config)), judges);
    assert.equal(matchUser(config.simulatedUser), user);
  }
  const judge = JudgePresets[PresetName.claude][0];
  assert.equal(matchJudges(configuredJudges({ judge })), PresetName.claude);
  assert.equal(matchUser({ effort: UserPresets[PresetName.claude].effort, model: UserPresets[PresetName.claude].model, tool: UserPresets[PresetName.claude].tool }), PresetName.claude);
});

test("custom model rows, judge ballots and changed list order show Custom", () => {
  assert.equal(matchImplementers([]), CustomPreset);
  assert.equal(matchImplementers([...ImplementerPresets[PresetName.both]].reverse()), CustomPreset);
  assert.equal(matchImplementers([{ ...ImplementerPresets[PresetName.claude][0], model: null }]), CustomPreset);
  assert.equal(matchJudges([{ ...JudgePresets[PresetName.claude][0], repeats: 1 }]), CustomPreset);
  assert.equal(matchJudges([...JudgePresets[PresetName.both]].reverse()), CustomPreset);
  assert.equal(matchUser({ ...UserPresets[PresetName.codex], effort: null }), CustomPreset);
});
