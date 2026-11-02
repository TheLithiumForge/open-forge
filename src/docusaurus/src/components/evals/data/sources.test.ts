import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { test } from "node:test";
import { AssessmentKinds, catalogue, RecordFiles, SchemaNames } from "../records/catalogue.ts";
import { previewBatch } from "../../../../scripts/evals/preview-fixture.ts";
import type { EvalsData } from "./snapshot.types.ts";
import { batchProgress } from "./progress.ts";
import { batchPath, commitSource, evidencePath, folderFiles, loadFolder, loadLocal, localPort, SourceKind } from "./sources.ts";

async function data(): Promise<EvalsData> {
  const json = async (path: string): Promise<object> => JSON.parse(await readFile(new URL(`../records/schemas/${path}`, import.meta.url), "utf8"));
  return { schemas: await Promise.all(SchemaNames.map((name) => json(`v2/${name}.json`))), indexSchema: await json("index.json"),
    snapshot: { batches: [], index: { schemaVersion: 1, batches: [] }, commit: "unknown", fetchedAt: "" } };
}

/** A results tree holding one batch, keyed by path, with each file inside a downloaded folder. */
function tree() {
  const batch = previewBatch(1), lines = (items: readonly unknown[]) => items.map((item) => JSON.stringify(item)).join("\n");
  const folder = batchPath(batch);
  const files = new Map<string, string>([["index.json", JSON.stringify(catalogue([batch]))],
    [`${folder}/${RecordFiles.experiment}`, JSON.stringify(batch.experiment)], [`${folder}/${RecordFiles.setups}`, lines(batch.setups)],
    [`${folder}/${RecordFiles.runs}`, lines(batch.runs)], [`${folder}/${RecordFiles.artifacts}`, JSON.stringify(batch.artifacts)],
    ...AssessmentKinds.map((kind): [string, string] => [`${folder}/assessments/${kind}.jsonl`, lines(batch.assessments.filter((item) => item.kind === kind))])]);
  return { batch, files };
}

test("only a whole port in range opens local results", () => {
  assert.equal(localPort("?local=4321"), 4321);
  for (const search of ["", "?local=", "?local=80", "?local=70000", "?local=4321.5", "?local=localhost"]) assert.equal(localPort(search), null);
});

test("a published commit links files and folders, and an unpublished copy links nothing", () => {
  const commit = "a".repeat(40), path = "versions/1/batches/b/runs.jsonl";
  assert.ok(commitSource(SourceKind.latest, "Latest", commit).file(path)?.endsWith(`/${commit}/${path}`));
  assert.ok(commitSource(SourceKind.latest, "Latest", commit).folder(path)?.includes(commit));
  assert.equal(commitSource(SourceKind.published, "Copy", "unknown").file(path), null);
});

test("evidence paths sit beside the batch records", () => {
  const { batch } = tree();
  assert.equal(evidencePath(batch, { schemaVersion: 2, runId: "run", assessmentId: null, path: "shots/a.png", bytes: 1, sha256: "" }), `${batchPath(batch)}/evidence/runs/run/shots/a.png`);
  assert.equal(evidencePath(batch, { schemaVersion: 2, runId: "run", assessmentId: "grade", path: "shots/a.png", bytes: 1, sha256: "" }), `${batchPath(batch)}/evidence/assessments/grade/shots/a.png`);
});

test("a picked folder finds its results root and loads, and local results load from the data server", async () => {
  const { batch, files } = tree();
  const picked = [...files].map(([path, text]) => Object.assign(new Blob([text]), { webkitRelativePath: `download/results-main/${path}` }));
  const found = folderFiles([...picked, Object.assign(new Blob([""]), { webkitRelativePath: "download/notes.txt" })]);
  assert.deepEqual([...found.keys()].sort(), [...files.keys()].sort());
  assert.throws(() => folderFiles([Object.assign(new Blob([""]), { webkitRelativePath: "download/notes.txt" })]));
  const fromFolder = await loadFolder(await data(), found, "results-main", () => "blob:fixture");
  assert.equal(fromFolder.snapshot.batches.length, 1);
  assert.equal(fromFolder.source.file("index.json"), "blob:fixture");
  const asked: string[] = [];
  const request = (async (url: string) => { asked.push(url); const text = files.get(url.replace("http://localhost:4321/", "")); return new Response(text ?? "", { status: text === undefined ? 404 : 200 }); }) as typeof fetch;
  const local = await loadLocal(await data(), 4321, request);
  assert.equal(local.snapshot.batches[0]?.experiment.id, batch.experiment.id);
  assert.ok(asked.every((url) => url.startsWith("http://localhost:4321/")));
});

test("progress counts implemented, checked and judged runs of a batch", () => {
  const { batch } = tree();
  const progress = batchProgress(batch);
  assert.ok(progress.implemented <= progress.planned);
  assert.ok(progress.checked <= progress.implemented);
  for (const judge of progress.judges) assert.ok(judge.judged <= progress.implemented);
});
