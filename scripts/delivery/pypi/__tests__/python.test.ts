import assert from "node:assert/strict";
import { test } from "node:test";
import { PythonCommand, pythonCandidates } from "../python.ts";

const Direct = [
  { executable: PythonCommand.executable, args: [] },
  { executable: PythonCommand.versioned, args: [] },
];
const Launcher = { executable: PythonCommand.launcher, args: [PythonCommand.select3] };

test("normal Python discovery retains the existing direct-command order", () => {
  assert.deepEqual(pythonCandidates(), [...Direct, Launcher]);
});

test("resolver discovery can prefer the launcher while retaining direct-command fallbacks", () => {
  assert.deepEqual(pythonCandidates(true), [Launcher, ...Direct]);
});
