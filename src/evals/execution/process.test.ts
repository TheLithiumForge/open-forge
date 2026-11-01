import assert from "node:assert/strict";
import { spawn } from "node:child_process";
import { fileURLToPath } from "node:url";
import { test } from "node:test";
import { waitForProcess } from "./process.ts";

const Fixture = fileURLToPath(new URL("./process-fixture.ts", import.meta.url));
const run = (mode: "flood" | "ready") => spawn(process.execPath, [Fixture, mode], { windowsHide: true });

test("output past maxBytes is dropped while it's read, and the process is stopped", async () => {
  const limit = 100_000;
  const result = await waitForProcess(run("flood"), { timeoutMs: 30_000, maxBytes: limit });
  assert.equal(result.truncated, true);
  assert.equal(result.timedOut, false);
  assert.ok(Buffer.byteLength(result.stdout) <= limit);
});

test("output under maxBytes is kept whole and not marked truncated", async () => {
  const result = await waitForProcess(run("ready"), { timeoutMs: 30_000, maxBytes: 1024 });
  assert.equal(result.stdout, "ready");
  assert.equal(result.truncated, undefined);
});
