import assert from "node:assert/strict";
import { join } from "node:path";
import { readOptions } from "../options.ts";
import { hostRuntime } from "../layout.ts";
import { parseTargets } from "../targets.ts";
import { npm, reportFailure } from "../process.ts";
import { repositoryRoot } from "../repository.ts";
import { runStage } from "../stage.ts";
import { PipelineCommand } from "./command-names.ts";
import { checkPlan, checksPlan, jobPlan, docsPlan, describeStep, type PipelineStep } from "./step-plan.ts";

try {
  const [input, ...args] = process.argv.slice(2);
  const command = Object.values(PipelineCommand).find((name) => name === input);
  assert.ok(command, `Unknown pipeline: ${input}`);
  const options = readOptions(command, args);
  if (options) {
    let steps: PipelineStep[];
    switch (command) {
      case PipelineCommand.job: {
        const [rid] = options.rid ? parseTargets(options.rid) : [];
        assert.ok(rid && options.rid === rid, "ci:job requires one --rid.");
        if (!options.plan) hostRuntime(rid);
        steps = jobPlan(rid, options);
        break;
      }
      case PipelineCommand.checks:
        steps = checksPlan(options);
        break;
      case PipelineCommand.docs:
        steps = docsPlan();
        break;
      case PipelineCommand.verify:
      case PipelineCommand.check:
      case PipelineCommand.fast:
      case PipelineCommand.delivery:
        steps = checkPlan(command);
        break;
      default: {
        const unknown: never = command;
        throw new Error(`Unknown pipeline: ${unknown}`);
      }
    }
    for (const [index, step] of steps.entries()) process.stdout.write(`${index + 1}. ${describeStep(step)}\n`);
    if (!options.plan) {
      for (const step of steps) runStage(step.name, () => npm(step.args, join(repositoryRoot, step.directory ?? "")));
    }
  }
} catch (error) {
  reportFailure(error);
}
