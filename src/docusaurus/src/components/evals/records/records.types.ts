export interface Choice { readonly tool: string; readonly model: string | null; readonly effort: string | null }
export interface ReferenceVersion { readonly commit: string; readonly contentHash: string; readonly specHash: string; readonly specVersion: number }
export interface GradingVersions { readonly checker: string | null; readonly rubric: string | null; readonly judge: string | null; readonly spec?: string | null; readonly specVersion?: number | null }
export interface Experiment {
  readonly reference?: ReferenceVersion;
  readonly schemaVersion: 2; readonly id: string; readonly suite: string; readonly cliVersion: string | null; readonly candidateLabel: string | null;
  readonly createdAt: string; readonly status: string; readonly legacy: boolean; readonly runs: readonly string[];
  readonly config: { readonly layer?: number; readonly judges?: readonly Choice[]; readonly judge?: Choice; readonly isolation: string; readonly setups: readonly string[]; readonly runsPerSetup: number; readonly agents: readonly Choice[] };
  readonly fingerprints: Readonly<Record<string, string | null>>; readonly versions: Readonly<Record<string, string>>;
}
export interface Outcome { readonly id: string; readonly area: string; readonly status: string; readonly evidence: string }
export interface Functional {
  readonly coverage: { readonly passed: number; readonly applicable: number; readonly errors: number; readonly share: number | null };
  readonly workingGate: boolean | null; readonly statedSuccess: boolean | null;
}
export interface Run {
  readonly implementer?: "reference"; readonly reference?: ReferenceVersion;
  readonly schemaVersion: 2; readonly id: string; readonly batchId: string; readonly setup: string; readonly agent: Choice; readonly repeat: number;
  readonly pipelineStatus: string; readonly failureKind: string | null; readonly replacesRunId: string | null; readonly replacementReason: string | null;
  readonly artifactHash: string | null; readonly requestedModel: string | null; readonly reportedModel: string | null;
  readonly functionalResult: Functional | null; readonly startedAt: string | null; readonly finishedAt: string | null; readonly agentSeconds: number | null;
  readonly error: string | null; readonly finalMessage: string | null;
  readonly telemetry: { readonly tokens: { readonly input: number; readonly cachedInput: number; readonly cacheWrite: number; readonly output: number } | null;
    readonly toolCalls: number; readonly failedToolCalls: number; readonly peakContext: number | null } | null;
}
export interface Setup {
  readonly schemaVersion: 2; readonly id: string; readonly experimentId: string; readonly label: string;
  readonly commands: readonly (readonly string[])[]; readonly contentHash: string | null;
  readonly files: readonly { readonly path: string; readonly bytes: number; readonly sha256: string }[];
}
export interface Artifact {
  readonly schemaVersion: 2; readonly runId: string; readonly assessmentId: string | null; readonly path: string; readonly bytes: number; readonly sha256: string;
}
export interface Ballot { readonly id: string; readonly area: string; readonly question: string; readonly yes: number; readonly no: number; readonly unsure: number; readonly evidence: readonly string[] }
interface AssessmentBase {
  readonly schemaVersion: 2; readonly id: string; readonly experimentId: string; readonly createdAt: string | null;
  readonly versions: GradingVersions;
  readonly replacesAssessmentId: string | null; readonly error: string | null;
}
export interface Checks extends AssessmentBase {
  readonly kind: "checks"; readonly runId: string; readonly outcomes: readonly Outcome[];
  readonly selection: { readonly applicable: readonly string[]; readonly working: readonly string[]; readonly stated: readonly string[] };
  readonly build: { readonly passed: boolean; readonly typecheck: boolean | null; readonly seconds: number; readonly packages: number | null; readonly html: string | null; readonly bytes: number | null; readonly log: string } | null;
  readonly performance: { readonly firstRenderMs: number | null; readonly largeBoardTapP95Ms: number | null } | null; readonly shots: readonly string[];
}
export interface Judge extends AssessmentBase { readonly ballots?: number; readonly kind: "judge"; readonly runId: string; readonly answers: readonly Ballot[]; readonly requestedModel: Choice | null; readonly reportedModel: string | null }
export interface Pick extends AssessmentBase {
  readonly kind: "pick"; readonly left: { readonly runId: string; readonly artifactHash: string | null; readonly side: "left" };
  readonly right: { readonly runId: string; readonly artifactHash: string | null; readonly side: "right" };
  readonly winner: "left" | "right" | "tie"; readonly revealed: boolean | null;
}
export type Assessment = Checks | Judge | Pick;
export interface Batch {
  readonly experiment: Experiment; readonly setups: readonly Setup[]; readonly runs: readonly Run[];
  readonly assessments: readonly Assessment[]; readonly artifacts: readonly Artifact[];
}
