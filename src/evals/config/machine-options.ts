import { resolve } from "node:path";
import { useCheckSlots } from "../execution/check-slot.ts";
import { useScratchRoot } from "../paths.ts";

/** Settings for this machine rather than for an experiment. Every command that runs agents or checks accepts them. */
export const MachineFlag = { scratch: "--scratch", checkSlots: "--check-slots" } as const;
export const MachineHelp = `${MachineFlag.scratch} <folder> puts attempt workspaces on a roomier drive, outside any Git repository. ${MachineFlag.checkSlots} <n> sets how many check stages run at once across every eval process on this machine, 1 by default as the approved design asks, since browser checks are timed.`;

export interface MachineOptions { readonly scratch?: string; readonly checkSlots?: number }
export interface SplitArgs { readonly args: readonly string[]; readonly machine: readonly string[]; readonly options: MachineOptions }

/** Separates machine flags from a command's own arguments, so each command still checks its own. */
export function splitMachineOptions(argv: readonly string[]): SplitArgs {
  const args: string[] = [], machine: string[] = [];
  let scratch: string | undefined, checkSlots: number | undefined;
  for (let index = 0; index < argv.length; index += 1) {
    const arg = argv[index] ?? "";
    if (arg !== MachineFlag.scratch && arg !== MachineFlag.checkSlots) { args.push(arg); continue; }
    const value = argv[index + 1];
    if (value === undefined || value.startsWith("--") || machine.includes(arg)) throw new Error(`Give one value for ${arg}.`);
    if (arg === MachineFlag.scratch) scratch = resolve(value);
    else {
      checkSlots = Number(value);
      if (!Number.isInteger(checkSlots) || checkSlots < 1) throw new Error(`Give ${MachineFlag.checkSlots} a whole number of at least 1.`);
    }
    machine.push(arg, value);
    index += 1;
  }
  return { args, machine, options: { ...(scratch === undefined ? {} : { scratch }), ...(checkSlots === undefined ? {} : { checkSlots }) } };
}

/** Applies machine flags for this process. Commands that run checks always share check slots machine-wide. */
export function applyMachineOptions(options: MachineOptions): void {
  if (options.scratch !== undefined) useScratchRoot(options.scratch);
  useCheckSlots(options.checkSlots);
}
