import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";
import { test } from "node:test";
import { definition, ExpectedCounts } from "./catalog.ts";
import { LayerId, loadRequirements, Method, object, parseRequirements, SpecVersion, supplied } from "./requirements.ts";
import { gradingFingerprint, loadDefinition } from "./load.ts";
import { rubric } from "./rubrics.ts";
import { loadLayer } from "../layer.ts";
import { fillTrace } from "./trace.ts";
import { CapClause, LayerThreeCaps } from "./cap-policy.ts";
import { reviewReason, SpokenJourneyRequirement } from "./review-policy.ts";

test("frozen sets cover every supplied clause with its declared method and cited review", async () => {
  const inventory = await loadRequirements();
  for (const layer of Object.values(LayerId)) {
    const frozen = await loadDefinition(layer), generated = await definition(layer, inventory), counts = ExpectedCounts[layer];
    assert.deepEqual(frozen, generated);
    assert.equal(frozen.checks.length, counts.checks);
    const assertions = frozen.checks.flatMap((check) => check.assertions);
    assert.equal(assertions.length, counts.assertions);
    assert.equal(frozen.review.length, counts.review);
    assert.deepEqual(new Set(assertions.map((item) => item.clause)), new Set(supplied(inventory, layer).filter((item) => reviewReason(item, layer) === undefined).map((item) => item.id)));
    for (const check of frozen.checks) for (const assertion of check.assertions) {
      assert.equal(assertion.method, inventory.find((clause) => clause.id === assertion.clause)?.method);
      assert.equal(check.method, assertion.method);
    }
    const review = rubric(layer, inventory);
    assert.deepEqual(new Set(review.questions.flatMap((question) => question.clauses)), new Set(frozen.review.map((item) => item.clause)));
    assert.equal(review.ballots, 3);
    assert.deepEqual(review.answers, ["yes", "no", "unsure"]);
    assert.equal(await gradingFingerprint(layer), await gradingFingerprint(layer));
    assert.match(await gradingFingerprint(layer), /^[a-f0-9]{64}$/);
    assert.deepEqual(frozen.checks.filter((check) => check.inheritedFrom).map((check) => check.id), (await loadLayer()).stated);
  }
});

test("numeric cap clauses belong only to Layer 3", async () => {
  const inventory = await loadRequirements(), l2 = await loadDefinition(LayerId.two), l3 = await loadDefinition(LayerId.three);
  for (const clause of LayerThreeCaps) {
    assert.ok(!l2.review.some((item) => item.clause === clause));
    assert.ok(!l2.checks.some((check) => check.clauses.includes(clause)));
    assert.ok(l3.checks.some((check) => check.clauses.includes(clause)));
  }
  for (const clause of [CapClause.elapsedPlay, CapClause.elapsedDisplay]) assert.ok(l2.checks.some((check) => check.clauses.includes(clause)));
  assert.ok(inventory.filter((clause) => (LayerThreeCaps as readonly string[]).includes(clause.id)).every((clause) => clause.method === Method.browser && clause.firstLayer === LayerId.three));
});

test("the complete accessibility-tree journey is executable without spoken confirmation", async () => {
  const inventory = await loadRequirements(), clauses = inventory.filter((clause) => clause.requirement === SpokenJourneyRequirement);
  assert.ok(clauses.length > 0 && clauses.every((clause) => clause.method === Method.browser));
  for (const layer of Object.values(LayerId)) {
    const declared = await loadDefinition(layer);
    for (const clause of clauses) {
      assert.ok(!declared.review.some((item) => item.clause === clause.id));
      assert.ok(declared.checks.some((item) => item.clauses.includes(clause.id)));
    }
  }
});

test("authoring process cleanup follows the inventory review method in both layers", async () => {
  const inventory = await loadRequirements();
  const process = inventory.find((clause) => clause.id === "T07.9"); assert.ok(process);
  assert.equal(process.method, Method.review);
  for (const layer of Object.values(LayerId)) {
    const declared = await loadDefinition(layer);
    assert.ok(declared.review.some((item) => item.clause === process.id));
    assert.ok(!declared.checks.some((check) => check.clauses.includes(process.id)));
    assert.ok(rubric(layer, inventory).questions.some((question) => question.clauses.includes(process.id)));
  }
});

test("all preexisting L1 checker files stay byte identical", async () => {
  const value: unknown = JSON.parse(await readFile(new URL("../../fixtures/spec-layers/L1-baseline.json", import.meta.url), "utf8"));
  assert.ok(object(value) && Array.isArray(value["files"]));
  for (const entry of value["files"]) {
    assert.ok(object(entry) && typeof entry["path"] === "string");
    const bytes = await readFile(new URL(`../../${entry["path"]}`, import.meta.url));
    assert.equal(createHash("sha256").update(bytes).digest("hex"), entry["sha256"], entry["path"]);
  }
});

test("requirement parser rejects duplicate clauses, wrong layers and methods", () => {
  const clause = { id: "R01.1", text: "Example", firstLayer: LayerId.two, method: Method.browser };
  const input = (clauses: unknown[]) => ({ specVersion: SpecVersion, requirements: [{ id: "R01", clauses }] });
  assert.equal(parseRequirements(input([clause])).length, 1);
  assert.throws(() => parseRequirements(input([clause, clause])));
  assert.throws(() => parseRequirements(input([{ ...clause, firstLayer: "L4" }])));
  assert.throws(() => parseRequirements(input([{ ...clause, method: "guessed" }])));
  assert.throws(() => parseRequirements({}));
  assert.throws(() => parseRequirements({ ...input([clause]), specVersion: 2 }));
});

test("every v2.1 trace slot names exactly its layer assertions and review questions", async () => {
  const inventory = await loadRequirements(), expected = new Map<string, string[]>();
  for (const layer of Object.values(LayerId)) {
    const declared = await loadDefinition(layer);
    for (const check of declared.checks) for (const assertion of check.assertions) expected.set(assertion.clause, [...(expected.get(assertion.clause) ?? []), `${layer}:${check.id}#${assertion.id}`]);
    for (const question of rubric(layer, inventory).questions) for (const clause of question.clauses) expected.set(clause, [...(expected.get(clause) ?? []), `${layer}:rubric#${question.id}`]);
  }
  const value: unknown = JSON.parse(await readFile(new URL("../../trace.json", import.meta.url), "utf8"));
  assert.ok(object(value) && value["specVersion"] === SpecVersion && Array.isArray(value["entries"]));
  assert.equal(value["entries"].length, inventory.length);
  for (const entry of value["entries"]) { assert.ok(object(entry) && typeof entry["clauseId"] === "string"); assert.deepEqual(entry["checks"], expected.get(entry["clauseId"]), entry["clauseId"]); }
});

test("trace edits replace only checks slots and keep reference bytes", () => {
  const input = '{ "entries": [{ "clauseId": "R01.1", "checks": [\n ], "reference": [ "concurrent edit" ], "extra": 4 }] }';
  const result = fillTrace(input, new Map([["R01.1", ["L2:test#assertion"]]]));
  assert.equal(result, input.replace("[\n ]", '["L2:test#assertion"]'));
  assert.throws(() => fillTrace(input, new Map([["unknown", []]])));
});
