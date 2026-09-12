import assert from "node:assert/strict";

export function validateBuildRun(value: unknown, repository: string, sha: string): void {
  assert.ok(typeof value === "object" && value !== null);
  assert.ok("repository" in value && typeof value.repository === "object" && value.repository !== null && "full_name" in value.repository);
  assert.equal(value.repository.full_name, repository, "Build belongs to another repository.");
  assert.ok("head_sha" in value && value.head_sha === sha, "Build does not match the selected source commit.");
  assert.ok("conclusion" in value && value.conclusion === "success", "Build must have completed successfully.");
  assert.ok("status" in value && value.status === "completed", "Build is not complete.");
  assert.ok("path" in value && value.path === ".github/workflows/build.yml", "Run must belong to build.yml.");
}

export function releaseChannel(version: string): string {
  const prerelease = version.split("+")[0]?.split("-").slice(1).join("-");
  if (!prerelease) return "latest";
  const channel = prerelease.split(".")[0] ?? "next";
  return /^\d+$/u.test(channel) ? "next" : channel;
}
