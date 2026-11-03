import { execFileSync } from "node:child_process";
import { readFile } from "node:fs/promises";
import { join } from "node:path";
import { resultsRoot, recordCopyRoot, hasResultsRepository } from "./prepare.ts";
import { loadRecords } from "../../src/components/evals/data/load.ts";
import { catalogue, SchemaNames } from "../../src/components/evals/records/catalogue.ts";
import type { EvalsData } from "../../src/components/evals/data/snapshot.types.ts";
function dataCommit(root: string): string {
  try {
    const options = { encoding: "utf8", stdio: "pipe" } as const;
    const commit = execFileSync("git", ["-C", root, "rev-parse", "HEAD"], options).trim();
    const dirty = execFileSync("git", ["-C", root, "status", "--porcelain"], options).trim() !== "";
    return `${commit}${dirty ? " (unpublished changes)" : ""}`;
  }
  catch {
    return "unknown";
  }
}
export async function loadEvalsData(repository = resultsRoot, records?: string): Promise<EvalsData> {
  const json = async (path: string): Promise<object> => JSON.parse(await readFile(path, "utf8"));
  const schemas = await Promise.all(SchemaNames.map((name) => json(join(recordCopyRoot, "schemas/v2", `${name}.json`))));
  const indexSchema = await json(join(recordCopyRoot, "schemas/index.json"));
  const present = await hasResultsRepository(repository);
  const content = records !== undefined || present
    ? await loadRecords((path) => readFile(join(records ?? repository, path), "utf8").catch((error: unknown) => {
      if (error instanceof Error && "code" in error && error.code === "ENOENT") return null;
      throw error;
    }), schemas, indexSchema)
    : { batches: [], index: catalogue([]) };
  const commit = present ? dataCommit(repository) : "unknown";
  return { snapshot: { ...content, commit: `${commit}${records === undefined ? "" : " (fixture records)"}`, fetchedAt: new Date().toISOString() }, schemas, indexSchema };
}
