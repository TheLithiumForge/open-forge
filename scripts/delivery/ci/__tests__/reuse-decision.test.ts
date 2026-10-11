import assert from "node:assert/strict";
import { test } from "node:test";
import { chooseBranchBuild, isDirectPush, PushEvent, type ReuseRequest } from "../reuse-decision.ts";
import { GithubState } from "../github-summary.ts";
import { BuildBranch, BuildId, BuildRepository, BuildSha, FailedBuild, OtherBuildSha, OtherRepository, OtherWorkflow, recordedBuild } from "../../__tests__/build-fixtures.ts";

const Request: ReuseRequest = { event: PushEvent, ref: "", repository: BuildRepository, sha: BuildSha, branch: BuildBranch.source, currentRun: BuildId.current };
const NonPushEvent = { pullRequest: "pull_request", manual: "workflow_dispatch", call: "workflow_call" } as const;

test("a direct push reuses a passed, queued or running Build from another branch", () => {
  assert.equal(isDirectPush(Request), true);
  for (const status of [GithubState.completed, GithubState.queued, GithubState.running]) {
    const run = recordedBuild({ status, conclusion: status === GithubState.completed ? GithubState.success : null });
    assert.equal(chooseBranchBuild(Request, [run])?.id, run.id);
  }
});

for (const [scenario, run] of [
  ["failed", recordedBuild({ conclusion: FailedBuild })],
  ["same branch", recordedBuild({ head_branch: Request.branch })],
  ["current run", recordedBuild({ id: Request.currentRun })],
  ["another commit", recordedBuild({ head_sha: OtherBuildSha })],
  ["another repository", recordedBuild({ repository: { full_name: OtherRepository } })],
  ["another workflow", recordedBuild({ path: OtherWorkflow })],
] as const) {
  test(`branch reuse excludes ${scenario}`, () => assert.equal(chooseBranchBuild(Request, [run]), undefined));
}

test("branches pushed together defer only to the earlier unfinished run", () => {
  for (const status of [GithubState.queued, GithubState.running]) {
    assert.equal(chooseBranchBuild(Request, [recordedBuild({ id: BuildId.newer, status, conclusion: null })]), undefined);
    assert.equal(chooseBranchBuild(Request, [recordedBuild({ id: BuildId.older, status, conclusion: null })])?.id, BuildId.older);
  }
  const passedLater = recordedBuild({ id: BuildId.newer });
  assert.equal(chooseBranchBuild(Request, [passedLater])?.id, BuildId.newer);
});

test("pull requests, manual runs and Release calls always build", () => {
  for (const event of Object.values(NonPushEvent)) {
    const request = { ...Request, event };
    assert.equal(isDirectPush(request), false);
    assert.equal(chooseBranchBuild(request, [recordedBuild()]), undefined);
  }
  const releaseCall = { ...Request, ref: BuildSha };
  assert.equal(isDirectPush(releaseCall), false);
  assert.equal(chooseBranchBuild(releaseCall, [recordedBuild()]), undefined);
  assert.equal(chooseBranchBuild(Request, []), undefined);
});
