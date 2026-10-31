import { existsSync } from "node:fs";
import { mkdir, readdir, readFile, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { EvalPaths, ResultFiles } from "../paths.ts";
import type { BatchRecord, Preference, RunRecord } from "./results.types.ts";

export function batchFolder(batchId: string): string {
  return join(EvalPaths.results, batchId);
}

export function runFolder(batchId: string, runId: string): string {
  return join(batchFolder(batchId), "runs", runId);
}

async function readJson<T>(path: string): Promise<T> {
  return JSON.parse(await readFile(path, "utf8")) as T; // Files this store wrote itself.
}

async function writeJson(path: string, value: unknown): Promise<void> {
  await writeFile(path, `${JSON.stringify(value, null, 2)}\n`);
}

export async function saveBatch(batch: BatchRecord): Promise<void> {
  await mkdir(batchFolder(batch.id), { recursive: true });
  await writeJson(join(batchFolder(batch.id), ResultFiles.batch), batch);
}

export function readBatch(batchId: string): Promise<BatchRecord> {
  return readJson(join(batchFolder(batchId), ResultFiles.batch));
}

export async function saveRun(run: RunRecord): Promise<void> {
  const folder = runFolder(run.batchId, run.id);
  await mkdir(folder, { recursive: true });
  await writeJson(join(folder, ResultFiles.run), run);
}

export function readRun(batchId: string, runId: string): Promise<RunRecord> {
  return readJson(join(runFolder(batchId, runId), ResultFiles.run));
}

export async function readRuns(batch: BatchRecord): Promise<RunRecord[]> {
  return Promise.all(batch.runs.map((runId) => readRun(batch.id, runId)));
}

export async function listBatches(): Promise<BatchRecord[]> {
  if (!existsSync(EvalPaths.results)) return [];
  const ids = await readdir(EvalPaths.results);
  const batches = ids.filter((id) => existsSync(join(batchFolder(id), ResultFiles.batch))).map((id) => readBatch(id));
  return (await Promise.all(batches)).sort((a, b) => b.createdAt.localeCompare(a.createdAt));
}

export async function readPreferences(batchId: string): Promise<Preference[]> {
  const path = join(batchFolder(batchId), ResultFiles.preferences);
  return existsSync(path) ? readJson(path) : [];
}

export async function addPreference(batchId: string, preference: Preference): Promise<void> {
  const preferences = await readPreferences(batchId);
  await writeJson(join(batchFolder(batchId), ResultFiles.preferences), [...preferences, preference]);
}
