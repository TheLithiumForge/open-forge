import assert from "node:assert/strict";
import { test } from "node:test";
import { CheckStatus } from "../../../../history/statuses.ts";
import { aggregate, Evidence } from "./outcomes.ts";
import { assertionId, type Declaration } from "./catalog.ts";
import { Method } from "./requirements.ts";

const Clauses = ["R01.1", "R01.2"];
const declaration: Declaration = { id: "example", area: "rules", method: Method.browser, clauses: Clauses, assertions: Clauses.map((clause) => ({ id: assertionId(clause), clause, method: Method.browser, text: "Example" })) };
test("a clause needs all probes and a check needs every assertion", () => {
  const evidence = new Evidence();
  evidence.test(Clauses[0] ?? "", true, "First probe");
  assert.equal(aggregate(declaration, evidence.outcomes).status, CheckStatus.error);
  evidence.test(Clauses[1] ?? "", true, "Second assertion");
  assert.equal(aggregate(declaration, evidence.outcomes).status, CheckStatus.pass);
  evidence.merge(Clauses[0] ?? "", CheckStatus.fail, "Second channel fails");
  evidence.merge(Clauses[0] ?? "", CheckStatus.pass, "Later success cannot erase failure");
  assert.equal(aggregate(declaration, evidence.outcomes).status, CheckStatus.fail);
  assert.throws(() => evidence.test(Clauses[0] ?? "", true, "Duplicate"));
});
test("journey prerequisites produce blocked outcomes and checker faults produce errors", async () => {
  const stopped = new Evidence(); stopped.test(Clauses[0] ?? "", false, "Wrong game behavior");
  await stopped.journey(Clauses, async () => { throw new Error("Stopped"); });
  assert.equal(stopped.outcomes[1]?.status, CheckStatus.blocked);
  const crashed = new Evidence(); await crashed.journey(Clauses, async () => { throw new Error("Browser crash"); });
  assert.ok(crashed.outcomes.every((item) => item.status === CheckStatus.error));
});
