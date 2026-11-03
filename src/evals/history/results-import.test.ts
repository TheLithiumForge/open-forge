import assert from "node:assert/strict";
import { mkdir, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { checksAssessment } from "./assessments.ts";
import { HistoryFiles } from "./artifacts.ts";
import { exportBatch } from "./export.ts";
import { importResults } from "./results-import.ts";
import { HistoryStore } from "./store.ts";
import { RecordSchemaVersion } from "./statuses.ts";
import { FixtureVersions, temporaryHistory, v2Fixture } from "./test-fixtures.ts";

test("an imported published batch has the same runs, setups and assessments as the batch that was published, and imports once", async () => {
  const { root, store, cleanup } = await temporaryHistory();
  try {
    const { batch, run } = v2Fixture();
    await store.saveBatch(batch);
    await store.saveRun(run);
    await store.saveSetups(batch.id, [{ schemaVersion: RecordSchemaVersion, id: run.setup, experimentId: batch.id, label: run.setup, commands: [], files: [], contentHash: null }]);
    await store.appendAssessment(checksAssessment(run, run.checks, FixtureVersions));
    const published = await exportBatch(store, batch.id, join(root, "published"));
    // Staging copies each run's evidence beside the records, so the import finds the run's own files there.
    const evidence = join(published, HistoryFiles.evidence, "runs", run.id);
    await mkdir(evidence, { recursive: true });
    await writeFile(join(evidence, "game.html"), "<!doctype html>");
    const copy = new HistoryStore(join(root, "imported"));
    assert.equal(await importResults(published, copy), batch.id);
    const original = await store.readBatch(batch.id), imported = await copy.readBatch(batch.id);
    assert.deepEqual(imported.runs, original.runs);
    assert.deepEqual((await copy.readRuns(imported)).map((view) => [view.id, view.setup, view.functionalResult]), (await store.readRuns(original)).map((view) => [view.id, view.setup, view.functionalResult]));
    assert.deepEqual(await copy.readSetups(imported), await store.readSetups(original));
    assert.deepEqual(await copy.readAssessments(batch.id), await store.readAssessments(batch.id));
    await assert.rejects(importResults(published, copy));
  } finally { await cleanup(); }
});
