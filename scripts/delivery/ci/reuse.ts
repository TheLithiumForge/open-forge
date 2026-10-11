import assert from "node:assert/strict";
import { appendFileSync } from "node:fs";
import { buildRuns } from "./build-api.ts";
import { chooseBranchBuild, isDirectPush, type ReuseRequest } from "./reuse-decision.ts";
import { reportFailure } from "../process.ts";

try {
  assert.equal(process.env["GITHUB_ACTIONS"], "true", "Build reuse runs only in GitHub Actions.");
  const output = process.env["GITHUB_OUTPUT"];
  const repository = process.env["GITHUB_REPOSITORY"];
  const sha = process.env["GITHUB_SHA"];
  const branch = process.env["GITHUB_REF_NAME"];
  const event = process.env["GITHUB_EVENT_NAME"];
  const currentRun = Number(process.env["GITHUB_RUN_ID"]);
  assert.ok(output && repository && sha && branch && event && Number.isSafeInteger(currentRun) && currentRun > 0);
  const request: ReuseRequest = { event, ref: process.env["BUILD_REF"] ?? "", repository, sha, branch, currentRun };
  let id = "";
  if (isDirectPush(request)) {
    const api = process.env["GITHUB_API_URL"];
    const token = process.env["GITHUB_TOKEN"];
    assert.ok(api && token);
    const choice = chooseBranchBuild(request, await buildRuns({ repository, api, token }, sha));
    if (choice) {
      id = String(choice.id);
      process.stdout.write(`Reusing Build ${id} for ${sha}\n`);
    }
  }
  appendFileSync(output, `run=${id}\n`);
} catch (error) {
  reportFailure(error);
}
