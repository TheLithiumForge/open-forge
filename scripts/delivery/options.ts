import assert from "node:assert/strict";
import { AllTargets } from "./targets.ts";
import { deliveryCommands, type DeliveryCommand } from "./commands.ts";
import { parseArgs } from "node:util";

const flags = {
  rid: { type: "string", description: "Native RID; defaults to this host and must match it." },
  targets: { type: "string", description: "Comma-separated wrapper/release RIDs; defaults to all six supported targets. Does not cross-compile." },
  sha: { type: "boolean", description: "Append the current commit SHA to the built version." },
  offline: { type: "boolean", description: "Restore using cached dependencies only; cannot combine with --no-restore." },
  "no-restore": { type: "boolean", description: "Reuse an earlier restore; cannot combine with --offline." },
  "skip-tests": { type: "boolean", description: "Skip .NET qualification and npm installation tests; mark packages untested." },
  plan: { type: "boolean", description: "Print stages and effective arguments without executing them." },
  tag: { type: "string", description: "Required npm distribution tag, such as preview or latest." },
  "dry-run": { type: "boolean", description: "Validate and print an offline publication plan; no registry contact." },
  from: { type: "string", description: "Collected release directory; defaults to artifacts/release." },
  preid: { type: "string", description: "Prerelease identifier passed to npm version, such as beta." },
  help: { type: "boolean", short: "h", description: "Show this command's options." },
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

export function printCommandHelp(
  name: string,
  command: { description: string; options: readonly (keyof typeof flags)[]; usage?: string; details: readonly string[]; examples: readonly string[] },
): void {
  const names: readonly (keyof typeof flags)[] = [...command.options, "help"];
  process.stdout.write(`Usage: npx forge ${command.usage ?? `${name} [options]`}
${command.description}

${command.details.join("\n")}

Options:
${names
  .map((name) => {
    const flag = flags[name];
    return `  ${name === "help" ? "-h, " : ""}--${name}${flag.type === "string" ? " <value>" : ""}  ${flag.description}`;
  })
  .join("\n")}
${command.options.includes("targets") || command.options.includes("rid") ? `\nSupported RIDs: ${AllTargets.join(", ")}.\n` : ""}
Examples (from the repository root):
${command.examples.map((example) => `  npx forge ${example}`).join("\n")}

Guide: scripts/delivery/README.md
`);
}
