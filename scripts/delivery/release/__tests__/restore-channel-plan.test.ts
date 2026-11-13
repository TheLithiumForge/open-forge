import assert from "node:assert/strict";
import { test } from "node:test";
import { applyRestorePlan, newAliasTagPlan, restoreArguments, restoreChannelPlan, restoreChannels } from "../restore-channel-plan.ts";
import { distTagArguments, DistTagCommand } from "../../npm/dist-tag.ts";
import { AliasPackageName, MainPackageName, PlatformPackages } from "../../package-model.ts";
import { AllTargets } from "../../targets.ts";
import { ReleaseChannel } from "../release-selection.ts";

const BadBeta = "0.9.0-beta.11";
const PreviousBeta = "0.9.0-beta.10";
const BadStable = "1.0.1";
const PreviousStable = "1.0.0";
const Channels = { beta: "beta", preview: "preview" } as const;
const Names = [...AllTargets.map((rid) => PlatformPackages[rid].packageName), MainPackageName, AliasPackageName];
const LegacyNames = Names.filter((name) => name !== AliasPackageName);

for (const [version, previous, channels, expected] of [
  [BadBeta, PreviousBeta, undefined, [ReleaseChannel.latest, Channels.beta]],
  [BadStable, PreviousStable, undefined, [ReleaseChannel.latest]],
  [BadBeta, PreviousBeta, Channels.preview, [Channels.preview]],
  [BadStable, PreviousStable, `${ReleaseChannel.latest},${Channels.preview}`, [ReleaseChannel.latest, Channels.preview]],
] as const) {
  test(`restore commands for ${version} with ${channels ?? "default channels"}`, () => {
    assert.deepEqual(restoreChannels(version, channels), expected);
    const plan = restoreChannelPlan(version, { version: previous, packages: Names }, channels);
    assert.deepEqual(
      plan.map(restoreArguments),
      Names.flatMap((name) => expected.map((channel) => [DistTagCommand.command, DistTagCommand.add, `${name}@${previous}`, channel])),
    );
    for (const change of plan) assert.deepEqual(restoreArguments(change), distTagArguments(change.name, change.version, change.channel));
  });
}

test("restore validates exact versions and channel lists", () => {
  assert.throws(() => restoreChannelPlan("invalid", { version: PreviousBeta, packages: Names }));
  assert.throws(() => restoreChannelPlan(BadBeta, { version: "invalid", packages: Names }));
  assert.throws(() => restoreChannels(BadBeta, ""));
  assert.throws(() => restoreChannels(BadBeta, "latest,invalid tag"));
  assert.deepEqual(restoreChannels(BadBeta, " latest,latest "), [ReleaseChannel.latest]);
});

test("first alias release restores the previous seven-package graph without requiring a prior alias version", () => {
  const previous = { version: PreviousBeta, packages: LegacyNames };
  const plan = restoreChannelPlan(BadBeta, previous);
  const checked: string[] = [];
  const applied: typeof plan = [];
  const exists = (name: string, version: string) => {
    assert.equal(version, PreviousBeta);
    checked.push(name);
    return name !== AliasPackageName;
  };
  applyRestorePlan(plan, exists, (change) => applied.push(change));
  assert.deepEqual(checked, LegacyNames);
  assert.deepEqual(applied, plan);
  assert.equal(
    applied.some((change) => change.name === AliasPackageName),
    false,
  );
  const review = newAliasTagPlan(BadBeta, previous);
  assert.deepEqual(
    review,
    restoreChannels(BadBeta).map((channel) => ({ name: AliasPackageName, version: BadBeta, channel })),
  );
  assert.deepEqual(newAliasTagPlan(BadBeta, { ...previous, packages: Names }), []);
});

test("recovery preflight refuses every mutation when any previous package is absent", () => {
  const plan = restoreChannelPlan(BadBeta, { version: PreviousBeta, packages: LegacyNames });
  const applied: typeof plan = [];
  assert.throws(() =>
    applyRestorePlan(
      plan,
      (name) => name !== MainPackageName,
      (change) => applied.push(change),
    ),
  );
  assert.deepEqual(applied, []);
});
