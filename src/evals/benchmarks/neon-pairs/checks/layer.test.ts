import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";
import { test } from "node:test";
import { suiteGradingInputs } from "../../registry.ts";
import { LayerFiles, LayerVersion, loadLayer, record } from "./layer.ts";
import { frozenQuestions } from "../../../grading/judge/rubric.ts";

test("every stated check maps to a prompt clause and wishes stay out of that map", async () => {
  const layer = await loadLayer();
  const requirements: unknown = JSON.parse(await readFile(new URL(`../${LayerFiles.requirements}`, import.meta.url), "utf8"));
  assert.ok(record(requirements));
  assert.equal(requirements["version"], LayerVersion);
  const clauses = requirements["clauses"];
  assert.ok(Array.isArray(clauses));
  const mapped = new Set<string>();
  for (const clause of clauses) {
    assert.ok(record(clause));
    assert.ok(Array.isArray(clause["checks"]));
    for (const id of clause["checks"]) { assert.equal(typeof id, "string"); mapped.add(String(id)); }
  }
  assert.deepEqual([...mapped].sort(), [...layer.stated].sort());
  assert.equal(layer.wishes.some((id) => mapped.has(id)), false);
});

test("versioned layer, requirements and rubric hashes feed grading inputs", async () => {
  const layer = await loadLayer();
  const inputs = await suiteGradingInputs(layer.suite);
  assert.deepEqual(inputs.map((input) => input.path), Object.values(LayerFiles));
  for (const input of inputs) {
    const bytes = await readFile(new URL(`../${input.path}`, import.meta.url));
    assert.equal(input.sha256, createHash("sha256").update(bytes).digest("hex"));
  }
  assert.equal((await frozenQuestions()).length, 8);
});
