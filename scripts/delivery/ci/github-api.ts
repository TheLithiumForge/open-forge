import assert from "node:assert/strict";
import { setTimeout } from "node:timers/promises";
import { readAnnotations, readJobs, readRuns, type GithubAnnotation, type GithubJob, type GithubRun } from "./github-summary.ts";
import { rateLimitDelay } from "./github-rate-limit.ts";

export const GithubRepository = "TheLithiumForge/open-forge";
export const GithubHeader = { accept: "Accept", version: "X-GitHub-Api-Version", authorization: "Authorization" } as const;
export const GithubApi = {
  root: "https://api.github.com",
  accept: "application/vnd.github+json",
  version: "2022-11-28",
  token: "GITHUB_TOKEN",
  pageSize: 100,
  timeoutMilliseconds: 30_000,
  pollMilliseconds: 15_000,
  // Anonymous requests share 60 per hour. Two requests every three minutes stays well below it.
  anonymousPollMilliseconds: 180_000,
  rateLimitRetries: 3,
} as const;
const MillisecondsPerSecond = 1_000;

export interface GithubConnection {
  repository: string;
  api: string;
  token?: string;
}

export function githubToken(): string | undefined {
  return process.env[GithubApi.token] || undefined;
}

export function githubHeaders(token?: string): Record<string, string> {
  return { [GithubHeader.accept]: GithubApi.accept, [GithubHeader.version]: GithubApi.version, ...(token ? { [GithubHeader.authorization]: `Bearer ${token}` } : {}) };
}

/** Reads one repository API path, waiting out a GitHub rate limit instead of failing. */
export async function readGithub(path: string, connection?: GithubConnection): Promise<unknown> {
  for (let attempt = 0; ; attempt++) {
    const response = await fetch(`${connection?.api ?? GithubApi.root}/repos/${connection?.repository ?? GithubRepository}/${path}`, {
      headers: githubHeaders(connection?.token ?? githubToken()),
      signal: AbortSignal.timeout(GithubApi.timeoutMilliseconds),
    });
    if (response.ok) return response.json();
    const delay = rateLimitDelay({ status: response.status, header: (name) => response.headers.get(name) }, Date.now());
    assert.ok(delay !== undefined && attempt < GithubApi.rateLimitRetries, `GitHub request failed (${response.status}): ${path}`);
    process.stderr.write(`GitHub rate limit reached. Waiting ${Math.ceil(delay / MillisecondsPerSecond)} seconds. Set GITHUB_TOKEN for a higher limit.\n`);
    await setTimeout(delay);
  }
}

export async function commitRuns(sha: string): Promise<GithubRun[]> {
  const runs: GithubRun[] = [];
  for (let page = 1; ; page++) {
    const current = readRuns(await readGithub(`actions/runs?head_sha=${encodeURIComponent(sha)}&per_page=${GithubApi.pageSize}&page=${page}`));
    assert.ok(
      current.every((run) => run.sha === sha),
      "GitHub returned a run for another commit.",
    );
    runs.push(...current);
    if (current.length < GithubApi.pageSize) return runs;
  }
}

export async function runJobs(id: number): Promise<GithubJob[]> {
  const jobs: GithubJob[] = [];
  for (let page = 1; ; page++) {
    const current = readJobs(await readGithub(`actions/runs/${id}/jobs?filter=latest&per_page=${GithubApi.pageSize}&page=${page}`));
    jobs.push(...current);
    if (current.length < GithubApi.pageSize) return jobs;
  }
}

/** A job's check run shares its ID, so its annotations come from the check run. */
export async function jobAnnotations(id: number): Promise<GithubAnnotation[]> {
  return readAnnotations(await readGithub(`check-runs/${id}/annotations?per_page=${GithubApi.pageSize}`));
}
