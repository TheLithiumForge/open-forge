import { randomUUID } from "node:crypto";
import { mkdtemp, rm } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { Isolation, Tool } from "../config/choices.ts";
import type { EvalConfig } from "../config/config.types.ts";
import type { LegacyBatchRecord, LegacyRunRecord } from "./results.types.ts";
import type { ExperimentRecord, RunView } from "./records.types.ts";
import { legacyExperiment, legacyRun } from "./legacy.ts";
import { BatchStatus, RunStatus } from "./statuses.ts";
import { createFingerprints } from "./fingerprints.ts";
import { HistoryStore } from "./store.ts";

export const FixtureTime = new Date(0).toISOString();
export const FixtureCliVersion = "0.0.0-candidate";
export const FixtureCheckId = "opens";
export const FixtureVersions = { checker: "checker-version", rubric: "rubric-version", judge: "judge-version" } as const;
export const FixtureChoice = { tool: Tool.mock, model: null, effort: null } as const;

export function legacyFixture(): { batch: LegacyBatchRecord; run: LegacyRunRecord } {
  const id = randomUUID();
  const config: EvalConfig = {
    suite: "fixture", runsPerSetup: 1, parallelRuns: 1, setups: ["plain"], agents: [FixtureChoice],
    judge: { ...FixtureChoice, repeats: 1 }, simulatedUser: FixtureChoice, isolation: Isolation.local,
    limits: { runMinutes: 1, questionRounds: 1, checkMinutes: 1 },
  };
  const batch: LegacyBatchRecord = {
    id: randomUUID(), suite: config.suite, config, fingerprint: "historical-fingerprint", intentEdited: false,
    createdAt: FixtureTime, status: BatchStatus.done, runs: [id], versions: { openForge: FixtureCliVersion }, revealed: true,
  };
  const run: LegacyRunRecord = {
    id, batchId: batch.id, setup: config.setups[0] ?? "", agent: FixtureChoice, repeat: 1, status: RunStatus.done,
    error: null, startedAt: FixtureTime, finishedAt: FixtureTime, agentSeconds: 60, finalMessage: null, questions: [], telemetry: null,
    build: { passed: true, typecheck: true, seconds: 1, packages: 1, html: "index.html", bytes: 1, log: "" },
    checks: [{ id: FixtureCheckId, area: "play", passed: true, detail: FixtureCheckId }], judge: [], performance: null, shots: [],
  };
  return { batch, run };
}

export function v2Fixture(): { batch: ExperimentRecord; run: RunView } {
  const old = legacyFixture();
  const fingerprints = createFingerprints({ cliPackage: {}, benchmarkInputs: {}, setupContent: {}, checks: {}, rubric: {}, harnessSource: {}, tooling: {}, requestedModels: [], reportedModels: null });
  return { batch: { ...legacyExperiment(old.batch), legacy: false, capturedAt: FixtureTime, fingerprints }, run: legacyRun(old.run) };
}

export async function temporaryHistory() {
  const root = await mkdtemp(join(tmpdir(), "eval-history-"));
  return { root, store: new HistoryStore(root), cleanup: () => rm(root, { recursive: true, force: true }) };
}
