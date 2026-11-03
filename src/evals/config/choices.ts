export const Tool = { claude: "claude", codex: "codex", mock: "mock" } as const;
export type Tool = (typeof Tool)[keyof typeof Tool];

export const Isolation = { docker: "docker", local: "local" } as const;
export type Isolation = (typeof Isolation)[keyof typeof Isolation];

/** When a batch's judges read each run. Later leaves every judgment to judge --missing, so runs finish after their checks. */
export const Judging = { run: "run", later: "later" } as const;
export type Judging = (typeof Judging)[keyof typeof Judging];

/** When a batch's hidden checks run. Later ends each run after its build and measures, so check --missing grades the saved files as a stage of its own. */
export const Checking = { run: "run", later: "later" } as const;
export type Checking = (typeof Checking)[keyof typeof Checking];

/**
 * How a work packet is done: one agent with the whole packet in one request, a new agent per task with each starting
 * from the previous task's result, or an orchestrator that hands each task to a subagent.
 */
export const WorkMode = { wholePacket: "whole-packet", taskByTask: "task-by-task", orchestrated: "orchestrated" } as const;
export type WorkMode = (typeof WorkMode)[keyof typeof WorkMode];
