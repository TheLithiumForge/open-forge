import assert from "node:assert/strict";
import { test } from "node:test";
import { applyEdits } from "../fixtures.ts";
import { auditFixtures, loadSpecLayerFixtures } from "./fixtures.ts";
import { compareExpectations } from "./validate.ts";
import { parseReport } from "./report.ts";
import { loadDefinition } from "./load.ts";
import { LayerId } from "./requirements.ts";
import { complete, Evidence } from "./outcomes.ts";
import type { LayerOutput } from "./container.ts";

test("fixture mutations match the reference exactly once and declare failed assertions", async () => {
  const fixtures = await loadSpecLayerFixtures();
  assert.ok(fixtures.filter((fixture) => fixture.edits.length > 0).length >= 6);
  assert.deepEqual(await auditFixtures(), fixtures.map((fixture) => fixture.id));
  assert.equal(applyEdits("one old two", [{ before: "old", after: "new" }]), "one new two");
  assert.throws(() => applyEdits("old old", [{ before: "old", after: "new" }]));
  assert.throws(() => applyEdits("absent", [{ before: "old", after: "new" }]));
});
test("report parser rejects invented, duplicate and malformed assertions", async () => {
  const declared = await loadDefinition(LayerId.three);
  const assertion = { id: "invented", clause: "R01.1", status: "pass", evidence: "Wrong ID" };
  await assert.rejects(parseReport({ schemaVersion: 1, layer: LayerId.three, assertions: [assertion] }, declared));
  await assert.rejects(parseReport({ schemaVersion: 1, layer: LayerId.three, assertions: [{ ...assertion, id: "clause-R01.1", status: "success" }] }, declared));
  await assert.rejects(parseReport({}, declared));
  const first = declared.checks.flatMap((check) => check.assertions)[0]; assert.ok(first);
  const valid = { id: first.id, clause: first.clause, status: "pass", evidence: "Duplicate evidence" };
  await assert.rejects(parseReport({ schemaVersion: 1, layer: LayerId.three, assertions: [valid, valid] }, declared));
});
test("validation compares required assertions and never treats missing evidence as passed", async () => {
  const definition = await loadDefinition(LayerId.two), evidence = new Evidence();
  const first = definition.checks.flatMap((check) => check.assertions)[0]; assert.ok(first);
  evidence.test(first.clause, false, "Broken copy");
  const output: LayerOutput = { checks: complete(definition, evidence.outcomes, []), assertions: evidence.outcomes, review: definition.review, shots: [], performance: null };
  assert.deepEqual(compareExpectations(output, { [first.clause]: "fail" }, false), []);
  assert.ok(compareExpectations(output, { [first.clause]: "pass" }, false).length > 0);
  assert.ok(compareExpectations(output, {}, true, ["missing"]).some((message) => message.includes("missing assertion")));
});
