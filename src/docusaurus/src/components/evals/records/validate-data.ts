import { Ajv2020 } from "ajv/dist/2020.js";
import type { ValidateFunction } from "ajv";
import type { Analysis, Artifact, Assessment, Batch, Experiment, Run, Setup } from "./records.types.ts";
import { AssessmentKinds, catalogue, type Catalogue } from "./catalogue.ts";
import { validateReferences } from "./references.ts";
export interface BatchFiles {
  readonly experiment: unknown;
  readonly setups: readonly unknown[];
  readonly runs: readonly unknown[];
  readonly assessments: readonly unknown[];
  readonly artifacts: unknown;
  /** Missing for batches published before analyses existed. */
  readonly analyses?: readonly unknown[];
}
function ordered(value: unknown): unknown {
  if (Array.isArray(value))
    return value.map((item: unknown) => ordered(item));
  if (typeof value === "object" && value !== null)
    return Object.fromEntries(Object.entries(value)
      .sort(([left], [right]) => left.localeCompare(right)).map(([key, item]) => [key, ordered(item)]));
  return value;
}
export interface ValidatorOptions {
  /** Accepts batches that are still running, which only a local view shows. */
  readonly unfinished?: boolean;
}
export function createValidator(schemas: readonly object[], indexSchema: object, options: ValidatorOptions = {}) {
  const ajv = new Ajv2020({ allErrors: true, strict: true });
  for (const schema of schemas)
    ajv.addSchema(schema);
  const indexCheck = ajv.compile<Catalogue>(indexSchema);
  function checked<T>(value: unknown, check: ValidateFunction<T> | undefined): T {
    if (check === undefined || !check(value))
      throw new Error(`Invalid results data: ${JSON.stringify(check?.errors)}`);
    return value;
  }
  function record<T>(value: unknown, kind: string): T {
    return checked(value, ajv.getSchema<T>(`${kind}.json`));
  }
  return {
    index(value: unknown): Catalogue {
      const result = checked(value, indexCheck);
      if (new Set(result.batches.map((entry) => entry.path)).size !== result.batches.length)
        throw new Error("Duplicate batch path");
      for (const entry of result.batches) {
        if (entry.path !== `versions/${entry.cliVersion}/batches/${entry.id}`)
          throw new Error("Index path differs from batch identity");
      }
      return result;
    },
    batch(files: BatchFiles): Batch {
      if (!Array.isArray(files.artifacts))
        throw new Error("artifacts.json must be an array");
      const batch: Batch = {
        experiment: record<Experiment>(files.experiment, "experiment"),
        setups: files.setups.map((item) => record<Setup>(item, "setup")),
        runs: files.runs.map((item) => record<Run>(item, "run")),
        assessments: files.assessments.map((item) => record<Assessment>(item, "assessment")),
        artifacts: files.artifacts.map((item: unknown) => record<Artifact>(item, "artifact")),
        analyses: (files.analyses ?? []).map((item) => record<Analysis>(item, "analysis")),
      };
      validateReferences(batch, options.unfinished !== true);
      return batch;
    },
    assessmentLines(input: string, kind: typeof AssessmentKinds[number]): Assessment[] {
      const records = jsonLines(input).map((item) => record<Assessment>(item, "assessment"));
      if (records.some((item) => item.kind !== kind))
        throw new Error("Assessment kind differs from its file");
      return records;
    },
    matches(index: Catalogue, batches: readonly Batch[]): void {
      if (JSON.stringify(ordered(index)) !== JSON.stringify(ordered(catalogue(batches))))
        throw new Error("index.json differs from published records. Run npm run index");
    },
  };
}
export function jsonLines(input: string): unknown[] {
  return input.split("\n").filter((line) => line.trim() !== "").map((line): unknown => JSON.parse(line));
}
