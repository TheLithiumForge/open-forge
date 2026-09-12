import { readOptions } from "./options.ts";
import { hostRuntime } from "./layout.ts";
import { npm, reportFailure } from "./process.ts";
import { repositoryRoot } from "./repository.ts";
import { distPlan } from "./dist-plan.ts";
import { runStage } from "./stage.ts";

try {
  const options = readOptions("dist");
  if (options) {
    const rid = hostRuntime(options.rid);
    const stages = distPlan(rid, options);
    process.stdout.write(`Native target: ${rid}\n`);
    for (const [index, stage] of stages.entries())
      process.stdout.write(`${index + 1}. ${stage.name}${stage.skipped ? " [SKIPPED]" : ""}: npm run ${stage.script} -- ${stage.args.join(" ")}\n`);
    if (!options.plan) {
      for (const stage of stages) if (!stage.skipped) runStage(stage.name, () => npm(["run", stage.script, "--", ...stage.args], repositoryRoot));
    }
  }
} catch (error) {
  reportFailure(error);
}
