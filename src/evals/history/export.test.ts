import assert from "node:assert/strict";
import { mkdir, readFile, readdir, writeFile } from "node:fs/promises";
import { dirname, join, relative } from "node:path";
import { test } from "node:test";
import { ResultFiles } from "../paths.ts";
import { checksAssessment } from "./assessments.ts";
import { artifactHash, HistoryFiles, PublishedFileBytes, publishedFile } from "./artifacts.ts";
import { exportBatch, ExportFolders } from "./export.ts";
import { readJson, readJsonLines } from "./json-files.ts";
import type { ArtifactRecord, PickAssessment, RunRecord } from "./records.types.ts";
import { Redacted, sanitizeRecords } from "./sanitize.ts";
import { AssessmentKind, PreferenceWinner, RecordSchemaVersion } from "./statuses.ts";
import { FixtureCliVersion, FixtureTime, FixtureVersions, temporaryHistory, v2Fixture } from "./test-fixtures.ts";

test("export writes the versioned records and removes machine paths and user names", async () => {
  const { root, store, cleanup } = await temporaryHistory();
  try {
    const { batch, run } = v2Fixture();
    const machinePath = join(root, "private", "source.ts");
    const userName = "example-person";
    const local = { ...run, finalMessage: `Read ${machinePath} for ${userName}.`, build: run.build === null ? null : { ...run.build, log: machinePath } };
    await store.saveBatch(batch);
    await store.saveRun(local);
    await store.saveSetups(batch.id, [{ schemaVersion: RecordSchemaVersion, id: run.setup, experimentId: batch.id,
      label: run.setup, commands: [], files: [], contentHash: null }]);
    await store.appendAssessment(checksAssessment(local, local.checks, FixtureVersions));
    const before = await readFile(join(store.runFolder(batch.id, run.id), ResultFiles.run));
    const output = await exportBatch(store, batch.id, join(root, "published"), { userNames: [userName] });
    assert.equal(relative(join(root, "published"), output), join(ExportFolders.versions, FixtureCliVersion, ExportFolders.batches, batch.id));
    assert.deepEqual((await readdir(output)).sort(), [HistoryFiles.experiment, HistoryFiles.setups, HistoryFiles.runs, HistoryFiles.artifacts, HistoryFiles.assessments].sort());
    const records = await readJsonLines<Record<string, unknown>>(join(output, HistoryFiles.runs));
    assert.equal(records[0]?.["schemaVersion"], RecordSchemaVersion);
    assert.equal("checks" in (records[0] ?? {}), false);
    assert.equal("status" in (records[0] ?? {}), false);
    const encoded = await readFile(join(output, HistoryFiles.runs), "utf8");
    assert.equal(encoded.includes(machinePath), false);
    assert.equal(encoded.includes(userName), false);
    assert.equal(encoded.includes(Redacted.path), true);
    assert.equal(encoded.includes(Redacted.user), true);
    for (const kind of Object.values(AssessmentKind)) await readFile(join(output, HistoryFiles.assessments, `${kind}.jsonl`));
    assert.equal((await readJson<Record<string, unknown>>(join(output, HistoryFiles.experiment)))["schemaVersion"], RecordSchemaVersion);
    assert.deepEqual(await readFile(join(store.runFolder(batch.id, run.id), ResultFiles.run)), before);
    await assert.rejects(exportBatch(store, batch.id, join(root, "published")));
  } finally { await cleanup(); }
});

test("a published batch leaves tool downloads out, and each run's and pick's hash covers the evidence that stays", async () => {
  const { root, store, cleanup } = await temporaryHistory();
  try {
    const { batch, run } = v2Fixture();
    const other = { ...run, id: `${run.id}-other` };
    const kept = "files/src/game.ts", download = "files/.playwright-browsers/shell/chrome";
    await store.saveBatch({ ...batch, runs: [run.id, other.id] });
    for (const view of [run, other]) {
      for (const path of [kept, download]) {
        await mkdir(dirname(join(store.runFolder(batch.id, view.id), path)), { recursive: true });
        await writeFile(join(store.runFolder(batch.id, view.id), path), `${view.id} ${path}`);
      }
      await store.saveRun(view);
    }
    await store.saveSetups(batch.id, [{ schemaVersion: RecordSchemaVersion, id: run.setup, experimentId: batch.id, label: run.setup, commands: [], files: [], contentHash: null }]);
    await store.addPreference(batch.id, { left: run.id, right: other.id, winner: PreferenceWinner.left, at: FixtureTime });
    const output = await exportBatch(store, batch.id, join(root, "published"), { scrubMachineUser: false });
    const artifacts = await readJson<ArtifactRecord[]>(join(output, HistoryFiles.artifacts));
    assert.deepEqual(artifacts.map((artifact) => artifact.path), [kept, kept]);
    const runs = await readJsonLines<RunRecord>(join(output, HistoryFiles.runs));
    for (const record of runs) assert.equal(record.artifactHash, artifactHash(artifacts.filter((artifact) => artifact.runId === record.id)));
    const [pick] = await readJsonLines<PickAssessment>(join(output, HistoryFiles.assessments, `${AssessmentKind.pick}.jsonl`));
    assert.deepEqual([pick?.left.artifactHash, pick?.right.artifactHash], runs.map((record) => record.artifactHash));
    assert.equal((await store.readArtifacts(await store.readBatch(batch.id))).filter((artifact) => artifact.path === download).length, 2);
  } finally { await cleanup(); }
});

test("a file over the published limit stays out wherever it is, and one at the limit goes in", () => {
  const path = "files/any-folder/download.zip";
  assert.equal(publishedFile({ path, bytes: PublishedFileBytes }), true);
  assert.equal(publishedFile({ path, bytes: PublishedFileBytes + 1 }), false);
});

test("sanitizing finds names in home paths and removes session and credential fields", () => {
  const name = "example-person";
  const windowsPath = ["Q:", "Users", name, "project", "file.ts"].join("\\");
  const unixPath = ["", "home", name, "project", "file.ts"].join("/");
  const result = sanitizeRecords({ evidence: `${windowsPath} ${unixPath} ${name}`, sessionId: "private-session", username: name,
    apiKey: "private-key", tokens: null, path: "files/source.ts", nested: [{ evidence: name }] });
  const encoded = JSON.stringify(result);
  for (const secret of [windowsPath, unixPath, name, "private-session", "private-key"]) assert.equal(encoded.includes(secret), false);
  assert.deepEqual(result, { evidence: `${Redacted.path} ${Redacted.path} ${Redacted.user}`, tokens: null, path: "files/source.ts", nested: [{ evidence: Redacted.user }] });
});

test("export leaves an unfinished batch out of the destination", async () => {
  const { root, store, cleanup } = await temporaryHistory();
  try {
    const { batch, run } = v2Fixture();
    await store.saveBatch(batch);
    await store.saveRun({ ...run, finishedAt: null });
    const destination = join(root, "published");
    await mkdir(destination);
    await assert.rejects(exportBatch(store, batch.id, destination));
    assert.deepEqual(await readdir(destination), []);
  } finally { await cleanup(); }
});
