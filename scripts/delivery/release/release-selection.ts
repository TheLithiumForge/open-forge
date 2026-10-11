import assert from "node:assert/strict";
import { BuildWorkflow } from "../ci/build-runs.ts";
import { GithubState } from "../ci/github-summary.ts";
import { LatestTag } from "../npm/dist-tag.ts";

export const ReleaseChannel = { latest: LatestTag, next: "next" } as const;

export function validateBuildRun(value: unknown, repository: string, sha: string): void {
  assert.ok(typeof value === "object" && value !== null);
  assert.ok("repository" in value && typeof value.repository === "object" && value.repository !== null && "full_name" in value.repository);
  assert.equal(value.repository.full_name, repository, "Build belongs to another repository.");
  assert.ok("head_sha" in value && value.head_sha === sha, "Build does not match the selected source commit.");
  assert.ok("conclusion" in value && value.conclusion === GithubState.success, "Build must have completed successfully.");
  assert.ok("status" in value && value.status === GithubState.completed, "Build is not complete.");
  assert.ok("path" in value && value.path === BuildWorkflow.path, "Run must belong to build.yml.");
}

export function releaseChannel(version: string): string {
  const prerelease = version.split("+")[0]?.split("-").slice(1).join("-");
  if (!prerelease) return ReleaseChannel.latest;
  const channel = prerelease.split(".")[0] ?? ReleaseChannel.next;
  return /^\d+$/u.test(channel) ? ReleaseChannel.next : channel;
}
