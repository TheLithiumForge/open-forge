import { adaptCheck, countChecks } from "../grading/counting.ts";
import { assessRun, defaultSelection } from "./assessments.ts";
import { unknownFingerprints } from "./fingerprints.ts";
import type { AssessmentRecord, ChecksAssessment, ExperimentRecord, PickAssessment, RunView, SetupRecord } from "./records.types.ts";
import type { LegacyBatchRecord, LegacyRunRecord, Preference } from "./results.types.ts";
import { AssessmentKind, PreferenceWinner, RecordSchemaVersion } from "./statuses.ts";

export function legacyExperiment(batch: LegacyBatchRecord): ExperimentRecord {
  return {
    ...batch, schemaVersion: RecordSchemaVersion, legacy: true, cliVersion: batch.versions["openForge"] ?? null,
    candidateLabel: null, fingerprints: unknownFingerprints(), capturedAt: null,
    pinnedTools: batch.versions, imageDigests: {}, requestedModels: batch.config.agents, reportedModels: null,
  };
}

export function legacyAssessments(run: LegacyRunRecord): AssessmentRecord[] {
  const versions = { checker: null, rubric: null, judge: null };
  const base = { schemaVersion: RecordSchemaVersion, experimentId: run.batchId, createdAt: run.finishedAt, versions, replacesAssessmentId: null, error: null } as const;
  const outcomes = run.checks.map(adaptCheck);
  const checks: ChecksAssessment = {
    ...base, id: `legacy-checks-${run.id}`, kind: AssessmentKind.checks, runId: run.id,
    outcomes, selection: defaultSelection(outcomes), build: run.build, performance: run.performance, shots: run.shots,
  };
  return [checks, {
    ...base, id: `legacy-judge-${run.id}`, kind: AssessmentKind.judge, runId: run.id,
    answers: run.judge, requestedModel: null, reportedModel: null,
  }];
}

export function legacyRun(run: LegacyRunRecord, assessments: readonly AssessmentRecord[] = []): RunView {
  const { status, checks, judge, ...fields } = run;
  const outcomes = checks.map(adaptCheck);
  return assessRun({
    ...fields, schemaVersion: RecordSchemaVersion, pipelineStatus: status,
    functionalResult: countChecks(outcomes, defaultSelection(outcomes)), failureKind: null,
    replacesRunId: null, replacementReason: null, artifactHash: null,
    requestedModel: run.agent.model, reportedModel: run.telemetry?.model ?? null,
  }, [...legacyAssessments(run), ...assessments]);
}

export function legacySetups(batch: ExperimentRecord): SetupRecord[] {
  return batch.config.setups.map((id) => ({
    schemaVersion: RecordSchemaVersion, id, experimentId: batch.id, label: id, commands: [], contentHash: null, files: [],
  }));
}

export function legacyPicks(batchId: string, preferences: readonly Preference[]): PickAssessment[] {
  return preferences.map((pick, index) => ({
    schemaVersion: RecordSchemaVersion, id: `legacy-pick-${index}`, experimentId: batchId, createdAt: pick.at,
    versions: { checker: null, rubric: null, judge: null }, replacesAssessmentId: null, error: null, kind: AssessmentKind.pick,
    left: { runId: pick.left, artifactHash: null, side: PreferenceWinner.left },
    right: { runId: pick.right, artifactHash: null, side: PreferenceWinner.right }, winner: pick.winner, revealed: null,
  }));
}
