import assert from "node:assert/strict";
import { test } from "node:test";
import { writeFile } from "node:fs/promises";
import { join } from "node:path";
import { AuthVariables, CodexAuthFile } from "../../config/auth.ts";
import { Tool } from "../../config/choices.ts";
import { assessorCommand, AssessorTools, CodexAssessorSettings, runAssessorStage } from "./assessor.ts";
import { ContainerPaths, StageKind } from "./settings.ts";
import { readdir } from "node:fs/promises";
import { assertStrictStage, containerFixture, FixtureText } from "./test-docker.ts";

const TimeoutMs = 10_000;
const Prompt = "Read the supplied evidence and answer once.";
const Model = "test-model";
const Effort = "high";
const BothProviders = { [AuthVariables.claudeToken]: "claude-secret", [AuthVariables.openAiKey]: "codex-secret" };

for (const tool of [Tool.claude, Tool.codex]) {
  test(`assessor makes one ${tool} call with its tool limits and only its sign-in`, async () => {
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
      const own = tool === Tool.codex ? AuthVariables.openAiKey : AuthVariables.claudeToken;
      assert.deepEqual(call.options.env, { [own]: BothProviders[own] });
      const other = tool === Tool.codex ? AuthVariables.claudeToken : AuthVariables.openAiKey;
      assert.equal(fixture.client.calls.some((entry) => entry.args.includes(other)), false);
      assert.equal(fixture.client.inputs.length, 1);
      assert.ok(fixture.client.inputs[0]?.includes(FixtureText.source));
      assert.equal(result.tools, tool === Tool.codex ? AssessorTools.restricted : AssessorTools.off);
      if (tool === Tool.codex) {
        const name = fixture.client.names[0];
        assert.ok(name);
        assert.deepEqual(await readdir(fixture.client.path(name, ContainerPaths.work)), []);
        assert.equal(fixture.client.calls.some((entry) => entry.args[0] === "cp" && entry.args[2] === `${name}:${ContainerPaths.work}`), false);
      }
    } finally { await fixture.dispose(); }
  });
}

test("assessor Claude arguments disable tools and MCP", () => {
  assert.deepEqual(assessorCommand({ tool: Tool.claude, model: null, effort: null }), [Tool.claude, "-p", "--output-format", "json", "--strict-mcp-config", "--mcp-config", '{"mcpServers":{}}', "--tools", "", "--max-turns", "1"]);
});

test("assessor rejects providers without a tools-off command before invoking Docker", async () => {
  const fixture = await containerFixture();
  try {
    for (const tool of [Tool.mock]) {
      await assert.rejects(runAssessorStage(fixture.space, { tool, model: null, effort: null }, Prompt, { docker: fixture.client, timeoutMs: TimeoutMs, env: {} }));
      assert.equal(fixture.client.calls.length, 0);
    }
  } finally { await fixture.dispose(); }
});

test("Codex assessor uses read-only mode and disables configurable tools", () => {
  assert.deepEqual(assessorCommand({ tool: Tool.codex, model: null, effort: null }), [Tool.codex, "exec", "--json", "--skip-git-repo-check", "--sandbox", "read-only", ...CodexAssessorSettings.flatMap((setting) => ["-c", setting]), "-"]);
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
