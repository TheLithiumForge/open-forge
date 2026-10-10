import { GithubState } from "../github-summary.ts";

// Reduced API-shaped records use deterministic fixture identities, not live provenance.
export const RecordedSha = "a".repeat(40);
export const RecordedRun = { id: 38043270935, name: "Release", head_sha: RecordedSha, status: GithubState.completed, conclusion: GithubState.success };
export const RecordedRuns = { workflow_runs: [RecordedRun] };
export const RecordedJobs = { jobs: [{ id: 101, name: "build (win-x64)", status: GithubState.completed, conclusion: GithubState.success }] };
export const FailedConclusion = "failure";
export const CancelledConclusion = "cancelled";
