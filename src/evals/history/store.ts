import { existsSync } from "node:fs";
import { appendFile, mkdir, readdir, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { EvalPaths, ResultFiles } from "../paths.ts";
import { artifactHash, artifactManifest, HistoryFiles } from "./artifacts.ts";
import { assessmentBase, assessRun, storedRun, type AssessmentSelection } from "./assessments.ts";
import { readJson, writeJson, readJsonLines, jsonLines, safeId } from "./json-files.ts";
import { legacyAssessments, legacyExperiment, legacyRun, legacySetups } from "./legacy.ts";
import type { ArtifactRecord, AssessmentRecord, ExperimentRecord, RunRecord, RunView, SetupRecord } from "./records.types.ts";
import type { LegacyBatchRecord, LegacyRunRecord, Preference } from "./results.types.ts";
import type { SetupRecord as LegacySetupRecord } from "../execution/snapshot.ts";
import { AssessmentKind, PreferenceWinner, RecordSchemaVersion } from "./statuses.ts";

export class HistoryStore {
  readonly root: string;
  private writes: Promise<void> = Promise.resolve();

  constructor(root: string) { this.root = root; }
  batchFolder(id: string): string { return join(this.root, safeId(id)); }
  runFolder(batchId: string, runId: string): string { return join(this.batchFolder(batchId), "runs", safeId(runId)); }
  assessmentEvidenceFolder(batchId: string, assessmentId: string): string {
    return join(this.root, HistoryFiles.overlay, safeId(batchId), HistoryFiles.evidence, safeId(assessmentId));
  }
  private isLegacy(id: string): boolean { return !existsSync(join(this.batchFolder(id), HistoryFiles.experiment)); }
  private assessmentFolder(id: string): string {
    return this.isLegacy(id) ? join(this.root, HistoryFiles.overlay, safeId(id)) : join(this.batchFolder(id), HistoryFiles.assessments);
  }

  async saveBatch(batch: ExperimentRecord): Promise<void> {
    if (batch.legacy) {
      await writeJson(join(this.assessmentFolder(batch.id), HistoryFiles.reveal), batch.revealed);
      return;
    }
    await writeJson(join(this.batchFolder(batch.id), HistoryFiles.experiment), batch);
  }

  async readBatch(id: string): Promise<ExperimentRecord> {
    if (!this.isLegacy(id)) return readJson(join(this.batchFolder(id), HistoryFiles.experiment));
    const batch = legacyExperiment(await readJson<LegacyBatchRecord>(join(this.batchFolder(id), ResultFiles.batch)));
    const reveal = join(this.assessmentFolder(id), HistoryFiles.reveal);
    return existsSync(reveal) ? { ...batch, revealed: await readJson<boolean>(reveal) } : batch;
  }

  async saveRun(view: RunView): Promise<void> {
    if (this.isLegacy(view.batchId)) throw new Error("Legacy run files are read only.");
    const folder = this.runFolder(view.batchId, view.id);
    await mkdir(folder, { recursive: true });
    const artifacts = await artifactManifest(folder, view.id);
    await writeJson(join(folder, HistoryFiles.artifacts), artifacts);
    await writeJson(join(folder, ResultFiles.run), { ...storedRun(view), artifactHash: artifactHash(artifacts) });
  }

  async readRun(batchId: string, id: string, selected: AssessmentSelection = {}): Promise<RunView> {
    const path = join(this.runFolder(batchId, id), ResultFiles.run);
    const assessments = await this.readAssessments(batchId);
    if (this.isLegacy(batchId)) {
      const old = await readJson<LegacyRunRecord>(path);
      return assessRun(legacyRun(old), [...legacyAssessments(old), ...assessments], selected);
    }
    return assessRun(await readJson<RunRecord>(path), assessments, selected);
  }

  async readRuns(batch: ExperimentRecord): Promise<RunView[]> {
    return Promise.all(batch.runs.map((id) => this.readRun(batch.id, id)));
  }

  async listBatches(): Promise<ExperimentRecord[]> {
    if (!existsSync(this.root)) return [];
    const entries = await readdir(this.root, { withFileTypes: true });
    const batches = await Promise.all(entries.filter((entry) => entry.isDirectory() && entry.name !== HistoryFiles.overlay)
      .filter((entry) => [HistoryFiles.experiment, ResultFiles.batch].some((file) => existsSync(join(this.batchFolder(entry.name), file))))
      .map((entry) => this.readBatch(entry.name)));
    return batches.sort((a, b) => b.createdAt.localeCompare(a.createdAt));
  }

  async readAssessments(batchId: string): Promise<AssessmentRecord[]> {
    const groups = await Promise.all(Object.values(AssessmentKind).map((kind) => readJsonLines<AssessmentRecord>(join(this.assessmentFolder(batchId), `${kind}.jsonl`))));
    return groups.flat();
  }

  appendAssessment(assessment: AssessmentRecord): Promise<void> {
    const write = this.writes.then(async () => {
      const batch = await this.readBatch(assessment.experimentId);
      const ids = assessment.kind === AssessmentKind.pick ? [assessment.left.runId, assessment.right.runId] : [assessment.runId];
      if (assessment.kind === AssessmentKind.pick && assessment.left.runId === assessment.right.runId) throw new Error("Choose two different runs.");
      if (ids.some((id) => !batch.runs.includes(id))) throw new Error("An assessment must name runs in its experiment.");
      if ((await this.readAssessments(batch.id)).some((saved) => saved.id === assessment.id)) throw new Error("Assessment IDs are append only.");
      const evidence = this.assessmentEvidenceFolder(batch.id, assessment.id);
      if (assessment.kind !== AssessmentKind.pick && existsSync(evidence)) {
        await writeJson(join(evidence, HistoryFiles.artifacts), await artifactManifest(evidence, assessment.runId, assessment.id));
      }
      const folder = this.assessmentFolder(batch.id);
      await mkdir(folder, { recursive: true });
      await appendFile(join(folder, `${assessment.kind}.jsonl`), jsonLines([assessment]));
    });
    this.writes = write.catch(() => {});
    return write;
  }

  async saveSetups(batchId: string, setups: readonly SetupRecord[]): Promise<void> {
    if (this.isLegacy(batchId)) throw new Error("Legacy setup files are read only.");
    await writeFile(join(this.batchFolder(batchId), HistoryFiles.setups), jsonLines(setups));
  }

  async readSetups(batch: ExperimentRecord): Promise<SetupRecord[]> {
    if (!batch.legacy) return readJsonLines(join(this.batchFolder(batch.id), HistoryFiles.setups));
    const path = join(this.batchFolder(batch.id), ResultFiles.setups);
    if (!existsSync(path)) return legacySetups(batch);
    return (await readJson<LegacySetupRecord[]>(path)).map((record) => ({
      schemaVersion: RecordSchemaVersion, id: record.setup, experimentId: batch.id, label: record.label,
      commands: record.commands, files: record.files, contentHash: artifactHash(record.files),
    }));
  }

  async readArtifacts(batch: ExperimentRecord): Promise<ArtifactRecord[]> {
    const groups = await Promise.all(batch.runs.map(async (id) => {
      const folder = this.runFolder(batch.id, id);
      return batch.legacy ? artifactManifest(folder, id) : readJson<ArtifactRecord[]>(join(folder, HistoryFiles.artifacts));
    }));
    const evidence: ArtifactRecord[] = [];
    for (const assessment of await this.readAssessments(batch.id)) {
      const path = join(this.assessmentEvidenceFolder(batch.id, assessment.id), HistoryFiles.artifacts);
      if (existsSync(path)) evidence.push(...await readJson<ArtifactRecord[]>(path));
    }
    return [...groups.flat(), ...evidence];
  }

  async readPreferences(batchId: string): Promise<Preference[]> {
    const batch = await this.readBatch(batchId);
    const path = join(this.batchFolder(batchId), ResultFiles.preferences);
    const legacy = batch.legacy && existsSync(path) ? await readJson<Preference[]>(path) : [];
    const picks = (await this.readAssessments(batchId)).filter((assessment) => assessment.kind === AssessmentKind.pick);
    return [...legacy, ...picks.map((pick) => ({ left: pick.left.runId, right: pick.right.runId, winner: pick.winner, at: pick.createdAt ?? "" }))];
  }

  async addPreference(batchId: string, preference: Preference): Promise<void> {
    const batch = await this.readBatch(batchId);
    const [left, right] = await Promise.all([this.readRun(batchId, preference.left), this.readRun(batchId, preference.right)]);
    const hash = async (run: RunView) => run.artifactHash ?? artifactHash(await artifactManifest(this.runFolder(batchId, run.id), run.id));
    await this.appendAssessment({
      ...assessmentBase(batchId, { checker: null, rubric: null, judge: null }), kind: AssessmentKind.pick,
      createdAt: preference.at, left: { runId: left.id, artifactHash: await hash(left), side: PreferenceWinner.left },
      right: { runId: right.id, artifactHash: await hash(right), side: PreferenceWinner.right }, winner: preference.winner, revealed: batch.revealed,
    });
  }
}

const store = new HistoryStore(EvalPaths.runs);
export const batchFolder = store.batchFolder.bind(store);
export const runFolder = store.runFolder.bind(store);
export const assessmentEvidenceFolder = store.assessmentEvidenceFolder.bind(store);
export const saveBatch = store.saveBatch.bind(store);
export const readBatch = store.readBatch.bind(store);
export const saveRun = store.saveRun.bind(store);
export const readRun = store.readRun.bind(store);
export const readRuns = store.readRuns.bind(store);
export const listBatches = store.listBatches.bind(store);
export const appendAssessment = store.appendAssessment.bind(store);
export const readAssessments = store.readAssessments.bind(store);
export const saveSetups = store.saveSetups.bind(store);
export const readSetups = store.readSetups.bind(store);
export const readPreferences = store.readPreferences.bind(store);
export const addPreference = store.addPreference.bind(store);
