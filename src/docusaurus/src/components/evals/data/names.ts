import type { Choice } from "../records/records.types.ts";
import { canonicalModel, PriceVersions } from "./prices.ts";

/** Reader-facing names for the record ids: every priced model's name, and the short names Claude Code takes, which name no version. */
const ShortModelNames: Readonly<Record<string, string>> = { opus: "Claude Opus", sonnet: "Claude Sonnet", haiku: "Claude Haiku", fable: "Claude Fable" };
function modelName(id: string | null | undefined): string | undefined {
  if (id === null || id === undefined) return undefined;
  const known = canonicalModel(id);
  return ShortModelNames[known] ?? PriceVersions.map((version) => version.models[known]?.name).find((name) => name !== undefined);
}

export function modelLabel(choice: Choice, reported?: string | null): string {
  const name = modelName(reported) ?? modelName(choice.model) ?? reported ?? choice.model ?? choice.tool;
  const solo = choice.effort === null ? name : `${name}, ${choice.effort} reasoning`;
  if (choice.team === undefined) return solo;
  // A team is part of the implementer, so its label names the subagents it leads and how hard they think.
  const team = modelName(choice.team.model) ?? choice.team.model ?? "unreported";
  return `${solo}, leading ${team} subagents${choice.team.effort === null ? "" : ` at ${choice.team.effort}`}`;
}

/** Setups in reading order: the controls, the other setups without Open Forge, then the Open Forge presets from small to large. */
export const SetupNames = {
  "no-setup": "No setup", "code-only": "Code only", "plain-docs": "Plain docs", "organized-docs": "Organized docs", "overloaded-docs": "Overloaded docs", "linked-agents": "Linked AGENTS.md", "spec-in-prompt": "Spec in prompt",
  minimal: "Minimal", "minimal-scoped": "Minimal, scoped", essentials: "Essentials", "essentials-scoped": "Essentials, scoped", "full-core": "Full Core", "full-core-scoped": "Full Core, scoped",
  "full-core-scoped-memory": "Full Core, scoped, with memory",
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

/** How the work was handed over, in reading order. */
export const WorkModeNames = { "single-task": "One task", "whole-packet": "Whole packet", "task-by-task": "Task by task", orchestrated: "Orchestrated" } as const;

/** The tasks and work packets of an existing app, B for a bug fix, F for a feature and P for a packet. Earlier starting points have one case each, named by the starting point. */
export const CaseNames = {
  "l4-b1-best-score": "B1 Best score", "l4-b2-arrow-keys": "B2 Arrow keys", "l4-b3-reduced-motion": "B3 Reduced motion", "l4-b4-silence": "B4 Silence",
  "l4-b5-pasted-saves": "B5 Pasted saves", "l4-f1-name-saves": "F1 Name saves", "l4-f2-tournament": "F2 Tournament",
  "l4-p1-four-bugs": "P1 Four bugs", "l4-p2-name-and-pasted-saves": "P2 Name and pasted saves",
} as const satisfies Readonly<Record<string, string>>;
export type CaseId = keyof typeof CaseNames;

export function isCaseId(id: string): id is CaseId {
  return Object.keys(CaseNames).includes(id);
}

/** How many tasks each work packet holds, for its cost per task. Any other case is one task. */
export const PacketTasks: Readonly<Partial<Record<CaseId, number>>> = { "l4-p1-four-bugs": 4, "l4-p2-name-and-pasted-saves": 2 };

/** The measures a reader can look up. */
export const MeasureNames = { "check-coverage": "Check coverage", "judge-score": "Judge score", "judge-quality": "Judge quality", "judge-adherence": "Judge adherence", tokens: "Tokens", cost: "Cost", time: "Time" } as const;
export type MeasureId = keyof typeof MeasureNames;
