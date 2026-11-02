import assert from "node:assert/strict";
import { test } from "node:test";
import { ClaudeTelemetry } from "./claude-telemetry.ts";

test("output tokens come from each turn's result, since stream events carry a message's usage from its start", () => {
  const telemetry = new ClaudeTelemetry();
  const started = { input_tokens: 2, cache_read_input_tokens: 100, cache_creation_input_tokens: 10, output_tokens: 8 };
  for (const id of ["first", "second"]) telemetry.record({ type: "assistant", message: { id, usage: started, content: [] } });
  telemetry.record({ type: "result", usage: { output_tokens: 700, output_tokens_details: { thinking_tokens: 300 } } });
  telemetry.record({ type: "result", usage: { output_tokens: 50 } });
  const tokens = telemetry.summary().tokens;
  assert.deepEqual(tokens, { input: 4, cachedInput: 200, cacheWrite: 20, output: 750, reasoning: 300 });
});

test("without a result, output falls back to the streamed message usage", () => {
  const telemetry = new ClaudeTelemetry();
  telemetry.record({ type: "assistant", message: { id: "only", usage: { input_tokens: 1, output_tokens: 8 }, content: [] } });
  assert.equal(telemetry.summary().tokens?.output, 8);
});
