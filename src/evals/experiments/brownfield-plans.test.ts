import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { resolvePlan } from "../config/plans.ts";
import { FixtureConfig } from "../config/test-fixtures.ts";
import { LayerFourCases, CodeOnlySetup, ScopedTaskCases } from "../materialize/brownfield-case.ts";
import { DocumentSetups } from "../materialize/delivery.ts";
import { Judging } from "../config/choices.ts";
import { BrownfieldSetups } from "../materialize/brownfield.ts";
import { evalsRoot } from "../paths.ts";
import { loadSuite } from "../benchmarks/suite.ts";
import { runCli, type CliServices } from "../cli.ts";
import { Command, Flag } from "../config/commands.ts";
import { FixtureSuite } from "../config/test-fixtures.ts";
import { ImplementerPresets, JudgePresets, UserPresets, PresetName } from "../config/presets.ts";

test("Layer 4 plans list all cases and setups with frozen models and independent case batches", async () => {
  for (const variant of ["check", "full"]) {
    const plan = resolvePlan(JSON.parse(await readFile(join(evalsRoot, "experiments", `neon-pairs-l4-${variant}.json`), "utf8")), FixtureConfig);
    assert.deepEqual(plan.cases, [...LayerFourCases]);
    assert.deepEqual(plan.setups, Object.keys(BrownfieldSetups));
    assert.ok(plan.setups.includes(CodeOnlySetup));
    assert.equal(plan.runsPerSetup, variant === "check" ? 1 : 3);
    assert.deepEqual(plan.agents, ImplementerPresets[PresetName.claude]);
    assert.deepEqual(plan.judges, JudgePresets[PresetName.claude]);
    assert.deepEqual(plan.judge, JudgePresets[PresetName.claude][0]);
    assert.deepEqual(plan.simulatedUser, UserPresets[PresetName.claude]);
    assert.equal(plan.isolation, "docker");
  }
  await assert.rejects(loadSuite("l4-missing-fixture"));
});

test("L4-1 plans run the scoped tasks with Sol on the four context steps, Plain docs first, and judge later", async () => {
  for (const variant of ["smoke", "full"]) {
    const plan = resolvePlan(JSON.parse(await readFile(join(evalsRoot, "experiments", `neon-pairs-l4-1-${variant}.json`), "utf8")), FixtureConfig);
    assert.deepEqual(plan.cases, variant === "smoke" ? [ScopedTaskCases[0]] : [...ScopedTaskCases]);
    assert.deepEqual(plan.setups, [DocumentSetups.plain, CodeOnlySetup, DocumentSetups.fullCore, DocumentSetups.fullCoreScoped]);
    assert.ok(plan.setups.every((setup) => setup in BrownfieldSetups));
    assert.equal(plan.runsPerSetup, variant === "smoke" ? 1 : 3);
    assert.deepEqual(plan.agents, ImplementerPresets[PresetName.codex]);
    assert.deepEqual(plan.simulatedUser, UserPresets[PresetName.codex]);
    assert.equal(plan.judging, Judging.later);
    for (const id of plan.cases ?? []) assert.ok((await loadSuite(id)).brownfield !== undefined, id);
  }
});

test("the product preflights every named case before installing any setup", async () => {
  const cases = [LayerFourCases[0], LayerFourCases[1]], qualified: string[] = [];
  const noEffect = async () => { throw new Error("An unrelated product command ran."); };
  const services: CliServices = { print: () => {}, doctor: noEffect, images: noEffect, smoke: noEffect, run: noEffect, site: noEffect, launch: noEffect, results: noEffect, validateCase: noEffect,
    plan: async () => ({ ...FixtureConfig, cases }), suite: async (id) => {
      if (id === cases[1]) throw new Error("The listed case is missing.");
      return { ...FixtureSuite, id };
    }, qualify: async (suite) => { qualified.push(suite.id); return []; } };
  await assert.rejects(runCli([Command.qualify, Flag.plan, "fixture.json"], services));
  assert.deepEqual(qualified, []);
  await runCli([Command.qualify, Flag.plan, "fixture.json"], { ...services, suite: async (id) => ({ ...FixtureSuite, id }) });
  assert.deepEqual(qualified, cases);
});
