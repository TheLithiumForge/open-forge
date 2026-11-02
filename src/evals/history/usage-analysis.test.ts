import assert from "node:assert/strict";
import { test } from "node:test";
import { eventUsage, readKind, ReadKind } from "./usage-analysis.ts";

const lines = (events: readonly unknown[]) => events.map((event) => JSON.stringify(event)).join("\n");
const assistant = (id: string, block: object, cached: number) => ({ type: "assistant", message: { id, usage: { input_tokens: 1, cache_read_input_tokens: cached, cache_creation_input_tokens: 0, output_tokens: 8 }, content: [block] } });
const result = (id: string, content: string) => ({ type: "user", message: { content: [{ type: "tool_result", tool_use_id: id, content }] } });

test("files sort into framework, spec documents and code", () => {
  assert.equal(readKind(".agents/loader.md"), ReadKind.framework);
  assert.equal(readKind("cat AGENTS.md"), ReadKind.framework);
  assert.equal(readKind("npx open-forge context"), ReadKind.framework);
  assert.equal(readKind("/work/docs/rules.md"), ReadKind.docs);
  assert.equal(readKind("src/game/rules.ts"), ReadKind.code);
});

test("Claude events split input tokens at the first edit, count reads by kind and take output from results", () => {
  const usage = eventUsage("claude", lines([
    assistant("first", { type: "tool_use", id: "read", name: "Read", input: { file_path: "/work/docs/rules.md" } }, 100),
    result("read", "ten chars."),
    assistant("first", { type: "tool_use", id: "list", name: "Bash", input: { command: "ls .agents" } }, 100),
    result("list", "loader.md"),
    assistant("second", { type: "tool_use", id: "write", name: "Write", input: { file_path: "/work/src/a.ts" } }, 300),
    { type: "result", usage: { output_tokens: 900 } }, "a partial line",
  ]));
  assert.equal(usage.steps, 3);
  assert.equal(usage.stepsBeforeEdit, 2);
  assert.deepEqual(usage.reads, { framework: { actions: 1, characters: 9 }, docs: { actions: 1, characters: 10 }, code: { actions: 0, characters: 0 } });
  assert.equal(usage.phases?.analysis.cachedInput, 100);
  assert.equal(usage.phases?.implementation.cachedInput, 300);
  assert.equal(usage.output, 900);
});

test("Codex events count read commands by kind and the steps before its first patch, without a token split", () => {
  const command = (text: string, output: string) => ({ type: "item.completed", item: { type: "command_execution", command: text, aggregated_output: output } });
  const usage = eventUsage("codex", lines([command("cat docs/rules.md", "rules"), command("npm test", "ok"), command("apply_patch <<EOF", ""), command("sed -n 1,5p src/a.ts", "code")]));
  assert.equal(usage.steps, 4);
  assert.equal(usage.stepsBeforeEdit, 2);
  assert.deepEqual(usage.reads, { framework: { actions: 0, characters: 0 }, docs: { actions: 1, characters: 5 }, code: { actions: 1, characters: 4 } });
  assert.equal(usage.phases, null);
  assert.equal(usage.output, null);
});
