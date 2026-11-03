import assert from "node:assert/strict";
import { readFile, readdir } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { evalsRoot } from "../paths.ts";
import { MatrixAgents, MatrixBallots, MatrixEffort, MatrixJudges, MatrixModels } from "./matrix.ts";
import { Tool } from "./choices.ts";
import { parseConfig } from "./config.ts";
import { FixtureConfig } from "./test-fixtures.ts";
import { LocalModel, PlanVersion, resolvePlan } from "./plans.ts";
import { ImplementerPresets, JudgePresets, UserPresets, PresetName, implementerPreset, judgePreset, userPreset, presetChoices } from "./presets.ts";

test("each named role uses the owned matrix models, efforts, ballots and order", () => {
  assert.deepEqual(implementerPreset(PresetName.claude), [MatrixAgents[1]]);
  assert.deepEqual(implementerPreset(PresetName.codex), [MatrixAgents[0]]);
  assert.deepEqual(implementerPreset(PresetName.both), MatrixAgents);
  assert.deepEqual(judgePreset(PresetName.claude), [MatrixJudges[0]]);
  assert.deepEqual(judgePreset(PresetName.codex), [MatrixJudges[1]]);
  assert.deepEqual(judgePreset(PresetName.both), MatrixJudges);
  assert.ok(judgePreset(PresetName.both).every((judge) => judge.repeats === MatrixBallots));
  assert.deepEqual(userPreset(PresetName.claude), { tool: Tool.claude, model: MatrixModels.claudeUser, effort: MatrixEffort.medium });
  assert.deepEqual(userPreset(PresetName.codex), { tool: Tool.codex, model: MatrixModels.codexUser, effort: MatrixEffort.max });
  for (const name of ["unknown", "toString"]) for (const read of [implementerPreset, judgePreset, userPreset]) assert.throws(() => read(name));
  assert.throws(() => userPreset(PresetName.both));
});

test("plans resolve role presets, explicit choices and local-config into concrete saved choices", () => {
  const source = { schemaVersion: PlanVersion, layer: 1, case: FixtureConfig.suite, setups: FixtureConfig.setups,
    runsPerSetup: 1, isolation: FixtureConfig.isolation, agents: PresetName.both, judges: PresetName.codex, simulatedUser: PresetName.claude };
  const plan = resolvePlan(source, FixtureConfig);
  assert.deepEqual(plan.agents, ImplementerPresets[PresetName.both]);
  assert.deepEqual(plan.judges, JudgePresets[PresetName.codex]);
  assert.deepEqual(plan.simulatedUser, UserPresets[PresetName.claude]);
  assert.deepEqual(parseConfig(JSON.parse(JSON.stringify(plan))), plan);
  const local = resolvePlan({ ...source, agents: LocalModel, judges: LocalModel, simulatedUser: LocalModel }, FixtureConfig);
  assert.deepEqual(local.agents, FixtureConfig.agents);
  assert.deepEqual(local.judges, [FixtureConfig.judge]);
  assert.deepEqual(local.simulatedUser, FixtureConfig.simulatedUser);
  for (const key of ["agents", "judges", "simulatedUser"]) assert.throws(() => resolvePlan({ ...source, [key]: "unknown" }, FixtureConfig));
  const selected = { ...plan, ...presetChoices({ judges: PresetName.claude }) };
  assert.deepEqual(selected.agents, plan.agents);
  assert.deepEqual(selected.simulatedUser, plan.simulatedUser);
  assert.deepEqual(selected.judge, JudgePresets[PresetName.claude][0]);
});

test("every saved experiment names one provider's role presets, so the plan states what runs", async () => {
  const folder = join(evalsRoot, "experiments"), names = (await readdir(folder)).filter((name) => name.endsWith(".json"));
  assert.ok(names.length > 0);
  // Each implementer preset belongs to one provider, whose judge and simulated user presets the plan names.
  const families = { [PresetName.claude]: PresetName.claude, [PresetName.codex]: PresetName.codex, [PresetName.codexOrchestrators]: PresetName.codex } as const;
  for (const name of names) {
    const source: unknown = JSON.parse(await readFile(join(folder, name), "utf8"));
    assert.ok(typeof source === "object" && source !== null && "agents" in source && "judges" in source && "simulatedUser" in source);
    const implementers = Object.keys(families).find((preset): preset is keyof typeof families => preset === source.agents);
    const provider = implementers === undefined ? undefined : families[implementers];
    assert.ok(provider !== undefined, name);
    assert.deepEqual([source.judges, source.simulatedUser], [provider, provider], name);
    const config = resolvePlan(source, FixtureConfig);
    assert.deepEqual(config.agents, ImplementerPresets[implementers ?? provider]);
    assert.deepEqual(config.judges, JudgePresets[provider]);
    assert.deepEqual(config.simulatedUser, UserPresets[provider]);
  }
});
