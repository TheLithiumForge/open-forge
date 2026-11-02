import assert from "node:assert/strict";
import { mkdir, writeFile } from "node:fs/promises";
import type { AddressInfo } from "node:net";
import { join } from "node:path";
import { test } from "node:test";
import { RecordSchemaVersion } from "../history/statuses.ts";
import { FixtureCliVersion, temporaryHistory, v2Fixture } from "../history/test-fixtures.ts";
import { startDataServer } from "./data-server.ts";
import { folderReader, IndexFile, localRunsReader } from "./results-reader.ts";

async function savedBatch() {
  const history = await temporaryHistory();
  const { batch, run } = v2Fixture();
  await history.store.saveBatch(batch);
  await mkdir(history.store.runFolder(batch.id, run.id), { recursive: true });
  await writeFile(join(history.store.runFolder(batch.id, run.id), "conversation.json"), "{}\n");
  await history.store.saveRun(run);
  await history.store.saveSetups(batch.id, [{ schemaVersion: RecordSchemaVersion, id: run.setup, experimentId: batch.id, label: run.setup, commands: [], files: [], contentHash: null }]);
  return { ...history, batch, run, folder: `versions/${FixtureCliVersion}/batches/${batch.id}` };
}

test("local runs read as the published layout: a catalogue, each batch's records and its evidence", async (t) => {
  const saved = await savedBatch(); t.after(saved.cleanup);
  const read = localRunsReader(saved.store);
  const index = await read(IndexFile);
  assert.ok(index);
  const catalogue = JSON.parse(index.bytes.toString("utf8"));
  assert.deepEqual(catalogue.batches.map((entry: { path: string }) => entry.path), [saved.folder]);
  for (const name of ["experiment.json", "setups.jsonl", "runs.jsonl", "artifacts.json", "assessments/checks.jsonl"]) assert.ok(await read(`${saved.folder}/${name}`));
  assert.ok(await read(`${saved.folder}/evidence/runs/${saved.run.id}/conversation.json`));
  for (const path of [`${saved.folder}/unknown.json`, `${saved.folder}/evidence/runs/${saved.run.id}/../../../experiment.json`, `versions/${FixtureCliVersion}/batches/missing/runs.jsonl`]) assert.equal(await read(path), null);
});

test("a results folder serves only files inside it", async (t) => {
  const saved = await savedBatch(); t.after(saved.cleanup);
  await writeFile(join(saved.root, IndexFile), "{}\n");
  const read = folderReader(saved.root);
  assert.ok(await read(IndexFile));
  assert.equal(await read(`../${IndexFile}`), null);
  assert.equal(await read(""), null);
});

test("the data server answers reads from the docs origin only, and nothing else", async (t) => {
  const origin = "http://localhost:4320";
  const server = await startDataServer(async (path) => path === IndexFile ? { bytes: Buffer.from("{}"), type: "application/json" } : null, 0, origin);
  t.after(() => server.close());
  const base = `http://127.0.0.1:${(server.address() as AddressInfo).port}`;
  const found = await fetch(`${base}/${IndexFile}`);
  assert.equal(found.status, 200);
  assert.equal(found.headers.get("access-control-allow-origin"), origin);
  assert.equal((await fetch(`${base}/missing.json`)).status, 404);
  assert.equal((await fetch(`${base}/${IndexFile}`, { method: "POST" })).status, 405);
});
