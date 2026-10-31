import type { Isolation, Tool } from "./choices.ts";

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

export interface EvalConfig {
  readonly suite: string;
  readonly runsPerSetup: number;
  /** How many runs go at once. Hidden checks still run one at a time. */
  readonly parallelRuns: number;
  readonly setups: readonly string[];
  readonly agents: readonly ModelChoice[];
  readonly judge: JudgeChoice;
  readonly simulatedUser: ModelChoice;
  readonly isolation: Isolation;
  readonly limits: EvalLimits;
}
