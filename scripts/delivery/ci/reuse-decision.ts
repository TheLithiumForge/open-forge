import { BuildWorkflow, newestBuilds, type BuildRun } from "./build-runs.ts";
import { GithubState } from "./github-summary.ts";

export const PushEvent = "push";

export interface ReuseRequest {
  event: string;
  ref: string;
  repository: string;
  sha: string;
  branch: string;
  currentRun: number;
}

export function isDirectPush(request: ReuseRequest): boolean {
  return request.event === PushEvent && request.ref === "";
}

export function chooseBranchBuild(request: ReuseRequest, runs: readonly BuildRun[]): BuildRun | undefined {
  if (!isDirectPush(request)) return undefined;
  return newestBuilds(runs).find((run) => {
    if (run.id === request.currentRun || run.head_branch === request.branch) return false;
    if (run.head_sha !== request.sha || run.repository.full_name !== request.repository || run.path !== BuildWorkflow.path) return false;
    if (run.status === GithubState.completed) return run.conclusion === GithubState.success;
    // Defer only to an earlier unfinished run. Two branches pushed together would otherwise wait for each other and neither would build.
    return (run.status === GithubState.queued || run.status === GithubState.running) && run.id < request.currentRun;
  });
}
