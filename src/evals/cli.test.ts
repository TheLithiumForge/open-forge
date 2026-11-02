import assert from "node:assert/strict";
import { test } from "node:test";
import { runCli, type CliServices } from "./cli.ts";
import { Command, commandHelp, Flag, ResultsAction } from "./config/commands.ts";
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
    suite: async () => FixtureSuite, run: async () => { calls.push(Command.run); return 0; }, site: async () => { calls.push(Command.site); return 0; },
    launch: async () => { calls.push("launch"); return 0; }, results: async (_batch, repository) => { calls.push(Command.results); return repository; },
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
  for (const args of [[Command.smoke], [Command.images], [Command.site], [Command.check, "batch", Flag.rebuild], [Command.judge, "batch"], [Command.judge, "batch", Flag.judges, PresetName.both], [Command.judge, "batch", "run", Flag.judges, PresetName.codex], [Command.results, ResultsAction.add, "batch"], [Command.results, ResultsAction.add, "batch", Flag.repository, "elsewhere"]]) assert.equal(await runCli(args, fake.services), 0);
  assert.deepEqual(fake.calls, [Command.smoke, Command.images, Command.site, "launch", "launch", "launch", "launch", Command.results, Command.results]);
  for (const args of [[Command.results], [Command.results, "remove", "batch"], [Command.results, ResultsAction.add], [Command.results, ResultsAction.add, "batch", "other"]]) await assert.rejects(runCli(args, fake.services));
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

test("run --case narrows a multi-case plan to one case and refuses a case the plan lacks", async () => {
  const fake = fakeServices(), started: EvalConfig[] = [];
  const cases = [FixtureSuite.id, `${FixtureSuite.id}-other`];
  const services = { ...fake.services, plan: async () => ({ ...FixtureConfig, cases }), run: async (config: EvalConfig) => { started.push(config); return 0; } };
  assert.equal(await runCli([Command.run, Flag.plan, "fixture.json", Flag.case, FixtureSuite.id], services), 0);
  assert.deepEqual(started[0]?.cases, [FixtureSuite.id]);
  await assert.rejects(runCli([Command.run, Flag.plan, "fixture.json", Flag.case, "unknown"], services));
  await assert.rejects(runCli([Command.run, Flag.plan, "fixture.json", Flag.case, FixtureSuite.id], { ...services, plan: async () => FixtureConfig }));
  assert.equal(started.length, 1);
});

test("resume continues one named batch and needs exactly one", async () => {
  const fake = fakeServices(), resumed: [string, number | undefined, boolean | undefined][] = [];
  const services = { ...fake.services, resume: async (batch: string, _print: (text: string) => void, parallel?: number, accept?: boolean) => { resumed.push([batch, parallel, accept]); return 0; } };
  assert.equal(await runCli([Command.resume, "batch"], services), 0);
  assert.equal(await runCli([Command.resume, "batch", Flag.parallel, "2"], services), 0);
  assert.equal(await runCli([Command.resume, "batch", Flag.acceptHarness], services), 0);
  assert.deepEqual(resumed, [["batch", undefined, false], ["batch", 2, false], ["batch", undefined, true]]);
  for (const args of [[Command.resume], [Command.resume, "batch", "other"], [Command.resume, "batch", Flag.plan, "fixture.json"], [Command.resume, "batch", Flag.parallel, "0"], [Command.resume, Flag.parallel, "2"]]) await assert.rejects(runCli(args, services));
  assert.equal(resumed.length, 3);
});

test("run --parallel caps the plan's parallel attempts and refuses anything but a positive whole number", async () => {
  const fake = fakeServices(), started: EvalConfig[] = [];
  const services = { ...fake.services, run: async (config: EvalConfig) => { started.push(config); return 0; } };
  assert.equal(await runCli([Command.run, Flag.plan, "fixture.json", Flag.parallel, "2"], services), 0);
  assert.deepEqual(started[0], { ...FixtureConfig, parallelRuns: 2 });
  for (const value of ["0", "1.5", "two"]) await assert.rejects(runCli([Command.run, Flag.plan, "fixture.json", Flag.parallel, value], services));
  assert.equal(started.length, 1);
});

test("unknown role presets and removed judge plan flags fail without loading or launching anything", async () => {
  const fake = fakeServices();
  for (const flag of [Flag.implementers, Flag.judges, Flag.user]) await assert.rejects(runCli([Command.run, Flag.plan, "fixture.json", flag, "unknown"], fake.services));
  for (const args of [[Command.run, Flag.plan, "fixture.json", Flag.user, PresetName.both],
    [Command.judge, "batch", Flag.plan, "fixture.json"], [Command.judge, "batch", Flag.judges, "unknown"],
    [Command.judge, "batch", Flag.judges, PresetName.claude, Flag.judges, PresetName.codex]]) await assert.rejects(runCli(args, fake.services));
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

test("check and judge carry the machine flags into the scripts they start", async () => {
  const fake = fakeServices(), launched: (readonly string[])[] = [];
  const services = { ...fake.services, launch: async (_file: string, args: readonly string[] = []) => { launched.push(args); return 0; } };
  const machine = ["--scratch", "work", "--check-slots", "1"];
  assert.equal(await runCli([Command.check, "batch", Flag.rebuild], services, machine), 0);
  assert.equal(await runCli([Command.judge, "batch"], services, machine), 0);
  assert.deepEqual(launched, [["batch", Flag.rebuild, ...machine], ["batch", ...machine]]);
});

test("progress shows the named batches, or every batch, and takes no flags", async () => {
  const fake = fakeServices(), asked: (readonly string[])[] = [];
  const services = { ...fake.services, progress: async (batches: readonly string[]) => { asked.push(batches); return ["fixture progress"]; } };
  assert.equal(await runCli([Command.progress], services), 0);
  assert.equal(await runCli([Command.progress, "first", "second"], services), 0);
  await assert.rejects(runCli([Command.progress, Flag.batch, "first"], services));
  assert.deepEqual(asked, [[], ["first", "second"]]);
  assert.deepEqual(fake.output, ["fixture progress", "fixture progress"]);
});
