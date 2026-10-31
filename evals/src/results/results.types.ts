import type { EvalConfig, ModelChoice } from "../config/config.types.ts";
import type { RunStatus, BatchStatus, PreferenceWinner } from "./statuses.ts";

/** Input is uncached input. Cached input and cache writes are counted apart. */
export interface TokenCounts {
  readonly input: number;
  readonly cachedInput: number;
  readonly cacheWrite: number;
  readonly output: number;
  readonly reasoning: number;
}

/** Null means the tool didn't report it, never zero. */
export interface AgentTelemetry {
  readonly tokens: TokenCounts | null;
  readonly requests: number | null;
  readonly peakContext: number | null;
  readonly contextWindow: number | null;
  readonly compactions: number | null;
  readonly toolCalls: number;
  readonly failedToolCalls: number;
  readonly openForgeReads: number;
  readonly openForgeBytes: number | null;
  readonly reportedCostUsd: number | null;
  readonly model: string | null;
  readonly sessionId: string | null;
}

export interface QuestionRound {
  readonly question: string;
  readonly answer: string;
}

export interface CheckResult {
  readonly id: string;
  readonly area: string;
  /** Null when the check couldn't run, for example because an earlier step failed. */
  readonly passed: boolean | null;
  readonly detail: string;
}

export interface BuildResult {
  readonly passed: boolean;
  readonly typecheck: boolean | null;
  readonly seconds: number;
  readonly packages: number | null;
  readonly html: string | null;
  readonly bytes: number | null;
  readonly log: string;
}

export interface PerformanceResult {
  readonly firstRenderMs: number | null;
  readonly largeBoardTapP95Ms: number | null;
}

export interface JudgeAnswer {
  readonly id: string;
  readonly area: string;
  readonly question: string;
  readonly yes: number;
  readonly no: number;
  readonly unsure: number;
  readonly evidence: readonly string[];
}

export interface RunRecord {
  readonly id: string;
  readonly batchId: string;
  readonly setup: string;
  readonly agent: ModelChoice;
  readonly repeat: number;
  readonly status: RunStatus;
  readonly error: string | null;
  readonly startedAt: string | null;
  readonly finishedAt: string | null;
  readonly agentSeconds: number | null;
  readonly finalMessage: string | null;
  readonly questions: readonly QuestionRound[];
  readonly telemetry: AgentTelemetry | null;
  readonly build: BuildResult | null;
  readonly checks: readonly CheckResult[];
  readonly performance: PerformanceResult | null;
  readonly judge: readonly JudgeAnswer[];
  readonly shots: readonly string[];
}

export interface BatchRecord {
  readonly id: string;
  readonly suite: string;
  readonly fingerprint: string;
  readonly intentEdited: boolean;
  readonly config: EvalConfig;
  readonly createdAt: string;
  readonly status: BatchStatus;
  readonly runs: readonly string[];
  readonly versions: Readonly<Record<string, string>>;
  readonly revealed: boolean;
}

export interface Preference {
  readonly left: string;
  readonly right: string;
  readonly winner: PreferenceWinner;
  readonly at: string;
}
