import { randomInt } from "node:crypto";
import { writeFile } from "node:fs/promises";
import { dirname, join } from "node:path";
import { dockerAvailable, ensureEvalImage, ImageVersions } from "../agents/docker-image.ts";
import { Isolation } from "../config/choices.ts";
import { loadConfig } from "../config/config.ts";
import { ResultFiles } from "../paths.ts";
import type { BatchRecord, RunRecord } from "../results/results.types.ts";
import { BatchStatus, rerunStatuses, RunStatus } from "../results/statuses.ts";
import { batchFolder, listBatches, readBatch, readRun, saveBatch, saveRun } from "../results/store.ts";
import { loadSuite } from "../suite/suite.ts";
import { progress } from "./progress.ts";
import { executeRun } from "./run.ts";
import { runInParallel } from "./run-queue.ts";
import { prepareSnapshot } from "./snapshot.ts";
import { opaqueId, removeFolder } from "./workspace.ts";

let activeBatch: string | null = null;

export function runningBatch(): string | null {
  return activeBatch;
}

function shuffled<T>(items: readonly T[]): T[] {
  const copy = [...items];
  for (let index = copy.length - 1; index > 0; index -= 1) {
    const other = randomInt(index + 1);
    const current = copy[index];
    const target = copy[other];
    if (current === undefined || target === undefined) continue;
    copy[index] = target;
    copy[other] = current;
  }
  return copy;
}

function plannedRun(batchId: string, setup: string, agent: RunRecord["agent"], repeat: number): RunRecord {
  return {
    id: opaqueId(), batchId, setup, agent, repeat, status: RunStatus.pending, error: null, startedAt: null, finishedAt: null,
    agentSeconds: null, finalMessage: null, questions: [], telemetry: null, build: null, checks: [], performance: null, judge: [], shots: [],
  };
}

/** Plans every run of the config, in random order so no setup always runs first, and starts them. */
export async function startBatch(): Promise<BatchRecord> {
  if (activeBatch !== null) throw new Error("A batch is already running.");
  const config = await loadConfig();
  const suite = await loadSuite(config.suite);
  const id = `${new Date().toISOString().replaceAll(/[:.]/g, "-")}-${opaqueId()}`;
  const runs = shuffled(config.agents.flatMap((agent) => config.setups.flatMap((setup) => Array.from({ length: config.runsPerSetup }, (_, repeat) => plannedRun(id, setup, agent, repeat + 1)))));
  const batch: BatchRecord = {
    id, suite: suite.id, fingerprint: suite.fingerprint, intentEdited: suite.intentEdited, config, createdAt: new Date().toISOString(),
    status: BatchStatus.preparing, runs: runs.map((run) => run.id), revealed: false,
    versions: { claude: ImageVersions.claude, codex: ImageVersions.codex, openForge: ImageVersions.openForge, node: process.version },
  };
  await saveBatch(batch);
  for (const run of runs) await saveRun(run);
  void executeBatch(batch.id);
  return batch;
}

/** A batch left preparing or running by a stopped server can only be resumed, so it shows as paused. */
export async function pauseInterruptedBatches(): Promise<void> {
  const interrupted: readonly string[] = [BatchStatus.preparing, BatchStatus.running];
  for (const batch of await listBatches()) {
    if (interrupted.includes(batch.status)) await saveBatch({ ...batch, status: BatchStatus.paused });
  }
}

export async function resumeBatch(batchId: string): Promise<void> {
  if (activeBatch !== null) throw new Error("A batch is already running.");
  void executeBatch(batchId);
}

async function executeBatch(batchId: string): Promise<void> {
  activeBatch = batchId;
  let batch = await readBatch(batchId);
  const setStatus = async (status: BatchRecord["status"]): Promise<void> => {
    batch = { ...batch, status };
    await saveBatch(batch);
  };
  const snapshots = new Map<string, string>();
  try {
    await setStatus(BatchStatus.preparing);
    const suite = await loadSuite(batch.suite);
    if (batch.config.isolation === Isolation.docker) {
      if (!(await dockerAvailable())) throw new Error("Docker isn't running. Start Docker, or set isolation to local.");
      progress.report(batchId, "Preparing the Docker image. The first time takes a few minutes.");
      await ensureEvalImage();
    }
    const records = [];
    for (const setup of batch.config.setups) {
      progress.report(batchId, `Installing setup ${snapshots.size + 1} of ${batch.config.setups.length}.`);
      const snapshot = await prepareSnapshot(batchId, suite, setup);
      snapshots.set(setup, snapshot.folder);
      records.push(snapshot.record);
    }
    await writeFile(join(batchFolder(batchId), ResultFiles.setups), `${JSON.stringify(records, null, 2)}\n`);
    await setStatus(BatchStatus.running);
    let limited = false;
    // Batches saved before parallel runs existed have no setting, so they run one at a time.
    await runInParallel(batch.runs, batch.config.parallelRuns || 1, async (runId, index) => {
      const run = await readRun(batchId, runId);
      if (!rerunStatuses.includes(run.status)) return;
      const done = await executeRun({ run, suite, config: batch.config, snapshot: snapshots.get(run.setup) ?? "", position: `${index + 1} of ${batch.runs.length}` });
      if (done.status === RunStatus.limited) limited = true;
    }, () => limited);
    if (limited) return await setStatus(BatchStatus.paused);
    await setStatus(BatchStatus.done);
    progress.report(batchId, "The batch is done.");
  } catch (error) {
    progress.report(batchId, `The batch stopped: ${error instanceof Error ? error.message : String(error)}`);
    await setStatus(BatchStatus.failed);
  } finally {
    // Every start or resume installs the setups again, so their snapshots never outlive the batch.
    for (const snapshot of snapshots.values()) await removeFolder(dirname(snapshot));
    activeBatch = null;
  }
}
