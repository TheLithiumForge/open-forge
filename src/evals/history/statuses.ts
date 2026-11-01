export const RunStatus = {
  pending: "pending",
  agent: "agent",
  building: "building",
  checking: "checking",
  judging: "judging",
  done: "done",
  failed: "failed",
  // The tool hit a usage or rate limit. The run starts over when the batch resumes.
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
