import { randomInt } from "node:crypto";
import { dirname } from "node:path";
import { dockerAvailable, ensureEvalImage } from "./containers/docker-image.ts";
import { Isolation, Tool } from "../config/choices.ts";
import { loadConfig } from "../config/config.ts";
import type { EvalConfig } from "../config/config.types.ts";
import { suiteProtocol } from "../config/protocol.ts";
import { answerBank, freezeProtocol } from "./conversation/answer-bank.ts";
import type { BatchRecord, RunRecord } from "../history/results.types.ts";
import { BatchStatus, FailureKind, RecordSchemaVersion, rerunStatuses, RunStatus } from "../history/statuses.ts";
import { listBatches, readBatch, readRun, readRuns, readSetups, saveBatch, saveRun, saveSetups } from "../history/store.ts";
import { assertFrozenExecution, captureExperiment, withReportedModels } from "../history/capture.ts";
import { fingerprint } from "../history/fingerprints.ts";
import { artifactHash } from "../history/artifacts.ts";
import { plannedRun, replacementRun } from "../history/run-records.ts";
import type { SetupRecord } from "../history/records.types.ts";
import { loadSuite } from "../benchmarks/suite.ts";
import { checksForSuite } from "../benchmarks/registry.ts";
import type { FileHash } from "../materialize/files.ts";
import { progress } from "./progress.ts";
import { executeRun } from "./run.ts";
import { completeSavedJudgments } from "../grading/rejudge.ts";
import { runInParallel } from "./run-queue.ts";
import { generateSetup } from "../config/setups.ts";
import { opaqueId, removeFolder, suiteForSetup } from "./workspace.ts";

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

export function planBatchRuns(config: EvalConfig, batchId: string): RunRecord[] {
  return shuffled(config.agents.flatMap((agent) => config.setups.flatMap((setup) => Array.from({ length: config.runsPerSetup }, (_, repeat) => plannedRun(opaqueId(), batchId, setup, agent, repeat + 1)))));
}

/** Plans every run of the config, in random order so no setup always runs first, and starts them. */
export async function startBatch(selected?: EvalConfig): Promise<BatchRecord> {
  if (activeBatch !== null) throw new Error("A batch is already running.");
  const requested = selected ?? await loadConfig();
  const suite = await loadSuite(requested.suite);
  const config = { ...requested, limits: { ...requested.limits, runMinutes: suite.runMinutes ?? requested.limits.runMinutes },
    protocol: suite.documents === undefined ? requested.protocol ?? suiteProtocol(suite) : freezeProtocol(answerBank(suite.intent), [suite.prompt, ...suite.documents]) };
  const id = `${new Date().toISOString().replaceAll(/[:.]/g, "-")}-${opaqueId()}`;
  const runs = planBatchRuns(config, id);
  const batch: BatchRecord = { ...await captureExperiment(id, config, suite), runs: runs.map((run) => run.id) };
  await saveBatch(batch);
  for (const run of runs) await saveRun(run);
  void executeBatch(batch.id);
  return batch;
}

/** A batch left preparing or running by a stopped server can only be resumed, so it shows as paused. */
export async function pauseInterruptedBatches(): Promise<void> {
  const interrupted: readonly string[] = [BatchStatus.preparing, BatchStatus.running];
  for (const batch of await listBatches()) {
    if (!batch.legacy && interrupted.includes(batch.status)) await saveBatch({ ...batch, status: BatchStatus.paused });
  }
}

export async function resumeBatch(batchId: string): Promise<void> {
  if (activeBatch !== null) throw new Error("A batch is already running.");
  const batch = await readBatch(batchId);
  if (batch.legacy) throw new Error("Legacy batches are read only. Start a new batch.");
  assertFrozenExecution(batch, await captureExperiment(batchId, batch.config, await loadSuite(batch.suite), batch.candidateLabel));
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
  const baselines = new Map<string, readonly FileHash[]>();
  try {
    await setStatus(BatchStatus.preparing);
    const suite = await loadSuite(batch.suite);
    if (batch.config.isolation === Isolation.docker || batch.config.agents.some((agent) => agent.tool !== Tool.mock)) {
      if (!(await dockerAvailable())) throw new Error("Docker isn't running. Start Docker, or set isolation to local.");
      progress.report(batchId, "Preparing the Docker image. The first time takes a few minutes.");
      await ensureEvalImage();
    }
    const records = [];
    for (const setup of batch.config.setups) {
      progress.report(batchId, `Installing setup ${snapshots.size + 1} of ${batch.config.setups.length}.`);
      const snapshot = await generateSetup(suite, setup);
      snapshots.set(setup, snapshot.folder);
      baselines.set(setup, snapshot.record.files);
      records.push(snapshot.record);
    }
    const setups: SetupRecord[] = records.map((record) => ({ schemaVersion: RecordSchemaVersion, id: record.setup, experimentId: batchId,
      label: record.label, commands: record.commands, files: record.files, contentHash: artifactHash(record.files) }));
    const setupContent = fingerprint(setups.map(({ id, contentHash }) => ({ id, contentHash })));
    if ((await readSetups(batch)).length === 0) {
      batch = { ...batch, fingerprints: { ...batch.fingerprints, setupContent } };
      await saveBatch(batch);
      await saveSetups(batchId, setups);
    } else if (batch.fingerprints.setupContent !== setupContent) throw new Error("The setup content changed since this batch started.");
    await setStatus(BatchStatus.running);
    let limited = false;
    // Batches saved before parallel runs existed have no setting, so they run one at a time.
    await runInParallel(batch.runs, batch.config.parallelRuns || 1, async (runId, index) => {
      let run: RunRecord = await readRun(batchId, runId);
      if (!rerunStatuses.includes(run.status)) {
        await completeSavedJudgments(suiteForSetup(suite, run.setup), run, batch.config,
          { checker: batch.fingerprints.checks, rubric: batch.fingerprints.rubric, judge: batch.versions["judge"] ?? null });
        return;
      }
      if (run.status !== RunStatus.pending) {
        if (run.finishedAt === null) await saveRun({ ...run, status: RunStatus.failed, failureKind: FailureKind.infrastructure, finishedAt: new Date().toISOString() });
        if (run.replacesRunId !== null) return;
        run = replacementRun(run, opaqueId(), "Resume after an interrupted or limited attempt.");
        batch = { ...batch, runs: [...batch.runs, run.id] };
        await saveRun(run);
        await saveBatch(batch);
      }
      const attemptSuite = suiteForSetup(suite, run.setup);
      const checks = suite.brownfield === undefined ? undefined : checksForSuite(suite, baselines.get(run.setup));
      const services = checks === undefined ? {} : { checks };
      let done = await executeRun({ run, suite: attemptSuite, config: batch.config, snapshot: snapshots.get(run.setup) ?? "", position: `${index + 1} of ${batch.runs.length}`, services });
      if (done.status === RunStatus.failed && done.failureKind === FailureKind.infrastructure && done.replacesRunId === null) {
        const replacement = replacementRun(done, opaqueId(), "One replacement for an infrastructure failure.");
        batch = { ...batch, runs: [...batch.runs, replacement.id] };
        await saveRun(replacement);
        await saveBatch(batch);
        done = await executeRun({ run: replacement, suite: attemptSuite, config: batch.config, snapshot: snapshots.get(run.setup) ?? "", position: `${index + 1} replacement`, services });
      }
      if (done.status === RunStatus.limited) limited = true;
    }, () => limited);
    batch = withReportedModels(batch, await readRuns(batch));
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
