import type { EvalConfig, JudgeChoice, ModelChoice } from "./config.types.ts";
import { integer, modelChoice, object } from "./validation.ts";

export function modelKey(choice: ModelChoice): string {
  return JSON.stringify([choice.tool, choice.model, choice.effort]);
}

export function judgeChoice(value: unknown): JudgeChoice {
  const source = object(value);
  return { ...modelChoice(source), repeats: integer(source["repeats"]) };
}

export function readJudges(single: unknown, multiple: unknown): Pick<EvalConfig, "judge" | "judges"> {
  if (multiple === undefined) return { judge: judgeChoice(single) };
  if (!Array.isArray(multiple) || multiple.length === 0) throw new Error("Pick at least one judge.");
  const judges = multiple.map(judgeChoice), first = judges[0];
  if (first === undefined || new Set(judges.map(modelKey)).size !== judges.length) throw new Error("List each judge once.");
  if (single !== undefined && JSON.stringify(judgeChoice(single)) !== JSON.stringify(first)) throw new Error("judge must match the first entry in judges.");
  return { judge: first, judges };
}

export function configuredJudges(config: Pick<EvalConfig, "judge" | "judges">): readonly JudgeChoice[] {
  return config.judges ?? [config.judge];
}
