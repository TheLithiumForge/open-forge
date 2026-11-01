import type { BuildResult, PerformanceResult } from "../history/results.types.ts";
import type { CheckOutcome, CheckSelection } from "../grading/grading.types.ts";
import type { StageOptions } from "../execution/containers/stage.ts";

/** What every suite's hidden checks get: the frozen project, its build, and where to put screenshots. */
export interface SuiteCheckInput {
  readonly project: string;
  readonly build: BuildResult;
  /** The built page, or null when the build failed. */
  readonly html: string | null;
  readonly shotsFolder: string;
  readonly stage?: StageOptions;
}

export interface SuiteCheckOutput {
  readonly checks: readonly CheckOutcome[];
  readonly selection?: CheckSelection;
  readonly performance: PerformanceResult | null;
  readonly shots: readonly string[];
}

export type SuiteChecks = (input: SuiteCheckInput) => Promise<SuiteCheckOutput>;
