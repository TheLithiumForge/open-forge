import assert from "node:assert/strict";
import { test } from "node:test";
import { ContextClass, ThreadRole } from "./classes.ts";
import { claudeThreads } from "./claude-stream.ts";

const Helper = "toolu_task";
const lines = (items: readonly object[]) => items.map((item) => JSON.stringify(item)).join("\n");
const assistant = (id: string, block: object, cached: number, written: number, parent: string | null = null) =>
  ({ type: "assistant", parent_tool_use_id: parent, message: { id, usage: { input_tokens: 2, cache_read_input_tokens: cached, cache_creation_input_tokens: written, output_tokens: 1 }, content: [block] } });
const result = (id: string, content: string, parent: string | null = null) => ({ type: "user", parent_tool_use_id: parent, message: { content: [{ type: "tool_result", tool_use_id: id, content }] } });

test("each message is one request, its blocks count once, and a read's result joins the next request as its file's class", () => {
  const [main] = claudeThreads(lines([
    assistant("m1", { type: "tool_use", id: "r", name: "Read", input: { file_path: "/work/docs/rules.md" } }, 900, 100),
    assistant("m1", { type: "text", text: "Reading the rules." }, 900, 100),
    result("r", "rules text"),
    assistant("m2", { type: "tool_use", id: "w", name: "Write", input: { file_path: "/work/src/a.ts", content: "x" } }, 1000, 60),
  ]), new Map());
  assert.deepEqual(main?.steps.map((step) => [step.input, step.fresh]), [[1002, 102], [1062, 62]]);
  assert.deepEqual(main?.steps[1]?.added.map((item) => item.class), [ContextClass.spec, ContextClass.agent]);
  assert.deepEqual(main?.tokens, { newInput: 4, cachedInput: 1900, cacheWrite: 160, output: 0, reasoning: null });
  assert.equal(main?.toolCalls, 2);
});

test("a helper's messages form their own thread under the call that started it, with its brief as delegation", () => {
  const threads = claudeThreads(lines([
    assistant("m1", { type: "tool_use", id: Helper, name: "Task", input: { prompt: "Fix the sound." } }, 500, 0),
    { type: "user", parent_tool_use_id: Helper, message: { content: "Fix the sound." } },
    assistant("h1", { type: "text", text: "On it." }, 300, 50, Helper),
    result(Helper, "Sound fixed."),
    assistant("m2", { type: "text", text: "Done." }, 600, 10),
  ]), new Map());
  const helper = threads.find((thread) => thread.role === ThreadRole.subagent), main = threads.find((thread) => thread.role === ThreadRole.main);
  assert.equal(helper?.parentId, main?.id);
  assert.deepEqual(helper?.steps[0]?.added.map((item) => item.class), [ContextClass.delegation]);
  assert.deepEqual(main?.steps[1]?.added.map((item) => item.class), [ContextClass.report, ContextClass.agent]);
});
