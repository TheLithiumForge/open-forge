import { join } from "node:path";
import { loadSuite } from "../benchmarks/suite.ts";
import { loadConfig } from "../config/config.ts";
import { loadLocalEnv } from "../config/auth.ts";
import { referenceConfig, type ReferenceOptions } from "../config/reference.ts";
import { Tool } from "../config/choices.ts";
import { captureExperiment } from "../history/capture.ts";
import { fingerprint } from "../history/fingerprints.ts";
import { ReferenceImplementer, ReferenceProject } from "../history/reference.ts";
import { BatchStatus, RecordSchemaVersion, RunStatus } from "../history/statuses.ts";
import { HistoryStore } from "../history/store.ts";
import { plannedRun } from "../history/run-records.ts";
import type { RunView } from "../history/records.types.ts";
import { evalsRoot, EvalPaths } from "../paths.ts";
import { dockerAvailable, ensureEvalImage } from "./containers/docker-image.ts";
import { progress } from "./progress.ts";
import { executeRun, type RunServices } from "./run.ts";
import { copyReference } from "./reference-source.ts";
import { opaqueId, removeFolder } from "./workspace.ts";

export interface ReferenceServices {
  readonly config?: typeof loadConfig;
  readonly suite?: typeof loadSuite;
  readonly capture?: typeof captureExperiment;
  readonly copy?: typeof copyReference;
  readonly store?: HistoryStore;
  readonly prepare?: () => Promise<void>;
  readonly run?: RunServices;
}

async function prepare(): Promise<void> {
  loadLocalEnv();
  if (!(await dockerAvailable())) throw new Error("Reference grading requires Docker.");
  await ensureEvalImage();
}

export async function runReference(options: ReferenceOptions, report: (text: string) => void, services: ReferenceServices = {}): Promise<number> {
  const store = services.store ?? new HistoryStore(EvalPaths.runs);
  const config = referenceConfig(await (services.config ?? loadConfig)(), options);
  const suite = await (services.suite ?? loadSuite)(config.suite);
  const id = opaqueId(), runId = opaqueId();
  let batch = await (services.capture ?? captureExperiment)(id, config, suite);
  const snapshot = join(store.batchFolder(id), "reference-source");
  const reference = await (services.copy ?? copyReference)(join(evalsRoot, ReferenceProject), snapshot, fingerprint(suite.documents ?? []));
  const run: RunView = { ...plannedRun(runId, id, ReferenceImplementer, { tool: Tool.mock, model: null, effort: null }, 1), implementer: ReferenceImplementer, reference };
  batch = { ...batch, reference, revealed: true, runs: [runId], fingerprints: { ...batch.fingerprints, setupContent: reference.contentHash } };
  await store.saveBatch(batch);
  await store.saveSetups(id, [{ schemaVersion: RecordSchemaVersion, id: ReferenceImplementer, experimentId: id,
    label: "Reference", commands: [], contentHash: reference.contentHash, files: reference.files }]);
  await store.saveRun(run);
  const unsubscribe = progress.subscribe((message) => { if (message.batchId === id) report(message.text); });
  try {
    report(`Reference batch ${id}. Layer ${options.layer}.`);
    await (services.prepare ?? prepare)();
    batch = { ...batch, status: BatchStatus.running };
    await store.saveBatch(batch);
    await executeRun({ run, suite, config, snapshot, position: "1 of 1", services: { ...services.run, store } });
    const assessments = await store.readAssessments(id);
    const saved = await store.readRun(id, runId);
    const passed = saved.status === RunStatus.done && assessments.every((item) => item.error === null);
    await store.saveBatch({ ...batch, status: passed ? BatchStatus.done : BatchStatus.failed });
    report(`Reference batch ${id} ${passed ? "done" : "failed"}.`);
    return passed ? 0 : 1;
  } catch (error) {
    const detail = error instanceof Error ? error.message : String(error);
    await store.saveRun({ ...await store.readRun(id, runId), status: RunStatus.failed, error: detail, finishedAt: new Date().toISOString() });
    await store.saveBatch({ ...batch, status: BatchStatus.failed });
    throw error;
  } finally {
    unsubscribe();
    await removeFolder(snapshot);
  }
}
