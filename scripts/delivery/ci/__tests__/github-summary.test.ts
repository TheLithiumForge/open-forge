import assert from "node:assert/strict";
import { test } from "node:test";
import { GithubState, WatchDecision, readRun, readRuns, readJobs, jobState, runDecision, runSummary } from "../github-summary.ts";
import { githubHeaders, GithubApi, GithubHeader } from "../github-api.ts";
import { RecordedRun, RecordedRuns, RecordedJobs, RecordedSha, FailedConclusion, CancelledConclusion } from "./github-fixtures.ts";

test("GitHub response projections retain run identity and changed job state", () => {
  const runs = readRuns(RecordedRuns);
  const [job] = readJobs(RecordedJobs);
  assert.ok(job);
  assert.equal(runs[0]?.sha, RecordedSha);
  assert.equal(readRun(RecordedRun).id, RecordedRun.id);
  assert.equal(jobState(job), `${GithubState.completed} (${GithubState.success})`);
  assert.equal(runSummary(runs), `${RecordedRun.id} ${RecordedRun.name}: ${jobState(job)}`);
  assert.equal(jobState({ ...job, status: GithubState.running, conclusion: null }), GithubState.running);
  for (const value of [null, {}, { workflow_runs: [{}] }]) assert.throws(() => readRuns(value));
  assert.throws(() => readJobs({ jobs: [{ ...RecordedJobs.jobs[0], id: "invalid" }] }));
});

test("watch succeeds only when all runs completed successfully", () => {
  const success = readRun(RecordedRun);
  assert.equal(runDecision([]), WatchDecision.pending);
  assert.equal(runDecision([success]), WatchDecision.success);
  assert.equal(runDecision([success, { ...success, id: 2, status: GithubState.queued, conclusion: null }]), WatchDecision.pending);
  for (const conclusion of [FailedConclusion, CancelledConclusion, null]) assert.equal(runDecision([{ ...success, conclusion }]), WatchDecision.failure);
});

test("public GitHub requests include a token only when supplied", () => {
  assert.deepEqual(githubHeaders(), { [GithubHeader.accept]: GithubApi.accept, [GithubHeader.version]: GithubApi.version });
  const FixtureToken = "fixture-token";
  assert.equal(githubHeaders(FixtureToken)[GithubHeader.authorization], `Bearer ${FixtureToken}`);
});
