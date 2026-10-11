import assert from "node:assert/strict";
import { test } from "node:test";
import { restoreArguments, restoreChannelPlan, restoreChannels, restoreCommands } from "../restore-channel-plan.ts";
import { distTagArguments, DistTagCommand } from "../../npm/dist-tag.ts";
import { MainPackageName, PlatformPackages } from "../../package-model.ts";
import { AllTargets } from "../../targets.ts";
import { ReleaseChannel } from "../release-selection.ts";

const BadBeta = "0.9.0-beta.11";
const PreviousBeta = "0.9.0-beta.10";
const BadStable = "1.0.1";
const PreviousStable = "1.0.0";
const Channels = { beta: "beta", preview: "preview" } as const;
const Names = [...AllTargets.map((rid) => PlatformPackages[rid].packageName), MainPackageName];

for (const [version, previous, channels, expected] of [
  [BadBeta, PreviousBeta, undefined, [ReleaseChannel.latest, Channels.beta]],
  [BadStable, PreviousStable, undefined, [ReleaseChannel.latest]],
  [BadBeta, PreviousBeta, Channels.preview, [Channels.preview]],
  [BadStable, PreviousStable, `${ReleaseChannel.latest},${Channels.preview}`, [ReleaseChannel.latest, Channels.preview]],
] as const) {
  test(`restore commands for ${version} with ${channels ?? "default channels"}`, () => {
    assert.deepEqual(restoreChannels(version, channels), expected);
    const plan = restoreChannelPlan(version, previous, channels);
    assert.deepEqual(
      restoreCommands(plan),
      Names.flatMap((name) => expected.map((channel) => `npm dist-tag add ${name}@${previous} ${channel}`)),
    );
    assert.deepEqual(
      plan.map(restoreArguments),
      Names.flatMap((name) => expected.map((channel) => [DistTagCommand.command, DistTagCommand.add, `${name}@${previous}`, channel])),
    );
    for (const change of plan) assert.deepEqual(restoreArguments(change), distTagArguments(change.name, change.version, change.channel));
  });
}

test("restore validates exact versions and channel lists", () => {
  assert.throws(() => restoreChannelPlan("invalid", PreviousBeta));
  assert.throws(() => restoreChannelPlan(BadBeta, "invalid"));
  assert.throws(() => restoreChannels(BadBeta, ""));
  assert.throws(() => restoreChannels(BadBeta, "latest,invalid tag"));
  assert.deepEqual(restoreChannels(BadBeta, " latest,latest "), [ReleaseChannel.latest]);
});
