import assert from "node:assert/strict";

export const GithubState = { completed: "completed", success: "success", queued: "queued", running: "in_progress" } as const;
export const GithubConclusion = { failure: "failure" } as const;
export const WatchDecision = { pending: "pending", success: "success", failure: "failure" } as const;
export type WatchDecision = (typeof WatchDecision)[keyof typeof WatchDecision];
export interface GithubRun {
  id: number;
  name: string;
  sha: string;
  status: string;
  conclusion: string | null;
}
export interface GithubJob {
  id: number;
  name: string;
  status: string;
  conclusion: string | null;
}

function record(value: unknown): Record<string, unknown> {
  assert.ok(typeof value === "object" && value !== null && !Array.isArray(value), "Invalid GitHub response.");
  return Object.fromEntries(Object.entries(value));
}

function state(value: unknown): GithubJob {
  const item = record(value);
  const { id, name, status, conclusion } = item;
  assert.ok(typeof id === "number" && Number.isSafeInteger(id) && id > 0);
  assert.ok(typeof name === "string" && typeof status === "string");
  assert.ok(conclusion === null || typeof conclusion === "string");
  return { id, name, status, conclusion };
}

export function readRun(value: unknown): GithubRun {
  const run = state(value);
  const sha = record(value)["head_sha"];
  assert.ok(typeof sha === "string" && sha.length > 0);
  return { ...run, sha };
}

export function readRuns(value: unknown): GithubRun[] {
  const runs = record(value)["workflow_runs"];
  assert.ok(Array.isArray(runs), "Missing GitHub workflow runs.");
  return runs.map((run: unknown) => readRun(run));
}

export function readJobs(value: unknown): GithubJob[] {
  const jobs = record(value)["jobs"];
  assert.ok(Array.isArray(jobs), "Missing GitHub jobs.");
  return jobs.map((job: unknown) => state(job));
}

export function runDecision(runs: readonly GithubRun[]): WatchDecision {
  if (!runs.length || runs.some((run) => run.status !== GithubState.completed)) return WatchDecision.pending;
  return runs.every((run) => run.conclusion === GithubState.success) ? WatchDecision.success : WatchDecision.failure;
}

export function jobState(job: GithubJob): string {
  return `${job.status}${job.conclusion ? ` (${job.conclusion})` : ""}`;
}

export function runSummary(runs: readonly GithubRun[]): string {
  return runs.map((run) => `${run.id} ${run.name}: ${jobState(run)}`).join("\n");
}

export interface GithubAnnotation {
  level: string;
  title: string;
  message: string;
  path: string;
  line: number | null;
}

/** Reads a check run's annotations, which name failures without signing in to read logs. */
export function readAnnotations(value: unknown): GithubAnnotation[] {
  assert.ok(Array.isArray(value), "Missing GitHub annotations.");
  return value.map((entry: unknown) => {
    const item = record(entry);
    const text = (key: string) => (typeof item[key] === "string" ? item[key] : "");
    return {
      level: text("annotation_level"),
      title: text("title"),
      message: text("message"),
      path: text("path"),
      line: typeof item["start_line"] === "number" ? item["start_line"] : null,
    };
  });
}

export function annotationLine(annotation: GithubAnnotation): string {
  const location = annotation.path && annotation.path !== ".github" ? ` ${annotation.path}${annotation.line === null ? "" : `:${annotation.line}`}` : "";
  return `  ${annotation.level}${location}: ${annotation.title ? `${annotation.title}: ` : ""}${annotation.message}`;
}
