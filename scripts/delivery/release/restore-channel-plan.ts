import assert from "node:assert/strict";
import { valid } from "semver";
import { AliasPackageName } from "../package-model.ts";
import { releaseChannel, ReleaseChannel } from "./release-selection.ts";
import { distTagArguments, removeDistTagArguments } from "../npm/dist-tag.ts";
import type { PreviousReleaseGraph } from "./previous-release-graph.ts";

export interface ChannelRestore {
  name: string;
  version: string;
  channel: string;
}

export function restoreChannels(version: string, channels?: string): string[] {
  assert.ok(valid(version), "--version must be an exact SemVer.");
  const selected = channels === undefined ? [ReleaseChannel.latest, releaseChannel(version)] : channels.split(",").map((channel) => channel.trim());
  assert.ok(selected.length > 0 && selected.every((channel) => /^[a-z][a-z0-9-]*$/u.test(channel)), "--channels must list npm tags such as latest,beta.");
  return [...new Set(selected)];
}

export function restoreChannelPlan(version: string, previous: PreviousReleaseGraph, channels?: string): ChannelRestore[] {
  assert.ok(valid(previous.version), "--to must be an exact SemVer.");
  assert.ok(previous.packages.length > 0 && new Set(previous.packages).size === previous.packages.length, "Previous package graph must be nonempty and unique.");
  const tags = restoreChannels(version, channels);
  return previous.packages.flatMap((name) => tags.map((channel) => ({ name, version: previous.version, channel })));
}

export function restoreArguments(change: ChannelRestore): string[] {
  return distTagArguments(change.name, change.version, change.channel);
}

export function restoreCommands(plan: readonly ChannelRestore[]): string[] {
  return plan.map((change) => `npm ${restoreArguments(change).join(" ")}`);
}

export function newAliasTagPlan(version: string, previous: PreviousReleaseGraph, channels?: string): ChannelRestore[] {
  if (previous.packages.includes(AliasPackageName)) return [];
  return restoreChannels(version, channels).map((channel) => ({ name: AliasPackageName, version, channel }));
}

export function newAliasTagCommands(plan: readonly ChannelRestore[]): string[] {
  return plan.map((change) => `npm ${removeDistTagArguments(change.name, change.channel).join(" ")}`);
}

export function applyRestorePlan(plan: readonly ChannelRestore[], exists: (name: string, version: string) => boolean, apply: (change: ChannelRestore) => void): void {
  for (const name of new Set(plan.map((change) => change.name))) {
    const version = plan.find((change) => change.name === name)?.version ?? assert.fail();
    assert.ok(exists(name, version), `${name}@${version} does not exist. No channels changed.`);
  }
  for (const change of plan) apply(change);
}
