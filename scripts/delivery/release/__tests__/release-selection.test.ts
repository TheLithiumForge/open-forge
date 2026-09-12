import assert from "node:assert/strict";
import { test } from "node:test";
import { releaseChannel, validateBuildRun } from "../release-selection.ts";

const repository = "TheLithiumForge/open-forge";
const sha = "a".repeat(40);
const run = { repository: { full_name: repository }, head_sha: sha, conclusion: "success", status: "completed", path: ".github/workflows/build.yml" };

test("release reuse requires the successful exact repository, commit and build workflow", () => {
  validateBuildRun(run, repository, sha);
  assert.throws(() => validateBuildRun({ ...run, conclusion: "failure" }, repository, sha));
  assert.throws(() => validateBuildRun({ ...run, head_sha: "b".repeat(40) }, repository, sha));
  assert.throws(() => validateBuildRun({ ...run, repository: { full_name: "another/repo" } }, repository, sha));
  assert.throws(() => validateBuildRun({ ...run, path: ".github/workflows/release.yml" }, repository, sha));
});

test("beta releases use their prerelease channel while stable releases use latest", () => {
  assert.equal(releaseChannel("0.1.0-beta.1"), "beta");
  assert.equal(releaseChannel("1.0.0"), "latest");
});

test("numeric prerelease identifiers use next rather than an invalid numeric npm tag", () => {
  assert.equal(releaseChannel("0.1.0-0"), "next");
  assert.equal(releaseChannel("0.1.0-1.sha-abcdef"), "next");
});
