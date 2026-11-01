import { materializeSpecs } from "./specs.ts";
import { materializeSetup, parseSetupKind, SetupKind } from "./setup.ts";
import { materializeSubset } from "./subset.ts";
import { MaterializePaths } from "./paths.ts";

export const MaterializeCommand = { specs: "specs", subset: "subset", setup: "setup" } as const;

export async function main(args: readonly string[]): Promise<void> {
  const [command, kind, folder, ...extra] = args;
  if (command === MaterializeCommand.subset && kind === undefined) {
    const manifest = await materializeSubset(MaterializePaths.playerSpecs);
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
  throw new Error("Run node materialize/cli.ts specs or node materialize/cli.ts setup <kind> <folder>.");
}

if (import.meta.main) {
  main(process.argv.slice(2)).catch((error: unknown) => {
    process.stderr.write(`${error instanceof Error ? error.message : String(error)}\n`);
    process.exitCode = 1;
  });
}
