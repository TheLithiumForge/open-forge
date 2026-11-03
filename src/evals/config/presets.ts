import { Tool } from "./choices.ts";
import type { EvalConfig, JudgeChoice, ModelChoice } from "./config.types.ts";
import { modelKey } from "./judges.ts";
import { MatrixAgents, MatrixEffort, MatrixJudges, MatrixModels } from "./matrix.ts";

export const PresetName = { claude: "claude", codex: "codex", both: "both", codexOrchestrators: "codex-orchestrators" } as const;
/** Sol leading Sol subagents and Sol leading Luna subagents, Sol at xhigh and Luna at max, for orchestrated work. The team is part of the implementer, so results filter by it. */
export const CodexOrchestrators = [
  { ...MatrixAgents[0], team: { model: MatrixModels.codex, effort: MatrixEffort.xhigh } },
  // Luna always runs at max reasoning, as the maintainer asked.
  { ...MatrixAgents[0], team: { model: MatrixModels.codexUser, effort: MatrixEffort.max } },
] as const satisfies readonly ModelChoice[];
export const CustomPreset = "Custom";
export const ImplementerPresets = {
  [PresetName.claude]: [MatrixAgents[1]], [PresetName.codex]: [MatrixAgents[0]], [PresetName.both]: MatrixAgents, [PresetName.codexOrchestrators]: CodexOrchestrators,
} as const satisfies Readonly<Record<string, readonly ModelChoice[]>>;
export const JudgePresets = {
  [PresetName.claude]: [MatrixJudges[0]], [PresetName.codex]: [MatrixJudges[1]], [PresetName.both]: MatrixJudges,
} as const satisfies Readonly<Record<string, readonly JudgeChoice[]>>;
export const UserPresets = {
  [PresetName.claude]: { tool: Tool.claude, model: MatrixModels.claudeUser, effort: MatrixEffort.medium },
  [PresetName.codex]: { tool: Tool.codex, model: MatrixModels.codexUser, effort: MatrixEffort.max },
} as const satisfies Readonly<Record<string, ModelChoice>>;

function named<T>(presets: Readonly<Record<string, T>>, name: string): T {
  const choice = Object.entries(presets).find(([key]) => key === name)?.[1];
  if (choice === undefined) throw new Error(`Unknown preset ${JSON.stringify(name)}. Choose ${Object.keys(presets).join(", ")}.`);
  return choice;
}
export const implementerPreset = (name: string): readonly ModelChoice[] => named(ImplementerPresets, name);
export const judgePreset = (name: string): readonly JudgeChoice[] => named(JudgePresets, name);
export const userPreset = (name: string): ModelChoice => named(UserPresets, name);

export interface PresetSelection { readonly implementers?: string; readonly judges?: string; readonly user?: string }
export function presetChoices(selection: PresetSelection): Partial<EvalConfig> {
  const agents = selection.implementers === undefined ? undefined : implementerPreset(selection.implementers);
  const judges = selection.judges === undefined ? undefined : judgePreset(selection.judges);
  const simulatedUser = selection.user === undefined ? undefined : userPreset(selection.user);
  const judge = judges?.[0];
  return { ...(agents === undefined ? {} : { agents }), ...(judge === undefined || judges === undefined ? {} : { judge, judges }),
    ...(simulatedUser === undefined ? {} : { simulatedUser }) };
}

function matching<T>(presets: Readonly<Record<string, T>>, matches: (choice: T) => boolean): string {
  return Object.entries(presets).find(([, choice]) => matches(choice))?.[0] ?? CustomPreset;
}
function modelsMatch(left: readonly ModelChoice[], right: readonly ModelChoice[]): boolean {
  return left.length === right.length && left.every((choice, index) => right[index] !== undefined && modelKey(choice) === modelKey(right[index]));
}
export function matchImplementers(choices: readonly ModelChoice[]): string {
  return matching(ImplementerPresets, (preset) => modelsMatch(preset, choices));
}
export function matchJudges(choices: readonly JudgeChoice[]): string {
  return matching(JudgePresets, (preset) => modelsMatch(preset, choices) && preset.every((judge, index) => judge.repeats === choices[index]?.repeats));
}
export function matchUser(choice: ModelChoice): string {
  return matching(UserPresets, (preset) => modelKey(preset) === modelKey(choice));
}
