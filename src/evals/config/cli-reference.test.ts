import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { test } from "node:test";
import { CliReferencePage, cliReference } from "./cli-reference.ts";
import { Command, Flag, FlagHelp } from "./commands.ts";
import { MachineFlag } from "./machine-options.ts";

test("the docs CLI reference matches the generated page, so run npm run docs:cli --prefix src/evals after changing help", async () => {
  assert.equal(await readFile(CliReferencePage, "utf8"), cliReference());
});

test("every flag the CLI knows is documented once, and every documented command exists", () => {
  const documented = FlagHelp.map((row) => row.flag.split(" ")[0]);
  assert.deepEqual([...documented].sort(), [...Object.values(Flag), ...Object.values(MachineFlag)].sort());
  const commands: readonly string[] = Object.values(Command);
  for (const row of FlagHelp) if (row.commands !== "any") for (const command of row.commands) assert.ok(commands.includes(command));
});
