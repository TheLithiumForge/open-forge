import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { test } from "node:test";
import { catalogue, SchemaNames, RecordFiles, AssessmentKinds } from "../records/catalogue.ts";
import { previewBatch } from "../../../../scripts/evals/preview-fixture.ts";
import type { Batch } from "../records/records.types.ts";
import type { EvalsData } from "./snapshot.types.ts";
import { fetchLatest, loadRecords, RawRoot } from "./load.ts";
async function schemaData(): Promise<Pick<EvalsData, "schemas" | "indexSchema">> {
  const json = async (path: string): Promise<object> => JSON.parse(await readFile(new URL(`../records/schemas/${path}`, import.meta.url), "utf8"));
  return { schemas: await Promise.all(SchemaNames.map((name) => json(`v2/${name}.json`))), indexSchema: await json("index.json") };
}
function files(batches: readonly Batch[]): Map<string, string> {
  const index = catalogue(batches);
  const result = new Map([["index.json", JSON.stringify(index)]]);
  for (const batch of batches) {
    const entry = index.batches.find((item) => item.id === batch.experiment.id);
    if (entry === undefined)
      throw new Error("Missing fixture entry");
    const put = (name: string, value: unknown) => result.set(`${entry.path}/${name}`, JSON.stringify(value));
    const lines = (items: readonly unknown[]) => items.map((item) => JSON.stringify(item)).join("\n");
    put(RecordFiles.experiment, batch.experiment);
    result.set(`${entry.path}/${RecordFiles.runs}`, lines(batch.runs));
    result.set(`${entry.path}/${RecordFiles.setups}`, lines(batch.setups));
    put(RecordFiles.artifacts, batch.artifacts);
    for (const kind of AssessmentKinds)
      result.set(`${entry.path}/assessments/${kind}.jsonl`, lines(batch.assessments.filter((item) => item.kind === kind)));
  }
  return result;
}
test("empty and populated catalogues load only schema-valid records and matching references", async () => {
  const { schemas, indexSchema } = await schemaData();
  const empty = files([]);
  const read = async (path: string) => {
    const text = empty.get(path); if (text === undefined)
      throw new Error("Missing file"); return text;
  };
  assert.equal((await loadRecords(read, schemas, indexSchema)).batches.length, 0);
  const batch = previewBatch(1);
  for (const [path, value] of files([batch]))
    empty.set(path, value);
  assert.equal((await loadRecords(read, schemas, indexSchema)).batches[0]?.runs.length, batch.runs.length);
  const entry = catalogue([batch]).batches[0];
  if (entry === undefined)
    throw new Error("Missing fixture");
  empty.set(`${entry.path}/${RecordFiles.runs}`, "{\"schemaVersion\":1}");
  await assert.rejects(loadRecords(read, schemas, indexSchema), /Invalid results data/);
  empty.set("index.json", JSON.stringify({ schemaVersion: 1, batches: [{ ...entry, path: "../unsafe" }] }));
  await assert.rejects(loadRecords(read, schemas, indexSchema), /Invalid results data/);
});
test("runtime fetch pins every raw record to one commit and rejects partial or invalid refreshes", async () => {
  const schema = await schemaData(), batch = previewBatch(1), content = files([batch]);
  const commit = "a".repeat(40), requests: string[] = [];
  const data: EvalsData = { ...schema, snapshot: { batches: [], index: catalogue([]), commit, fetchedAt: "fixture" } };
  const request: typeof fetch = async (input) => {
    const url = String(input);
    requests.push(url);
    if (url.startsWith("https://api.github.com/"))
      return Response.json({ sha: commit });
    assert.ok(url.startsWith(`${RawRoot}/${commit}/`));
    const text = content.get(url.slice(`${RawRoot}/${commit}/`.length));
    return new Response(text ?? "Missing", { status: text === undefined ? 404 : 200 });
  };
  const latest = await fetchLatest(data, request);
  assert.equal(latest.commit, commit);
  assert.equal(latest.batches.length, 1);
  assert.ok(requests.length > 1);
  content.set("index.json", "{\"schemaVersion\":999,\"batches\":[]}");
  await assert.rejects(fetchLatest(data, request), /Invalid results data/);
  assert.equal(data.snapshot.batches.length, 0);
});
