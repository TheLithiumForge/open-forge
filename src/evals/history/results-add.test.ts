import assert from "node:assert/strict";
import { mkdir, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { addResults, evidenceLeaks, LeakKind, ResultsScripts } from "./results-add.ts";
import { RecordSchemaVersion } from "./statuses.ts";
import { temporaryHistory, v2Fixture } from "./test-fixtures.ts";

const Machine = { home: ["Q:", "Users", "example-person"].join("\\"), user: "example-person" } as const;

test("evidence that holds a sign-in, a machine path or the user name is reported by file, and encoded blobs don't count", () => {
  const secret = "a-sign-in-value-long-enough";
  const files = new Map([
    ["runs/a/events.jsonl", Buffer.from(`{"token":"${secret}"}`)],
    ["runs/a/build.log", Buffer.from(`Built in ${Machine.home.replaceAll("\\", "/")}/project`)],
    ["runs/a/conversation.json", Buffer.from(`Thanks, ${Machine.user}.`)],
    ["runs/a/encoded.txt", Buffer.from(`QmFzZTY0${Machine.user}cmVhc29uaW5n`)],
    ["runs/a/game.html", Buffer.from("<main>Neon Pairs</main>")],
  ]);
  assert.deepEqual(evidenceLeaks(files, [secret], Machine), [
    { file: "runs/a/events.jsonl", kind: LeakKind.secret },
    { file: "runs/a/build.log", kind: LeakKind.machinePath },
    { file: "runs/a/conversation.json", kind: LeakKind.userName },
  ]);
});

async function finishedBatch(store: Awaited<ReturnType<typeof temporaryHistory>>["store"], evidence: string) {
  const { batch, run } = v2Fixture();
  await store.saveBatch(batch);
  // Saving a run lists the files already in its folder as its evidence.
  await mkdir(store.runFolder(batch.id, run.id), { recursive: true });
  await writeFile(join(store.runFolder(batch.id, run.id), "conversation.json"), evidence);
  await store.saveRun(run);
  await store.saveSetups(batch.id, [{ schemaVersion: RecordSchemaVersion, id: run.setup, experimentId: batch.id, label: run.setup, commands: [], files: [], contentHash: null }]);
  return batch;
}

test("results add stages the exported batch with its evidence folder, then validates the repository", async () => {
  const { root, store, cleanup } = await temporaryHistory();
  try {
    const batch = await finishedBatch(store, "Plain evidence.");
    const repository = join(root, "repository");
    await mkdir(join(repository, "scripts"), { recursive: true });
    await mkdir(join(repository, "node_modules"));
    await writeFile(join(repository, ResultsScripts.stage), "");
    const calls: [string, readonly string[], string][] = [];
    await addResults(store, batch.id, repository, { run: async (script, args, cwd) => { calls.push([script, args, cwd]); return 0; }, secrets: async () => [], machine: Machine });
    assert.deepEqual(calls.map(([script, args, cwd]) => [script, args.filter((arg) => arg.startsWith("--")), cwd]), [
      [ResultsScripts.stage, ["--artifacts"], repository],
      [ResultsScripts.validate, [], repository],
    ]);
  } finally { await cleanup(); }
});

test("results add stops before staging when evidence would publish private data or the repository isn't ready", async () => {
  const { root, store, cleanup } = await temporaryHistory();
  try {
    const batch = await finishedBatch(store, `Thanks, ${Machine.user}.`);
    const repository = join(root, "repository");
    const calls: string[] = [];
    const services = { run: async (script: string) => { calls.push(script); return 0; }, secrets: async () => [], machine: Machine };
    await assert.rejects(addResults(store, batch.id, repository, services));
    await mkdir(join(repository, "scripts"), { recursive: true });
    await writeFile(join(repository, ResultsScripts.stage), "");
    await assert.rejects(addResults(store, batch.id, repository, services));
    await mkdir(join(repository, "node_modules"));
    await assert.rejects(addResults(store, batch.id, repository, services));
    assert.deepEqual(calls, []);
  } finally { await cleanup(); }
});

test("an imported batch is republished without treating this machine's user name as a leak, while its home path still stops it", async () => {
  const { root, store, cleanup } = await temporaryHistory();
  try {
    const repository = join(root, "repository");
    await mkdir(join(repository, "scripts"), { recursive: true });
    await writeFile(join(repository, ResultsScripts.stage), "");
    await mkdir(join(repository, "node_modules"));
    const named = await finishedBatch(store, `Thanks, ${Machine.user}.`);
    const calls: string[] = [];
    const services = { run: async (script: string) => { calls.push(script); return 0; }, secrets: async () => [], machine: Machine, imported: true };
    await addResults(store, named.id, repository, services);
    assert.deepEqual(calls, [ResultsScripts.stage, ResultsScripts.validate]);
    const pathed = await finishedBatch(store, `Read ${Machine.home}.`);
    await assert.rejects(addResults(store, pathed.id, repository, services));
  } finally { await cleanup(); }
});
