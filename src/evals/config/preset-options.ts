import type { EvalConfig } from "./config.types.ts";
import { Flag, option } from "./commands.ts";
import { presetChoices } from "./presets.ts";

export function runPresetOptions(args: readonly string[]): Partial<EvalConfig> {
  return presetChoices({ ...(args.includes(Flag.implementers) ? { implementers: option(args, Flag.implementers) } : {}),
    ...(args.includes(Flag.judges) ? { judges: option(args, Flag.judges) } : {}),
    ...(args.includes(Flag.user) ? { user: option(args, Flag.user) } : {}) });
}

export function rejudgeOptions(args: readonly string[]): { batchId: string; onlyRun: string | undefined; overrides: Partial<EvalConfig> } {
  const at = args.indexOf(Flag.judges);
  const overrides = at < 0 ? {} : presetChoices({ judges: option(args, Flag.judges) });
  const operands = args.filter((_, index) => at < 0 || index !== at && index !== at + 1);
  const [batchId, onlyRun] = operands;
  if (batchId === undefined || operands.length > 2 || operands.some((value) => value.startsWith("--"))) throw new Error("Use rejudge <batch> [attempt] [--judges <preset>].");
  return { batchId, onlyRun, overrides };
}
