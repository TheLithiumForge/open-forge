import assert from "node:assert/strict";
import { deliveryCommands, type DeliveryCommand } from "./commands.ts";
import { parseArgs } from "node:util";

const flags = {
  rid: { type: "string", description: "Native RID; defaults to this host and must match it." },
  targets: { type: "string", description: "Comma-separated wrapper/release RIDs; defaults to all six supported targets." },
  sha: { type: "boolean", description: "Append the current commit SHA to the built version." },
  offline: { type: "boolean", description: "Restore using cached dependencies only." },
  "no-restore": { type: "boolean", description: "Reuse an earlier restore." },
  "skip-tests": { type: "boolean", description: "Skip .NET qualification and npm installation tests; mark packages untested." },
  plan: { type: "boolean", description: "Print stages and effective arguments without executing them." },
  tag: { type: "string", description: "Required npm distribution tag, such as preview or latest." },
  "dry-run": { type: "boolean", description: "Validate and print an offline publication plan; no registry contact." },
  from: { type: "string", description: "Collected release directory; defaults to artifacts/release." },
  preid: { type: "string", description: "Prerelease identifier passed to npm version, such as beta." },
  help: { type: "boolean", description: "Show this command's options." },
} as const;

export function readOptions(command: DeliveryCommand, args = process.argv.slice(2)) {
  const allowed: readonly (keyof typeof flags)[] = [...deliveryCommands[command].options, "help"];
  const { values } = parseArgs({ args, options: flags });
  for (const name of Object.keys(values))
    assert.ok(
      allowed.some((key) => key === name),
      `${command} does not accept --${name}. Run npx forge ${command} --help.`,
    );
  assert.ok(!(values.offline && values["no-restore"]), "Choose --offline or --no-restore, not both.");
  if (values.help) {
    printCommandHelp(command, deliveryCommands[command]);
    return undefined;
  }
  return values;
}

export function printCommandHelp(name: string, command: { description: string; options: readonly (keyof typeof flags)[] }): void {
  const names: readonly (keyof typeof flags)[] = [...command.options, "help"];
  process.stdout.write(`Usage: npx forge ${name} [options]
${command.description}
${names
  .map((name) => {
    const flag = flags[name];
    return `  --${name}${flag.type === "string" ? " <value>" : ""}  ${flag.description}`;
  })
  .join("\n")}
`);
}
