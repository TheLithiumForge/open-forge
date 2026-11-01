import assert from "node:assert/strict";
import { mkdir, mkdtemp, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { test } from "node:test";
import { changedFiles, projectHashes } from "./scope.ts";

const Scope = ["src/game/**"];

async function project(t: { after(fn: () => Promise<void>): void }): Promise<string> {
  const root = await mkdtemp(join(tmpdir(), "scope-"));
  t.after(() => rm(root, { recursive: true, force: true }));
  await mkdir(join(root, "src", "game"), { recursive: true });
  await writeFile(join(root, "src", "game", "game.ts"), "export {};\n");
  return root;
}

test("build output at the root never counts, but a nested folder of the same name does", async (t) => {
  const root = await project(t);
  const before = await projectHashes(root);
  await mkdir(join(root, "dist"));
  await writeFile(join(root, "dist", "index.html"), "built");
  await mkdir(join(root, "src", "lib", "dist"), { recursive: true });
  await writeFile(join(root, "src", "lib", "dist", "hidden.ts"), "export const hidden = 1;\n");
  const changes = changedFiles(before, await projectHashes(root), Scope);
  assert.deepEqual(changes.map((change) => change.path), ["src/lib/dist/hidden.ts"]);
  assert.equal(changes[0]?.allowed, false);
});

test("an agent's config folder counts as a change outside scope", async (t) => {
  const root = await project(t);
  const before = await projectHashes(root);
  await mkdir(join(root, ".claude"));
  await writeFile(join(root, ".claude", "settings.json"), "{}");
  const changes = changedFiles(before, await projectHashes(root), Scope);
  assert.deepEqual(changes.map((change) => [change.path, change.allowed]), [[".claude/settings.json", false]]);
});

test("a change inside the declared scope is allowed", async (t) => {
  const root = await project(t);
  const before = await projectHashes(root);
  await writeFile(join(root, "src", "game", "game.ts"), "export const changed = true;\n");
  assert.deepEqual(changedFiles(before, await projectHashes(root), Scope).map((change) => [change.path, change.allowed]), [["src/game/game.ts", true]]);
});
