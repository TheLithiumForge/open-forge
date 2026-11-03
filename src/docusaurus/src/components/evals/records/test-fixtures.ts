import { createHash, randomUUID } from "node:crypto";
import { mkdir, writeFile } from "node:fs/promises";
import { join } from "node:path";
import type { Batch } from "./records.types.ts";
import { SchemaVersion } from "./schema-values.ts";
import { RecordFiles, AssessmentKinds, AnalysisFile } from "./catalogue.ts";

export const FixtureCheck = "opens";
export const FixtureVersion = "0.0.0-fixture";
export const FixtureDigest = createHash("sha256").update("fixture").digest("hex");
export function fixture(): Batch {
  const choice = { tool: "mock", model: "fixture-model", effort: null };
  const batchId = randomUUID();
  const runId = randomUUID();
  const time = new Date(0).toISOString();
  const run = { schemaVersion: SchemaVersion, id: runId, batchId, setup: "none", agent: choice, repeat: 1, pipelineStatus: "done",
    failureKind: null, replacesRunId: null, replacementReason: null, artifactHash: null, requestedModel: choice.model, reportedModel: choice.model,
    functionalResult: null, error: null, startedAt: time, finishedAt: time, agentSeconds: 60, finalMessage: null, questions: [], telemetry: null, build: null, performance: null, shots: [] } as const;
  const experiment = { schemaVersion: SchemaVersion, id: batchId, suite: "fixture", cliVersion: FixtureVersion, candidateLabel: null, createdAt: time, status: "done", legacy: false,
    runs: [runId], revealed: true, fingerprint: FixtureDigest, intentEdited: false, capturedAt: time, pinnedTools: {}, imageDigests: {}, requestedModels: [choice], reportedModels: [choice.model], versions: {},
    fingerprints: Object.fromEntries(["cliPackage", "benchmarkInputs", "setupContent", "checks", "rubric", "harnessSource", "tooling", "requestedModels", "reportedModels", "schema"].map((key) => [key, FixtureDigest])),
    config: { suite: "fixture", isolation: "docker", setups: [run.setup], runsPerSetup: 1, parallelRuns: 1, agents: [choice], judge: { ...choice, repeats: 1 }, simulatedUser: choice, limits: { runMinutes: 1, questionRounds: 1, checkMinutes: 1 } } } as const;
  const versions = { checker: FixtureDigest, rubric: FixtureDigest, judge: FixtureDigest };
  const base = { schemaVersion: SchemaVersion, id: randomUUID(), experimentId: batchId, createdAt: time, replacesAssessmentId: null, error: null, versions } as const;
  const setup = { schemaVersion: SchemaVersion, id: run.setup, experimentId: batchId, label: run.setup, commands: [], contentHash: FixtureDigest, files: [] } as const;
  const checks = { ...base, kind: "checks", runId, outcomes: [{ id: FixtureCheck, area: "play", status: "pass", evidence: "authored fixture" }],
      selection: { applicable: [FixtureCheck], working: [FixtureCheck], stated: [FixtureCheck] }, build: null, performance: null, shots: [] },
    judge = { ...base, id: randomUUID(), kind: "judge", runId, answers: [], requestedModel: choice, reportedModel: choice.model } as const;
  const tokens = { newInput: 10, cachedInput: 90, cacheWrite: 0, output: 5, reasoning: 1 };
  const analysis = { schemaVersion: SchemaVersion, kind: "analysis", id: randomUUID(), experimentId: batchId, runId, createdAt: time, version: "fixture", replacesAnalysisId: null, error: null,
    method: "rollout", sources: [], tokens, totalsAgree: true, requests: 2, peakContext: 60, compactions: 0, beforeEdit: { requests: 1, input: 40, fresh: 40 }, parts: [{ class: "upfront", fileType: null, input: 100, fresh: 10, peak: 60 }],
    process: null, turns: [{ turn: 1, tokens }], threads: [] } as const;
  return { experiment, runs: [run], setups: [setup], assessments: [{ ...checks, kind: "checks" }, judge], artifacts: [], analyses: [analysis] };
}
export async function writeBatch(folder: string, batch: Batch): Promise<void> {
  await mkdir(join(folder, "assessments"), { recursive: true });
  const lines = (items: readonly unknown[]) => items.map((item) => JSON.stringify(item) + "\n").join("");
  await writeFile(join(folder, RecordFiles.experiment), JSON.stringify(batch.experiment));
  await writeFile(join(folder, RecordFiles.setups), lines(batch.setups));
  await writeFile(join(folder, RecordFiles.runs), lines(batch.runs));
  await writeFile(join(folder, RecordFiles.artifacts), JSON.stringify(batch.artifacts));
  for (const kind of AssessmentKinds) await writeFile(join(folder, "assessments", `${kind}.jsonl`), lines(batch.assessments.filter((assessment) => assessment.kind === kind)));
  if (batch.analyses !== undefined) await writeFile(join(folder, AnalysisFile), lines(batch.analyses));
}
