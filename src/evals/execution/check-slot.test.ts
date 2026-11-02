import assert from "node:assert/strict";
import { mkdir, mkdtemp, readdir, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { test } from "node:test";
import { withCheckSlot } from "./check-slot.ts";

const PollMs = 10;
// Process IDs stay far below this on Windows and Linux, so no live process owns it.
const DeadPid = 2 ** 30;

test("check stages beyond the slot count wait for a free slot and every slot is released", async (t) => {
  const root = await mkdtemp(join(tmpdir(), "eval-check-slots-"));
  t.after(() => rm(root, { recursive: true, force: true }));
  let running = 0, peak = 0;
  const stage = () => withCheckSlot(async () => {
    running += 1; peak = Math.max(peak, running);
    await new Promise((resolve) => setTimeout(resolve, 30));
    running -= 1;
  }, { root, count: 2, pollMs: PollMs });
  await Promise.all([stage(), stage(), stage(), stage(), stage()]);
  assert.equal(peak, 2);
  assert.deepEqual(await readdir(root), []);
});

test("a slot left by a process that died is claimed again and a failing stage still frees its slot", async (t) => {
  const root = await mkdtemp(join(tmpdir(), "eval-check-slots-"));
  t.after(() => rm(root, { recursive: true, force: true }));
  await mkdir(join(root, "slot-0"));
  await writeFile(join(root, "slot-0", "owner.json"), JSON.stringify({ pid: DeadPid, at: new Date().toISOString() }));
  assert.equal(await withCheckSlot(async () => true, { root, count: 1, pollMs: PollMs }), true);
  await assert.rejects(withCheckSlot(async () => { throw new Error("Fixture check failure."); }, { root, count: 1, pollMs: PollMs }));
  assert.deepEqual(await readdir(root), []);
});

test("without configured slots a check stage runs at once and leaves no slot folder", async (t) => {
  const root = await mkdtemp(join(tmpdir(), "eval-check-slots-"));
  t.after(() => rm(root, { recursive: true, force: true }));
  assert.equal(await withCheckSlot(async () => true, null), true);
  assert.deepEqual(await readdir(root), []);
});
