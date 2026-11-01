import assert from "node:assert/strict";
import { test } from "node:test";
import { freeze, LayersOnlyFlag } from "./freeze.ts";
import { LayerId } from "./requirements.ts";
import { loadDefinition } from "./load.ts";

test("layers-only regeneration leaves trace, rubrics and specs untouched", async () => {
  assert.equal(LayersOnlyFlag, "--layers");
  const writes = new Map<string, unknown>();
  await freeze(undefined, true, async (path, text) => { writes.set(path.pathname.split("/").slice(-2).join("/"), JSON.parse(text)); });
  assert.deepEqual([...writes.keys()], Object.values(LayerId).map((layer) => `layers/${layer}.json`));
  for (const layer of Object.values(LayerId)) assert.deepEqual(writes.get(`layers/${layer}.json`), await loadDefinition(layer));
});
