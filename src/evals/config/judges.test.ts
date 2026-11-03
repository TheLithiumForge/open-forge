import assert from "node:assert/strict";
import { test } from "node:test";
import { parseConfig } from "./config.ts";
import { configuredJudges, modelKey, readJudges } from "./judges.ts";
import { MatrixAgents, MatrixJudges } from "./matrix.ts";
import { Checking, Judging } from "./choices.ts";
import { CodexTeams } from "./presets.ts";
import { modelChoice } from "./validation.ts";
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

test("a plan can leave its checks for later only when it judges later too, since the judges read the checks' screenshots", () => {
  const plan = { schemaVersion: PlanVersion, layer: 4, cases: ["l4-r02"], agents: MatrixAgents, judges: MatrixJudges,
    simulatedUser: LocalModel, isolation: FixtureConfig.isolation, setups: FixtureConfig.setups, runsPerSetup: 1, judging: Judging.later };
  const later = resolvePlan({ ...plan, checking: Checking.later }, FixtureConfig);
  assert.equal(later.checking, Checking.later);
  assert.equal(parseConfig(later).checking, Checking.later);
  assert.equal(resolvePlan(plan, FixtureConfig).checking, undefined);
  assert.throws(() => resolvePlan({ ...plan, checking: Checking.later, judging: Judging.run }, FixtureConfig));
  assert.throws(() => resolvePlan({ ...plan, checking: "never" }, FixtureConfig));
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

test("a chain plan names its chain and milestone for one Layer 4 case, and other shapes are refused", () => {
  const plan = { schemaVersion: PlanVersion, layer: 4, cases: ["l4-r03"], agents: MatrixAgents, judges: MatrixJudges,
    simulatedUser: LocalModel, isolation: FixtureConfig.isolation, setups: FixtureConfig.setups, runsPerSetup: 1, chain: { id: "l4-2", milestone: 2 } };
  const chained = resolvePlan(plan, FixtureConfig);
  assert.deepEqual(chained.chain, { id: "l4-2", milestone: 2 });
  assert.deepEqual(parseConfig({ ...chained, chain: { ...plan.chain, parentBatchId: "parent-batch" } }).chain, { ...plan.chain, parentBatchId: "parent-batch" });
  for (const chain of [{ id: "l4-2", milestone: 1 }, { id: "l4 2", milestone: 2 }]) assert.throws(() => resolvePlan({ ...plan, chain }, FixtureConfig));
  assert.throws(() => resolvePlan({ ...plan, cases: ["l4-r03", "l4-r04"] }, FixtureConfig));
});

test("a team is part of the implementer, so solo and team runs plan and count apart, and only Codex leads one", () => {
  const [solo, sol, luna] = CodexTeams;
  assert.deepEqual(modelChoice(JSON.parse(JSON.stringify(luna))), luna);
  assert.equal(new Set(CodexTeams.map(modelKey)).size, 3);
  assert.equal(modelKey(solo), modelKey({ tool: solo.tool, model: solo.model, effort: solo.effort }), "A solo key stays what it was before teams existed.");
  assert.throws(() => modelChoice({ tool: "claude", model: "opus", effort: "high", team: { model: "opus", effort: "high" } }));
  const runs = planBatchRuns({ ...FixtureConfig, agents: [solo, sol], setups: ["plain-docs"], runsPerSetup: 1 }, "batch");
  assert.deepEqual(new Set(runs.map((run) => modelKey(run.agent))), new Set([modelKey(solo), modelKey(sol)]));
});
