import assert from "node:assert/strict";
import { test } from "node:test";
import { join } from "node:path";
import { fileURLToPath } from "node:url";
import { mkdir, mkdtemp, rm, writeFile } from "node:fs/promises";
import { copyDifferences, hasResultsRepository, recordCopyRoot, prepare } from "./prepare.ts";
import { loadEvalsData } from "./load-content.ts";
const present = await hasResultsRepository();
test("checked-in record copy matches the present results repository", { skip: !present }, async () => {
  assert.deepEqual(await copyDifferences(), [], "Run npm run evals:prepare to refresh the checked-in records.");
});
test("missing results repository keeps the checked-in copy and gives an empty validated snapshot", async () => {
  const missing = join(recordCopyRoot, "missing-results-repository");
  assert.equal(await prepare(missing), false);
  const data = await loadEvalsData(missing);
  assert.equal(data.snapshot.commit, "unknown");
  assert.deepEqual(data.snapshot.batches, []);
  assert.equal(data.schemas.length, 6);
  assert.deepEqual(data.snapshot.index.batches, []);
});

test("an empty checkout is absent and an incomplete present checkout fails the copy check", async () => {
  const temporaryRoot = fileURLToPath(new URL("../../.evals-fixture/", import.meta.url));
  await mkdir(temporaryRoot, { recursive: true });
  const root = await mkdtemp(join(temporaryRoot, "record-copy-"));
  try {
    assert.equal(await hasResultsRepository(root), false);
    await writeFile(join(root, "package.json"), "{}");
    assert.equal(await hasResultsRepository(root), true);
    await assert.rejects(copyDifferences(root));
  } finally {
    await rm(root, { recursive: true, force: true });
  }
});

