import assert from "node:assert/strict";
import { readFile, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { orderedDocuments } from "./document-order.ts";
import { hashes, inventory } from "./files.ts";
import { readFrozenSpecs } from "./frozen.ts";
import { MaterializePaths } from "./paths.ts";
import { CaseLayer, readRequirements } from "./requirements.ts";
import { checkSubset, LayerTwoClauseCount, LayerTwoRequirementCount, materializeSubset } from "./subset.ts";
import { SpecsFiles } from "./specs.ts";
import { sample } from "./test-fixture.ts";

test("the frozen player pack contains each L2 clause once in document and clause order", async (t) => {
  const fixture = await sample(t);
  const manifest = await materializeSubset(fixture.destination);
  const frozen = await readFrozenSpecs(MaterializePaths.specs);
  const requirements = readRequirements(JSON.parse(await readFile(MaterializePaths.requirements, "utf8")));
  const expected = orderedDocuments(frozen.manifest, frozen.documents).flatMap((file) => requirements.filter((entry) => entry.document === `specs/${file.path}`)
    .flatMap((entry) => entry.clauses.filter((clause) => clause.firstLayer === CaseLayer.two)));
  assert.equal(expected.length, LayerTwoClauseCount);
  assert.equal(new Set(expected.map((clause) => clause.id.split(".")[0])).size, LayerTwoRequirementCount);
  assert.deepEqual(manifest.clauseIds, expected.map((clause) => clause.id));
  const documents = (await inventory(fixture.destination)).filter((file) => file.path !== SpecsFiles.manifest);
  const text = documents.map((file) => file.bytes.toString()).join("\n");
  const plain = text.replace(/\[([^\]]+)\]\([^)]+\)/g, "$1");
  for (const clause of expected) {
    assert.equal(text.split(`- ${clause.id} - `).length, 2);
    assert.ok(plain.includes(clause.text), clause.id);
  }
  for (const clause of requirements.flatMap((entry) => entry.clauses).filter((clause) => clause.firstLayer === CaseLayer.three)) assert.ok(!text.includes(`- ${clause.id} - `), clause.id);
  assert.doesNotMatch(text, /board codes?|snapshots?|replay|applyTo|trace\.json/i);
  assert.deepEqual(hashes(await inventory(fixture.destination)), hashes(await inventory(MaterializePaths.playerSpecs)));
  await checkSubset(fixture.destination);
});

test("player pack validation rejects edits and materializing refuses a nonempty target", async (t) => {
  const fixture = await sample(t);
  await materializeSubset(fixture.destination);
  const path = join(fixture.destination, SpecsFiles.overview);
  await writeFile(path, `${await readFile(path, "utf8")}An extra product sentence.\n`);
  await assert.rejects(checkSubset(fixture.destination));
  await assert.rejects(readFrozenSpecs(fixture.destination));
  await assert.rejects(materializeSubset(fixture.destination));
});
