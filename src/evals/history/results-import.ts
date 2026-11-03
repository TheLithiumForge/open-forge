import { existsSync } from "node:fs";
import { cp, mkdir, readFile, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { HistoryFiles } from "./artifacts.ts";
import { readJson, readJsonLines, writeJson } from "./json-files.ts";
import type { ArtifactRecord, ExperimentRecord, RunRecord } from "./records.types.ts";
import type { HistoryStore } from "./store.ts";
import { ResultFiles } from "../paths.ts";

/**
 * Rebuilds a published batch as a batch in a run store: its records, each run's evidence and each assessment's evidence.
 * The checks and judges then grade the published code anywhere, such as a CI runner, and results add publishes the batch again.
 */
export async function importResults(published: string, store: HistoryStore): Promise<string> {
  const experiment = await readJson<ExperimentRecord>(join(published, HistoryFiles.experiment));
  const target = store.batchFolder(experiment.id);
  if (existsSync(target)) throw new Error(`${experiment.id} is already in the run store.`);
  const artifacts = await readJson<ArtifactRecord[]>(join(published, HistoryFiles.artifacts));
  const evidence = join(published, HistoryFiles.evidence);
  await mkdir(target, { recursive: true });
  await writeFile(join(target, HistoryFiles.experiment), await readFile(join(published, HistoryFiles.experiment)));
  await writeFile(join(target, HistoryFiles.setups), await readFile(join(published, HistoryFiles.setups)));
  for (const run of await readJsonLines<RunRecord>(join(published, HistoryFiles.runs))) {
    const folder = store.runFolder(experiment.id, run.id), saved = join(evidence, "runs", run.id);
    await mkdir(folder, { recursive: true });
    if (existsSync(saved)) await cp(saved, folder, { recursive: true });
    await writeJson(join(folder, HistoryFiles.artifacts), artifacts.filter((item) => item.runId === run.id && item.assessmentId === null));
    await writeJson(join(folder, ResultFiles.run), run);
  }
  const assessments = join(published, HistoryFiles.assessments);
  if (existsSync(assessments)) await cp(assessments, join(target, HistoryFiles.assessments), { recursive: true });
  for (const id of new Set(artifacts.flatMap((item) => item.assessmentId === null ? [] : [item.assessmentId]))) {
    const saved = join(evidence, HistoryFiles.assessments, id);
    if (!existsSync(saved)) continue;
    const folder = store.assessmentEvidenceFolder(experiment.id, id);
    await mkdir(folder, { recursive: true });
    await cp(saved, folder, { recursive: true });
    await writeJson(join(folder, HistoryFiles.artifacts), artifacts.filter((item) => item.assessmentId === id));
  }
  return experiment.id;
}
