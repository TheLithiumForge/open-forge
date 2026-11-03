import type { Isolation, Judging, Tool } from "./choices.ts";
import type { ContainerLimits } from "../execution/containers/settings.ts";
import type { FrozenProtocol } from "../execution/conversation/answer-bank.ts";

/** A tool and model. A null model or effort uses the tool's own default. */
export interface ModelChoice {
  readonly tool: Tool;
  readonly model: string | null;
  readonly effort: string | null;
}

export interface JudgeChoice extends ModelChoice {
  readonly repeats: number;
}

export interface EvalLimits {
  readonly runMinutes: number;
  readonly questionRounds: number;
  readonly checkMinutes: number;
}

/** A milestone of a chain. Its runs start from the parent batch's runs with the next defect planted. The plan names the chain and milestone, and run --parent names the parent batch. */
export interface ChainConfig {
  readonly id: string;
  readonly milestone: number;
  readonly parentBatchId?: string;
}

export interface EvalConfig {
  readonly suite: string;
  readonly cases?: readonly string[];
  readonly runsPerSetup: number;
  /** How many runs go at once. Hidden checks still run one at a time. */
  readonly parallelRuns: number;
  readonly setups: readonly string[];
  readonly agents: readonly ModelChoice[];
  readonly judge: JudgeChoice;
  readonly judges?: readonly JudgeChoice[];
  /** Absent means the judges read each run as it finishes. */
  readonly judging?: Judging;
  readonly chain?: ChainConfig;
  readonly simulatedUser: ModelChoice;
  readonly isolation: Isolation;
  readonly containers?: ContainerLimits;
  readonly protocol?: FrozenProtocol;
  readonly layer?: number;
  readonly development?: boolean;
  readonly limits: EvalLimits;
}
