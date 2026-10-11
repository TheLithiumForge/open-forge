import assert from "node:assert/strict";

export const BuildWorkflow = { file: "build.yml", path: ".github/workflows/build.yml", artifactPrefix: "package-" } as const;

/**
 * Events whose Build compiles the exact head commit with this repository's
 * workflow. A pull_request run builds a merge commit, and a fork can change the
 * workflow, so neither may stand in for a commit's own Build.
 */
export const TrustedBuildEvents: readonly string[] = ["push", "workflow_dispatch"];

export interface BuildRun {
  id: number;
  repository: { full_name: string };
  head_repository: { full_name: string };
  head_sha: string;
  head_branch: string;
  event: string;
  path: string;
  status: string;
  conclusion: string | null;
  created_at: string;
}

export interface BuildArtifact {
  name: string;
  expired: boolean;
}

function record(value: unknown): Record<string, unknown> {
  assert.ok(typeof value === "object" && value !== null && !Array.isArray(value), "Invalid Build API response.");
  return Object.fromEntries(Object.entries(value));
}

// A deleted fork leaves head_repository null. An empty name never matches this repository.
function repositoryName(value: unknown): string {
  if (value === null || value === undefined) return "";
  const name = record(value)["full_name"];
  assert.ok(typeof name === "string", "Invalid Build repository.");
  return name;
}

export function readBuildRun(value: unknown): BuildRun {
  const { id, repository, head_repository, head_sha, head_branch, event, path, status, conclusion, created_at } = record(value);
  assert.ok(typeof id === "number" && Number.isSafeInteger(id) && id > 0);
  assert.ok(typeof head_sha === "string" && typeof head_branch === "string" && typeof event === "string");
  assert.ok(typeof path === "string" && typeof status === "string");
  assert.ok(conclusion === null || typeof conclusion === "string");
  assert.ok(typeof created_at === "string" && Number.isFinite(Date.parse(created_at)));
  return {
    id,
    repository: { full_name: repositoryName(repository) },
    head_repository: { full_name: repositoryName(head_repository) },
    head_sha,
    head_branch,
    event,
    path,
    status,
    conclusion,
    created_at,
  };
}

/** True when the run built this repository's own commit with this repository's Build workflow. */
export function isTrustedBuild(run: BuildRun, repository: string): boolean {
  return TrustedBuildEvents.includes(run.event) && run.repository.full_name === repository && run.head_repository.full_name === repository && run.path === BuildWorkflow.path;
}

export function readBuildRuns(value: unknown): BuildRun[] {
  const runs = record(value)["workflow_runs"];
  assert.ok(Array.isArray(runs), "Missing Build workflow runs.");
  return runs.map((run: unknown) => readBuildRun(run));
}

export function readBuildArtifacts(value: unknown): BuildArtifact[] {
  const artifacts = record(value)["artifacts"];
  assert.ok(Array.isArray(artifacts), "Missing Build artifacts.");
  return artifacts.map((value: unknown) => {
    const { name, expired } = record(value);
    assert.ok(typeof name === "string" && typeof expired === "boolean");
    return { name, expired };
  });
}

export function newestBuilds(runs: readonly BuildRun[]): BuildRun[] {
  return [...runs].sort((left, right) => Date.parse(right.created_at) - Date.parse(left.created_at) || right.id - left.id);
}
