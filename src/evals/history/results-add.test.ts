import assert from "node:assert/strict";
import { mkdir, readdir, readFile, writeFile } from "node:fs/promises";
import { dirname, join } from "node:path";
import { test } from "node:test";
import { CodexRolloutFile } from "../execution/providers/codex-telemetry.ts";
import { checksAssessment } from "./assessments.ts";
import { artifactHash, HistoryFiles } from "./artifacts.ts";
import { sha256 } from "./fingerprints.ts";
import { readJson, readJsonLines } from "./json-files.ts";
import type { ArtifactRecord, RunRecord } from "./records.types.ts";
import { addResults, authFileSecrets, evidenceLeaks, LeakKind, ResultsScripts, scrubbedTranscript } from "./results-add.ts";
import { RecordSchemaVersion } from "./statuses.ts";
import { FixtureVersions, temporaryHistory, v2Fixture } from "./test-fixtures.ts";

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

test("tool downloads reach neither staging nor the leak check, since they never publish", async () => {
  const { root, store, cleanup } = await temporaryHistory();
  try {
    const { batch, run } = v2Fixture();
    const download = join("files", ".npm-cache", "tool.tgz");
    await store.saveBatch(batch);
    await mkdir(dirname(join(store.runFolder(batch.id, run.id), download)), { recursive: true });
    await writeFile(join(store.runFolder(batch.id, run.id), download), `Thanks, ${Machine.user}.`);
    await writeFile(join(store.runFolder(batch.id, run.id), "conversation.json"), "Plain evidence.");
    await store.saveRun(run);
    await store.saveSetups(batch.id, [{ schemaVersion: RecordSchemaVersion, id: run.setup, experimentId: batch.id, label: run.setup, commands: [], files: [], contentHash: null }]);
    const repository = join(root, "repository");
    await mkdir(join(repository, "scripts"), { recursive: true });
    await mkdir(join(repository, "node_modules"));
    await writeFile(join(repository, ResultsScripts.stage), "");
    let staged: string[] = [];
    await addResults(store, batch.id, repository, { run: async (script, args) => {
      if (script === ResultsScripts.stage) staged = await readdir(join(args[2] ?? "", "runs", run.id));
      return 0;
    }, secrets: async () => [], machine: Machine });
    assert.deepEqual(staged, ["conversation.json"]);
  } finally { await cleanup(); }
});

test("a transcript's account fields are scrubbed with its record following, and the same id elsewhere still stops the publish", async () => {
  const { root, store, cleanup } = await temporaryHistory();
  try {
    const repository = join(root, "repository");
    await mkdir(join(repository, "scripts"), { recursive: true });
    await mkdir(join(repository, "node_modules"));
    await writeFile(join(repository, ResultsScripts.stage), "");
    const { batch, run } = v2Fixture();
    const account = "account-id-long-enough-to-count", transcript = (text: string) => `{"type":"session_meta","payload":{"creator_user_id":"${account}","creator_account_id":"${account}","id":"kept"}}\n${text}\n`;
    await store.saveBatch(batch);
    await mkdir(store.runFolder(batch.id, run.id), { recursive: true });
    const save = async (text: string) => { await writeFile(join(store.runFolder(batch.id, run.id), CodexRolloutFile), transcript(text)); await store.saveRun(run); };
    await save("plain");
    await store.saveSetups(batch.id, [{ schemaVersion: RecordSchemaVersion, id: run.setup, experimentId: batch.id, label: run.setup, commands: [], files: [], contentHash: null }]);
    const services = (seen: (exported: string, evidence: string) => Promise<void>) => ({ secrets: async () => [account], machine: Machine,
      run: async (script: string, args: readonly string[]) => { if (script === ResultsScripts.stage) await seen(args[0] ?? "", args[2] ?? ""); return 0; } });
    let checked = false;
    await addResults(store, batch.id, repository, services(async (exported, evidence) => {
      const staged = await readFile(join(evidence, "runs", run.id, CodexRolloutFile));
      assert.equal(staged.includes(account), false);
      assert.deepEqual(staged, scrubbedTranscript(Buffer.from(transcript("plain"))));
      const [artifact] = await readJson<ArtifactRecord[]>(join(exported, HistoryFiles.artifacts));
      assert.deepEqual([artifact?.bytes, artifact?.sha256], [staged.length, sha256(staged)]);
      const [record] = await readJsonLines<RunRecord>(join(exported, HistoryFiles.runs));
      assert.equal(record?.artifactHash, artifactHash(artifact === undefined ? [] : [artifact]));
      checked = true;
    }));
    assert.equal(checked, true);
    await save(account);
    await assert.rejects(addResults(store, batch.id, repository, services(async () => { assert.fail(); })));
  } finally { await cleanup(); }
});

test("a sign-in file's secrets include its tokens' claims, except the provider's web addresses", () => {
  const email = "someone@example.invalid", provider = "https://issuer.example.invalid";
  const token = ["header", Buffer.from(JSON.stringify({ email, iss: provider })).toString("base64url"), "signature"].join(".");
  const auth = { refresh: "not-a-token", tokens: { id_token: token, broken: "one.%%%.two" } };
  assert.deepEqual(authFileSecrets(auth), ["not-a-token", token, "one.%%%.two", email]);
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

test("an imported batch checks only its new evidence, so published run evidence can quote any path and a new check's evidence can't", async () => {
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
    await addResults(store, pathed.id, repository, services);
    const [run] = await store.readRuns(pathed);
    assert.ok(run !== undefined);
    const check = checksAssessment(run, run.checks, FixtureVersions);
    await mkdir(store.assessmentEvidenceFolder(pathed.id, check.id), { recursive: true });
    await writeFile(join(store.assessmentEvidenceFolder(pathed.id, check.id), "report.json"), `Read ${Machine.home}.`);
    await store.appendAssessment(check);
    await assert.rejects(addResults(store, pathed.id, repository, services));
  } finally { await cleanup(); }
});
