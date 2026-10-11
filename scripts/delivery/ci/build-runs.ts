import assert from "node:assert/strict";

export const BuildWorkflow = { file: "build.yml", path: ".github/workflows/build.yml", artifactPrefix: "package-" } as const;

export interface BuildRun {
  id: number;
  repository: { full_name: string };
  head_sha: string;
  head_branch: string;
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

export function readBuildRun(value: unknown): BuildRun {
  const { id, repository, head_sha, head_branch, path, status, conclusion, created_at } = record(value);
  const full_name = record(repository)["full_name"];
  assert.ok(typeof id === "number" && Number.isSafeInteger(id) && id > 0);
  assert.ok(typeof full_name === "string" && typeof head_sha === "string" && typeof head_branch === "string");
  assert.ok(typeof path === "string" && typeof status === "string");
  assert.ok(conclusion === null || typeof conclusion === "string");
  assert.ok(typeof created_at === "string" && Number.isFinite(Date.parse(created_at)));
  return { id, repository: { full_name }, head_sha, head_branch, path, status, conclusion, created_at };
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
