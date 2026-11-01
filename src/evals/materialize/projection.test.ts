import assert from "node:assert/strict";
import { test } from "node:test";
import { FrameworkVersion } from "./forge.ts";
import { FullSpecification, ProjectionVersion, readProjection } from "./projection.ts";
import { SpecsFiles } from "./specs.ts";

const seed = { schemaVersion: ProjectionVersion, fixture: "sample", level: FullSpecification, frameworkVersion: FrameworkVersion, documents: { [SpecsFiles.overview]: { description: "Game overview", tags: [] } } };

test("projection reading accepts version differences and unknown keys while keeping schema links", (t) => {
  const warnings = t.mock.method(process, "emitWarning", () => undefined);
  const schema = "schema.json";
  for (const schemaVersion of [undefined, ProjectionVersion - 1, ProjectionVersion, ProjectionVersion + 1]) {
    const value = readProjection({ ...seed, schemaVersion, extra: true, $schema: schema });
    assert.deepEqual(value.documents, seed.documents);
    assert.equal(value.$schema, schema);
  }
  assert.equal(warnings.mock.calls.length, 3);
});

test("projection reading refuses malformed fields and path filters", () => {
  for (const invalid of [{ ...seed, fixture: 1 }, { ...seed, documents: [] }, { ...seed, documents: { [SpecsFiles.overview]: { description: "Game", tags: [], applyTo: ["src/**"] } } }]) assert.throws(() => readProjection(invalid));
});
