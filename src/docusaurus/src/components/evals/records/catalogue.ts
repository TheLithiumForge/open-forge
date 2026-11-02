import type { Batch, Choice, GradingVersions } from "./records.types.ts";
export const IndexVersion = 1;
export const RecordFiles = { experiment: "experiment.json", setups: "setups.jsonl", runs: "runs.jsonl", artifacts: "artifacts.json" } as const;
export const AssessmentKinds = ["checks", "judge", "pick"] as const;
// Each batch keeps its evidence beside its records, at the paths artifacts.json lists.
export const EvidenceFolder = "evidence";
export const SchemaNames = ["common", "experiment", "setup", "run", "assessment", "artifact"] as const;
export interface CatalogueEntry {
  readonly id: string;
  readonly layer: number | null;
  readonly cliVersion: string;
  readonly createdAt: string;
  readonly implementers: readonly Choice[];
  readonly judges: readonly Choice[];
  readonly gradingVersions: readonly GradingVersions[];
  readonly path: string;
}
export interface Catalogue {
  readonly schemaVersion: number;
  readonly batches: readonly CatalogueEntry[];
}
function unique<T>(values: readonly T[]): T[] {
  return [...new Map(values.map((value) => [JSON.stringify(value), value])).values()];
}
export function catalogue(batches: readonly Batch[]): Catalogue {
  return {
    schemaVersion: IndexVersion, batches: batches.map(({ experiment, runs, assessments }) => {
      if (experiment.cliVersion === null)
        throw new Error("Published batches need a CLI version");
      return {
        id: experiment.id, layer: experiment.config.layer ?? null, cliVersion: experiment.cliVersion,
        createdAt: experiment.createdAt, implementers: unique(runs.map((run) => run.agent)),
        // A judge choice also carries its ballot count, which the catalogue leaves to the records.
        judges: unique(assessments.flatMap((item) => item.kind === "judge" && item.requestedModel !== null ? [{ tool: item.requestedModel.tool, model: item.requestedModel.model, effort: item.requestedModel.effort }] : [])),
        gradingVersions: unique(assessments.map((item) => item.versions)),
        path: `versions/${experiment.cliVersion}/batches/${experiment.id}`
      };
    }).sort((a, b) => a.path.localeCompare(b.path))
  };
}
