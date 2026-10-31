export const JudgedBy = { checks: "checks", judge: "judge" } as const;
export type JudgedBy = (typeof JudgedBy)[keyof typeof JudgedBy];

export const RubricArea = { rules: "rules", code: "code" } as const;
export type RubricArea = (typeof RubricArea)[keyof typeof RubricArea];
