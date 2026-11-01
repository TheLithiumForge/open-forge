import { materializeSpecs } from "./specs.ts";
import { materializeSetup, parseSetupKind } from "./setup.ts";

export const MaterializeCommand = { specs: "specs", setup: "setup" } as const;

export async function main(args: readonly string[]): Promise<void> {
  const [command, kind, folder, ...extra] = args;
  if (command === MaterializeCommand.specs && kind === undefined) {
    const manifest = await materializeSpecs();
    process.stdout.write(`${manifest.sourceHash}\n`);
    return;
  }
  if (command === MaterializeCommand.setup && folder && !extra.length) {
    const result = await materializeSetup({ kind: parseSetupKind(kind), destination: folder });
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
