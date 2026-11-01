import assert from "node:assert/strict";
import { mkdtemp, mkdir, rm, symlink, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { test } from "node:test";
import { GameSandbox, gameHeaders } from "../game-policy.ts";
import { gameUrl } from "./compare.ts";
import { readPlainFile } from "./plain-file.ts";

test("games have separate origins and sandbox permissions allow scripts and storage only", () => {
  const port = 4318;
  const first = "a".repeat(12);
  const second = "b".repeat(12);
  assert.notEqual(new URL(gameUrl(first, port)).origin, new URL(gameUrl(second, port)).origin);
  assert.deepEqual(GameSandbox.split(" "), ["allow-scripts", "allow-same-origin"]);
  const directives = gameHeaders(port)["content-security-policy"]?.split("; ") ?? [];
  assert.ok(directives.includes(`sandbox ${GameSandbox}`));
  assert.ok(directives.includes("form-action 'none'"));
  assert.ok(directives.includes("connect-src 'none'"));
  assert.ok(directives.includes("frame-src 'none'"));
});

test("the file boundary reads plain files and refuses paths outside their saved folder", async () => {
  const root = await mkdtemp(join(tmpdir(), "eval-web-"));
  try {
    const folder = join(root, "saved");
    await mkdir(folder);
    const path = join(folder, "game.html");
    await writeFile(path, "authored fixture");
    assert.equal((await readPlainFile(folder, path)).toString(), "authored fixture");
    await assert.rejects(readPlainFile(folder, join(root, "outside.html")));
    const link = join(root, "linked");
    await symlink(folder, link, process.platform === "win32" ? "junction" : "dir");
    await assert.rejects(readPlainFile(root, join(link, "game.html")));
  } finally { await rm(root, { recursive: true, force: true }); }
});
