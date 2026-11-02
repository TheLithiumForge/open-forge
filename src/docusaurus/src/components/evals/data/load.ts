import { AssessmentKinds, RecordFiles, type Catalogue } from "../records/catalogue.ts";
import { createValidator, jsonLines, type ValidatorOptions } from "../records/validate-data.ts";
import type { EvalsData, Snapshot } from "./snapshot.types.ts";
export const ResultsRepository = "TheLithiumForge/open-forge-eval-results";
export const RawRoot = `https://raw.githubusercontent.com/${ResultsRepository}`;
export type ReadText = (path: string) => Promise<string>;
export async function loadRecords(read: ReadText, schemas: readonly object[], indexSchema: object, options: ValidatorOptions = {}): Promise<Pick<Snapshot, "batches" | "index">> {
  const validator = createValidator(schemas, indexSchema, options);
  const index: Catalogue = validator.index(JSON.parse(await read("index.json")));
  const batches = await Promise.all(index.batches.map(async (entry) => {
    const file = (path: string) => read(`${entry.path}/${path}`);
    const [experiment, setups, runs, artifacts, ...assessments] = await Promise.all([
      file(RecordFiles.experiment), file(RecordFiles.setups), file(RecordFiles.runs), file(RecordFiles.artifacts),
      ...AssessmentKinds.map(async (kind) => validator.assessmentLines(await file(`assessments/${kind}.jsonl`), kind)),
    ]);
    if (typeof experiment !== "string" || typeof setups !== "string" || typeof runs !== "string" || typeof artifacts !== "string")
      throw new Error("Missing batch files");
    return validator.batch({
      experiment: JSON.parse(experiment), setups: jsonLines(setups), runs: jsonLines(runs),
      artifacts: JSON.parse(artifacts), assessments: assessments.flat()
    });
  }));
  // Running batches change between reading the catalogue and their records, so only finished data must match exactly.
  if (options.unfinished !== true) validator.matches(index, batches);
  return { batches, index };
}
export async function fetchLatest(data: EvalsData, request: typeof fetch = fetch): Promise<Snapshot> {
  async function get(url: string): Promise<string> {
    const response = await request(url, { cache: "no-store", credentials: "omit" });
    if (!response.ok)
      throw new Error(`Results fetch failed (${response.status})`);
    return response.text();
  }
  // Resolve once, then read every raw file from the same immutable data commit.
  const commit: unknown = JSON.parse(await get(`https://api.github.com/repos/${ResultsRepository}/commits/main`));
  if (typeof commit !== "object" || commit === null || !("sha" in commit) || typeof commit.sha !== "string" || !/^[a-f0-9]{40}$/.test(commit.sha))
    throw new Error("Invalid data commit");
  const root = `${RawRoot}/${commit.sha}`;
  const records = await loadRecords((path) => get(`${root}/${path}`), data.schemas, data.indexSchema);
  return { ...records, commit: commit.sha, fetchedAt: new Date().toISOString() };
}
