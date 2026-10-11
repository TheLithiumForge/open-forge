import { GithubApi, readGithub, type GithubConnection } from "./github-api.ts";
import { BuildWorkflow, readBuildRun, readBuildRuns, readBuildArtifacts, type BuildRun, type BuildArtifact } from "./build-runs.ts";

export async function buildRuns(connection: GithubConnection, sha: string): Promise<BuildRun[]> {
  const runs: BuildRun[] = [];
  for (let page = 1; ; page++) {
    const current = readBuildRuns(
      await readGithub(`actions/workflows/${BuildWorkflow.file}/runs?head_sha=${encodeURIComponent(sha)}&per_page=${GithubApi.pageSize}&page=${page}`, connection),
    );
    runs.push(...current);
    if (current.length < GithubApi.pageSize) return runs;
  }
}

export async function buildRun(connection: GithubConnection, id: string): Promise<BuildRun> {
  return readBuildRun(await readGithub(`actions/runs/${id}`, connection));
}

export async function buildArtifacts(connection: GithubConnection, id: number): Promise<BuildArtifact[]> {
  const artifacts: BuildArtifact[] = [];
  for (let page = 1; ; page++) {
    const current = readBuildArtifacts(await readGithub(`actions/runs/${id}/artifacts?per_page=${GithubApi.pageSize}&page=${page}`, connection));
    artifacts.push(...current);
    if (current.length < GithubApi.pageSize) return artifacts;
  }
}
