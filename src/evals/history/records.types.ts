import type { ModelChoice } from "../config/config.types.ts";
import type { ScopePolicy } from "../materialize/scope.types.ts";
import type { BallotFailure } from "../grading/judge/report.ts";
import type { ReferenceImplementer, ReferenceVersion } from "./reference.ts";
import type { CheckOutcome, CheckSelection, FunctionalResult } from "../grading/grading.types.ts";
import type { BuildResult, JudgeAnswer, LegacyBatchRecord, LegacyRunRecord, PerformanceResult } from "./results.types.ts";
import type { AssessmentKind, FailureKind, PreferenceWinner, RecordSchemaVersion, RunStatus } from "./statuses.ts";

export interface Fingerprints {
  readonly cliPackage: string | null;
  readonly benchmarkInputs: string | null;
  readonly setupContent: string | null;
  readonly checks: string | null;
  readonly rubric: string | null;
  readonly harnessSource: string | null;
  readonly tooling: string | null;
  readonly requestedModels: string | null;
  readonly reportedModels: string | null;
  readonly schema: string | null;
}

export interface ExperimentRecord extends LegacyBatchRecord {
  readonly reference?: ReferenceVersion;
  readonly schemaVersion: typeof RecordSchemaVersion;
  readonly legacy: boolean;
  readonly cliVersion: string | null;
  readonly candidateLabel: string | null;
  readonly fingerprints: Fingerprints;
  readonly capturedAt: string | null;
  readonly pinnedTools: Readonly<Record<string, string>>;
  readonly imageDigests: Readonly<Record<string, string | null>>;
  readonly requestedModels: readonly ModelChoice[];
  readonly reportedModels: readonly (string | null)[] | null;
}

export interface ArtifactRecord {
  readonly schemaVersion: typeof RecordSchemaVersion;
  readonly runId: string;
  readonly assessmentId: string | null;
  readonly path: string;
  readonly bytes: number;
  readonly sha256: string;
}

export interface SetupRecord {
  readonly scope?: ScopePolicy;
  readonly schemaVersion: typeof RecordSchemaVersion;
  readonly id: string;
  readonly experimentId: string;
  readonly label: string;
  readonly commands: readonly (readonly string[])[];
  readonly contentHash: string | null;
  readonly files: readonly { readonly path: string; readonly bytes: number; readonly sha256: string }[];
}

export interface RunRecord extends Omit<LegacyRunRecord, "status" | "checks" | "judge"> {
  readonly implementer?: typeof ReferenceImplementer;
  readonly reference?: ReferenceVersion;
  readonly schemaVersion: typeof RecordSchemaVersion;
  readonly pipelineStatus: RunStatus;
  readonly functionalResult: FunctionalResult | null;
  readonly failureKind: FailureKind | null;
  readonly replacesRunId: string | null;
  readonly replacementReason: string | null;
  readonly artifactHash: string | null;
  readonly requestedModel: string | null;
  readonly reportedModel: string | null;
}

export interface AssessmentVersions {
  readonly spec?: string | null;
  readonly specVersion?: number | null;
  readonly checker: string | null;
  readonly rubric: string | null;
  readonly judge: string | null;
}

interface AssessmentBase {
  readonly schemaVersion: typeof RecordSchemaVersion;
  readonly id: string;
  readonly experimentId: string;
  readonly createdAt: string | null;
  readonly versions: AssessmentVersions;
  readonly replacesAssessmentId: string | null;
  readonly error: string | null;
}

export interface ChecksAssessment extends AssessmentBase {
  readonly kind: typeof AssessmentKind.checks;
  readonly runId: string;
  readonly outcomes: readonly CheckOutcome[];
  readonly selection: CheckSelection;
  readonly build: BuildResult | null;
  readonly performance: PerformanceResult | null;
  readonly shots: readonly string[];
}

export interface JudgeAssessment extends AssessmentBase {
  readonly ballots?: number;
  readonly kind: typeof AssessmentKind.judge;
  readonly runId: string;
  readonly answers: readonly JudgeAnswer[];
  readonly requestedModel: ModelChoice | null;
  readonly reportedModel: string | null;
  readonly formatRetries?: number;
  readonly ballotFailures?: readonly BallotFailure[];
}

export interface PickAssessment extends AssessmentBase {
  readonly kind: typeof AssessmentKind.pick;
  readonly left: { readonly runId: string; readonly artifactHash: string | null; readonly side: typeof PreferenceWinner.left };
  readonly right: { readonly runId: string; readonly artifactHash: string | null; readonly side: typeof PreferenceWinner.right };
  readonly winner: PreferenceWinner;
  readonly revealed: boolean | null;
}

export type AssessmentRecord = ChecksAssessment | JudgeAssessment | PickAssessment;

/** The local viewer adds the chosen assessments without changing the saved run. */
export interface RunView extends RunRecord {
  readonly status: RunStatus;
  readonly checks: LegacyRunRecord["checks"];
  readonly judge: readonly JudgeAnswer[];
  readonly checkOutcomes: readonly CheckOutcome[];
  readonly checkSelection: CheckSelection;
  readonly checksAssessmentId: string | null;
  readonly judgeAssessmentId: string | null;
  readonly checksError: string | null;
  readonly judgeError: string | null;
}
