import assert from "node:assert/strict";
import { mkdir, mkdtemp, readFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { test } from "node:test";
import { CodexTelemetry, saveRollouts } from "./codex-telemetry.ts";

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

test("a run with subagents reports every thread, and its tokens are the whole system's", async (t) => {
  const root = await mkdtemp(join(tmpdir(), "codex-threads-")); t.after(() => rm(root, { recursive: true, force: true }));
  const sessions = join(root, "sessions", "2026", "11", "03");
  await mkdir(sessions, { recursive: true });
  const usage = (thread: string, input: number, cached: number) => ({ type: "token_usage_record", payload: { thread_id: thread, usage: { input_tokens: input, cached_input_tokens: cached, output_tokens: 10, reasoning_output_tokens: 2 } } });
  const log = (items: readonly object[]) => items.map((item) => JSON.stringify(item)).join("\n");
  await writeFile(join(sessions, "rollout-a.jsonl"), log([{ type: "session_meta", payload: { id: "main", source: "exec" } }, usage("main", 100, 0), usage("main", 200, 100)]));
  await writeFile(join(sessions, "rollout-b.jsonl"), log([{ type: "session_meta", payload: { id: "helper", source: { subagent: { parent_thread_id: "main" } } } }, usage("helper", 50, 0)]));
  const telemetry = new CodexTelemetry();
  telemetry.record({ type: "turn.completed", usage: { input_tokens: 300, cached_input_tokens: 100, output_tokens: 20, reasoning_output_tokens: 4 } });
  const summary = await telemetry.summary(root, null);
  assert.deepEqual(summary.threads?.map((thread) => [thread.id, thread.parentId, thread.requests]), [["main", null, 2], ["helper", "main", 1]]);
  assert.deepEqual(summary.tokens, { input: 250, cachedInput: 100, cacheWrite: 0, output: 30, reasoning: 6 });
});

test("a run without subagents keeps its turn totals and reports no threads", async (t) => {
  const root = await mkdtemp(join(tmpdir(), "codex-threads-")); t.after(() => rm(root, { recursive: true, force: true }));
  const telemetry = new CodexTelemetry();
  telemetry.record({ type: "turn.completed", usage: { input_tokens: 300, cached_input_tokens: 100, output_tokens: 20, reasoning_output_tokens: 4 } });
  const summary = await telemetry.summary(root, null);
  assert.equal(summary.threads, undefined);
  assert.deepEqual(summary.tokens, { input: 200, cachedInput: 100, cacheWrite: 0, output: 20, reasoning: 4 });
});
