import assert from "node:assert/strict";
import { mkdir, mkdtemp, readFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { test } from "node:test";
import { saveRollouts } from "./codex-telemetry.ts";

test("Codex session logs are kept beside the run in order, and a home without logs saves nothing", async (t) => {
  const root = await mkdtemp(join(tmpdir(), "codex-rollout-")); t.after(() => rm(root, { recursive: true, force: true }));
  const sessions = join(root, "home", "sessions", "2026", "11", "02");
  await mkdir(sessions, { recursive: true });
  await writeFile(join(sessions, "rollout-b.jsonl"), "{\"second\":true}");
  await writeFile(join(sessions, "rollout-a.jsonl"), "{\"first\":true}\n");
  const target = join(root, "saved.jsonl");
  assert.equal(await saveRollouts(join(root, "home"), target), true);
  assert.deepEqual((await readFile(target, "utf8")).split("\n").filter(Boolean).map((line) => JSON.parse(line)), [{ first: true }, { second: true }]);
  assert.equal(await saveRollouts(join(root, "empty"), join(root, "none.jsonl")), false);
});
