import type { CheckStatus } from "../history/statuses.ts";

export interface CheckOutcome {
  readonly id: string;
  readonly area: string;
  readonly status: CheckStatus;
  readonly evidence: string;
}

export interface Coverage {
  readonly passed: number;
  readonly applicable: number;
  readonly blocked: number;
  readonly errors: number;
  readonly share: number | null;
}

export interface CheckSelection {
  readonly applicable: readonly string[];
  readonly working: readonly string[];
  readonly stated: readonly string[];
}

export interface FunctionalResult {
  readonly coverage: Coverage;
  readonly workingGate: boolean | null;
  readonly statedSuccess: boolean | null;
}

export interface MeasurementSummary {
  readonly values: readonly (number | null)[];
  readonly samples: number;
  readonly missing: number;
  readonly mean: number | null;
}
