import { AnalysisFile, AssessmentKinds, RecordFiles, type Catalogue } from "../records/catalogue.ts";
import { createValidator, jsonLines, type ValidatorOptions } from "../records/validate-data.ts";
import type { EvalsData, Snapshot } from "./snapshot.types.ts";
export const ResultsRepository = "TheLithiumForge/open-forge-eval-results";
export const RawRoot = `https://raw.githubusercontent.com/${ResultsRepository}`;
/** The status a results host gives for a file it doesn't have. */
export const NotFound = 404;
/** Reads one file of a results tree, or gives null when the tree has no such file. */
export type ReadText = (path: string) => Promise<string | null>;

async function required(read: ReadText, path: string): Promise<string> {
  const text = await read(path);
  if (text === null) throw new Error(`Missing batch file ${path}`);
  return text;
}
export async function loadRecords(read: ReadText, schemas: readonly object[], indexSchema: object, options: ValidatorOptions = {}): Promise<Pick<Snapshot, "batches" | "index">> {
  const validator = createValidator(schemas, indexSchema, options);
  const index: Catalogue = validator.index(JSON.parse(await required(read, "index.json")));
  const batches = await Promise.all(index.batches.map(async (entry) => {
    const file = (path: string) => required(read, `${entry.path}/${path}`);
    const [experiment, setups, runs, artifacts, analyses] = await Promise.all([
      file(RecordFiles.experiment), file(RecordFiles.setups), file(RecordFiles.runs), file(RecordFiles.artifacts), read(`${entry.path}/${AnalysisFile}`),
    ]);
    const assessments = await Promise.all(AssessmentKinds.map(async (kind) => validator.assessmentLines(await file(`assessments/${kind}.jsonl`), kind)));
    // Batches published before analyses existed have no analysis file.
    return validator.batch({
      experiment: JSON.parse(experiment), setups: jsonLines(setups), runs: jsonLines(runs),
      artifacts: JSON.parse(artifacts), assessments: assessments.flat(), analyses: analyses === null ? [] : jsonLines(analyses)
    });
  }));
  // Running batches change between reading the catalogue and their records, so only finished data must match exactly.
  if (options.unfinished !== true) validator.matches(index, batches);
  return { batches, index };
}
export async function fetchLatest(data: EvalsData, request: typeof fetch = fetch): Promise<Snapshot> {
  async function get(url: string): Promise<string | null> {
    const response = await request(url, { cache: "no-store", credentials: "omit" });
    if (response.status === NotFound)
      return null;
    if (!response.ok)
      throw new Error(`Results fetch failed (${response.status})`);
    return response.text();
  }
  // Resolve once, then read every raw file from the same immutable data commit.
  const commit: unknown = JSON.parse(await get(`https://api.github.com/repos/${ResultsRepository}/commits/main`) ?? "null");
  if (typeof commit !== "object" || commit === null || !("sha" in commit) || typeof commit.sha !== "string" || !/^[a-f0-9]{40}$/.test(commit.sha))
    throw new Error("Invalid data commit");
  const root = `${RawRoot}/${commit.sha}`;
  const records = await loadRecords((path) => get(`${root}/${path}`), data.schemas, data.indexSchema);
  return { ...records, commit: commit.sha, fetchedAt: new Date().toISOString() };
}
