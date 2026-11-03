import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { resolvePlan } from "../config/plans.ts";
import { FixtureConfig } from "../config/test-fixtures.ts";
import { LayerFourCases, CodeOnlySetup, FourBugsPacket } from "../materialize/brownfield-case.ts";
import { DocumentSetups, OverloadSetups } from "../materialize/delivery.ts";
import { Checking, Judging, WorkMode } from "../config/choices.ts";
import { BrownfieldSetups } from "../materialize/brownfield.ts";
import { evalsRoot } from "../paths.ts";
import { loadSuite } from "../benchmarks/suite.ts";
import { runCli, type CliServices } from "../cli.ts";
import { Command, Flag } from "../config/commands.ts";
import { FixtureSuite } from "../config/test-fixtures.ts";
import { CodexOrchestrators, ImplementerPresets, JudgePresets, UserPresets, PresetName } from "../config/presets.ts";

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

const P1Scenarios = [DocumentSetups.organized, CodeOnlySetup, DocumentSetups.plain, OverloadSetups.docs, DocumentSetups.fullCore, DocumentSetups.fullCoreScoped, OverloadSetups.memory];
const plan = async (name: string) => resolvePlan(JSON.parse(await readFile(join(evalsRoot, "experiments", `neon-pairs-${name}.json`), "utf8")), FixtureConfig);

test("P1 plans hand the four bug fixes over in each work mode, on every scenario with Organized docs first, checking and judging later", async () => {
  for (const [mode, agents] of [[WorkMode.wholePacket, ImplementerPresets[PresetName.codex]], [WorkMode.taskByTask, ImplementerPresets[PresetName.codex]], [WorkMode.orchestrated, CodexOrchestrators]] as const) {
    const resolved = await plan(`p1-${mode}`);
    assert.equal(resolved.suite, FourBugsPacket);
    assert.equal(resolved.packet, FourBugsPacket);
    assert.equal(resolved.workMode, mode);
    assert.equal(resolved.cases, undefined);
    assert.deepEqual(resolved.setups, P1Scenarios);
    assert.ok(resolved.setups.every((setup) => setup in BrownfieldSetups));
    assert.equal(resolved.runsPerSetup, 1);
    assert.deepEqual(resolved.agents, agents);
    assert.deepEqual(resolved.simulatedUser, UserPresets[PresetName.codex]);
    assert.equal(resolved.judging, Judging.later);
    assert.equal(resolved.checking, Checking.later);
  }
  assert.equal((await loadSuite(FourBugsPacket)).runMinutes, 90);
});

test("the P1 smoke hands the whole packet to one agent with Organized docs and checks inline, since it proves grading works", async () => {
  const smoke = await plan("p1-smoke");
  assert.equal(smoke.workMode, WorkMode.wholePacket);
  assert.deepEqual(smoke.setups, [DocumentSetups.organized]);
  assert.equal(smoke.checking, undefined);
});

test("a packet plan matches its implementers to its work mode and names no cases or chain", async () => {
  const whole = JSON.parse(await readFile(join(evalsRoot, "experiments", "neon-pairs-p1-whole-packet.json"), "utf8"));
  assert.throws(() => resolvePlan({ ...whole, agents: CodexOrchestrators }, FixtureConfig));
  assert.throws(() => resolvePlan({ ...whole, workMode: WorkMode.orchestrated }, FixtureConfig));
  assert.throws(() => resolvePlan({ ...whole, cases: [FourBugsPacket] }, FixtureConfig));
  assert.throws(() => resolvePlan({ ...whole, workMode: undefined }, FixtureConfig));
});
