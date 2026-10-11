import assert from "node:assert/strict";
import { test } from "node:test";
import { releaseChannel, validateBuildRun } from "../release-selection.ts";
import { BuildEvent, BuildRepository, BuildSha, FailedBuild, OtherBuildSha, OtherRepository, OtherWorkflow, recordedBuild } from "../../__tests__/build-fixtures.ts";
import { GithubState } from "../../ci/github-summary.ts";

const UntrustedBuild = /pull request or fork/u;

test("release reuse requires the successful exact repository, commit and build workflow", () => {
  validateBuildRun(recordedBuild(), BuildRepository, BuildSha);
  validateBuildRun(recordedBuild({ event: BuildEvent.manual }), BuildRepository, BuildSha);
  assert.throws(() => validateBuildRun(recordedBuild({ conclusion: FailedBuild }), BuildRepository, BuildSha));
  assert.throws(() => validateBuildRun(recordedBuild({ status: GithubState.running, conclusion: null }), BuildRepository, BuildSha));
  assert.throws(() => validateBuildRun(recordedBuild({ head_sha: OtherBuildSha }), BuildRepository, BuildSha));
  assert.throws(() => validateBuildRun(recordedBuild({ repository: { full_name: OtherRepository } }), BuildRepository, BuildSha));
  assert.throws(() => validateBuildRun(recordedBuild({ path: OtherWorkflow }), BuildRepository, BuildSha));
});

test("release reuse rejects pull request and fork Builds, whose artifacts are not this commit's own Build", () => {
  assert.throws(() => validateBuildRun(recordedBuild({ event: BuildEvent.pullRequest }), BuildRepository, BuildSha), UntrustedBuild);
  assert.throws(() => validateBuildRun(recordedBuild({ head_repository: { full_name: OtherRepository } }), BuildRepository, BuildSha), UntrustedBuild);
  assert.throws(() => validateBuildRun({ ...recordedBuild(), head_repository: null }, BuildRepository, BuildSha), UntrustedBuild);
});

test("beta releases use their prerelease channel while stable releases use latest", () => {
  assert.equal(releaseChannel("0.1.0-beta.1"), "beta");
  assert.equal(releaseChannel("1.0.0"), "latest");
});

test("numeric prerelease identifiers use next rather than an invalid numeric npm tag", () => {
  assert.equal(releaseChannel("0.1.0-0"), "next");
  assert.equal(releaseChannel("0.1.0-1.sha-abcdef"), "next");
});
