import assert from "node:assert/strict";

export const ReleaseDirectory = "artifacts/release";
export const CheckpointWaitSeconds = 60;
export const MillisecondsPerSecond = 1_000;
export const ReleaseTarget = { all: "all", npm: "npm", github: "github" } as const;
export type ReleaseTarget = (typeof ReleaseTarget)[keyof typeof ReleaseTarget];
export const ReleaseFile = { portableSuffix: ".tar.gz", checksums: "SHA256SUMS" } as const;

export interface ReleasePlan {
  target: ReleaseTarget;
  channel: string;
  version: string;
  sha: string;
  packages: readonly string[];
  files: readonly string[];
}

export function readReleaseTarget(value?: string): ReleaseTarget {
  const target = value || ReleaseTarget.all;
  assert.ok(target === ReleaseTarget.all || target === ReleaseTarget.npm || target === ReleaseTarget.github, "Unsupported release target.");
  return target;
}

export function checkpointSeconds(value: string = String(CheckpointWaitSeconds)): number {
  const seconds = Number(value);
  assert.ok(Number.isFinite(seconds) && seconds >= 0, "--wait must be a non-negative number of seconds.");
  return seconds;
}

export function publicationPlan(plan: ReleasePlan): string {
  return [
    "Publication plan",
    `Target: ${plan.target}`,
    `Channel: ${plan.channel}`,
    `Version: ${plan.version}`,
    `Source commit: ${plan.sha}`,
    "npm packages:",
    ...plan.packages.map((name) => `  ${name}@${plan.version}`),
    "GitHub release files:",
    ...plan.files.map((file) => `  ${file}`),
  ].join("\n");
}
