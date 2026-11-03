import assert from "node:assert/strict";
import { test } from "node:test";
import { parseConfig } from "./config.ts";
import { configuredJudges, modelKey, readJudges } from "./judges.ts";
import { MatrixAgents, MatrixJudges } from "./matrix.ts";
import { Judging } from "./choices.ts";
import { LocalModel, PlanVersion, resolvePlan } from "./plans.ts";
import { FixtureConfig } from "./test-fixtures.ts";
import { planBatchRuns } from "../execution/batch.ts";
import { randomUUID } from "node:crypto";

test("judge lists keep their order and old single-judge plans override a current matrix", () => {
  const current = { ...FixtureConfig, judge: MatrixJudges[0], judges: MatrixJudges };
  const plan = { schemaVersion: PlanVersion, layer: 1, case: current.suite, agents: MatrixAgents, judges: MatrixJudges,
    simulatedUser: LocalModel, isolation: current.isolation, setups: current.setups, runsPerSetup: 1 };
  const resolved = resolvePlan(plan, current);
  assert.deepEqual(configuredJudges(resolved), MatrixJudges);
  assert.deepEqual(parseConfig(resolved), resolved);
  const { judges: _judges, ...single } = plan;
  const older = resolvePlan({ ...single, judge: LocalModel }, current);
  assert.deepEqual(configuredJudges(older), [current.judge]);
  assert.equal(older.judges, undefined);
  assert.throws(() => resolvePlan({ ...plan, judge: current.judge }, current));
});

test("a plan can leave judging for later, keep its judges for judge --missing and refuse other timings", () => {
  const plan = { schemaVersion: PlanVersion, layer: 4, cases: ["l4-r02"], agents: MatrixAgents, judges: MatrixJudges,
    simulatedUser: LocalModel, isolation: FixtureConfig.isolation, setups: FixtureConfig.setups, runsPerSetup: 1 };
  const later = resolvePlan({ ...plan, judging: Judging.later }, FixtureConfig);
  assert.equal(later.judging, Judging.later);
  assert.deepEqual(configuredJudges(later), MatrixJudges);
  assert.equal(parseConfig(later).judging, Judging.later);
  assert.equal(resolvePlan(plan, FixtureConfig).judging, undefined);
  assert.throws(() => resolvePlan({ ...plan, judging: "never" }, FixtureConfig));
});

test("empty, duplicate, conflicting and malformed judges are refused", () => {
  assert.throws(() => readJudges(undefined, []));
  assert.throws(() => readJudges(undefined, [MatrixJudges[0], MatrixJudges[0]]));
  assert.throws(() => readJudges(MatrixJudges[1], MatrixJudges));
  for (const repeats of [0, -1, 1.5]) assert.throws(() => readJudges(undefined, [{ ...MatrixJudges[0], repeats }]));
});

test("both implementers get each cell and repeat once regardless of the judge count", () => {
  const config = { ...FixtureConfig, agents: MatrixAgents, judges: MatrixJudges, judge: MatrixJudges[0], runsPerSetup: 3 };
  const batchId = randomUUID(), runs = planBatchRuns(config, batchId);
  assert.equal(runs.length, config.agents.length * config.setups.length * config.runsPerSetup);
  assert.equal(new Set(runs.map((run) => run.id)).size, runs.length);
  for (const agent of config.agents) for (const setup of config.setups) for (let repeat = 1; repeat <= config.runsPerSetup; repeat += 1) {
    assert.equal(runs.filter((run) => modelKey(run.agent) === modelKey(agent) && run.setup === setup && run.repeat === repeat).length, 1);
  }
});
