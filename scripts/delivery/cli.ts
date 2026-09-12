#!/usr/bin/env node
import assert from "node:assert/strict";
import { fileURLToPath } from "node:url";
import { deliveryCommands } from "./commands.ts";
import { printCommandHelp } from "./options.ts";
import { npm, reportFailure, run } from "./process.ts";
import { repositoryRoot } from "./repository.ts";

// This entry point uses only Node built-ins: setup must run before dependencies exist.
try {
  const [name, ...args] = process.argv.slice(2);
  if (name === undefined || name === "--help") {
    process.stdout.write("Usage: npx forge <command> [options]\n\n");
    for (const [command, definition] of Object.entries(deliveryCommands)) process.stdout.write(`  ${command.padEnd(18)} ${definition.description}\n`);
    process.stdout.write("\nUse <command> --help for arguments, or dist --plan to inspect the pipeline.\n");
  } else {
    const entry = Object.entries(deliveryCommands).find(([command]) => command === name);
    assert.ok(entry, `Unknown delivery command: ${name}. Run npx forge --help.`);
    const [, command] = entry;
    if (args.includes("--help")) printCommandHelp(name, command);
    else if (!("script" in command)) {
      assert.ok(args.length > 0, "Supply patch, minor, major, prerelease or an explicit version.");
      npm(["version", ...args, "--no-git-tag-version"], repositoryRoot);
    } else {
      const preset = "arguments" in command ? command.arguments : [];
      run(process.execPath, [fileURLToPath(new URL(command.script, import.meta.url)), ...preset, ...args], repositoryRoot);
    }
  }
} catch (error) {
  reportFailure(error);
}
