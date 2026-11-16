import { readOptions } from "./options.ts";
import { hostRuntime } from "./layout.ts";
import { npm, reportFailure } from "./process.ts";
import { repositoryRoot } from "./repository.ts";
import { distPlan, distPlanLines } from "./dist-plan.ts";
import { runStage } from "./stage.ts";

try {
  const options = readOptions("dist");
  if (options) {
    const rid = hostRuntime(options.rid);
    const stages = distPlan(rid, options);
    for (const line of distPlanLines(rid, stages)) process.stdout.write(`${line}\n`);
    if (!options.plan) {
      for (const stage of stages) if (!stage.skipped) runStage(stage.name, () => npm(["run", stage.script, "--", ...stage.args], repositoryRoot));
    }
  }
} catch (error) {
  reportFailure(error);
}
