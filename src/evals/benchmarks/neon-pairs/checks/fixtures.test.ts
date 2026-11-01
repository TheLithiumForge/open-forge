import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { test } from "node:test";
import { CheckStatus } from "../../../history/statuses.ts";
import { loadFixtures, applyEdits, ReferenceFolder, RequiredFixtureIds } from "./fixtures.ts";
import { loadLayer } from "./layer.ts";
import { validateFixture } from "./validate.ts";

test("every mutation matches the unchanged reference and declares every expected result", async () => {
  const layer = await loadLayer();
  const fixtures = await loadFixtures();
  assert.equal(fixtures.length, Object.keys(RequiredFixtureIds).length);
  for (const fixture of fixtures) {
    assert.deepEqual(Object.keys(fixture.expected).sort(), layer.checks.map((check) => check.id).sort());
    for (const edit of fixture.edits) {
      const bytes = await readFile(new URL(edit.path, ReferenceFolder));
      assert.notEqual(applyEdits(bytes.toString().replace(/\r\n/g, "\n"), [edit]), bytes.toString().replace(/\r\n/g, "\n"));
      assert.deepEqual(await readFile(new URL(edit.path, ReferenceFolder)), bytes);
    }
  }
  assert.throws(() => applyEdits("changed reference", [{ before: "old reference", after: "new" }]));
});

test("validation compares observed outcomes with its registered fixture expectations through fakes", async () => {
  const fixture = (await loadFixtures()).find((entry) => entry.id === RequiredFixtureIds.faces);
  assert.ok(fixture);
  const layer = await loadLayer();
  const result = await validateFixture(fixture, {
    build: async () => ({ passed: true, typecheck: true, seconds: 0, packages: null, html: null, bytes: null, log: "" }),
    checks: async () => ({ checks: layer.checks.map((check) => ({ ...check, status: fixture.expected[check.id] ?? CheckStatus.error, evidence: "Fake validation outcome" })), performance: null, shots: [] }),
  });
  assert.ok(result.outcomes.every((check) => check.actual === check.expected));
});
