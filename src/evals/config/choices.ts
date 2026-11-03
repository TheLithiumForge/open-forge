export const Tool = { claude: "claude", codex: "codex", mock: "mock" } as const;
export type Tool = (typeof Tool)[keyof typeof Tool];

export const Isolation = { docker: "docker", local: "local" } as const;
export type Isolation = (typeof Isolation)[keyof typeof Isolation];

/** When a batch's judges read each run. Later leaves every judgment to judge --missing, so runs finish after their checks. */
export const Judging = { run: "run", later: "later" } as const;
export type Judging = (typeof Judging)[keyof typeof Judging];

/** How a setup works: one agent does every task, or an orchestrator hands each task to a subagent of its own. */
export const WorkMode = { oneAgent: "one-agent", orchestrated: "orchestrated" } as const;
export type WorkMode = (typeof WorkMode)[keyof typeof WorkMode];
