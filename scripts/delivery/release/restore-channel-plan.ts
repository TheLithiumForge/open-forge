import assert from "node:assert/strict";
import { valid } from "semver";
import { MainPackageName, PlatformPackages } from "../package-model.ts";
import { AllTargets } from "../targets.ts";
import { releaseChannel, ReleaseChannel } from "./release-selection.ts";
import { distTagArguments } from "../npm/dist-tag.ts";

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

export function restoreChannelPlan(version: string, previous: string, channels?: string): ChannelRestore[] {
  assert.ok(valid(previous), "--to must be an exact SemVer.");
  const tags = restoreChannels(version, channels);
  const names = [...AllTargets.map((rid) => PlatformPackages[rid].packageName), MainPackageName];
  return names.flatMap((name) => tags.map((channel) => ({ name, version: previous, channel })));
}

export function restoreArguments(change: ChannelRestore): string[] {
  return distTagArguments(change.name, change.version, change.channel);
}

export function restoreCommands(plan: readonly ChannelRestore[]): string[] {
  return plan.map((change) => `npm ${restoreArguments(change).join(" ")}`);
}
