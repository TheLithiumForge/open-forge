import assert from "node:assert/strict";
import { mkdir, readFile, readdir, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { ResultFiles } from "../paths.ts";
import { summarize } from "./summary.ts";
import { checkerErrors, checkerErrorSamples, recheckRun } from "../grading/recheck.ts";
import { writeJson } from "./json-files.ts";
import { CheckStatus, RecordSchemaVersion } from "./statuses.ts";
import { FixtureVersions, legacyFixture, temporaryHistory } from "./test-fixtures.ts";

test("an old folder lists and feeds the summaries without rewriting its files", async () => {
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
    const runs = await store.readRuns(await store.readBatch(batch.id));
    const summary = summarize(runs, await store.readPreferences(batch.id));
    assert.equal(runs[0]?.checkOutcomes[1]?.status, CheckStatus.blocked);
    assert.equal(summary[0]?.coverage, 0.5);
    assert.equal(summary[0]?.tokenMissing, 1);
    assert.equal(summary[0]?.workingGate, null);
    assert.equal((await store.readSetups(listed[0] ?? await store.readBatch(batch.id)))[0]?.label, "Plain");
    const mapped = await store.readRun(batch.id, run.id);
    assert.equal(mapped.artifactHash, null);
    await recheckRun(mapped, FixtureVersions, { append: (record) => store.appendAssessment(record), evaluate: async () => ({ checks: [], shots: [], performance: null }) });
    await store.saveBatch({ ...await store.readBatch(batch.id), revealed: false });
    assert.equal((await store.readBatch(batch.id)).revealed, false);
    assert.deepEqual(await readFile(join(folder, ResultFiles.run)), before);
    assert.deepEqual(await readFile(join(store.batchFolder(batch.id), ResultFiles.batch)), beforeBatch);
    assert.deepEqual((await readdir(store.batchFolder(batch.id))).sort(), [ResultFiles.batch, "runs", ResultFiles.setups].sort());
  } finally { await cleanup(); }
});

test("a check counts the clauses it couldn't grade, and one that ended early counts as one", () => {
  const outcome = (status: CheckStatus) => ({ id: status, area: status, status, evidence: status });
  assert.equal(checkerErrors({ error: null, outcomes: [outcome(CheckStatus.pass), outcome(CheckStatus.fail), outcome(CheckStatus.blocked)] }), 0);
  assert.equal(checkerErrors({ error: null, outcomes: [outcome(CheckStatus.pass), outcome(CheckStatus.error), outcome(CheckStatus.error)] }), 2);
  assert.equal(checkerErrors({ error: "The verifier stopped.", outcomes: [] }), 1);
  const errors = [outcome(CheckStatus.error), { ...outcome(CheckStatus.error), id: "second" }, { ...outcome(CheckStatus.error), id: "third" }, { ...outcome(CheckStatus.error), id: "fourth" }];
  assert.deepEqual(checkerErrorSamples({ outcomes: [outcome(CheckStatus.pass), ...errors] }).map((sample) => sample.split(":")[0]), errors.slice(0, 3).map((item) => item.id));
});
