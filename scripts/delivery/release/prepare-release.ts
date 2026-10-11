import assert from "node:assert/strict";
import { appendFileSync } from "node:fs";
import { sourceIdentity } from "../source.ts";
import { committedVersion } from "../version.ts";
import { releaseChannel } from "./release-selection.ts";
import { validateReleaseTag } from "./release-tag.ts";
import { GithubApi, githubHeaders } from "../ci/github-api.ts";
import { selectBuild } from "./select-build.ts";
import { readReleaseTarget } from "./release-plan.ts";

assert.equal(process.env["GITHUB_ACTIONS"], "true", "Release selection runs only in GitHub Actions.");
const repository = process.env["GITHUB_REPOSITORY"];
const output = process.env["GITHUB_OUTPUT"];
assert.ok(repository && output);
const source = sourceIdentity(process.cwd());
assert.equal(source.dirty, false, "Release source must be committed.");
const version = committedVersion(process.cwd());
const tag = process.env["RELEASE_TAG"] ?? "";
if (tag) assert.equal(tag, `v${version}`, "The release tag must match package.json.");
const target = readReleaseTarget(process.env["RELEASE_TARGET"]);
const token = process.env["GITHUB_TOKEN"];
const api = process.env["GITHUB_API_URL"];
assert.ok(token && api);
const read = (path: string) =>
  fetch(`${api}/repos/${repository}/${path}`, {
    headers: githubHeaders(token),
    signal: AbortSignal.timeout(GithubApi.timeoutMilliseconds),
  });
await validateReleaseTag(read, `v${version}`, source.sha);
const runId = await selectBuild({ repository, api, token }, source.sha, process.env["BUILD_RUN_ID"] ?? "");
appendFileSync(output, `sha=${source.sha}\nversion=${version}\ntarget=${target}\nbuild_run_id=${runId}\nchannel=${releaseChannel(version)}\nprerelease=${version.includes("-")}\n`);
