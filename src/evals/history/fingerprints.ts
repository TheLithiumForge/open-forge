import { createHash } from "node:crypto";
import type { Fingerprints } from "./records.types.ts";
import { RecordSchemaVersion } from "./statuses.ts";

export function sha256(value: string | Uint8Array): string {
  return createHash("sha256").update(value).digest("hex");
}

export function stableJson(value: unknown): string {
  if (Array.isArray(value)) return `[${value.map(stableJson).join(",")}]`;
  if (value !== null && typeof value === "object") {
    return `{${Object.entries(value).sort(([a], [b]) => a.localeCompare(b)).map(([key, item]) => `${JSON.stringify(key)}:${stableJson(item)}`).join(",")}}`;
  }
  const encoded = JSON.stringify(value);
  if (encoded === undefined) throw new Error("Fingerprint inputs must be JSON values.");
  return encoded;
}

export function fingerprint(value: unknown): string {
  return sha256(stableJson(value));
}

export interface FingerprintInputs {
  readonly cliPackage: unknown;
  readonly benchmarkInputs: unknown;
  readonly setupContent: unknown;
  readonly checks: unknown;
  readonly rubric: unknown;
  readonly harnessSource: unknown;
  readonly tooling: unknown;
  readonly requestedModels: unknown;
  readonly reportedModels: unknown;
}

export function createFingerprints(inputs: FingerprintInputs): Fingerprints {
  return {
    cliPackage: fingerprint(inputs.cliPackage), benchmarkInputs: fingerprint(inputs.benchmarkInputs),
    setupContent: fingerprint(inputs.setupContent), checks: fingerprint(inputs.checks), rubric: fingerprint(inputs.rubric),
    harnessSource: fingerprint(inputs.harnessSource), tooling: fingerprint(inputs.tooling),
    requestedModels: fingerprint(inputs.requestedModels), reportedModels: fingerprint(inputs.reportedModels),
    schema: fingerprint(RecordSchemaVersion),
  };
}

export function unknownFingerprints(): Fingerprints {
  return {
    cliPackage: null, benchmarkInputs: null, setupContent: null, checks: null, rubric: null,
    harnessSource: null, tooling: null, requestedModels: null, reportedModels: null, schema: null,
  };
}
