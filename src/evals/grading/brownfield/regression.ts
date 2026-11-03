import type { Definition } from "../../benchmarks/neon-pairs/checks/spec-layers/catalog.ts";
import { aggregate, type AssertionOutcome } from "../../benchmarks/neon-pairs/checks/spec-layers/outcomes.ts";
import type { CheckOutcome } from "../grading.types.ts";
import { CheckStatus } from "../../history/statuses.ts";

export function regressionDefinition(frozen: Definition, supersedes: readonly string[]): Definition {
  const excluded = new Set(supersedes);
  const pending = new Map([...frozen.review, ...frozen.uncheckable].filter((entry) => !excluded.has(entry.clause)).map((entry) => [entry.clause, entry]));
  const known = new Set([...frozen.checks.flatMap((check) => check.clauses), ...frozen.review.map((entry) => entry.clause)]);
  if (supersedes.some((clause) => !known.has(clause))) throw new Error("A superseded clause is absent from the frozen Layer 3 set.");
  const checks = frozen.checks.flatMap((check) => {
    if (check.inheritedFrom) return [check];
    const assertions = check.assertions.filter((assertion) => !excluded.has(assertion.clause) && !pending.has(assertion.clause));
    return assertions.length ? [{ ...check, assertions, clauses: assertions.map((assertion) => assertion.clause) }] : [];
  });
  const ids = new Set(checks.map((check) => check.id));
  return { ...frozen, checks, stated: frozen.stated.filter((id) => ids.has(id)), working: frozen.working.filter((id) => ids.has(id)),
    review: [...pending.values()], uncheckable: frozen.uncheckable.filter((entry) => !excluded.has(entry.clause)) };
}

/** The regression set without a case's superseded clauses and the clauses it grades as change checks, which must each be asserted. */
export function caseRegression(frozen: Definition, supersedes: readonly string[], targets: readonly string[]): Definition {
  const asserted = new Set(frozen.checks.flatMap((check) => check.assertions.map((assertion) => assertion.clause)));
  if (targets.some((clause) => !asserted.has(clause))) throw new Error("A clause check names a clause the full-spec checker doesn't assert.");
  return regressionDefinition(frozen, [...supersedes, ...targets]);
}

export const RegressionReviewArea = "regression-review";
export function regressionReview(definition: Definition): CheckOutcome[] {
  return definition.review.map((entry) => ({ id: `review-${entry.clause}`, area: RegressionReviewArea, status: CheckStatus.blocked, evidence: `${entry.clause}: ${entry.reason}` }));
}

export function regressionOutcomes(definition: Definition, assertions: readonly AssertionOutcome[], inherited: readonly CheckOutcome[]): CheckOutcome[] {
  return definition.checks.map((check) => {
    const result = check.inheritedFrom ? inherited.find((outcome) => outcome.id === check.id)
      ?? { id: check.id, area: "regression", status: CheckStatus.error, evidence: "The inherited regression checker returned no result." } : aggregate(check, assertions);
    return { ...result, id: `regression-${check.id}`, area: "regression" };
  });
}
