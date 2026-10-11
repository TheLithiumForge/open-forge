import assert from "node:assert/strict";
import { isTrustedBuild, readBuildRun } from "../ci/build-runs.ts";
import { GithubState } from "../ci/github-summary.ts";
import { LatestTag } from "../npm/dist-tag.ts";

export const ReleaseChannel = { latest: LatestTag, next: "next" } as const;

/** Accepts only a finished, successful Build of this repository's exact source commit. */
export function validateBuildRun(value: unknown, repository: string, sha: string): void {
  const run = readBuildRun(value);
  assert.equal(run.repository.full_name, repository, "Build belongs to another repository.");
  assert.ok(isTrustedBuild(run, repository), "Build must come from a push or manual run of this repository's build.yml, not a pull request or fork.");
  assert.ok(run.head_sha === sha, "Build does not match the selected source commit.");
  assert.ok(run.conclusion === GithubState.success, "Build must have completed successfully.");
  assert.ok(run.status === GithubState.completed, "Build is not complete.");
}

export function releaseChannel(version: string): string {
  const prerelease = version.split("+")[0]?.split("-").slice(1).join("-");
  if (!prerelease) return ReleaseChannel.latest;
  const channel = prerelease.split(".")[0] ?? ReleaseChannel.next;
  return /^\d+$/u.test(channel) ? ReleaseChannel.next : channel;
}
