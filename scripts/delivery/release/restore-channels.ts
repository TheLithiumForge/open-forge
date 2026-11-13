import assert from "node:assert/strict";
import { npm, reportFailure } from "../process.ts";
import { repositoryRoot } from "../repository.ts";
import { registryVersionExists } from "../npm/registry-version.ts";
import { applyRestorePlan, newAliasTagCommands, newAliasTagPlan, restoreArguments, restoreChannelPlan, restoreChannels, restoreCommands } from "./restore-channel-plan.ts";
import { readPreviousGraph } from "./read-previous-graph.ts";
import { readOptions } from "../options.ts";

try {
  const values = readOptions("release:restore-channels");
  if (values) {
    assert.ok(values.version && values.to, "Supply --version <bad> and --to <previous>.");
    assert.ok(!(values.from !== undefined && values.registry), "Choose --from or --registry, not both.");
    restoreChannels(values.version, values.channels);
    const previous = readPreviousGraph(repositoryRoot, values.to, values.from, values.from === undefined && (values.registry || values.apply) === true);
    const plan = restoreChannelPlan(values.version, previous, values.channels);
    process.stdout.write(`${restoreCommands(plan).join("\n")}\n`);
    const aliasCommands = newAliasTagCommands(newAliasTagPlan(values.version, previous, values.channels));
    if (aliasCommands.length > 0) {
      process.stdout.write(
        `\nSeparate alias review, never applied by --apply:\nIf the alias was introduced by ${values.version}, check its dist-tags and remove only tags still pointing to that version.\n${aliasCommands.join("\n")}\n`,
      );
    }
    if (values.apply)
      applyRestorePlan(
        plan,
        (name, version) => registryVersionExists(repositoryRoot, name, version),
        (change) => npm(restoreArguments(change), repositoryRoot),
      );
  }
} catch (error) {
  reportFailure(error);
}
