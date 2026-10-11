import assert from "node:assert/strict";
import { parseArgs } from "node:util";
import { npm, reportFailure } from "../process.ts";
import { repositoryRoot } from "../repository.ts";
import { registryVersionExists } from "../npm/registry-version.ts";
import { restoreArguments, restoreChannelPlan, restoreCommands } from "./restore-channel-plan.ts";

try {
  const { values } = parseArgs({ options: { version: { type: "string" }, to: { type: "string" }, channels: { type: "string" }, apply: { type: "boolean" } } });
  assert.ok(values.version && values.to, "Supply --version <bad> and --to <previous>.");
  const plan = restoreChannelPlan(values.version, values.to, values.channels);
  process.stdout.write(`${restoreCommands(plan).join("\n")}\n`);
  if (values.apply) {
    for (const name of new Set(plan.map((change) => change.name))) {
      assert.ok(registryVersionExists(repositoryRoot, name, values.to), `${name}@${values.to} does not exist. No channels changed.`);
    }
    for (const change of plan) npm(restoreArguments(change), repositoryRoot);
  }
} catch (error) {
  reportFailure(error);
}
