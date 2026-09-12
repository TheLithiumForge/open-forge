import assert from "node:assert/strict";
import { test } from "node:test";
import { spawnSync } from "node:child_process";
import { mkdir, mkdtemp, readFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import path from "node:path";
import { fileURLToPath } from "node:url";

const PatcherPath = fileURLToPath(new URL("./patch-codex-agent-models.ts", import.meta.url));
const AuthoredAgent = `---
name: review-agent
model: openai/gpt-5.5
reasoningEffort: high
---

# Review Agent
`;
const Overrides = `${JSON.stringify({ "review-agent": { model: "gpt-5.6-sol" } }, null, 2)}\n`;
const GeneratedAgent = `name = "review-agent"
description = "Generated review agent"
model = "gpt-5.4"
`;
const ExpectedGeneratedAgent = `name = "review-agent"
description = "Generated review agent"
model = "gpt-5.6-sol"
model_reasoning_effort = "high"
`;

function runPatcher(arguments_: readonly string[], cwd: string): { status: number; stdout: string; stderr: string } {
  const result = spawnSync(process.execPath, ["--experimental-strip-types", PatcherPath, ...arguments_], { cwd, encoding: "utf8", shell: false });
  if (result.error) throw result.error;
  if (result.status === null) throw new Error("The projection patcher terminated without an exit status.");
  return { status: result.status, stdout: result.stdout ?? "", stderr: result.stderr ?? "" };
}

test("merges authored and override policy before updating and checking a generated Codex agent", async () => {
  const fixtureRoot = await mkdtemp(path.join(tmpdir(), "open-forge-agent-projection-"));
  const apmAgents = path.join(fixtureRoot, ".apm", "agents");
  const codexAgents = path.join(fixtureRoot, ".codex", "agents");
  const overrides = path.join(fixtureRoot, ".apm", "codex-agent-models.json");
  const authoredAgent = path.join(apmAgents, "topics", "review-agent.agent.md");
  const generatedAgent = path.join(codexAgents, "review-agent.toml");
  const arguments_ = ["--apm-agents", apmAgents, "--codex-agents", codexAgents, "--overrides", overrides];

  try {
    await mkdir(path.dirname(authoredAgent), { recursive: true });
    await mkdir(codexAgents, { recursive: true });
    await writeFile(authoredAgent, AuthoredAgent);
    await writeFile(overrides, Overrides);
    await writeFile(generatedAgent, GeneratedAgent);

    const patched = runPatcher(arguments_, fixtureRoot);
    assert.deepEqual(patched, {
      status: 0,
      stdout: "patched .codex/agents/review-agent.toml (review-agent)\nDone: 1 file(s) patched, 1 configured agent(s) matched.\n",
      stderr: "",
    });
    assert.equal(await readFile(generatedAgent, "utf8"), ExpectedGeneratedAgent);

    const checked = runPatcher([...arguments_, "--check"], fixtureRoot);
    assert.deepEqual(checked, { status: 0, stdout: "OK: 1 generated Codex agent model policies match.\n", stderr: "" });
    assert.equal(await readFile(generatedAgent, "utf8"), ExpectedGeneratedAgent);
  } finally {
    await rm(fixtureRoot, { recursive: true, force: true });
  }
});
