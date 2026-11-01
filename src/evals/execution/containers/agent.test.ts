import assert from "node:assert/strict";
import { test } from "node:test";
import { mkdir, readFile, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { AuthVariables, CodexAuthFile } from "../../config/auth.ts";
import { Isolation, Tool } from "../../config/choices.ts";
import { PinnedTool } from "../providers/pinned-tools.ts";
import { ClaudeSession } from "../providers/claude-session.ts";
import { TurnOutcome } from "../providers/agent-session.ts";
import { copySignInTo, keepRefreshedSignIn } from "../providers/codex-sign-in.ts";
import { waitForProcess } from "../process.ts";
import { withAgentStage } from "./agent.ts";
import { spawnInRun, syncRunHome } from "./executor.ts";
import { ContainerPaths, ContainerUsers, StageKind } from "./settings.ts";
import { toolEnvironment } from "./tool-home.ts";
import { assertStrictStage, containerFixture, FixtureFiles, FixtureText } from "./test-docker.ts";

const BothProviders = { [AuthVariables.openAiKey]: "codex-secret", [AuthVariables.claudeToken]: "claude-secret" };
const ProbeArguments = ["probe"];
const TimeoutMs = 10_000;

for (const provider of [Tool.codex, Tool.claude]) {
  test(`agent copies only ${provider} sign-in and executes through stdin`, async () => {
    const fixture = await containerFixture();
    const { space, client, signInHome } = fixture;
    try {
      await writeFile(join(signInHome, CodexAuthFile), FixtureText.before);
      await mkdir(join(space.home, ".claude"));
      await writeFile(join(space.home, ".claude", "settings.json"), "private host settings");
      await withAgentStage(space, provider, { docker: client, timeoutMs: TimeoutMs, env: BothProviders, codexSignInHome: signInHome }, async () => {
        const child = spawnInRun({ isolation: Isolation.docker, space, command: provider, args: ProbeArguments, network: true });
        assert.equal((await waitForProcess(child, { timeoutMs: TimeoutMs, input: FixtureText.source })).code, 0);
        const name = client.names[0];
        assert.ok(name);
        await writeFile(client.path(name, `${ContainerPaths.work}/${FixtureFiles.source}`), FixtureText.built);
        if (provider === Tool.codex) {
          await writeFile(client.path(name, `${ContainerPaths.home}/.codex/${CodexAuthFile}`), FixtureText.after);
          await syncRunHome(space);
        }
      });
      const name = assertStrictStage(client, StageKind.agent);
      const authName = provider === Tool.codex ? AuthVariables.openAiKey : AuthVariables.claudeToken;
      const otherName = provider === Tool.codex ? AuthVariables.claudeToken : AuthVariables.openAiKey;
      const exec = client.calls.find((call) => call.args.includes(provider));
      assert.ok(exec);
      assert.deepEqual(exec.args, ["exec", "-i", "--user", ContainerUsers.agent, "--workdir", ContainerPaths.work,
        ...Object.entries(toolEnvironment(provider)).flatMap(([key, value]) => ["--env", `${key}=${value}`]), "--env", authName, name, provider, ...ProbeArguments]);
      assert.deepEqual(exec.options.env, { [authName]: BothProviders[authName] });
      assert.equal(JSON.stringify(client.calls.map((call) => call.args)).includes(otherName), false);
      assert.equal(JSON.stringify(client.calls.map((call) => call.args)).includes(BothProviders[authName]), false);
      assert.equal(client.calls.some((call) => call.args[0] === "cp" && call.args[1]?.endsWith(CodexAuthFile)), provider === Tool.codex);
      assert.equal(await readFile(join(space.work, FixtureFiles.source), "utf8"), FixtureText.built);
      assert.equal(await readFile(join(signInHome, CodexAuthFile), "utf8"), provider === Tool.codex ? FixtureText.after : FixtureText.before);
      await assert.rejects(async () => spawnInRun({ isolation: Isolation.docker, space, command: PinnedTool.codex, args: [], network: true }));
    } finally { await fixture.dispose(); }
  });
}

test("agent recovers files and removes the container after the conversation throws", async () => {
  const fixture = await containerFixture();
  try {
    const failure = new Error("Conversation failed.");
    await assert.rejects(withAgentStage(fixture.space, Tool.claude, { docker: fixture.client, timeoutMs: TimeoutMs, env: {} }, async () => { throw failure; }), (error) => error === failure);
    assertStrictStage(fixture.client, StageKind.agent);
    assert.equal(await readFile(join(fixture.space.work, FixtureFiles.source), "utf8"), FixtureText.source);
  } finally { await fixture.dispose(); }
});

test("agent refuses the other provider in the active stage", async () => {
  const fixture = await containerFixture();
  try {
    await withAgentStage(fixture.space, Tool.claude, { docker: fixture.client, timeoutMs: TimeoutMs, env: {} }, async () => {
      assert.throws(() => spawnInRun({ isolation: Isolation.docker, space: fixture.space, command: PinnedTool.codex, args: [], network: true }));
    });
    assertStrictStage(fixture.client, StageKind.agent);
  } finally { await fixture.dispose(); }
});

test("Claude Docker spawn errors finish the session and remove the container", async () => {
  const fixture = await containerFixture();
  try {
    fixture.client.spawnFailure = true;
    await withAgentStage(fixture.space, Tool.claude, { docker: fixture.client, timeoutMs: TimeoutMs, env: {} }, async () => {
      const session = new ClaudeSession({ isolation: Isolation.docker, space: fixture.space, choice: { tool: Tool.claude, model: null, effort: null }, eventsPath: join(fixture.space.root, "events.jsonl") });
      assert.equal((await session.turn(FixtureText.source, TimeoutMs)).outcome, TurnOutcome.failed);
      await session.finish();
    });
    assertStrictStage(fixture.client, StageKind.agent);
  } finally { await fixture.dispose(); }
});

test("a newer shared Codex sign-in survives a refresh from an older staged copy", async () => {
  const fixture = await containerFixture();
  try {
    const source = join(fixture.signInHome, CodexAuthFile);
    await writeFile(source, FixtureText.before);
    await withAgentStage(fixture.space, Tool.codex, { docker: fixture.client, timeoutMs: TimeoutMs, env: {}, codexSignInHome: fixture.signInHome }, async () => {
      await writeFile(source, FixtureText.after);
      const copy = copySignInTo(fixture.space.home);
      assert.ok(copy);
      assert.equal(await readFile(copy.path, "utf8"), FixtureText.before);
      await syncRunHome(fixture.space);
      keepRefreshedSignIn(copy);
    });
    assert.equal(await readFile(source, "utf8"), FixtureText.after);
    assertStrictStage(fixture.client, StageKind.agent);
  } finally { await fixture.dispose(); }
});

test("ending a lingering CLI keeps the container available for the next turn", async () => {
  const fixture = await containerFixture();
  try {
    await withAgentStage(fixture.space, Tool.claude, { docker: fixture.client, timeoutMs: TimeoutMs, env: {} }, async () => {
      const first = spawnInRun({ isolation: Isolation.docker, space: fixture.space, command: PinnedTool.claude, args: ProbeArguments, network: true });
      const exited = new Promise<void>((resolve) => first.once("exit", () => resolve()));
      first.kill();
      await exited;
      assert.equal(fixture.client.calls.some((call) => call.args[0] === "kill"), false);
      assert.ok(fixture.client.calls.some((call) => call.args.includes(ContainerPaths.stopTools)));
      const second = spawnInRun({ isolation: Isolation.docker, space: fixture.space, command: PinnedTool.claude, args: ProbeArguments, network: true });
      assert.equal((await waitForProcess(second, { timeoutMs: TimeoutMs, input: FixtureText.source })).code, 0);
    });
    assertStrictStage(fixture.client, StageKind.agent);
    assert.equal(fixture.client.calls.filter((call) => call.args[0] === "create").length, 1);
  } finally { await fixture.dispose(); }
});

test("cancelled agents recover their project and remove the container", async () => {
  const fixture = await containerFixture();
  const controller = new AbortController();
  try {
    await assert.rejects(withAgentStage(fixture.space, Tool.claude, { docker: fixture.client, timeoutMs: TimeoutMs, signal: controller.signal, env: {} }, async () => { controller.abort(); }));
    assertStrictStage(fixture.client, StageKind.agent);
    assert.equal(await readFile(join(fixture.space.work, FixtureFiles.source), "utf8"), FixtureText.source);
  } finally { await fixture.dispose(); }
});

test("agent removal still runs when project recovery fails", async () => {
  const fixture = await containerFixture();
  try {
    fixture.client.hook = async (args) => {
      if (args[0] === "cp" && args[1]?.endsWith(`:${ContainerPaths.work}/.`)) throw new Error("Output copy failed.");
      return undefined;
    };
    await assert.rejects(withAgentStage(fixture.space, Tool.claude, { docker: fixture.client, timeoutMs: TimeoutMs, env: {} }, async () => {}));
    assertStrictStage(fixture.client, StageKind.agent);
  } finally { await fixture.dispose(); }
});
