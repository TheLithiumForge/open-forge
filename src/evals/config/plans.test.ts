import assert from "node:assert/strict";
import { test } from "node:test";
import { readFile } from "node:fs/promises";
import { join } from "node:path";
import { evalsRoot } from "../paths.ts";
import { LayerOnePlan, resolvePlan, ScreeningRuns, ScreeningSetups } from "./plans.ts";
import { FixtureConfig } from "./test-fixtures.ts";
import { Isolation } from "./choices.ts";
import { ImplementerPresets, JudgePresets, UserPresets, PresetName } from "./presets.ts";
import { freezeProtocol, QuestionRounds } from "../execution/conversation/answer-bank.ts";
import { readProtocol } from "./protocol.ts";

test("the saved Layer 1 plan screens four setups and freezes current assessor choices", async () => {
  const source: unknown = JSON.parse(await readFile(join(evalsRoot, LayerOnePlan), "utf8"));
  const config = resolvePlan(source, FixtureConfig);
  assert.equal(config.runsPerSetup, ScreeningRuns);
  assert.deepEqual(config.setups, [...ScreeningSetups]);
  assert.deepEqual(config.agents, ImplementerPresets[PresetName.claude]);
  assert.equal(config.isolation, Isolation.docker);
  assert.equal(config.limits.questionRounds, QuestionRounds);
  assert.deepEqual(config.judges, JudgePresets[PresetName.claude]);
  assert.deepEqual(config.simulatedUser, UserPresets[PresetName.claude]);
  assert.equal(config.parallelRuns, 1);
});

test("local plans require the development flag", async () => {
  const source: unknown = JSON.parse(await readFile(join(evalsRoot, LayerOnePlan), "utf8"));
  assert.ok(typeof source === "object" && source !== null);
  const local = { ...source, isolation: Isolation.local };
  assert.throws(() => resolvePlan(local, FixtureConfig));
  assert.equal(resolvePlan(local, FixtureConfig, true).development, true);
});

test("frozen protocol rejects an edited literal answer", () => {
  const protocol = freezeProtocol([{ topicId: "sound", answer: "No sound." }], ["Build a game."]);
  assert.deepEqual(readProtocol(protocol), protocol);
  assert.throws(() => readProtocol({ ...protocol, bank: [{ topicId: "sound", answer: "Changed." }] }));
});
