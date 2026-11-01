import assert from "node:assert/strict";
import { test } from "node:test";
import { runCli, type CliServices } from "../../cli.ts";
import { Command, Flag } from "../../config/commands.ts";
import { FixtureConfig, FixtureSuite } from "../../config/test-fixtures.ts";
import { validateCase } from "./validate.ts";
import { validationFixture } from "./validation-test-fixture.ts";

function cliServices(): CliServices {
  const noEffect = async () => { throw new Error("An unrelated command ran."); };
  return { print: () => {}, doctor: noEffect, images: noEffect, smoke: noEffect, qualify: noEffect, plan: noEffect, suite: noEffect,
    run: noEffect, ui: noEffect, launch: noEffect, export: noEffect, validateCase: noEffect };
}

test("validate-case runs a case through the product and fake verifier stages without model services", async (t) => {
  const fixture = await validationFixture(t);
  const services = { ...cliServices(), suite: async () => fixture.suite, validateCase: (suite: typeof fixture.suite) => validateCase(suite, fixture.tools) };
  assert.equal(await runCli([Command.validateCase, Flag.case, fixture.suite.id], services), 0);
  assert.equal(fixture.builds.length, 2);
});

test("validate-case all preflights plan cases, visits every case and exits nonzero on a mismatch", async (t) => {
  const fixture = await validationFixture(t), ids = [fixture.suite.id, "l4-other-fixture"], validated: string[] = [];
  const report = await validateCase(fixture.suite, fixture.tools);
  const services: CliServices = { ...cliServices(), plan: async () => ({ ...FixtureConfig, cases: ids }), suite: async (id) => ({ ...fixture.suite, id }),
    validateCase: async (suite) => { validated.push(suite.id); return { ...report, case: suite.id, passed: suite.id === ids[1] }; } };
  const args = [Command.validateCase, Flag.all, Flag.plan, "fixture-plan.json"];
  await assert.rejects(runCli(args, { ...services, suite: async (id) => {
    if (id === ids[1]) throw new Error("Missing listed case.");
    return fixture.suite;
  } }));
  assert.deepEqual([...validated], []);
  assert.equal(await runCli(args, services), 1);
  assert.deepEqual(validated, ids);
  validated.length = 0;
  assert.equal(await runCli(args, { ...services, validateCase: async (suite) => {
    validated.push(suite.id);
    if (suite.id === ids[0]) throw new Error("Invalid case expectations.");
    return report;
  } }), 1);
  assert.deepEqual(validated, ids);
});

test("validate-case rejects conflicting, missing and non-brownfield inputs before effects", async () => {
  for (const args of [[], [Flag.all], [Flag.case, "fixture", Flag.all], [Flag.case, "fixture", Flag.plan, "plan"], [Flag.plan, "plan"], [Flag.all, Flag.all, Flag.plan, "plan"]]) {
    await assert.rejects(runCli([Command.validateCase, ...args], cliServices()));
  }
  await assert.rejects(runCli([Command.validateCase, Flag.case, FixtureSuite.id], { ...cliServices(), suite: async () => FixtureSuite }));
});
