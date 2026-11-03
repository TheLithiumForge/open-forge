import { setTimeout } from "node:timers/promises";
import type { EvalConfig } from "./config.types.ts";
import { loadLocalEnv } from "./auth.ts";
import { resumeBatch, runningBatch, startBatch } from "../execution/batch.ts";
import { readBatch, readRuns } from "../history/store.ts";
import { BatchStatus, RunStatus } from "../history/statuses.ts";
import { progress } from "../execution/progress.ts";
import { loadSuite } from "../benchmarks/suite.ts";
import { join } from "node:path";
import { EvalPaths } from "../paths.ts";
import { readPacket } from "../materialize/packets.ts";
import { WorkMode } from "./choices.ts";

const PollMs = 1000;
export async function runPlan(config: EvalConfig, report: (text: string) => void): Promise<number> {
  if (config.workMode === WorkMode.taskByTask) return runTaskByTask(config, report);
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
  return (await runStep(config, report)).code;
}

async function runStep(config: EvalConfig, report: (text: string) => void): Promise<{ readonly code: number; readonly batchId: string | null }> {
  let batchId: string | null = null;
  const code = await followBatch(report, async () => {
    const started = await startBatch(config);
    batchId = started.id;
    report(`Started ${started.runs.length} attempts.`);
    return started.id;
  });
  return { code, batchId };
}

/**
 * Task by task runs each of the packet's tasks as a batch of its own, in order. Every run of a later task starts a new
 * agent from the files of the run with the same scenario, implementer and repeat in the task before.
 */
async function runTaskByTask(config: EvalConfig, report: (text: string) => void): Promise<number> {
  const packet = await readPacket(join(EvalPaths.suites, config.suite));
  for (const task of packet.tasks) {
    const suite = await loadSuite(task.id);
    if (config.setups.some((setup) => suite.setups[setup] === undefined)) throw new Error(`The plan names an unknown setup for ${task.id}.`);
  }
  let parentBatchId: string | null = null;
  for (const [index, task] of packet.tasks.entries()) {
    report(`Task ${index + 1} of ${packet.tasks.length}: ${task.id}.`);
    const step = await runStep({ ...config, suite: task.id, chain: { id: packet.id, milestone: index + 1, ...(parentBatchId === null ? {} : { parentBatchId }) } }, report);
    if (step.code !== 0 || step.batchId === null) return step.code === 0 ? 1 : step.code;
    parentBatchId = step.batchId;
  }
  return 0;
}

/** Continues a paused or interrupted batch in this process and waits for it. */
export async function resumePlan(batchId: string, report: (text: string) => void, parallel?: number, acceptHarness = false): Promise<number> {
  return followBatch(report, async () => {
    await resumeBatch(batchId, parallel, acceptHarness);
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
