import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { resolvePlan } from "../config/plans.ts";
import { FixtureConfig } from "../config/test-fixtures.ts";
import { LayerFourCases, CodeOnlySetup } from "../materialize/brownfield-case.ts";
import { BrownfieldSetups } from "../materialize/brownfield.ts";
import { evalsRoot } from "../paths.ts";
import { loadSuite } from "../benchmarks/suite.ts";
import { runCli, type CliServices } from "../cli.ts";
import { Command, Flag } from "../config/commands.ts";
import { FixtureSuite } from "../config/test-fixtures.ts";
import { PlanAgents, PlanJudges } from "../config/matrix.ts";

test("Layer 4 plans list all cases and setups with frozen models and independent case batches", async () => {
  for (const variant of ["check", "full"]) {
    const plan = resolvePlan(JSON.parse(await readFile(join(evalsRoot, "experiments", `neon-pairs-l4-${variant}.json`), "utf8")), FixtureConfig);
    assert.deepEqual(plan.cases, [...LayerFourCases]);
    assert.deepEqual(plan.setups, Object.keys(BrownfieldSetups));
    assert.ok(plan.setups.includes(CodeOnlySetup));
    assert.equal(plan.runsPerSetup, variant === "check" ? 1 : 3);
    assert.deepEqual(plan.agents, PlanAgents);
    assert.deepEqual(plan.judges, PlanJudges);
    assert.deepEqual(plan.judge, PlanJudges[0]);
    assert.deepEqual(plan.simulatedUser, FixtureConfig.simulatedUser);
    assert.equal(plan.isolation, "docker");
  }
  await assert.rejects(loadSuite("l4-missing-fixture"));
});

test("the product preflights every named case before installing any setup", async () => {
  const cases = [LayerFourCases[0], LayerFourCases[1]], qualified: string[] = [];
  const noEffect = async () => { throw new Error("An unrelated product command ran."); };
  const services: CliServices = { print: () => {}, doctor: noEffect, images: noEffect, smoke: noEffect, run: noEffect, ui: noEffect, launch: noEffect, export: noEffect, validateCase: noEffect,
    plan: async () => ({ ...FixtureConfig, cases }), suite: async (id) => {
      if (id === cases[1]) throw new Error("The listed case is missing.");
      return { ...FixtureSuite, id };
    }, qualify: async (suite) => { qualified.push(suite.id); return []; } };
  await assert.rejects(runCli([Command.qualify, Flag.plan, "fixture.json"], services));
  assert.deepEqual(qualified, []);
  await runCli([Command.qualify, Flag.plan, "fixture.json"], { ...services, suite: async (id) => ({ ...FixtureSuite, id }) });
  assert.deepEqual(qualified, cases);
});
