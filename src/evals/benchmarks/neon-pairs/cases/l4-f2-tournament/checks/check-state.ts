export const WorkerVersion = 1;
export const CheckCount = { minimum: 10, maximum: 50 } as const;
export const Status = { pass: 'pass', fail: 'fail', error: 'error', blocked: 'blocked' } as const;
export type Status = (typeof Status)[keyof typeof Status];
export const Kind = { feature: 'feature', repair: 'repair' } as const;
export type Kind = (typeof Kind)[keyof typeof Kind];
export const ChangeType = { feature: 'F', repair: 'R', combined: 'FR' } as const;
export type ChangeType = (typeof ChangeType)[keyof typeof ChangeType];
