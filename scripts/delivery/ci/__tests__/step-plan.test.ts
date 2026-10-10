import assert from "node:assert/strict";
import { test } from "node:test";
import { checkPlan, checksPlan, jobPlan, docsPlan, describeStep, WorkflowLogStage } from "../step-plan.ts";
import { PipelineCommand, LeafCommand, CiCommand, PipelineFlag, NpmCommand } from "../command-names.ts";
import { DocumentationRoot, deliveryLog } from "../../layout.ts";

const Rid = "win-x64";

test("composite checks preserve the existing leaf order", () => {
  assert.deepEqual(
    checkPlan(PipelineCommand.verify).map((step) => step.name),
    [PipelineCommand.delivery, LeafCommand.deliveryTests, LeafCommand.packageLayout, LeafCommand.dotnet, LeafCommand.docs],
  );
  assert.deepEqual(
    checkPlan(PipelineCommand.check).map((step) => step.name),
    [LeafCommand.format, LeafCommand.typecheck, LeafCommand.lint],
  );
  assert.deepEqual(
    checkPlan(PipelineCommand.fast).map((step) => step.name),
    [LeafCommand.typecheck, LeafCommand.lint],
  );
  assert.deepEqual(
    checkPlan(PipelineCommand.delivery).map((step) => step.name),
    [LeafCommand.deliveryTypes, LeafCommand.deliveryLint, LeafCommand.deliveryFormat],
  );
  assert.throws(() => checkPlan(PipelineCommand.docs), /dedicated plan/u);
});

test("shared CI checks run setup before verify and support cached setup", () => {
  assert.deepEqual(
    checksPlan().map((step) => step.name),
    [LeafCommand.setup, PipelineCommand.verify],
  );
  assert.ok(checksPlan({ offline: true })[0]?.args.includes(PipelineFlag.offline));
});

test("matrix job preserves stage order, RID and workflow log paths", () => {
  const steps = jobPlan(Rid);
  assert.deepEqual(
    steps.map((step) => step.name),
    [LeafCommand.setup, LeafCommand.dist, LeafCommand.native, CiCommand.bundle, LeafCommand.testBuilt, LeafCommand.pack],
  );
  for (const [step, stage] of steps.slice(2).map((step, index) => [step, Object.values(WorkflowLogStage)[index]] as const)) {
    assert.ok(stage);
    assert.deepEqual(step.args.slice(-2), [PipelineFlag.log, deliveryLog(Rid, stage)]);
    assert.ok(step.args.includes(Rid));
  }
  assert.ok(steps[1]?.args.includes(PipelineFlag.plan));
  assert.ok(describeStep(steps[0] ?? assert.fail()).includes(`npm ${NpmCommand.run} ${LeafCommand.setup}`));
});

test("matrix restore options propagate without redundant setup for no-restore", () => {
  const reused = jobPlan(Rid, { "no-restore": true });
  assert.equal(reused[0]?.name, LeafCommand.dist);
  assert.ok(reused[1]?.args.includes(PipelineFlag.noRestore));
  const offline = jobPlan(Rid, { offline: true });
  assert.ok(offline[0]?.args.includes(PipelineFlag.offline));
  assert.ok(offline[1]?.args.includes(PipelineFlag.offline));
  assert.ok(offline[2]?.args.includes(PipelineFlag.offline));
  assert.throws(() => jobPlan(Rid, { offline: true, "no-restore": true }));
});

test("documentation stages stay in the site project", () => {
  const steps = docsPlan();
  assert.deepEqual(
    steps.map((step) => step.args),
    [[NpmCommand.install], [NpmCommand.run, NpmCommand.typecheck], [NpmCommand.run, NpmCommand.build]],
  );
  assert.ok(steps.every((step) => step.directory === DocumentationRoot));
  assert.ok(describeStep(steps[0] ?? assert.fail()).includes(DocumentationRoot));
});
