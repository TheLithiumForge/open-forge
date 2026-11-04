import type { EvalConfig } from "./config.types.ts";
import { Flag, option } from "./commands.ts";
import { presetChoices } from "./presets.ts";

export function runPresetOptions(args: readonly string[]): Partial<EvalConfig> {
  return presetChoices({ ...(args.includes(Flag.implementers) ? { implementers: option(args, Flag.implementers) } : {}),
    ...(args.includes(Flag.judges) ? { judges: option(args, Flag.judges) } : {}),
    ...(args.includes(Flag.user) ? { user: option(args, Flag.user) } : {}) });
}

/** How many attempts may run at once. A plan's own setting applies unless this caps it. */
export function parallelOption(args: readonly string[]): number {
  const value = Number(option(args, Flag.parallel));
  if (!Number.isInteger(value) || value < 1) throw new Error("Give --parallel a whole number of at least 1.");
  return value;
}

export function judgeOptions(args: readonly string[]): { batchId: string; onlyRun: string | undefined; overrides: Partial<EvalConfig>; missingOnly: boolean } {
  const flags = [Flag.judges, Flag.parallel].map((flag) => args.indexOf(flag)).filter((at) => at >= 0);
  const overrides = { ...(args.includes(Flag.judges) ? presetChoices({ judges: option(args, Flag.judges) }) : {}), ...(args.includes(Flag.parallel) ? { parallelRuns: parallelOption(args) } : {}) };
  const missing = args.filter((arg) => arg === Flag.missing).length;
  const operands = args.filter((arg, index) => arg !== Flag.missing && !flags.some((at) => index === at || index === at + 1));
  const [batchId, onlyRun] = operands;
  if (batchId === undefined || operands.length > 2 || missing > 1 || operands.some((value) => value.startsWith("--"))) throw new Error("Use judge <batch> [run] [--judges <preset>] [--parallel <n>] [--missing].");
  return { batchId, onlyRun, overrides, missingOnly: missing === 1 };
}
