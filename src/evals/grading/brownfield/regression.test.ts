import assert from "node:assert/strict";
import { test } from "node:test";
import { loadDefinition } from "../../benchmarks/neon-pairs/checks/spec-layers/load.ts";
import { LayerId } from "../../benchmarks/neon-pairs/checks/spec-layers/requirements.ts";
import { Limitations } from "../../benchmarks/neon-pairs/checks/spec-layers/limitations.ts";
import { CheckStatus } from "../../history/statuses.ts";
import { regressionDefinition, regressionOutcomes, regressionReview } from "./regression.ts";

test("declared checker limits and reviews stay pending while all gradable clauses can pass", async () => {
  const original = await loadDefinition(LayerId.three), selected = regressionDefinition(original, []);
  const pending = regressionReview(selected), excluded = new Set([...original.review, ...original.uncheckable].map((entry) => entry.clause));
  assert.ok(pending.length >= Object.keys(Limitations).length);
  assert.ok(original.uncheckable.every((entry) => selected.review.some((review) => review.clause === entry.clause)));
  assert.ok(selected.checks.every((check) => check.assertions.every((assertion) => !excluded.has(assertion.clause))));
  const assertions = original.checks.flatMap((check) => check.assertions.map((assertion) => ({ id: assertion.id, clause: assertion.clause, area: check.area,
    status: excluded.has(assertion.clause) ? CheckStatus.error : CheckStatus.pass, evidence: "Fixture assertion evidence." })));
  const inherited = original.checks.map((check) => ({ id: check.id, area: check.area, status: CheckStatus.pass, evidence: "Fixture inherited evidence." }));
  assert.ok(regressionOutcomes(selected, assertions, inherited).every((check) => check.status === CheckStatus.pass));
  const first = selected.checks.flatMap((check) => check.assertions)[0]; assert.ok(first);
  const errored = assertions.map((assertion) => assertion.id === first.id ? { ...assertion, status: CheckStatus.error } : assertion);
  assert.ok(regressionOutcomes(selected, errored, inherited).some((check) => check.status === CheckStatus.error));
});
