export const RunStatus = {
  pending: "pending",
  agent: "agent",
  building: "building",
  checking: "checking",
  judging: "judging",
  done: "done",
  failed: "failed",
  // The provider hit a usage limit or rejected the sign-in. The run starts over when the batch resumes.
  limited: "limited",
} as const;
export type RunStatus = (typeof RunStatus)[keyof typeof RunStatus];

/** A run in one of these states runs again when its batch resumes. */
export const rerunStatuses: readonly RunStatus[] = [
  RunStatus.pending,
  RunStatus.agent,
  RunStatus.building,
  RunStatus.checking,
  RunStatus.judging,
  RunStatus.limited,
];

export const BatchStatus = {
  preparing: "preparing",
  running: "running",
  paused: "paused",
  done: "done",
  failed: "failed",
} as const;
export type BatchStatus = (typeof BatchStatus)[keyof typeof BatchStatus];

export const PreferenceWinner = { left: "left", right: "right", tie: "tie" } as const;
export type PreferenceWinner = (typeof PreferenceWinner)[keyof typeof PreferenceWinner];

export const RecordSchemaVersion = 2;
export const CheckStatus = { pass: "pass", fail: "fail", error: "error", blocked: "blocked" } as const;
export type CheckStatus = (typeof CheckStatus)[keyof typeof CheckStatus];
export const AssessmentKind = { checks: "checks", judge: "judge", pick: "pick" } as const;
export type AssessmentKind = (typeof AssessmentKind)[keyof typeof AssessmentKind];
export const FailureKind = { infrastructure: "infrastructure", attempt: "attempt", grading: "grading" } as const;
export type FailureKind = (typeof FailureKind)[keyof typeof FailureKind];
export const IntentCheckArea = "Intent";
