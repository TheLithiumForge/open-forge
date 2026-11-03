import assert from "node:assert/strict";
import { test } from "node:test";
import { ContextClass, ThreadRole } from "./classes.ts";
import { codexThreads } from "./codex-rollout.ts";

const Main = "thread-main", Helper = "thread-helper";
const Known = new Map([["docs/rules.md", 400], ["src/game/rules.ts", 600]]);
// A string stands for a partial line a stopped tool can leave.
const lines = (items: readonly (object | string)[]) => items.map((item) => (typeof item === "string" ? item : JSON.stringify(item))).join("\n");
const session = (id: string, source: unknown = "exec") => ({ type: "session_meta", payload: { id, source } });
const message = (role: string, text: string) => ({ type: "response_item", payload: { type: "message", role, content: [{ type: "input_text", text }] } });
const call = (id: string, command: string) => ({ type: "response_item", payload: { type: "function_call", name: "exec_command", call_id: id, arguments: JSON.stringify({ cmd: command }) } });
const output = (id: string, text: string) => ({ type: "response_item", payload: { type: "function_call_output", call_id: id, output: text } });
const usage = (thread: string, input: number, cached: number, out: number) => ({ type: "token_usage_record", payload: { thread_id: thread, usage: { input_tokens: input, cached_input_tokens: cached, output_tokens: out, reasoning_output_tokens: 1 } } });
const added = (thread: { steps: readonly { added: readonly { class: string }[] }[] } | undefined, index: number) => thread?.steps[index]?.added.map((item) => item.class);

test("each request's exact usage closes a step, with the tool outputs and the agent's own output that joined since", () => {
  const [thread] = codexThreads(lines([
    session(Main), message("user", "Fix the best score."), call("a", "cat docs/rules.md"), usage(Main, 1000, 0, 50),
    output("a", "r".repeat(400)), call("b", "npm test"), usage(Main, 1250, 1000, 40), output("b", "ok"), usage(Main, 1300, 1250, 30), "partial",
  ]), Known);
  assert.equal(thread?.role, ThreadRole.main);
  assert.deepEqual(thread?.steps.map((step) => step.input), [1000, 1250, 1300]);
  assert.deepEqual(thread?.steps.map((step) => step.fresh), [1000, 250, 50]);
  assert.deepEqual(added(thread, 1), [ContextClass.spec, ContextClass.agent]);
  assert.deepEqual(added(thread, 2), [ContextClass.testOutput, ContextClass.agent]);
  assert.deepEqual(thread?.tokens, { newInput: 1000 + 250 + 50, cachedInput: 2250, cacheWrite: 0, output: 120, reasoning: 3 });
  assert.equal(thread?.peakContext, 1300);
  assert.equal(thread?.toolCalls, 2);
});

test("a subagent's thread keeps its own requests, its parent and its brief as delegation, and its report reaches the orchestrator", () => {
  // The saved log joins each thread's own file, so the orchestrator's lines come first and the subagent's after.
  const threads = codexThreads(lines([
    session(Main), message("user", "Fix four things."), { type: "response_item", payload: { type: "function_call", name: "wait_agent", call_id: "w", arguments: "{}" } },
    usage(Main, 800, 0, 20), output("w", "Done: best score fixed."), usage(Main, 900, 800, 5),
    session(Helper, { subagent: { thread_spawn: { parent_thread_id: Main } } }), message("user", "Fix the best score only."), usage(Helper, 500, 0, 10),
  ]), Known);
  const helper = threads.find((thread) => thread.id === Helper), main = threads.find((thread) => thread.id === Main);
  assert.equal(helper?.role, ThreadRole.subagent);
  assert.equal(helper?.parentId, Main);
  assert.deepEqual(added(helper, 0), [ContextClass.delegation]);
  assert.deepEqual(added(main, 1), [ContextClass.report, ContextClass.agent]);
});

test("code mode's commands share one call's output by their own output sizes", () => {
  const executed = (command: string, text: string) => ({ type: "event_msg", payload: { type: "item_completed", item: { type: "CommandExecution", command: ["/bin/sh", "-lc", command], aggregated_output: text } } });
  const [thread] = codexThreads(lines([
    session(Main), { type: "response_item", payload: { type: "custom_tool_call", name: "exec", call_id: "x", input: "await tools.exec_command({cmd:\"cat docs/rules.md\"})" } }, usage(Main, 100, 0, 5),
    executed("cat docs/rules.md", "r".repeat(300)), executed("cat src/game/rules.ts", "c".repeat(100)), { type: "response_item", payload: { type: "custom_tool_call_output", call_id: "x", output: [{ type: "input_text", text: "z".repeat(200) }] } },
    usage(Main, 200, 100, 5),
  ]), Known);
  const growth = thread?.steps[1]?.added ?? [];
  assert.deepEqual(growth.map((item) => [item.class, item.tokens]), [[ContextClass.spec, 150 / 4], [ContextClass.code, 50 / 4], [ContextClass.agent, 5]]);
});

test("older logs without usage records count each token count as one request", () => {
  const count = (input: number) => ({ type: "event_msg", payload: { type: "token_count", info: { last_token_usage: { input_tokens: input, cached_input_tokens: 0, output_tokens: 1 } } } });
  const [thread] = codexThreads(lines([session(Main), count(100), count(150)]), Known);
  assert.equal(thread?.steps.length, 2);
});

test("the first request after an edit's result starts the implementation", () => {
  const patch = (type: string, field: object) => ({ type: "response_item", payload: { type, call_id: "p", ...field } });
  const [thread] = codexThreads(lines([
    session(Main), call("a", "cat docs/rules.md"), usage(Main, 100, 0, 5), output("a", "rules"),
    patch("custom_tool_call", { name: "apply_patch", input: "*** Begin Patch" }), usage(Main, 150, 100, 5), patch("custom_tool_call_output", { output: "Success." }), usage(Main, 170, 150, 5),
  ]), Known);
  assert.equal(thread?.firstEditStep, 2);
});
