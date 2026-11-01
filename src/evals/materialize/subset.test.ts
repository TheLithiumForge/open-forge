import assert from "node:assert/strict";
import { readFile, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { orderedDocuments } from "./document-order.ts";
import { hashes, inventory } from "./files.ts";
import { readFrozenSpecs } from "./frozen.ts";
import { MaterializePaths } from "./paths.ts";
import { CaseLayer, readRequirements, SpecVersion } from "./requirements.ts";
import { selectedClauses } from "./subset-selection.ts";
import { checkSubset, LayerTwoClauseCount, LayerTwoRequirementCount, materializeSubset } from "./subset.ts";
import { SpecsFiles } from "./specs.ts";
import { sample } from "./test-fixture.ts";

const PlayerCapClauses = ["R05.5", "R05.6", "R05.7", "R05.8", "R07.9", "R07.10", "R07.11"];
const PlayerCapHistory = { before: 180, after: 187 } as const;

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
  const caps = manifest.selectionChanges.find((change) => PlayerCapClauses.every((id) => change.added.includes(id)));
  assert.ok(caps);
  assert.equal(caps.from.count, PlayerCapHistory.before);
  assert.equal(caps.to.count, PlayerCapHistory.after);
  assert.deepEqual(caps.added, PlayerCapClauses);
  assert.deepEqual(caps.removed, []);
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

test("updating an existing pack records acceptance and unchanged regeneration keeps the record", async (t) => {
  const fixture = await sample(t);
  const original = await materializeSubset(fixture.destination);
  const before = hashes(await inventory(fixture.destination));
  const requirements = readRequirements(JSON.parse(await readFile(MaterializePaths.requirements, "utf8")));
  const first = original.clauseIds[0];
  assert.ok(first);
  const next = requirements.map((requirement) => ({ ...requirement, clauses: requirement.clauses.map((clause) => clause.id === first ? { ...clause, text: `${clause.text} Updated product wording.` } : clause) }));
  const requirementsPath = join(fixture.root, "updated-requirements.json");
  await writeFile(requirementsPath, JSON.stringify({ specVersion: SpecVersion, requirements: next }));
  await assert.rejects(materializeSubset(fixture.destination, MaterializePaths.specs, requirementsPath));
  assert.deepEqual(hashes(await inventory(fixture.destination)), before);
  await assert.rejects(materializeSubset(join(fixture.root, "new-pack"), MaterializePaths.specs, requirementsPath));
  const options = { acceptSelectionChange: "Accept the updated product wording." };
  const accepted = await materializeSubset(fixture.destination, MaterializePaths.specs, requirementsPath, options);
  assert.equal(accepted.selectionChanges.length, original.selectionChanges.length + 1);
  assert.deepEqual(accepted.selectionChanges.at(-1)?.updated, [first]);
  assert.deepEqual(accepted.clauseIds, original.clauseIds);
  assert.deepEqual(await materializeSubset(fixture.destination, MaterializePaths.specs, requirementsPath), accepted);
  const clauses = selectedClauses(orderedDocuments(accepted, (await readFrozenSpecs(fixture.destination)).documents));
  assert.equal(clauses.length, LayerTwoClauseCount);
});
