export const SchemaVersion = 2;
export const SchemaKinds = ["experiment", "setup", "run", "assessment", "artifact"] as const;
/** When a batch's judges read each run. Later leaves every judgment to judge --missing. */
export const Judging = { run: "run", later: "later" } as const;
