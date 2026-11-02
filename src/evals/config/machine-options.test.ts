import assert from "node:assert/strict";
import { resolve } from "node:path";
import { test } from "node:test";
import { MachineFlag, splitMachineOptions } from "./machine-options.ts";

test("machine flags leave the command's own arguments in order and refuse missing, repeated or invalid values", () => {
  const split = splitMachineOptions(["run", "--plan", "plan.json", MachineFlag.scratch, "work", MachineFlag.checkSlots, "3"]);
  assert.deepEqual(split.args, ["run", "--plan", "plan.json"]);
  assert.deepEqual(split.machine, [MachineFlag.scratch, "work", MachineFlag.checkSlots, "3"]);
  assert.deepEqual(split.options, { scratch: resolve("work"), checkSlots: 3 });
  assert.deepEqual(splitMachineOptions(["summary"]), { args: ["summary"], machine: [], options: {} });
  for (const argv of [[MachineFlag.scratch], [MachineFlag.checkSlots, "0"], [MachineFlag.checkSlots, "two"], [MachineFlag.scratch, "--plan"], [MachineFlag.scratch, "a", MachineFlag.scratch, "b"]]) {
    assert.throws(() => splitMachineOptions(argv));
  }
});
