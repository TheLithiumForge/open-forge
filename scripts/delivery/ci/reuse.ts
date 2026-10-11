import assert from "node:assert/strict";
import { appendFileSync } from "node:fs";
import { setTimeout } from "node:timers/promises";
import { buildRun, buildRuns } from "./build-api.ts";
import type { BuildRun } from "./build-runs.ts";
import type { GithubConnection } from "./github-api.ts";
import { chooseBranchBuild, isDirectPush, ReuseOutcome, ReuseWait, settleReuse, type ReuseRequest } from "./reuse-decision.ts";
import { reportFailure } from "../process.ts";

/** Waits for the deferred Build to finish and returns whether this push may reuse it. */
async function waitForBuild(connection: GithubConnection, deferred: BuildRun): Promise<boolean> {
  const started = Date.now();
  for (let run = deferred; ; run = await buildRun(connection, String(deferred.id))) {
    const outcome = settleReuse(run, Date.now() - started);
    if (outcome === ReuseOutcome.reuse) return true;
    if (outcome === ReuseOutcome.build) {
      process.stdout.write(`Build ${run.id} ended ${run.conclusion ?? "unfinished"}. Building this commit here.\n`);
      return false;
    }
    process.stdout.write(`Waiting for Build ${run.id} (${run.status}) before deciding.\n`);
    await setTimeout(ReuseWait.pollMilliseconds);
  }
}

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
    const connection = { repository, api, token };
    const choice = chooseBranchBuild(request, await buildRuns(connection, sha));
    if (choice && (await waitForBuild(connection, choice))) {
      id = String(choice.id);
      process.stdout.write(`Reusing Build ${id} for ${sha}\n`);
    }
  }
  appendFileSync(output, `run=${id}\n`);
} catch (error) {
  reportFailure(error);
}
