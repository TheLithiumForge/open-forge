export const SchemaVersion = 2;
export const SchemaKinds = ["experiment", "setup", "run", "assessment", "artifact", "analysis"] as const;
/** When a batch's judges read each run. Later leaves every judgment to judge --missing. */
export const Judging = { run: "run", later: "later" } as const;
/** How an analysis split a run's tokens: from each request's exact usage, from events alone, or not at all. */
export const AnalysisMethods = ["rollout", "stream", "events", "none"] as const;
export const ThreadRoles = ["main", "subagent"] as const;
/** One agent does the work, or an orchestrator hands each task to a subagent. */
export const WorkModes = ["one-agent", "orchestrated"] as const;
