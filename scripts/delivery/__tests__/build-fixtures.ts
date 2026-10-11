import { BuildWorkflow, type BuildRun } from "../ci/build-runs.ts";
import { GithubState } from "../ci/github-summary.ts";
import { AllTargets } from "../targets.ts";

export const BuildRepository = "TheLithiumForge/open-forge";
export const BuildSha = "a".repeat(40);
export const OtherBuildSha = "b".repeat(40);
export const BuildBranch = { source: "develop", other: "main" } as const;
export const BuildId = { current: 100, older: 99, newer: 101 } as const;
export const BuildTime = { older: "2026-10-10T19:00:00Z", newer: "2026-10-10T20:00:00Z" } as const;
export const FailedBuild = "failure";
export const OtherRepository = "another/repo";
export const OtherWorkflow = ".github/workflows/release.yml";

export const BuildEvent = { push: "push", manual: "workflow_dispatch", pullRequest: "pull_request" } as const;

export function recordedBuild(overrides: Partial<BuildRun> = {}): BuildRun {
  return {
    id: BuildId.older,
    repository: { full_name: BuildRepository },
    head_repository: { full_name: BuildRepository },
    head_sha: BuildSha,
    head_branch: BuildBranch.other,
    event: BuildEvent.push,
    path: BuildWorkflow.path,
    status: GithubState.completed,
    conclusion: GithubState.success,
    created_at: BuildTime.older,
    ...overrides,
  };
}

export const PackageArtifacts = AllTargets.map((rid) => ({ name: `${BuildWorkflow.artifactPrefix}${rid}`, expired: false }));
