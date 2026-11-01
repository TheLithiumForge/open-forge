import type { CheckResult, TokenCounts } from "../history/results.types.ts";
import { CheckStatus } from "../history/statuses.ts";
import type { CheckOutcome, CheckSelection, Coverage, FunctionalResult, MeasurementSummary } from "./grading.types.ts";

export function adaptCheck(check: CheckResult | CheckOutcome): CheckOutcome {
  if ("status" in check) return check;
  return {
    id: check.id, area: check.area, evidence: check.detail,
    status: check.passed === null ? CheckStatus.blocked : check.passed ? CheckStatus.pass : CheckStatus.fail,
  };
}

/** The old display shape stays at the saved-run compatibility edge. */
export function legacyCheckView(check: CheckOutcome): CheckResult {
  return { id: check.id, area: check.area, detail: check.evidence,
    passed: check.status === CheckStatus.pass ? true : check.status === CheckStatus.fail ? false : null };
}

/** Keep the first outcome when a checker retries a check within one assessment. */
export function uniqueChecks(checks: readonly CheckOutcome[]): CheckOutcome[] {
  const first = new Map<string, CheckOutcome>();
  for (const check of checks) if (!first.has(check.id)) first.set(check.id, check);
  return [...first.values()];
}

export function coverage(checks: readonly CheckOutcome[], applicableIds: readonly string[] = checks.map((check) => check.id)): Coverage {
  const byId = new Map(uniqueChecks(checks).map((check) => [check.id, check]));
  const statuses = [...new Set(applicableIds)].map((id) => byId.get(id)?.status ?? CheckStatus.blocked);
  const passed = statuses.filter((status) => status === CheckStatus.pass).length;
  return {
    passed, applicable: statuses.length,
    blocked: statuses.filter((status) => status === CheckStatus.blocked).length,
    errors: statuses.filter((status) => status === CheckStatus.error).length,
    share: statuses.length === 0 ? null : passed / statuses.length,
  };
}

export function passesGate(checks: readonly CheckOutcome[], requiredIds: readonly string[]): boolean | null {
  if (requiredIds.length === 0) return null;
  const result = coverage(checks, requiredIds);
  return result.passed === result.applicable;
}

export function countChecks(checks: readonly CheckOutcome[], selection: CheckSelection): FunctionalResult {
  return {
    coverage: coverage(checks, selection.applicable),
    workingGate: passesGate(checks, selection.working),
    statedSuccess: passesGate(checks, selection.stated),
  };
}

export function tokenTotal(counts: TokenCounts | null | undefined): number | null {
  return counts == null ? null : counts.input + counts.cachedInput + counts.cacheWrite + counts.output;
}

export function summarizeMeasurements(values: readonly (number | null)[]): MeasurementSummary {
  const known = values.filter((value): value is number => value !== null);
  return {
    values, samples: known.length, missing: values.length - known.length,
    mean: known.length === 0 ? null : known.reduce((sum, value) => sum + value, 0) / known.length,
  };
}
