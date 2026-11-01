import { materializeSpecs } from "./specs.ts";
import { materializeSetup, parseSetupKind, SetupKind } from "./setup.ts";
import { materializeSubset, type SubsetOptions } from "./subset.ts";
import { SubsetFlag } from "./subset-selection.ts";
import { MaterializePaths } from "./paths.ts";

export const MaterializeCommand = { specs: "specs", subset: "subset", setup: "setup" } as const;

export function subsetOptions(args: readonly string[]): SubsetOptions {
  if (!args.length) return {};
  if (args.length === 2 && args[0] === SubsetFlag.acceptSelectionChange && args[1]?.trim() && !args[1].startsWith("--")) return { acceptSelectionChange: args[1] };
  throw new Error(`Use ${SubsetFlag.acceptSelectionChange} <reason> to accept a changed Layer 2 selection.`);
}

export async function main(args: readonly string[]): Promise<void> {
  const [command, kind, folder, ...extra] = args;
  if (command === MaterializeCommand.subset) {
    const manifest = await materializeSubset(MaterializePaths.playerSpecs, MaterializePaths.specs, MaterializePaths.requirements, subsetOptions(args.slice(1)));
    process.stdout.write(`${manifest.sourceHash}\n`);
    return;
  }
  if (command === MaterializeCommand.specs && kind === undefined) {
    const manifest = await materializeSpecs();
    process.stdout.write(`${manifest.sourceHash}\n`);
    return;
  }
  if (command === MaterializeCommand.setup && folder && !extra.length) {
    const delivery = parseSetupKind(kind);
    const result = await materializeSetup({ kind: delivery, destination: folder, specs: delivery === SetupKind.inline ? MaterializePaths.playerSpecs : MaterializePaths.specs });
    process.stdout.write(`${result.manifest.sourceHash}\n`);
    return;
  }
  throw new Error("Run node materialize/cli.ts specs, subset or setup <kind> <folder>.");
}

if (import.meta.main) {
  main(process.argv.slice(2)).catch((error: unknown) => {
    process.stderr.write(`${error instanceof Error ? error.message : String(error)}\n`);
    process.exitCode = 1;
  });
}
