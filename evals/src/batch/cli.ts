// Runs one batch from the config without the UI, prints its progress, then the results table.
import { setTimeout as sleep } from "node:timers/promises";
import { loadLocalEnv } from "../config/auth.ts";
import { readBatch, readPreferences, readRuns } from "../results/store.ts";
import { summarize } from "../results/summary.ts";
import { runningBatch, startBatch } from "./batch.ts";
import { progress } from "./progress.ts";

const PollMs = 1000;

loadLocalEnv();
const started = await startBatch();
console.log(`Batch ${started.id}: ${started.runs.length} runs.`);
progress.subscribe((message) => console.log(message.text));
while (runningBatch() !== null) await sleep(PollMs);
const batch = await readBatch(started.id);
const runs = await readRuns(batch);
console.log(`Batch ${batch.status}.`);
console.table(summarize(runs, await readPreferences(batch.id)));
for (const run of runs) {
  const failed = run.checks.filter((check) => check.passed !== true);
  console.log(`${run.id} ${run.setup} ${run.status}${run.error === null ? "" : `: ${run.error.slice(0, 300)}`}`);
  for (const check of failed) console.log(`  ${check.passed === null ? "?" : "✗"} ${check.id}: ${check.detail}`);
}
