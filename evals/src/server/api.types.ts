import type { SignInStatus } from "../config/auth.ts";
import type { EvalConfig, ModelChoice } from "../config/config.types.ts";
import type { BatchRecord, RunRecord } from "../results/results.types.ts";
import type { RunStatus } from "../results/statuses.ts";
import type { SetupSummary } from "../results/summary.ts";
import type { IntentItem, RubricQuestion } from "../suite/suite.types.ts";
import type { ProgressMessage } from "../batch/progress.ts";

export interface SuiteView {
  readonly id: string;
  readonly title: string;
  readonly prompt: string;
  readonly intent: readonly IntentItem[];
  readonly rubric: readonly RubricQuestion[];
  readonly setups: Readonly<Record<string, string>>;
  readonly fingerprint: string;
  readonly intentEdited: boolean;
}

export interface StateResponse {
  readonly config: EvalConfig;
  readonly suite: SuiteView;
  readonly signIn: SignInStatus;
  readonly docker: boolean;
  readonly batches: readonly BatchRecord[];
  readonly running: string | null;
}

/** A run before the reveal: nothing that could tell its setup. */
export interface HiddenRun {
  readonly id: string;
  readonly status: RunStatus;
  readonly agent: ModelChoice;
}

export interface BatchResponse {
  readonly batch: BatchRecord;
  readonly hidden: readonly HiddenRun[];
  readonly runs: readonly RunRecord[] | null;
  readonly summary: readonly SetupSummary[] | null;
  readonly progress: readonly ProgressMessage[];
  readonly compared: number;
  readonly pairs: number;
}

export interface ComparePair {
  readonly left: string;
  readonly right: string;
  readonly leftGame: string;
  readonly rightGame: string;
}
