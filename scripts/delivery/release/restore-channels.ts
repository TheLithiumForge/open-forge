import assert from "node:assert/strict";
import { npm, reportFailure } from "../process.ts";
import { repositoryRoot } from "../repository.ts";
import { registryVersionExists } from "../npm/registry-version.ts";
import { applyRestorePlan, restoreArguments, restoreChannelPlan, restoreCommands } from "./restore-channel-plan.ts";
import { readOptions } from "../options.ts";

try {
  const values = readOptions("release:restore-channels");
  if (values) {
    assert.ok(values.version && values.to, "Supply --version <bad> and --to <previous>.");
    const plan = restoreChannelPlan(values.version, values.to, values.channels);
    process.stdout.write(`${restoreCommands(plan).join("\n")}\n`);
    if (values.apply) {
      const skipped = applyRestorePlan(
        plan,
        (name, version) => registryVersionExists(repositoryRoot, name, version),
        (change) => npm(restoreArguments(change), repositoryRoot),
      );
      for (const name of skipped) process.stdout.write(`Skipped ${name}: ${name}@${values.to} does not exist. Check its tags with npm view ${name} dist-tags.\n`);
    }
  }
} catch (error) {
  reportFailure(error);
}
