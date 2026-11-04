import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { resolvePlan } from "../config/plans.ts";
import { FixtureConfig } from "../config/test-fixtures.ts";
import { Checking, Isolation, Judging } from "../config/choices.ts";
import { ImplementerPresets, JudgePresets, UserPresets, PresetName } from "../config/presets.ts";
import { loadSuite } from "../benchmarks/suite.ts";
import { DocumentCase } from "../materialize/delivery.ts";
import { evalsRoot } from "../paths.ts";

const FullAttempts = 3;
const FullParallel = 2;
const PlanCases = ["greenfield-baseline", DocumentCase.two, DocumentCase.three];

test("the saved check and full plans freeze their models, limits and complete case cells", async () => {
  for (const [index, id] of PlanCases.entries()) {
    const suite = await loadSuite(id);
    for (const variant of ["check", "full"]) {
      const plan = resolvePlan(JSON.parse(await readFile(join(evalsRoot, "experiments", `neon-pairs-l${index + 1}-${variant}.json`), "utf8")), FixtureConfig);
      assert.equal(plan.suite, suite.id);
      assert.equal(plan.layer, index + 1);
      assert.deepEqual(plan.setups, Object.keys(suite.setups).filter((setup) => setup !== "helpful"));
      assert.equal(plan.runsPerSetup, variant === "check" ? 1 : FullAttempts);
      assert.equal(plan.parallelRuns, variant === "check" ? plan.setups.length : FullParallel);
      assert.deepEqual(plan.agents, ImplementerPresets[PresetName.claude]);
      assert.deepEqual(plan.judges, JudgePresets[PresetName.claude]);
      assert.deepEqual(plan.judge, JudgePresets[PresetName.claude][0]);
      assert.deepEqual(plan.simulatedUser, UserPresets[PresetName.claude]);
      assert.equal(plan.isolation, Isolation.docker);
      if (index > 0) assert.equal(plan.limits.runMinutes, suite.runMinutes);
    }
  }
});

test("the Sol full-spec plan builds twice per scenario with Sol alone and leading Sol or Luna subagents, and leaves checks and judges for later", async () => {
  const suite = await loadSuite(DocumentCase.three);
  const plan = resolvePlan(JSON.parse(await readFile(join(evalsRoot, "experiments", "neon-pairs-l3-sol.json"), "utf8")), FixtureConfig);
  assert.equal(plan.suite, suite.id);
  assert.deepEqual(plan.setups, Object.keys(suite.setups).filter((setup) => setup !== "helpful"));
  assert.equal(plan.runsPerSetup, 2);
  assert.deepEqual(plan.agents, ImplementerPresets[PresetName.codexAll]);
  assert.deepEqual(plan.simulatedUser, UserPresets[PresetName.codex]);
  assert.equal(plan.judging, Judging.later);
  assert.equal(plan.checking, Checking.later);
  assert.equal(plan.limits.runMinutes, suite.runMinutes);
});
