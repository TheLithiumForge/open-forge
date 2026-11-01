import assert from "node:assert/strict";
import { mkdir, mkdtemp, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { dirname, join } from "node:path";
import { test } from "node:test";
import { changedFiles, projectHashes, scopeOutcome } from "./scope.ts";
import { inventory, sha256, writeNew } from "../../materialize/files.ts";
import { captureScope, verifyScope, ForgeScope } from "../../materialize/scope-policy.ts";
import { IgnoreFile } from "../../materialize/ignore.ts";
import { brownfieldFixture } from "../../materialize/brownfield-test-fixture.ts";
import { loadBrownfieldSuite } from "../../materialize/brownfield.ts";
import { documentInstaller, DocumentFixtureFiles } from "../../materialize/document-test-fixture.ts";
import { generateSetup } from "../../config/setups.ts";
import { CheckStatus } from "../../history/statuses.ts";
import { CodeOnlySetup } from "../../materialize/brownfield-case.ts";

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

test("scope uses the saved ignore rules, counts edits to them and ignores the project's own output", async (t) => {
  const root = await project(t), folders = [".art", ".session", ".npm-cache", ".playwright-browsers", ".browser-work"];
  await writeNew(root, IgnoreFile, `${folders.map((folder) => `${folder}/`).join("\n")}\n`);
  const policy = captureScope(await inventory(root), false), before = await projectHashes(root, policy);
  verifyScope(policy, before);
  const syntheticOutput = folders.map((folder) => ({ path: `${folder}/output.txt`, sha256: sha256("Fixture output") }));
  assert.deepEqual(changedFiles(before, [...before, ...syntheticOutput], Scope, policy), []);
  await writeFile(join(root, IgnoreFile), "outside.txt\n");
  await writeNew(root, "outside.txt", "Unrelated source.");
  const changes = changedFiles(before, await projectHashes(root, policy), Scope, policy);
  assert.deepEqual(changes.map((file) => file.path), [IgnoreFile, "outside.txt"]);
  assert.equal(scopeOutcome(changes).status, CheckStatus.fail);
  assert.throws(() => verifyScope({ ...policy, ignores: [{ path: IgnoreFile, content: "outside.txt\n" }] }, before));
});

test("Open Forge files and routes are reported without failing scope only in an Open Forge cell", async (t) => {
  const fixture = await brownfieldFixture(t), suite = await loadBrownfieldSuite(fixture.caseRoot, undefined, fixture.reference, fixture.destination);
  for (const setup of ["minimal", CodeOnlySetup]) {
    const generated = await generateSetup(suite, setup, documentInstaller());
    try {
      const policy = generated.materialized?.scope; assert.ok(policy);
      const before = generated.record.files;
      await writeNew(generated.folder, `${ForgeScope.root}memory/new.md`, "Record the work.\n");
      await writeFile(join(generated.folder, ForgeScope.agents), "Read the updated project context.\n");
      if (policy.openForge) await writeFile(join(generated.folder, DocumentFixtureFiles.catalogue), "# Updated document routes\n");
      const changes = changedFiles(before, await projectHashes(generated.folder, policy), Scope, policy);
      assert.ok(changes.length >= 2);
      assert.equal(scopeOutcome(changes).status, policy.openForge ? CheckStatus.pass : CheckStatus.fail);
      assert.ok(changes.every((change) => Boolean(change.openForge) === policy.openForge));
    } finally { await rm(dirname(generated.folder), { recursive: true, force: true }); }
  }
});
