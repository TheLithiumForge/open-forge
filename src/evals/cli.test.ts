import assert from "node:assert/strict";
import { test } from "node:test";
import { runCli, type CliServices } from "./cli.ts";
import { Command, commandHelp, Flag } from "./config/commands.ts";
import { FixtureConfig, FixtureSuite } from "./config/test-fixtures.ts";
import { RunStatus } from "./history/statuses.ts";

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
  for (const args of [[Command.smoke], [Command.images], [Command.ui], [Command.recheck, "batch", Flag.rebuild], [Command.rejudge, "batch"], [Command.rejudge, "batch", Flag.plan, "plan.json"], [Command.rejudge, "batch", "run", Flag.plan, "plan.json"], [Command.export, "batch", Flag.out, "destination"]]) assert.equal(await runCli(args, fake.services), 0);
  assert.deepEqual(fake.calls, [Command.smoke, Command.images, Command.ui, "launch", "launch", "launch", "launch", Command.export]);
});
