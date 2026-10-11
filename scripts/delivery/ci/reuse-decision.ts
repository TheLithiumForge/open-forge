import { isTrustedBuild, newestBuilds, type BuildRun } from "./build-runs.ts";
import { GithubState } from "./github-summary.ts";

export const PushEvent = "push";

/** How long a push waits for an earlier Build of the same commit before building itself. */
export const ReuseWait = { pollMilliseconds: 30_000, limitMilliseconds: 90 * 60_000 } as const;

export const ReuseOutcome = { reuse: "reuse", build: "build", wait: "wait" } as const;
export type ReuseOutcome = (typeof ReuseOutcome)[keyof typeof ReuseOutcome];

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

function isUnfinished(run: BuildRun): boolean {
  return run.status === GithubState.queued || run.status === GithubState.running;
}

/**
 * Picks another branch's trusted Build of the same commit to stand in for this
 * push: a passed one, or an earlier unfinished one to wait for. Deferring only
 * to an earlier run stops two branches pushed together from waiting on each other.
 */
export function chooseBranchBuild(request: ReuseRequest, runs: readonly BuildRun[]): BuildRun | undefined {
  if (!isDirectPush(request)) return undefined;
  return newestBuilds(runs).find((run) => {
    if (run.id === request.currentRun || run.head_branch === request.branch || run.head_sha !== request.sha) return false;
    if (!isTrustedBuild(run, request.repository)) return false;
    if (run.status === GithubState.completed) return run.conclusion === GithubState.success;
    return isUnfinished(run) && run.id < request.currentRun;
  });
}

/**
 * Decides what a push does about the Build it deferred to. It reuses only a
 * Build that passed, so a failed, cancelled or overdue Build never leaves this
 * commit looking green without a passing run.
 */
export function settleReuse(run: BuildRun, waitedMilliseconds: number): ReuseOutcome {
  if (run.status === GithubState.completed) return run.conclusion === GithubState.success ? ReuseOutcome.reuse : ReuseOutcome.build;
  return waitedMilliseconds >= ReuseWait.limitMilliseconds ? ReuseOutcome.build : ReuseOutcome.wait;
}
