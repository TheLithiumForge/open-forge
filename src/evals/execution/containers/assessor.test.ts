import assert from "node:assert/strict";
import { test } from "node:test";
import { link, readFile, readdir, symlink, truncate, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { AuthVariables, CodexAuthFile } from "../../config/auth.ts";
import { Tool } from "../../config/choices.ts";
import { assessorCommand, AssessorImagePrefix, AssessorTools, CodexAssessorSettings, runAssessorStage } from "./assessor.ts";
import { ContainerPaths, StageKind } from "./settings.ts";
import { AssessorImageLimits, ImageMediaType } from "./files.ts";
import { PNG } from "pngjs";
import { assertStrictStage, containerFixture, FixtureFiles, FixtureText } from "./test-docker.ts";

const TimeoutMs = 10_000;
const Prompt = "Read the supplied evidence and answer once.";
const Model = "test-model";
const Effort = "high";
const BothProviders = { [AuthVariables.claudeToken]: "claude-secret", [AuthVariables.openAiKey]: "codex-secret" };
const ImageFiles = { board: "board.png", faces: "faces.jpeg" } as const;
const ImageBytes = {
  board: PNG.sync.write(new PNG({ width: 4, height: 4 })),
  faces: Buffer.from("/9j/4AAQSkZJRgABAQAAAQABAAD/2wBDAP//////////////////////////////////////////////////////////////////////////////////////2wBDAf//////////////////////////////////////////////////////////////////////////////////////wAARCAABAAEDASIAAhEBAxEB/8QAFQABAQAAAAAAAAAAAAAAAAAAAAX/xAAUEAEAAAAAAAAAAAAAAAAAAAAA/8QAFQEBAQAAAAAAAAAAAAAAAAAAAAX/xAAUEQEAAAAAAAAAAAAAAAAAAAAA/9oADAMBAAIRAxEAPwCdAB//2Q==", "base64"),
};

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

for (const tool of [Tool.claude, Tool.codex]) {
  test(`${tool} receives the same PNG and JPEG bytes without enabling a tool`, async () => {
    const fixture = await containerFixture();
    try {
      const images = [join(fixture.space.root, ImageFiles.board), join(fixture.space.root, ImageFiles.faces)];
      await writeFile(images[0] ?? "", ImageBytes.board);
      await writeFile(images[1] ?? "", ImageBytes.faces);
      await runAssessorStage(fixture.space, { tool, model: null, effort: null }, Prompt, { docker: fixture.client, timeoutMs: TimeoutMs, env: {}, images });
      const name = assertStrictStage(fixture.client, StageKind.assessor);
      const call = fixture.client.calls.find((entry) => entry.args.includes(tool));
      assert.ok(call);
      const input = fixture.client.inputs.join("");
      const filenames = [`${AssessorImagePrefix}0.png`, `${AssessorImagePrefix}1.jpg`];
      const attached = filenames.map((file) => `${ContainerPaths.work}/${file}`);
      if (tool === Tool.claude) {
        const expected = [Tool.claude, "-p", "--input-format", "stream-json", "--output-format", "stream-json", "--verbose", "--strict-mcp-config", "--mcp-config", '{"mcpServers":{}}', "--tools", "", "--max-turns", "1"];
        assert.deepEqual(call.args.slice(-expected.length), expected);
        const text = `${Prompt}\n\n${JSON.stringify({ [FixtureFiles.source]: FixtureText.source })}`;
        assert.deepEqual(JSON.parse(input), { type: "user", parent_tool_use_id: null, message: { role: "user", content: [
          { type: "text", text },
          { type: "text", text: `Image: ${ImageFiles.board}` }, { type: "image", source: { type: "base64", media_type: ImageMediaType.png, data: ImageBytes.board.toString("base64") } },
          { type: "text", text: `Image: ${ImageFiles.faces}` }, { type: "image", source: { type: "base64", media_type: ImageMediaType.jpeg, data: ImageBytes.faces.toString("base64") } },
        ] } });
        assert.ok(input.endsWith("\n"));
      } else {
        const expected = [Tool.codex, "exec", "--json", "--skip-git-repo-check", "--sandbox", "read-only", ...CodexAssessorSettings.flatMap((setting) => ["-c", setting]), ...attached.flatMap((path) => ["--image", path]), "-"];
        assert.deepEqual(call.args.slice(-expected.length), expected);
        assert.deepEqual(await readdir(fixture.client.path(name, ContainerPaths.work)), filenames);
        for (const [index, bytes] of [ImageBytes.board, ImageBytes.faces].entries()) {
          const path = attached[index];
          assert.ok(path);
          assert.deepEqual(await readFile(fixture.client.path(name, path)), bytes);
          assert.equal(fixture.client.calls.filter((entry) => entry.args[0] === "cp" && entry.args[2] === `${name}:${path}`).length, 1);
        }
      }
    } finally { await fixture.dispose(); }
  });
}

test("assessor refuses oversized, excess, escaped, linked and non-image files before Docker", async () => {
  const fixture = await containerFixture();
  try {
    const image = join(fixture.space.root, ImageFiles.board);
    await writeFile(image, ImageBytes.board);
    const bad = join(fixture.space.root, "bad.png");
    await writeFile(bad, FixtureText.source);
    const unsupported = join(fixture.space.root, "board.gif");
    await writeFile(unsupported, ImageBytes.board);
    const large = join(fixture.space.root, "large.png");
    await writeFile(large, ImageBytes.board);
    await truncate(large, AssessorImageLimits.bytes);
    const hard = join(fixture.space.root, "hard.png");
    await link(image, hard);
    const linked = join(fixture.space.root, "linked");
    await symlink(fixture.signInHome, linked, process.platform === "win32" ? "junction" : "dir");
    const linkedImage = join(fixture.signInHome, ImageFiles.board);
    await writeFile(linkedImage, ImageBytes.board);
    const options = { docker: fixture.client, timeoutMs: TimeoutMs, env: {} };
    for (const images of [[bad], [unsupported], [large], [hard], [fixture.space.work], [join(linked, ImageFiles.board)], [join(fixture.space.root, "..", ImageFiles.board)], Array.from({ length: AssessorImageLimits.count + 1 }, () => linkedImage)]) {
      await assert.rejects(runAssessorStage(fixture.space, { tool: Tool.claude, model: null, effort: null }, Prompt, { ...options, images }));
      assert.equal(fixture.client.calls.length, 0);
    }
    await assert.rejects(runAssessorStage({ ...fixture.space, root: linked }, { tool: Tool.codex, model: null, effort: null }, Prompt, { ...options, images: [join(linked, ImageFiles.board)] }));
    assert.equal(fixture.client.calls.length, 0);
  } finally { await fixture.dispose(); }
});

test("assessor removes its container when copying an image fails", async () => {
  const fixture = await containerFixture();
  try {
    const path = join(fixture.space.root, ImageFiles.board);
    await writeFile(path, ImageBytes.board);
    fixture.client.hook = async (args) => {
      if (args[0] === "cp" && args[2]?.endsWith(`${AssessorImagePrefix}0.png`)) throw new Error("Fake image copy failure.");
      return undefined;
    };
    await assert.rejects(runAssessorStage(fixture.space, { tool: Tool.codex, model: null, effort: null }, Prompt, { docker: fixture.client, timeoutMs: TimeoutMs, env: {}, images: [path] }));
    assertStrictStage(fixture.client, StageKind.assessor);
  } finally { await fixture.dispose(); }
});

test("assessor accepts the image count and byte limits below their ceilings", async () => {
  const fixture = await containerFixture();
  try {
    const path = join(fixture.space.root, ImageFiles.board);
    await writeFile(path, ImageBytes.board);
    await truncate(path, AssessorImageLimits.bytes - 1);
    await runAssessorStage(fixture.space, { tool: Tool.codex, model: null, effort: null }, Prompt, { docker: fixture.client, timeoutMs: TimeoutMs, env: {}, images: Array.from({ length: AssessorImageLimits.count }, () => path) });
    const name = assertStrictStage(fixture.client, StageKind.assessor);
    assert.equal((await readdir(fixture.client.path(name, ContainerPaths.work))).length, AssessorImageLimits.count);
  } finally { await fixture.dispose(); }
});
