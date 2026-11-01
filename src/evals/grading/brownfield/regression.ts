import type { Definition } from "../../benchmarks/neon-pairs/checks/spec-layers/catalog.ts";
import { aggregate, type AssertionOutcome } from "../../benchmarks/neon-pairs/checks/spec-layers/outcomes.ts";
import type { CheckOutcome } from "../grading.types.ts";
import { CheckStatus } from "../../history/statuses.ts";

export function regressionDefinition(frozen: Definition, supersedes: readonly string[]): Definition {
  const excluded = new Set(supersedes);
  const known = new Set([...frozen.checks.flatMap((check) => check.clauses), ...frozen.review.map((entry) => entry.clause)]);
  if (supersedes.some((clause) => !known.has(clause))) throw new Error("A superseded clause is absent from the frozen Layer 3 set.");
  const checks = frozen.checks.flatMap((check) => {
    if (check.inheritedFrom) return [check];
    const assertions = check.assertions.filter((assertion) => !excluded.has(assertion.clause));
    return assertions.length ? [{ ...check, assertions, clauses: assertions.map((assertion) => assertion.clause) }] : [];
  });
  const ids = new Set(checks.map((check) => check.id));
  return { ...frozen, checks, stated: frozen.stated.filter((id) => ids.has(id)), working: frozen.working.filter((id) => ids.has(id)),
    review: frozen.review.filter((entry) => !excluded.has(entry.clause)) };
}

export function regressionOutcomes(definition: Definition, assertions: readonly AssertionOutcome[], inherited: readonly CheckOutcome[]): CheckOutcome[] {
  return definition.checks.map((check) => {
    const result = check.inheritedFrom ? inherited.find((outcome) => outcome.id === check.id)
      ?? { id: check.id, area: "regression", status: CheckStatus.error, evidence: "The inherited regression checker returned no result." } : aggregate(check, assertions);
    return { ...result, id: `regression-${check.id}`, area: "regression" };
  });
}
