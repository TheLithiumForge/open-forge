import { randomInt } from "node:crypto";
import { existsSync } from "node:fs";
import { readFile } from "node:fs/promises";
import { dirname, join } from "node:path";
import { ResultFiles } from "../paths.ts";
import { dockerAvailable, ensureEvalImage } from "./containers/docker-image.ts";
import { Isolation, Judging, Tool } from "../config/choices.ts";
import { loadConfig } from "../config/config.ts";
import type { EvalConfig } from "../config/config.types.ts";
import { suiteProtocol } from "../config/protocol.ts";
import { answerBank, freezeProtocol } from "./conversation/answer-bank.ts";
import type { BatchRecord, RunRecord } from "../history/results.types.ts";
import { BatchStatus, FailureKind, RecordSchemaVersion, rerunStatuses, RunStatus } from "../history/statuses.ts";
import { listBatches, readBatch, readRun, readRuns, readSetups, runFolder, saveBatch, saveRun, saveSetups } from "../history/store.ts";
import { captureExperiment, resumableExperiment, withReportedModels } from "../history/capture.ts";
import { fingerprint } from "../history/fingerprints.ts";
import { artifactHash } from "../history/artifacts.ts";
import { plannedRun, replacementRun } from "../history/run-records.ts";
import type { ChainPosition, SetupRecord } from "../history/records.types.ts";
import type { Suite } from "../benchmarks/suite.types.ts";
import { chainedRunStart, parentFiles, planChainRuns } from "./chain-runs.ts";
import { loadSuite } from "../benchmarks/suite.ts";
import { checksForSuite } from "../benchmarks/registry.ts";
import type { FileHash } from "../materialize/files.ts";
import type { ScopePolicy } from "../materialize/scope.types.ts";
import { progress } from "./progress.ts";
import { executeRun } from "./run.ts";
import { completeSavedJudgments } from "../grading/rejudge.ts";
import { JudgeLimited } from "../grading/judge/reply.ts";
import { runInParallel } from "./run-queue.ts";
import { generateSetup } from "../config/setups.ts";
import { opaqueId, removeFolder, suiteForSetup } from "./workspace.ts";
import { requireSignIns } from "../config/auth.ts";
import { configuredJudges } from "../config/judges.ts";

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

/** Every tool a batch calls: its implementers, judges and simulated user. */
export function batchTools(config: EvalConfig): Tool[] {
  return [...config.agents, ...configuredJudges(config), config.simulatedUser].map((choice) => choice.tool);
}

export function planBatchRuns(config: EvalConfig, batchId: string): RunRecord[] {
  return shuffled(config.agents.flatMap((agent) => config.setups.flatMap((setup) => Array.from({ length: config.runsPerSetup }, (_, repeat) => plannedRun(opaqueId(), batchId, setup, agent, repeat + 1)))));
}

/** Plans every run of the config, in random order so no setup always runs first, and starts them. */
export async function startBatch(selected?: EvalConfig): Promise<BatchRecord> {
  if (activeBatch !== null) throw new Error("A batch is already running.");
  const requested = selected ?? await loadConfig();
  requireSignIns(batchTools(requested));
  const suite = await loadSuite(requested.suite);
  const config = { ...requested, limits: { ...requested.limits, runMinutes: suite.runMinutes ?? requested.limits.runMinutes },
    protocol: suite.documents === undefined ? requested.protocol ?? suiteProtocol(suite) : freezeProtocol(answerBank(suite.intent), [suite.prompt, ...suite.documents]) };
  const id = `${new Date().toISOString().replaceAll(/[:.]/g, "-")}-${opaqueId()}`;
  const chained = config.chain === undefined ? null : await planChain(config, id, suite);
  const runs = chained?.runs ?? planBatchRuns(config, id);
  const batch: BatchRecord = { ...await captureExperiment(id, config, suite), runs: runs.map((run) => run.id), ...(chained === null ? {} : { chain: chained.position }) };
  await saveBatch(batch);
  for (const run of runs) await saveRun(run);
  void executeBatch(batch.id);
  return batch;
}

/** A chain milestone's runs, each paired with its parent run in the previous milestone's batch, and the pairs that can't start. */
async function planChain(config: EvalConfig, batchId: string, suite: Suite): Promise<{ readonly runs: RunRecord[]; readonly position: ChainPosition }> {
  const chain = config.chain, item = suite.brownfield?.case;
  if (chain?.parentBatchId === undefined) throw new Error("A chain milestone needs its parent batch.");
  if (item?.seedPatch === null || item === undefined) throw new Error("A chain milestone needs a Layer 4 case with a planted defect.");
  const parentBatchId = chain.parentBatchId, parent = await readBatch(parentBatchId);
  const seedPatch = await readFile(join(item.root, item.seedPatch), "utf8");
  const planned = await planChainRuns(config, batchId, await readRuns(parent), (run) => parentFiles(runFolder, parentBatchId, run.id), seedPatch, opaqueId);
  if (planned.runs.length === 0) throw new Error(`No parent run can start this milestone: ${planned.skipped.map((pair) => `${pair.setup} ${pair.repeat} ${pair.reason}`).join(", ")}.`);
  return { runs: shuffled(planned.runs), position: { id: chain.id, milestone: chain.milestone, parentBatchId, skipped: planned.skipped } };
}

/** Whether resume replaces an interrupted or limited attempt. One replacement caps infrastructure failures, and a usage limit isn't one. A run that already has a replacement keeps its status, so it's never replaced twice. */
export function replacedOnResume(run: Pick<RunRecord, "status" | "replacesRunId">, alreadyReplaced: boolean): boolean {
  return !alreadyReplaced && (run.replacesRunId === null || run.status === RunStatus.limited);
}

/**
 * Whether resume grades a run's saved files instead of starting it over, because the agent finished and only grading stopped.
 * A first build runs before the files are saved, so a building run with saved files was stopped while its saved files were graded.
 */
export function gradedOnResume(run: Pick<RunRecord, "status" | "agentSeconds">, filesSaved: boolean): boolean {
  const grading: readonly RunStatus[] = [RunStatus.building, RunStatus.checking, RunStatus.judging];
  return grading.includes(run.status) && run.agentSeconds !== null && filesSaved;
}

/** A batch left preparing or running by a stopped server can only be resumed, so it shows as paused. */
export async function pauseInterruptedBatches(): Promise<void> {
  const interrupted: readonly string[] = [BatchStatus.preparing, BatchStatus.running];
  for (const batch of await listBatches()) {
    if (!batch.legacy && interrupted.includes(batch.status)) await saveBatch({ ...batch, status: BatchStatus.paused });
  }
}

/** Continues a paused batch. A parallel count overrides the saved one for this resume only, since it changes no attempt. */
export async function resumeBatch(batchId: string, parallel?: number, acceptHarness = false): Promise<void> {
  if (activeBatch !== null) throw new Error("A batch is already running.");
  const batch = await readBatch(batchId);
  if (batch.reference !== undefined) throw new Error("Start a new reference grading with the reference command.");
  if (batch.legacy) throw new Error("Legacy batches are read only. Start a new batch.");
  requireSignIns(batchTools(batch.config));
  const resumable = resumableExperiment(batch, await captureExperiment(batchId, batch.config, await loadSuite(batch.suite), batch.candidateLabel), acceptHarness);
  if (resumable !== batch) await saveBatch({ ...batch, harnessChanges: resumable.harnessChanges ?? [] });
  void executeBatch(batchId, parallel);
}

async function executeBatch(batchId: string, parallel?: number): Promise<void> {
  activeBatch = batchId;
  let batch = await readBatch(batchId);
  const setStatus = async (status: BatchRecord["status"]): Promise<void> => {
    batch = { ...batch, status };
    await saveBatch(batch);
  };
  const snapshots = new Map<string, string>();
  const baselines = new Map<string, readonly FileHash[]>();
  const scopes = new Map<string, ScopePolicy>();
  try {
    await setStatus(BatchStatus.preparing);
    const suite = await loadSuite(batch.suite);
    const chainCase = suite.brownfield?.case;
    const chainSeed = batch.chain === undefined || chainCase === undefined || chainCase.seedPatch === null ? null : await readFile(join(chainCase.root, chainCase.seedPatch), "utf8");
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
      if (snapshot.materialized?.scope !== undefined) scopes.set(setup, snapshot.materialized.scope);
      records.push(snapshot.record);
    }
    const setups: SetupRecord[] = records.map((record) => {
      const scope = scopes.get(record.setup);
      return { schemaVersion: RecordSchemaVersion, id: record.setup, experimentId: batchId,
        label: record.label, commands: record.commands, files: record.files, contentHash: artifactHash(record.files), ...(scope === undefined ? {} : { scope }) };
    });
    const setupContent = fingerprint(setups.map(({ id, contentHash }) => ({ id, contentHash })));
    if ((await readSetups(batch)).length === 0) {
      batch = { ...batch, fingerprints: { ...batch.fingerprints, setupContent } };
      await saveBatch(batch);
      await saveSetups(batchId, setups);
    } else if (batch.fingerprints.setupContent !== setupContent) throw new Error("The setup content changed since this batch started.");
    await setStatus(BatchStatus.running);
    let limited = false, judgmentsMissing = false;
    const replaced = new Set((await readRuns(batch)).flatMap((run) => run.replacesRunId === null ? [] : [run.replacesRunId]));
    // Batches saved before parallel runs existed have no setting, so they run one at a time.
    await runInParallel(batch.runs, parallel ?? (batch.config.parallelRuns || 1), async (runId, index) => {
      try { await attempt(runId, index); }
      catch (error) {
        // Judging never blocks implementation. The judgment stays missing, and judge --missing fills it later.
        if (!(error instanceof JudgeLimited)) throw error;
        judgmentsMissing = true;
      }
    }, () => limited);
    async function attempt(runId: string, index: number): Promise<void> {
      let run: RunRecord = await readRun(batchId, runId);
      if (!rerunStatuses.includes(run.status)) {
        if (batch.config.judging === Judging.later) return;
        await completeSavedJudgments(suiteForSetup(suite, run.setup), run, batch.config,
          { checker: batch.fingerprints.checks, rubric: batch.fingerprints.rubric, judge: batch.versions["judge"] ?? null });
        return;
      }
      const attemptSuite = suiteForSetup(suite, run.setup);
      const saved = gradedOnResume(run, existsSync(join(runFolder(batchId, run.id), ResultFiles.files)));
      const chainStarts: string[] = [];
      // A chained run starts from its parent's files, so its start and the scope guard's baseline are its own.
      const prepare = async (target: RunRecord, savedFiles: boolean) => {
        const parentBatchId = batch.chain?.parentBatchId ?? null, parentRunId = target.parentRunId ?? null;
        const chained = parentBatchId === null || parentRunId === null || chainSeed === null ? null : await chainedRunStart({ folder: runFolder(batchId, target.id), parentBatchId, parentRunId,
          parentFiles: parentFiles(runFolder, parentBatchId, parentRunId), scope: scopes.get(target.setup), seedPatch: chainSeed, project: suite.project, saved: savedFiles });
        if (chained?.start) chainStarts.push(dirname(chained.start.folder));
        const checks = suite.brownfield === undefined ? undefined : checksForSuite(suite, chained?.files ?? baselines.get(target.setup), scopes.get(target.setup));
        return { snapshot: chained?.start?.folder ?? snapshots.get(target.setup) ?? "", services: checks === undefined ? {} : { checks } };
      };
      try {
        if (saved) {
          const { services } = await prepare(run, true);
          const graded = await executeRun({ run, suite: attemptSuite, config: batch.config, snapshot: "", position: `${index + 1} of ${batch.runs.length}`, services, savedFiles: true });
          if (graded.status === RunStatus.limited) limited = true;
          return;
        }
        if (run.status !== RunStatus.pending) {
          if (run.finishedAt === null) await saveRun({ ...run, status: RunStatus.failed, failureKind: FailureKind.infrastructure, finishedAt: new Date().toISOString() });
          if (!replacedOnResume(run, replaced.has(run.id))) return;
          replaced.add(run.id);
          run = replacementRun(run, opaqueId(), "Resume after an interrupted or limited attempt.");
          batch = { ...batch, runs: [...batch.runs, run.id] };
          await saveRun(run);
          await saveBatch(batch);
        }
        const first = await prepare(run, false);
        let done = await executeRun({ run, suite: attemptSuite, config: batch.config, snapshot: first.snapshot, position: `${index + 1} of ${batch.runs.length}`, services: first.services });
        if (done.status === RunStatus.failed && done.failureKind === FailureKind.infrastructure && done.replacesRunId === null) {
          const replacement = replacementRun(done, opaqueId(), "One replacement for an infrastructure failure.");
          batch = { ...batch, runs: [...batch.runs, replacement.id] };
          await saveRun(replacement);
          await saveBatch(batch);
          const next = await prepare(replacement, false);
          done = await executeRun({ run: replacement, suite: attemptSuite, config: batch.config, snapshot: next.snapshot, position: `${index + 1} replacement`, services: next.services });
        }
        if (done.status === RunStatus.limited) limited = true;
      } finally {
        for (const folder of chainStarts) await removeFolder(folder);
      }
    }
    batch = withReportedModels(batch, await readRuns(batch));
    if (judgmentsMissing) progress.report(batchId, `A judge hit its usage limit, so some judgments are missing. Fill them later with npm run evals -- judge ${batchId} --missing.`);
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
