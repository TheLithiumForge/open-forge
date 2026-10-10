#!/usr/bin/env node
import assert from "node:assert/strict";
import { fileURLToPath } from "node:url";
import { deliveryCommands } from "./commands.ts";
import { printCommandHelp } from "./options.ts";
import { npm, reportFailure, run } from "./process.ts";
import { extractLogOption } from "./log-option.ts";
import { runLogged } from "./process-log.ts";
import { repositoryRoot } from "./repository.ts";

// This entry point uses only Node built-ins: setup must run before dependencies exist.
try {
  const [name, ...input] = process.argv.slice(2);
  const { args, log } = extractLogOption(input);
  if (log) {
    await runLogged(process.execPath, [fileURLToPath(import.meta.url), ...(name === undefined ? [] : [name]), ...args], repositoryRoot, log);
  } else if (name === undefined || name === "--help" || name === "-h") {
    process.stdout.write("Usage: npx forge <command> [options]\nRepository build, test, package and release commands. Run from the repository root.\n\n");
    for (const [command, definition] of Object.entries(deliveryCommands)) process.stdout.write(`  ${command.padEnd(18)} ${definition.description}\n`);
    process.stdout.write(`
Start here:
  npx forge setup
  npx forge test --no-restore
  npx forge dist --no-restore --plan

Pack without running tests:
  npx forge dist --skip-tests --no-restore

Use npx forge <command> --help (or -h) for prerequisites, options and examples.
Native commands target this host; --targets selects wrapper dependencies.
Use --log <file> to print and record a command's stdout and stderr.
Publication requires --tag; add --dry-run for an offline preview.
Guide: scripts/delivery/README.md
`);
  } else {
    const entry = Object.entries(deliveryCommands).find(([command]) => command === name);
    assert.ok(entry, `Unknown delivery command: ${name}. Run npx forge --help.`);
    const [, command] = entry;
    if (args.includes("--help") || args.includes("-h")) printCommandHelp(name, command);
    else if (!("script" in command)) {
      assert.ok(args.length > 0, "Supply patch, minor, major, prerelease or an explicit version.");
      npm(["version", ...args, "--no-git-tag-version"], repositoryRoot);
    } else {
      const preset = "arguments" in command ? command.arguments : [];
      const invocation = [fileURLToPath(new URL(command.script, import.meta.url)), ...preset, ...args];
      run(process.execPath, invocation, repositoryRoot);
    }
  }
} catch (error) {
  reportFailure(error);
}
