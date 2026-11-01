import assert from "node:assert/strict";
import { readFile, writeFile } from "node:fs/promises";
import { test } from "node:test";
import { hashes, inventory } from "./files.ts";
import { readFrozenSpecs } from "./frozen.ts";
import { MaterializePaths } from "./paths.ts";
import { materializeSpecs, SpecsFiles } from "./specs.ts";
import { sample } from "./test-fixture.ts";

test("the full pack freezes twice to the recorded ten plain documents", async (t) => {
  const options = await sample(t);
  const source = MaterializePaths.source;
  const original = await readFrozenSpecs(MaterializePaths.specs);
  assert.deepEqual(hashes(await inventory(source)), original.manifest.sourceFiles);
  await writeFile(options.projection, await readFile(MaterializePaths.projection));
  const first = await materializeSpecs({ ...options, source });
  const output = hashes(await inventory(options.destination));
  const second = await materializeSpecs({ ...options, source });
  assert.deepEqual(first, original.manifest);
  assert.deepEqual(second, first);
  assert.deepEqual(hashes(await inventory(options.destination)), output);
  assert.deepEqual(first.files.map((file) => file.path), original.documents.map((file) => file.path));
  assert.equal(first.files.some((file) => file.path === SpecsFiles.entrypoint), false);
});
