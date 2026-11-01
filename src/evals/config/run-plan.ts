import { setTimeout } from "node:timers/promises";
import type { EvalConfig } from "./config.types.ts";
import { loadLocalEnv } from "./auth.ts";
import { runningBatch, startBatch } from "../execution/batch.ts";
import { readBatch, readRuns } from "../history/store.ts";
import { BatchStatus, RunStatus } from "../history/statuses.ts";
import { progress } from "../execution/progress.ts";
import { loadSuite } from "../benchmarks/suite.ts";

const PollMs = 1000;
export async function runPlan(config: EvalConfig, report: (text: string) => void): Promise<number> {
  const cases = config.cases ?? [config.suite];
  for (const id of cases) {
    const suite = await loadSuite(id);
    if (config.setups.some((setup) => suite.setups[setup] === undefined)) throw new Error(`The plan names an unknown setup for ${id}.`);
  }
  const { cases: _cases, ...single } = config;
  for (const id of cases) {
    const result = await runSinglePlan({ ...single, suite: id }, report);
    if (result !== 0) return result;
  }
  return 0;
}

async function runSinglePlan(config: EvalConfig, report: (text: string) => void): Promise<number> {
  loadLocalEnv();
  const unsubscribe = progress.subscribe((message) => report(message.text));
  try {
    const started = await startBatch(config);
    report(`Started ${started.runs.length} attempts.`);
    while (runningBatch() !== null) await setTimeout(PollMs);
    const batch = await readBatch(started.id);
    const runs = await readRuns(batch);
    report(`Batch ${batch.status}. ${runs.filter((run) => run.status === RunStatus.done).length} of ${runs.length} attempts completed.`);
    return batch.status === BatchStatus.done ? 0 : 1;
  } finally { unsubscribe(); }
}
