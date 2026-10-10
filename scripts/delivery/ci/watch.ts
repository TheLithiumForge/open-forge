import assert from "node:assert/strict";
import { setTimeout } from "node:timers/promises";
import { readOptions } from "../options.ts";
import { reportFailure, ProcessExit } from "../process.ts";
import { FullGitShaPattern } from "../source-identity.ts";
import { CiCommand } from "./command-names.ts";
import { commitRuns, runJobs, readGithub, GithubApi } from "./github-api.ts";
import { jobState, readRun, runDecision, runSummary, WatchDecision } from "./github-summary.ts";

try {
  const options = readOptions(CiCommand.watch);
  if (options) {
    assert.ok(Boolean(options.commitSha) !== Boolean(options.run), "Supply --sha <commit> or --run <id>, not both.");
    if (options.run) assert.match(options.run, /^\d+$/u, "The workflow run ID must be numeric.");
    const selected = options.run ? readRun(await readGithub(`actions/runs/${options.run}`)) : undefined;
    const sha = options.commitSha?.toLowerCase() ?? selected?.sha;
    assert.ok(sha, "Supply a commit SHA.");
    assert.match(sha, FullGitShaPattern, "Supply a full commit SHA.");
    const observed = new Map<string, string>();
    const changed = (key: string, line: string) => {
      if (observed.get(key) !== line) {
        process.stdout.write(`${line}\n`);
        observed.set(key, line);
      }
    };
    for (;;) {
      const runs = await commitRuns(sha);
      // Include the selected run even while the commit listing is catching up.
      if (selected && !runs.some((run) => run.id === selected.id)) runs.push(readRun(await readGithub(`actions/runs/${selected.id}`)));
      for (const run of runs) {
        changed(`run/${run.id}`, runSummary([run]));
        for (const job of await runJobs(run.id)) changed(`${run.id}/${job.id}`, `${run.id} ${job.name}: ${jobState(job)}`);
      }
      const decision = runDecision(runs);
      if (decision !== WatchDecision.pending) {
        process.exitCode = decision === WatchDecision.success ? ProcessExit.success : ProcessExit.failure;
        break;
      }
      await setTimeout(GithubApi.pollMilliseconds);
    }
  }
} catch (error) {
  reportFailure(error);
}
