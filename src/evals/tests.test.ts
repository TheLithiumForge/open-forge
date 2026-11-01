import assert from "node:assert/strict";
import { test } from "node:test";
import { mkdtemp, rm } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { IgnoredTestFolders, productTests } from "./tests.ts";
import { writeNew } from "./materialize/files.ts";

test("product discovery keeps reference game tests outside the product suite", async () => {
  const files = await productTests();
  assert.ok(files.length > 0);
  assert.equal(files.some((path) => path.split(/[\\/]/).includes("reference")), false);
});

test("product discovery skips caches, dependencies and generated results at every depth", async () => {
  const root = await mkdtemp(join(tmpdir(), "eval-test-discovery-"));
  const source = "config/fixture.test.ts";
  try {
    await writeNew(root, source, "Fixture source test.");
    for (const folder of IgnoredTestFolders) {
      await writeNew(root, `${folder}/config/ignored.test.ts`, "Ignored fixture.");
      await writeNew(root, `config/${folder}/ignored.test.ts`, "Ignored fixture.");
    }
    assert.deepEqual((await productTests(root)).map((path) => path.replaceAll("\\", "/")), [source]);
  } finally { await rm(root, { recursive: true, force: true }); }
});
