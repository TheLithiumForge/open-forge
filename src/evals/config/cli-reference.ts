import { writeFile } from "node:fs/promises";
import { join } from "node:path";
import { evalsRoot } from "../paths.ts";
import { Command, CommandHelp, FlagHelp } from "./commands.ts";

/** The generated docs page, so the CLI help and the docs say the same thing. */
export const CliReferencePage = join(evalsRoot, "..", "docusaurus", "docs", "evals", "cli.md");
const ExamplePrefix = /\s*Examples?: /;

function section(command: Command): string[] {
  const [description = "", examples = ""] = CommandHelp[command].split(ExamplePrefix);
  const commands = examples.split(" | ").map((example) => example.trim()).filter((example) => example.length > 0);
  return [`## \`${command}\``, "", description.trim(), ...(commands.length === 0 ? [] : ["", "```sh", ...commands, "```"]), ""];
}

/** Renders every command and flag from the definitions the CLI itself uses. */
export function cliReference(): string {
  const flags = FlagHelp.map((row) => `| \`${row.flag}\` | ${row.commands === "any" ? "Any command" : row.commands.map((command) => `\`${command}\``).join(", ")} | ${row.effect} |`);
  return [
    "---", "title: CLI reference", "---", "", "# CLI reference",
    "Open Forge Evals is one command, `npm run evals -- <command>`, run from the repository root.", "",
    "This page is generated from the CLI's own help, so it always matches `npm run evals -- help`. Every setting is a flag. Sign-in values are the only exception, and they stay in `src/evals/.env.local`.", "",
    ...Object.values(Command).filter((command) => command !== Command.help).flatMap(section),
    "## Flags", "", "| Flag | Commands | Effect |", "| --- | --- | --- |", ...flags, "",
    "Back to [Evals](index.md).", "",
  ].join("\n");
}

if (import.meta.main) {
  await writeFile(CliReferencePage, cliReference());
  console.log(`Wrote ${CliReferencePage}`);
}
