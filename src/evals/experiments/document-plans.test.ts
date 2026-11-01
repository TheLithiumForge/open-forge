import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { resolvePlan } from "../config/plans.ts";
import { FixtureConfig } from "../config/test-fixtures.ts";
import { Isolation } from "../config/choices.ts";
import { MatrixAgents, MatrixJudges } from "../config/matrix.ts";
import { loadSuite } from "../benchmarks/suite.ts";
import { CaseTimeLimitMinutes, DocumentCase } from "../materialize/delivery.ts";
import { evalsRoot } from "../paths.ts";

const FullAttempts = 3;
const FullParallel = 5;
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
      assert.deepEqual(plan.agents, MatrixAgents);
      assert.deepEqual(plan.judges, MatrixJudges);
      assert.deepEqual(plan.judge, MatrixJudges[0]);
      assert.deepEqual(plan.simulatedUser, FixtureConfig.simulatedUser);
      assert.equal(plan.isolation, Isolation.docker);
      if (index > 0) assert.equal(plan.limits.runMinutes, CaseTimeLimitMinutes);
    }
  }
});
