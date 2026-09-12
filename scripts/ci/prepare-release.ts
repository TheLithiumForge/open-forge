import assert from "node:assert/strict";
import { appendFileSync } from "node:fs";
import { sourceIdentity } from "../delivery/source.ts";
import { committedVersion } from "../delivery/version-sync.ts";
import { releaseChannel, validateBuildRun } from "./release-selection.ts";
import { validateReleaseTag } from "./release-tag.ts";

assert.equal(process.env["GITHUB_ACTIONS"], "true", "Release selection runs only in GitHub Actions.");
const repository = process.env["GITHUB_REPOSITORY"];
const output = process.env["GITHUB_OUTPUT"];
assert.ok(repository && output);
const source = sourceIdentity(process.cwd());
assert.equal(source.dirty, false, "Release source must be committed.");
const version = committedVersion(process.cwd());
const tag = process.env["RELEASE_TAG"] ?? "";
if (tag) assert.equal(tag, `v${version}`, "The release tag must match package.json.");
const target = process.env["RELEASE_TARGET"] || "all";
assert.ok(["github", "npm", "all"].includes(target), "Unsupported release target.");
const token = process.env["GITHUB_TOKEN"];
const api = process.env["GITHUB_API_URL"];
assert.ok(token && api);
const read = (path: string) =>
  fetch(`${api}/repos/${repository}/${path}`, {
    headers: { Authorization: `Bearer ${token}`, Accept: "application/vnd.github+json", "X-GitHub-Api-Version": "2022-11-28" },
  });
await validateReleaseTag(read, `v${version}`, source.sha);
const runId = process.env["BUILD_RUN_ID"] ?? "";
if (runId) {
  assert.match(runId, /^\d+$/u, "The build run ID must be numeric.");
  const response = await read(`actions/runs/${runId}`);
  assert.ok(response.ok, `Unable to inspect selected build (${response.status}).`);
  validateBuildRun(await response.json(), repository, source.sha);
}
appendFileSync(output, `sha=${source.sha}\nversion=${version}\ntarget=${target}\nbuild_run_id=${runId}\nchannel=${releaseChannel(version)}\nprerelease=${version.includes("-")}\n`);
