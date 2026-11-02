import { setTimeout } from "node:timers/promises";
import type { EvalConfig } from "./config.types.ts";
import { loadLocalEnv } from "./auth.ts";
import { resumeBatch, runningBatch, startBatch } from "../execution/batch.ts";
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
  return followBatch(report, async () => {
    const started = await startBatch(config);
    report(`Started ${started.runs.length} attempts.`);
    return started.id;
  });
}

/** Continues a paused or interrupted batch in this process and waits for it. */
export async function resumePlan(batchId: string, report: (text: string) => void): Promise<number> {
  return followBatch(report, async () => {
    await resumeBatch(batchId);
    report(`Resumed batch ${batchId}.`);
    return batchId;
  });
}

async function followBatch(report: (text: string) => void, begin: () => Promise<string>): Promise<number> {
  loadLocalEnv();
  const unsubscribe = progress.subscribe((message) => report(message.text));
  try {
    const id = await begin();
    while (runningBatch() !== null) await setTimeout(PollMs);
    const batch = await readBatch(id);
    const runs = await readRuns(batch);
    report(`Batch ${batch.status}. ${runs.filter((run) => run.status === RunStatus.done).length} of ${runs.length} attempts completed.`);
    return batch.status === BatchStatus.done ? 0 : 1;
  } finally { unsubscribe(); }
}
