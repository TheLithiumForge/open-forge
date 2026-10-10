import assert from "node:assert/strict";
import { readJobs, readRuns, type GithubJob, type GithubRun } from "./github-summary.ts";

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
} as const;

export function githubHeaders(token?: string): Record<string, string> {
  return { [GithubHeader.accept]: GithubApi.accept, [GithubHeader.version]: GithubApi.version, ...(token ? { [GithubHeader.authorization]: `Bearer ${token}` } : {}) };
}

export async function readGithub(path: string): Promise<unknown> {
  const response = await fetch(`${GithubApi.root}/repos/${GithubRepository}/${path}`, {
    headers: githubHeaders(process.env[GithubApi.token]),
    signal: AbortSignal.timeout(GithubApi.timeoutMilliseconds),
  });
  assert.ok(response.ok, `GitHub request failed (${response.status}): ${path}`);
  return response.json();
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
