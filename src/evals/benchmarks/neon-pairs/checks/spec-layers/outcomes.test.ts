import assert from "node:assert/strict";
import { test } from "node:test";
import { CheckStatus } from "../../../../history/statuses.ts";
import { aggregate, Evidence, EvidenceLimit } from "./outcomes.ts";
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
  assert.match(crashed.outcomes[0]?.evidence ?? "", /Expected.*Observed: Browser crash/);
});
test("failure samples survive merges, capture errors and report limits", async () => {
  const evidence = new Evidence(); let samples = 0;
  evidence.observation = async () => { samples++; return { clockMs: 801, names: ["revealed"] }; };
  evidence.test(Clauses[0]!, true, "Initial sample");
  evidence.merge(Clauses[0]!, CheckStatus.fail, "Expected closed at 800ms", { remainingMs: 1 });
  evidence.merge(Clauses[0]!, CheckStatus.fail, "Second failure");
  await evidence.flush();
  assert.equal(samples, 1);
  assert.match(evidence.outcomes[0]?.evidence ?? "", /Expected closed.*Observed:.*remainingMs.*Observed page:.*clockMs.*revealed/s);
  evidence.merge(Clauses[0]!, CheckStatus.pass, "x".repeat(EvidenceLimit * 2));
  assert.ok((evidence.outcomes[0]?.evidence.length ?? Infinity) <= EvidenceLimit);
  assert.match(evidence.outcomes[0]?.evidence ?? "", /Expected closed/);
  const broken = new Evidence(); broken.observation = async () => { throw new Error("page closed"); };
  broken.record(Clauses[0]!, CheckStatus.error, "Expected a completed probe. Observed browser failure.");
  await broken.flush();
  assert.match(broken.outcomes[0]?.evidence ?? "", /Observed page:.*captureError.*page closed/);
});
