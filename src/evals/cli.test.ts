import assert from "node:assert/strict";
import { test } from "node:test";
import { runCli, type CliServices } from "./cli.ts";
import { Command, commandHelp, Flag } from "./config/commands.ts";
import { FixtureConfig, FixtureSuite } from "./config/test-fixtures.ts";
import { RunStatus } from "./history/statuses.ts";
import { ImplementerPresets, JudgePresets, UserPresets, PresetName } from "./config/presets.ts";
import type { EvalConfig } from "./config/config.types.ts";
import { referenceOptions, ReferenceLayer } from "./config/reference.ts";

function fakeServices() {
  const calls: string[] = [], output: string[] = [];
  const services: CliServices = {
    print: (text) => output.push(text), doctor: async () => [], images: async () => { calls.push(Command.images); },
    smoke: async () => { calls.push(Command.smoke); return { status: RunStatus.done, questions: 1, assessments: 2 }; },
    qualify: async () => { calls.push(Command.qualify); return []; }, plan: async (_path, development) => { calls.push(`plan:${development}`); return FixtureConfig; },
    suite: async () => FixtureSuite, run: async () => { calls.push(Command.run); return 0; }, ui: async () => { calls.push(Command.ui); return 0; },
    launch: async () => { calls.push("launch"); return 0; }, export: async (_batch, target) => { calls.push(Command.export); return target; },
    validateCase: async () => { throw new Error("Unexpected case validation."); },
  };
  return { services, calls, output };
}

test("every command has help with no effects", async () => {
  const fake = fakeServices();
  for (const command of Object.values(Command)) {
    assert.equal(await runCli([command, Flag.help], fake.services), 0);
    assert.equal(fake.output.at(-1), command === Command.help ? commandHelp() : commandHelp(command));
  }
  assert.deepEqual(fake.calls, []);
});

test("run and qualify resolve the named plan without starting unintended commands", async () => {
  const fake = fakeServices();
  await runCli([Command.run, Flag.plan, "fixture.json", Flag.development], fake.services);
  assert.deepEqual(fake.calls, ["plan:true", Command.run]);
  fake.calls.length = 0;
  await runCli([Command.qualify, Flag.plan, "fixture.json"], fake.services);
  assert.deepEqual(fake.calls, ["plan:true", Command.qualify]);
});

test("CLI rejects unknown and conflicting input before running anything", async () => {
  const fake = fakeServices();
  for (const args of [["unknown"], [Command.run], [Command.smoke, Flag.development], [Command.run, Flag.plan, "fixture.json", Flag.plan, "other.json"]]) await assert.rejects(runCli(args, fake.services));
  assert.deepEqual(fake.calls, []);
});

test("CLI dispatches smoke, images, UI, saved grading and export", async () => {
  const fake = fakeServices();
  for (const args of [[Command.smoke], [Command.images], [Command.ui], [Command.recheck, "batch", Flag.rebuild], [Command.rejudge, "batch"], [Command.rejudge, "batch", Flag.judges, PresetName.both], [Command.rejudge, "batch", "run", Flag.judges, PresetName.codex], [Command.export, "batch", Flag.out, "destination"]]) assert.equal(await runCli(args, fake.services), 0);
  assert.deepEqual(fake.calls, [Command.smoke, Command.images, Command.ui, "launch", "launch", "launch", "launch", Command.export]);
});

test("run preset flags override only the requested plan roles before any implementation starts", async () => {
  const fake = fakeServices(), started: EvalConfig[] = [];
  await runCli([Command.run, Flag.plan, "fixture.json", Flag.implementers, PresetName.both, Flag.judges, PresetName.codex, Flag.user, PresetName.claude],
    { ...fake.services, run: async (config) => { started.push(config); return 0; } });
  assert.equal(started.length, 1);
  assert.deepEqual(started[0], { ...FixtureConfig, agents: ImplementerPresets[PresetName.both], judges: JudgePresets[PresetName.codex],
    judge: JudgePresets[PresetName.codex][0], simulatedUser: UserPresets[PresetName.claude] });
  await runCli([Command.run, Flag.plan, "fixture.json", Flag.user, PresetName.codex],
    { ...fake.services, run: async (config) => { started.push(config); return 0; } });
  assert.deepEqual(started[1], { ...FixtureConfig, simulatedUser: UserPresets[PresetName.codex] });
});

test("unknown role presets and removed rejudge plan flags fail without loading or launching anything", async () => {
  const fake = fakeServices();
  for (const flag of [Flag.implementers, Flag.judges, Flag.user]) await assert.rejects(runCli([Command.run, Flag.plan, "fixture.json", flag, "unknown"], fake.services));
  for (const args of [[Command.run, Flag.plan, "fixture.json", Flag.user, PresetName.both],
    [Command.rejudge, "batch", Flag.plan, "fixture.json"], [Command.rejudge, "batch", Flag.judges, "unknown"],
    [Command.rejudge, "batch", Flag.judges, PresetName.claude, Flag.judges, PresetName.codex]]) await assert.rejects(runCli(args, fake.services));
  assert.deepEqual(fake.calls, []);
});

test("reference defaults and overrides dispatch without a model implementation", async () => {
  const fake = fakeServices(), selections: ReturnType<typeof referenceOptions>[] = [];
  const services = { ...fake.services, reference: async (options: ReturnType<typeof referenceOptions>) => { selections.push(options); return 0; } };
  assert.equal(await runCli([Command.reference], services), 0);
  assert.deepEqual(selections[0], referenceOptions());
  assert.equal(await runCli([Command.reference, Flag.layer, String(ReferenceLayer.two), Flag.judges, PresetName.both], services), 0);
  assert.deepEqual(selections[1], referenceOptions(String(ReferenceLayer.two), PresetName.both));
  for (const args of [[Flag.layer, "1"], [Flag.judges, "unknown"], [Flag.implementers, PresetName.claude], [Flag.layer, "3", Flag.layer, "2"]]) {
    await assert.rejects(runCli([Command.reference, ...args], services));
  }
  assert.equal(selections.length, 2);
  assert.deepEqual(fake.calls, []);
});
