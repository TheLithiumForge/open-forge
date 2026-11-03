import type { Choice } from "../records/records.types.ts";

/** Reader-facing names for the record ids. Each id doubles as its definition snippet's name. */
const ModelNames: Readonly<Record<string, string>> = { "claude-opus-5-5": "Claude Opus 5.5", opus: "Claude Opus", sonnet: "Claude Sonnet", "gpt-6.1-sol": "GPT-6.1 Sol", "gpt-6-luna": "GPT-6 Luna" };

export function modelLabel(choice: Choice, reported?: string | null): string {
  const name = ModelNames[reported ?? ""] ?? ModelNames[choice.model ?? ""] ?? reported ?? choice.model ?? choice.tool;
  return choice.effort === null ? name : `${name}, ${choice.effort} reasoning`;
}

/** Setups in reading order: the controls, the other setups without Open Forge, then the Open Forge presets from small to large. */
export const SetupNames = {
  "no-setup": "No setup", "plain-docs": "Plain docs", "linked-agents": "Linked AGENTS.md", "spec-in-prompt": "Spec in prompt",
  "code-only": "Code only", minimal: "Minimal", essentials: "Essentials", "full-core": "Full Core", "full-core-scoped": "Full Core, scoped",
} as const satisfies Readonly<Record<string, string>>;
export type SetupId = keyof typeof SetupNames;
const SetupOrder: readonly string[] = Object.keys(SetupNames);

export function isSetupId(id: string): id is SetupId {
  return SetupOrder.includes(id);
}

export function setupName(id: string): string {
  return isSetupId(id) ? SetupNames[id] : id;
}

/** Sorts setups in reading order. Unknown setups follow the known ones. */
export function setupRank(id: string): number {
  const rank = SetupOrder.indexOf(id);
  return rank === -1 ? SetupOrder.length : rank;
}

/** The cases of an existing app. Earlier starting points have one case each, named by the starting point. */
export const CaseNames = {
  "l4-f01": "Feature", "l4-r01": "Repair", "l4-fr01": "Feature and repair",
  "l4-r02": "Best score", "l4-r03": "Arrow keys", "l4-r04": "Reduced motion", "l4-r05": "Silence",
} as const satisfies Readonly<Record<string, string>>;
export type CaseId = keyof typeof CaseNames;

export function isCaseId(id: string): id is CaseId {
  return Object.keys(CaseNames).includes(id);
}

/** The measures a reader can look up. */
export const MeasureNames = { "check-coverage": "Check coverage", "judge-score": "Judge score", tokens: "Tokens", time: "Time" } as const;
export type MeasureId = keyof typeof MeasureNames;
