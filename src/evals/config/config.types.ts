import type { Checking, Isolation, Judging, Tool, WorkMode } from "./choices.ts";
import type { ContainerLimits } from "../execution/containers/settings.ts";
import type { FrozenProtocol } from "../execution/conversation/answer-bank.ts";

/** A tool and model. A null model or effort uses the tool's own default. */
/** The subagents an implementer orchestrates, each on this model and effort. Absent means the implementer works alone. */
export interface AgentTeam {
  readonly model: string | null;
  readonly effort: string | null;
}

export interface ModelChoice {
  readonly tool: Tool;
  readonly model: string | null;
  readonly effort: string | null;
  readonly team?: AgentTeam;
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
  /** How many runs go at once. The machine-wide check slots pace the hidden checks. */
  readonly parallelRuns: number;
  readonly setups: readonly string[];
  readonly agents: readonly ModelChoice[];
  readonly judge: JudgeChoice;
  readonly judges?: readonly JudgeChoice[];
  /** Absent means the judges read each run as it finishes. */
  readonly judging?: Judging;
  /** Absent means each run's checks start after its build. Later needs judging later too, since the judges read the checks' screenshots. */
  readonly checking?: Checking;
  readonly chain?: ChainConfig;
  /** The work packet a plan hands over, and how. Both are absent for a plan of single tasks. */
  readonly packet?: string;
  readonly workMode?: WorkMode;
  readonly simulatedUser: ModelChoice;
  readonly isolation: Isolation;
  readonly containers?: ContainerLimits;
  readonly protocol?: FrozenProtocol;
  readonly layer?: number;
  readonly development?: boolean;
  readonly limits: EvalLimits;
}
