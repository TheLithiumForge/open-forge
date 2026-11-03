import { readFile } from "node:fs/promises";
import { join } from "node:path";
import type { BrownfieldCase } from "../../materialize/brownfield-case.ts";
import { ChangeType } from "../../materialize/brownfield-case.ts";
import { isRecord } from "../../materialize/frontmatter.ts";
import { CheckStatus } from "../../history/statuses.ts";
import { loadDefinition } from "../../benchmarks/neon-pairs/checks/spec-layers/load.ts";
import { LayerId } from "../../benchmarks/neon-pairs/checks/spec-layers/requirements.ts";
import { ChangeCheckFiles, clauseTargets, readChangeChecks } from "./change-checks.ts";
import { caseRegression } from "./regression.ts";
import { BrownfieldGuard } from "./scope.ts";
import type { CheckOutcome } from "../grading.types.ts";

export const ValidationPhase = { starting: "starting", solution: "solution", alternative: "alternative" } as const;
export type ValidationPhase = (typeof ValidationPhase)[keyof typeof ValidationPhase];
export const ValidationStatus = { missing: "missing", unexpected: "unexpected" } as const;
export const ValidationCheck = { build: "validation-build", typecheck: "validation-typecheck", unit: "validation-unit-tests", browser: "validation-browser-tests", inputs: "validation-inputs", stage: "validation-stage" } as const;
export interface ValidationExpectation { readonly id: string; readonly starting: CheckStatus; readonly solution: CheckStatus }
export interface ValidationComparison { readonly id: string; readonly expected: CheckStatus; readonly actual: CheckStatus | (typeof ValidationStatus)[keyof typeof ValidationStatus]; readonly matches: boolean; readonly evidence: string | null }

function startingStatus(value: unknown): CheckStatus {
  if (value !== CheckStatus.pass && value !== CheckStatus.fail && value !== CheckStatus.blocked) throw new Error("Starting expectations must be pass, fail or blocked.");
  return value;
}

export async function readValidationExpectations(item: BrownfieldCase): Promise<ValidationExpectation[]> {
  const declared = await readChangeChecks(item);
  const manifest: unknown = JSON.parse(await readFile(join(item.root, item.checks, ChangeCheckFiles.manifest), "utf8"));
  if (!isRecord(manifest) || !Array.isArray(manifest["checks"])) throw new Error("Missing change check expectations.");
  const entries = manifest["checks"];
  const changes = declared.map((check): ValidationExpectation => {
    const entry: unknown = entries.find((entry: unknown) => isRecord(entry) && entry["id"] === check.id);
    const expected = isRecord(entry) ? entry["expected"] : undefined;
    if (!isRecord(expected) || expected[ValidationPhase.solution] !== CheckStatus.pass) throw new Error(`Check ${check.id} needs starting and passing solution expectations.`);
    return { id: `${check.kind}-${check.id}`, starting: startingStatus(expected[ValidationPhase.starting]), solution: CheckStatus.pass };
  });
  for (const kind of new Set(declared.map((check) => check.kind))) {
    if (!changes.some((check) => check.id.startsWith(`${kind}-`) && check.starting === CheckStatus.fail)) throw new Error(`The starting project must fail at least one ${kind} check.`);
  }
  const frozen = caseRegression(await loadDefinition(LayerId.three), item.supersedes, clauseTargets(declared));
  const overrides = manifest["regressionExpectations"] ?? {};
  if (!isRecord(overrides)) throw new Error("Invalid starting regression expectations.");
  const ids = frozen.checks.map((check) => `regression-${check.id}`);
  if (Object.keys(overrides).some((id) => !ids.includes(id))) throw new Error("A regression expectation names an unknown or superseded check.");
  const regression = ids.map((id): ValidationExpectation => {
    const starting = startingStatus(overrides[id] ?? CheckStatus.pass);
    if (item.changeType === ChangeType.feature && starting !== CheckStatus.pass) throw new Error("A feature case must start with passing frozen regressions.");
    return { id, starting, solution: CheckStatus.pass };
  });
  return [...changes, ...regression, ...Object.values(BrownfieldGuard).map((id) => ({ id, starting: CheckStatus.pass, solution: CheckStatus.pass }))];
}

export function compareValidation(expected: readonly ValidationExpectation[], checks: readonly CheckOutcome[], phase: ValidationPhase): ValidationComparison[] {
  const results: ValidationComparison[] = expected.map((entry) => {
    const found = checks.filter((check) => check.id === entry.id);
    const actual = found.length === 1 ? found[0]?.status ?? ValidationStatus.missing : found.length ? ValidationStatus.unexpected : ValidationStatus.missing;
    const status = phase === ValidationPhase.starting ? entry.starting : entry.solution;
    return { id: entry.id, expected: status, actual, matches: actual === status, evidence: found[0]?.evidence ?? null };
  });
  for (const check of checks) if (!expected.some((entry) => entry.id === check.id)) results.push({ id: check.id, expected: CheckStatus.pass, actual: ValidationStatus.unexpected, matches: false, evidence: check.evidence });
  return results;
}
