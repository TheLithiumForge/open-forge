import type { BuildResult, CheckResult, PerformanceResult } from "../src/results/results.types.ts";

/** What every suite's hidden checks get: the frozen project, its build, and where to put screenshots. */
export interface SuiteCheckInput {
  readonly project: string;
  readonly build: BuildResult;
  /** The built page, or null when the build failed. */
  readonly html: string | null;
  readonly shotsFolder: string;
}

export interface SuiteCheckOutput {
  readonly checks: readonly CheckResult[];
  readonly performance: PerformanceResult | null;
  readonly shots: readonly string[];
}

export type SuiteChecks = (input: SuiteCheckInput) => Promise<SuiteCheckOutput>;
