import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";
import { test } from "node:test";
import { definition, ExpectedCounts } from "./catalog.ts";
import { LayerId, loadRequirements, Method, object, parseRequirements, supplied } from "./requirements.ts";
import { gradingFingerprint, loadDefinition } from "./load.ts";
import { rubric } from "./rubrics.ts";
import { loadLayer } from "../layer.ts";
import { fillTrace } from "./trace.ts";

test("frozen sets cover every supplied clause with its declared method and cited review", async () => {
  const inventory = await loadRequirements();
  for (const layer of Object.values(LayerId)) {
    const frozen = await loadDefinition(layer), generated = await definition(layer, inventory), counts = ExpectedCounts[layer];
    assert.deepEqual(frozen, generated);
    assert.equal(frozen.checks.length, counts.checks);
    const assertions = frozen.checks.flatMap((check) => check.assertions);
    assert.equal(assertions.length, counts.assertions);
    assert.equal(frozen.review.length, counts.review);
    assert.deepEqual(new Set(assertions.map((item) => item.clause)), new Set(supplied(inventory, layer).filter((item) => item.method !== Method.review).map((item) => item.id)));
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
  const input = (clauses: unknown[]) => ({ specVersion: 2, requirements: [{ id: "R01", clauses }] });
  assert.equal(parseRequirements(input([clause])).length, 1);
  assert.throws(() => parseRequirements(input([clause, clause])));
  assert.throws(() => parseRequirements(input([{ ...clause, firstLayer: "L4" }])));
  assert.throws(() => parseRequirements(input([{ ...clause, method: "guessed" }])));
  assert.throws(() => parseRequirements({}));
});

test("trace edits replace only checks slots and keep reference bytes", () => {
  const input = '{ "entries": [{ "clauseId": "R01.1", "checks": [\n ], "reference": [ "concurrent edit" ], "extra": 4 }] }';
  const result = fillTrace(input, new Map([["R01.1", ["L2:test#assertion"]]]));
  assert.equal(result, input.replace("[\n ]", '["L2:test#assertion"]'));
  assert.throws(() => fillTrace(input, new Map([["unknown", []]])));
});
