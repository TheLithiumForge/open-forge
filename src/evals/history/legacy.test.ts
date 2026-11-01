import assert from "node:assert/strict";
import { mkdir, readFile, readdir, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { batchView } from "../web/server/routes.ts";
import { allPairs, nextPair } from "../web/server/compare.ts";
import { ResultFiles } from "../paths.ts";
import { recheckRun } from "../grading/recheck.ts";
import { writeJson } from "./json-files.ts";
import { CheckStatus, RecordSchemaVersion } from "./statuses.ts";
import { FixtureVersions, legacyFixture, temporaryHistory } from "./test-fixtures.ts";

test("an old folder lists and feeds the UI reader without rewriting its files", async () => {
  const { store, cleanup } = await temporaryHistory();
  try {
    const { batch, run } = legacyFixture();
    const oldRun = { ...run, checks: [...run.checks, { id: "keyboard", area: "play", passed: null, detail: "blocked" }] };
    const folder = store.runFolder(batch.id, run.id);
    await mkdir(folder, { recursive: true });
    await writeJson(join(store.batchFolder(batch.id), ResultFiles.batch), batch);
    await writeJson(join(folder, ResultFiles.run), oldRun);
    await writeJson(join(store.batchFolder(batch.id), ResultFiles.setups), [{ setup: run.setup, label: "Plain", commands: [], output: "", files: [] }]);
    await writeFile(join(folder, ResultFiles.game), "legacy game");
    const before = await readFile(join(folder, ResultFiles.run));
    const beforeBatch = await readFile(join(store.batchFolder(batch.id), ResultFiles.batch));
    const listed = await store.listBatches();
    assert.equal(listed[0]?.schemaVersion, RecordSchemaVersion);
    assert.equal(listed[0]?.legacy, true);
    const view = await batchView(batch.id, {
      readBatch: (id) => store.readBatch(id), readRuns: (value) => store.readRuns(value), readPreferences: (id) => store.readPreferences(id),
    });
    assert.equal(view.runs?.[0]?.checkOutcomes[1]?.status, CheckStatus.blocked);
    assert.equal(view.summary?.[0]?.coverage, 0.5);
    assert.equal(view.summary?.[0]?.tokenMissing, 1);
    assert.equal(view.summary?.[0]?.workingGate, null);
    assert.equal((await store.readSetups(listed[0] ?? await store.readBatch(batch.id)))[0]?.label, "Plain");
    const mapped = await store.readRun(batch.id, run.id);
    assert.equal(mapped.artifactHash, null);
    const other = { ...oldRun, id: `${oldRun.id}-other` };
    await writeJson(join(store.runFolder(batch.id, other.id), ResultFiles.run), other);
    await writeFile(join(store.runFolder(batch.id, other.id), ResultFiles.game), "another legacy game");
    const both = [mapped, await store.readRun(batch.id, other.id)];
    assert.equal(allPairs(both, store.runFolder.bind(store)).length, 1);
    const pair = nextPair(both, [], 4318, store.runFolder.bind(store));
    assert.deepEqual(new Set([pair?.left, pair?.right]), new Set([run.id, other.id]));
    await recheckRun(mapped, FixtureVersions, { append: (record) => store.appendAssessment(record), evaluate: async () => ({ checks: [], shots: [], performance: null }) });
    await store.saveBatch({ ...await store.readBatch(batch.id), revealed: false });
    assert.equal((await store.readBatch(batch.id)).revealed, false);
    assert.deepEqual(await readFile(join(folder, ResultFiles.run)), before);
    assert.deepEqual(await readFile(join(store.batchFolder(batch.id), ResultFiles.batch)), beforeBatch);
    assert.deepEqual((await readdir(store.batchFolder(batch.id))).sort(), [ResultFiles.batch, "runs", ResultFiles.setups].sort());
  } finally { await cleanup(); }
});
