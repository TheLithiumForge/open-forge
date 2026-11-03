import assert from "node:assert/strict";
import { mkdir, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { Tool } from "../../config/choices.ts";
import { ResultFiles } from "../../paths.ts";
import { localRunsReader } from "../../site/results-reader.ts";
import { HistoryFiles } from "../artifacts.ts";
import { batchFiles } from "../export.ts";
import { RecordSchemaVersion } from "../statuses.ts";
import { FixtureCliVersion, temporaryHistory, v2Fixture } from "../test-fixtures.ts";
import { analyzeBatch, needsAnalysis } from "./analyze-batches.ts";
import { AnalysisFile, AnalysisMethod, AnalysisVersion } from "./classes.ts";

const Events = [{ type: "turn.started" }, { type: "item.completed", item: { type: "command_execution", command: "cat docs/rules.md", aggregated_output: "rules", exit_code: 0 } },
  { type: "turn.completed", usage: { input_tokens: 300, cached_input_tokens: 200, output_tokens: 10 } }].map((event) => JSON.stringify(event)).join("\n");

async function savedBatch() {
  const history = await temporaryHistory();
  const { batch, run: template } = v2Fixture();
  const run = { ...template, agent: { ...template.agent, tool: Tool.codex }, telemetry: { tokens: { input: 100, cachedInput: 200, cacheWrite: 0, output: 10, reasoning: 0 }, requests: 2, peakContext: 200,
    contextWindow: null, compactions: 0, toolCalls: 1, failedToolCalls: 0, openForgeReads: 0, openForgeBytes: 0, reportedCostUsd: null, model: null, sessionId: null } };
  await history.store.saveBatch(batch);
  await mkdir(history.store.runFolder(batch.id, run.id), { recursive: true });
  await writeFile(join(history.store.runFolder(batch.id, run.id), ResultFiles.events), Events);
  await history.store.saveRun(run);
  await history.store.saveSetups(batch.id, [{ schemaVersion: RecordSchemaVersion, id: run.setup, experimentId: batch.id, label: run.setup, commands: [], contentHash: null, files: [{ path: "docs/rules.md", bytes: 5, sha256: "" }] }]);
  return { ...history, batch, run };
}

test("analyze appends one record per finished run, and missing skips runs whose current analysis succeeded", async (t) => {
  const saved = await savedBatch(); t.after(saved.cleanup);
  await analyzeBatch(saved.store, saved.batch, false);
  const [first] = await saved.store.readAnalyses(saved.batch.id);
  assert.equal(first?.runId, saved.run.id);
  assert.equal(first?.method, AnalysisMethod.events);
  assert.equal(first?.parts.reduce((sum, part) => sum + part.input, 0), 300);
  assert.equal(needsAnalysis(first), false);
  await analyzeBatch(saved.store, saved.batch, true);
  assert.equal((await saved.store.readAnalyses(saved.batch.id)).length, 1);
  await analyzeBatch(saved.store, saved.batch, false);
  const records = await saved.store.readAnalyses(saved.batch.id);
  assert.deepEqual(records.map((record) => record.replacesAnalysisId), [null, first?.id]);
  assert.equal(needsAnalysis(first === undefined ? undefined : { ...first, version: `${AnalysisVersion}-earlier` }), true);
});

test("analyses travel with a batch's records and stay out of its assessments", async (t) => {
  const saved = await savedBatch(); t.after(saved.cleanup);
  await analyzeBatch(saved.store, saved.batch, false);
  const name = `${HistoryFiles.assessments}/${AnalysisFile}`;
  const files = await batchFiles(saved.store, saved.batch, await saved.store.readRuns(saved.batch));
  assert.equal(Array.isArray(files[name]) ? files[name].length : null, 1);
  const read = localRunsReader(saved.store);
  assert.ok(await read(`versions/${FixtureCliVersion}/batches/${saved.batch.id}/${name}`));
});
