import assert from "node:assert/strict";
import { readOptions } from "../options.ts";
import { reportFailure } from "../process.ts";
import { BuildWorkflow, readBuildRuns } from "./build-runs.ts";
import { CiCommand } from "./command-names.ts";
import { GithubApi, readGithub } from "./github-api.ts";
import { readTimedJobs, timingOutliers, timingReport, timingRows, TimingDefaults, type TimedJob } from "./timings-model.ts";

/** Reads every attempt of one run's jobs, so a failed attempt stays visible after a retry. */
async function runAttempts(run: number): Promise<TimedJob[]> {
  const jobs: TimedJob[] = [];
  for (let page = 1; ; page++) {
    const current = readTimedJobs(await readGithub(`actions/runs/${run}/jobs?filter=all&per_page=${GithubApi.pageSize}&page=${page}`), run);
    jobs.push(...current);
    if (current.length < GithubApi.pageSize) return jobs;
  }
}

try {
  const options = readOptions(CiCommand.timings);
  if (options) {
    const count = Number(options.limit ?? TimingDefaults.runs);
    assert.ok(Number.isSafeInteger(count) && count > 0 && count <= TimingDefaults.maximumRuns, `--limit must be a whole number from 1 to ${TimingDefaults.maximumRuns}.`);
    const runs = readBuildRuns(await readGithub(`actions/workflows/${BuildWorkflow.file}/runs?status=completed&per_page=${count}`));
    const jobs: TimedJob[] = [];
    for (const run of runs) jobs.push(...(await runAttempts(run.id)));
    const rows = timingRows(jobs);
    process.stdout.write(timingReport(runs.length, rows, timingOutliers(jobs, rows)));
  }
} catch (error) {
  reportFailure(error);
}
