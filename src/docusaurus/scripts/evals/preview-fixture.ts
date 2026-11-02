import { mkdir, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { fileURLToPath } from "node:url";
import type { Batch, Choice } from "../../src/components/evals/records/records.types.ts";
import { fixture, FixtureDigest, writeBatch } from "../../src/components/evals/records/test-fixtures.ts";
import { catalogue } from "../../src/components/evals/records/catalogue.ts";
const fixtureRoot = fileURLToPath(new URL("../../.evals-fixture/", import.meta.url));
const choices: readonly Choice[] = [{ tool: "mock", model: "fixture-alpha", effort: "high" }, { tool: "mock", model: "fixture-beta", effort: "high" }];
const setups = ["none", "minimal", "essentials", "full-core"] as const;
export function previewBatch(version: number, reference = false): Batch {
  const base = fixture(), seedRun = base.runs[0], check = base.assessments.find((item) => item.kind === "checks");
  if (seedRun === undefined || check === undefined)
    throw new Error("Incomplete fixture");
  const batchId = `fixture-${reference ? "reference" : "models"}-${version}`;
  const at = `2026-01-0${version}T00:00:00.000Z`;
  const referenceVersion = { commit: "f".repeat(40), contentHash: FixtureDigest, specHash: FixtureDigest, specVersion: 1 };
  const runs = (reference ? [setups[0]] : setups).flatMap((setup, setupIndex) => (reference ? choices.slice(0, 1) : choices).flatMap((agent, agentIndex) => [1, 2].map((repeat) => ({
    ...seedRun, id: `${batchId}-${setup}-${agentIndex}-${repeat}`, batchId, setup, agent, repeat,
    requestedModel: agent.model, reportedModel: agent.model, startedAt: at, finishedAt: at,
    agentSeconds: 60 + setupIndex * 10 + repeat,
    telemetry: repeat === 1 && setupIndex === 0 && agentIndex === 0 ? {
      tokens: { input: 100, cachedInput: 20, cacheWrite: 0, output: 40, reasoning: 0 }, requests: 1,
      peakContext: 200, contextWindow: null, compactions: 0, toolCalls: 3, failedToolCalls: 0,
      openForgeReads: 0, openForgeBytes: 0, reportedCostUsd: null, model: agent.model,
    } : null,
    ...reference ? { implementer: "reference" as const, reference: referenceVersion } : {},
  }))));
  const assessments = runs.flatMap((run, index) => {
    const checked = {
      ...check, id: `${run.id}-checks`, experimentId: batchId, runId: run.id, createdAt: at,
      outcomes: [...check.outcomes.map((outcome) => ({ ...outcome, status: version === 1 && index % 3 === 0 ? "fail" : "pass" })),
      { id: "wish", area: "intent", status: "pass", evidence: "Authored fixture wish" }]
    };
    return [checked, ...choices.map((choice, judgeIndex) => ({
      schemaVersion: 2 as const, kind: "judge" as const, id: `${run.id}-judge-${judgeIndex}`, experimentId: batchId,
      runId: run.id, createdAt: at, versions: check.versions, replacesAssessmentId: null, error: null,
      requestedModel: choice, reportedModel: choice.model, ballots: 3,
      answers: ["rules", "code", "intent"].map((area) => ({
        id: area, area, question: `Fixture ${area} question`,
        yes: judgeIndex === 0 ? 3 : 2, no: judgeIndex === 0 ? 0 : 1, unsure: 0, evidence: ["src/game.ts:1. Authored fixture evidence. <b>Plain text</b>"]
      })),
    }))];
  });
  return {
    experiment: {
      ...base.experiment, id: batchId, createdAt: at, cliVersion: `0.0.${version}-fixture`,
      ...reference ? { reference: referenceVersion } : {}, runs: runs.map((run) => run.id),
      config: { ...base.experiment.config, layer: reference ? 3 : 1, setups: reference ? [setups[0]] : setups, agents: choices, judges: choices, runsPerSetup: 2 }
    },
    runs, setups: (reference ? base.setups : setups.map((id) => ({ ...base.setups[0], schemaVersion: 2 as const, id, label: id, commands: [], contentHash: FixtureDigest, files: [] }))).map((setup) => ({ ...setup, experimentId: batchId })),
    assessments, artifacts: [],
  };
}
export async function writePreviewFixture(): Promise<void> {
  const batches = [previewBatch(1), previewBatch(2), previewBatch(1, true), previewBatch(2, true)];
  await mkdir(join(fixtureRoot, "versions"), { recursive: true });
  for (const batch of batches)
    await writeBatch(join(fixtureRoot, `versions/${batch.experiment.cliVersion}/batches/${batch.experiment.id}`), batch);
  await writeFile(join(fixtureRoot, "index.json"), JSON.stringify(catalogue(batches), null, 2));
  console.log("Wrote authored preview data to .evals-fixture. Set RESULTS_RECORDS=.evals-fixture for a fixture build.");
}
if (import.meta.main)
  await writePreviewFixture();
