import assert from "node:assert/strict";
import { test } from "node:test";
import { NamedToolchain, parseToolchain, ToolchainClause } from "./toolchain.ts";
import { Limitations } from "./limitations.ts";
import { loadDefinition } from "./load.ts";
import { LayerId } from "./requirements.ts";
import { ZoomClause } from "./browser-zoom.ts";

test("verifier version evidence is bounded and contains every exact version", () => {
  assert.deepEqual(parseToolchain(JSON.stringify(NamedToolchain)), NamedToolchain);
  for (const value of ["{}", "[]", "null", "{" , " ".repeat(1025), JSON.stringify({ ...NamedToolchain, npm: 11 }), JSON.stringify({ ...NamedToolchain, node: "26" })]) assert.throws(() => parseToolchain(value));
});

test("implemented zoom and toolchain clauses have no checker limit", async () => {
  const clauses = [...Object.values(ZoomClause), ToolchainClause];
  for (const clause of clauses) assert.equal(Limitations[clause], undefined);
  for (const layer of Object.values(LayerId)) {
    const definition = await loadDefinition(layer);
    assert.ok(definition.uncheckable.every((item) => !clauses.includes(item.clause)));
    assert.ok(definition.review.some((item) => item.clause === "T07.9"));
  }
});
