import assert from "node:assert/strict";
import { test } from "node:test";
import { writeFile } from "node:fs/promises";
import { join } from "node:path";
import { AuthVariables, CodexAuthFile } from "../../config/auth.ts";
import { Tool } from "../../config/choices.ts";
import { assessorCommand, runAssessorStage } from "./assessor.ts";
import { StageKind } from "./settings.ts";
import { assertStrictStage, containerFixture, FixtureText } from "./test-docker.ts";

const TimeoutMs = 10_000;
const Prompt = "Read the supplied evidence and answer once.";
const Model = "test-model";
const Effort = "high";
const BothProviders = { [AuthVariables.claudeToken]: "claude-secret", [AuthVariables.openAiKey]: "codex-secret" };

for (const tool of [Tool.claude]) {
  test(`assessor makes one ${tool} call with tools off and only its sign-in`, async () => {
    const fixture = await containerFixture();
    const choice = { tool, model: Model, effort: Effort };
    try {
      await writeFile(join(fixture.signInHome, CodexAuthFile), FixtureText.before);
      const result = await runAssessorStage(fixture.space, choice, Prompt, { docker: fixture.client, timeoutMs: TimeoutMs, env: BothProviders, codexSignInHome: fixture.signInHome });
      assert.equal(result.code, 0);
      assertStrictStage(fixture.client, StageKind.assessor);
      const calls = fixture.client.calls.filter((call) => call.args.includes(tool));
      assert.equal(calls.length, 1);
      const call = calls[0];
      assert.ok(call);
      const command = assessorCommand(choice);
      assert.deepEqual(call.args.slice(-command.length), command);
      const own = AuthVariables.claudeToken;
      assert.deepEqual(call.options.env, { [own]: BothProviders[own] });
      const other = AuthVariables.openAiKey;
      assert.equal(fixture.client.calls.some((entry) => entry.args.includes(other)), false);
      assert.equal(fixture.client.inputs.length, 1);
      assert.ok(fixture.client.inputs[0]?.includes(FixtureText.source));
    } finally { await fixture.dispose(); }
  });
}

test("assessor Claude arguments disable tools and MCP", () => {
  assert.deepEqual(assessorCommand({ tool: Tool.claude, model: null, effort: null }), [Tool.claude, "-p", "--output-format", "json", "--strict-mcp-config", "--mcp-config", '{"mcpServers":{}}', "--tools", "", "--max-turns", "1"]);
});

test("assessor rejects providers without a tools-off command before invoking Docker", async () => {
  const fixture = await containerFixture();
  try {
    for (const tool of [Tool.codex, Tool.mock]) {
      await assert.rejects(runAssessorStage(fixture.space, { tool, model: null, effort: null }, Prompt, { docker: fixture.client, timeoutMs: TimeoutMs, env: {} }));
      assert.equal(fixture.client.calls.length, 0);
    }
  } finally { await fixture.dispose(); }
});

test("assessor removes its container on a CLI spawn error", async () => {
  const fixture = await containerFixture();
  try {
    fixture.client.spawnFailure = true;
    const result = await runAssessorStage(fixture.space, { tool: Tool.claude, model: null, effort: null }, Prompt, { docker: fixture.client, timeoutMs: TimeoutMs, env: {} });
    assert.equal(result.code, null);
    assertStrictStage(fixture.client, StageKind.assessor);
  } finally { await fixture.dispose(); }
});
