import assert from "node:assert/strict";
import { join } from "node:path";
import { test } from "node:test";
import { loadSuite } from "../benchmarks/suite.ts";
import { evalsRoot } from "../paths.ts";
import { DocumentCase, DocumentSetups } from "./delivery.ts";
import { readFrozenSpecs } from "./frozen.ts";
import { validateLinks } from "./markdown.ts";
import { MaterializePaths } from "./paths.ts";
import { loadScopeLayout, readScopeLayout, relink, scopedMetadata, scopedPath, ScopeLayoutVersion, unlink, UpFrontTag } from "./scopes.ts";

async function neonPairsLayout() {
  const path = (await loadSuite(DocumentCase.three)).setups[DocumentSetups.fullCoreScoped]?.delivery?.scopes;
  assert.ok(path !== undefined, "The full-spec case names its scope layout.");
  return loadScopeLayout(join(evalsRoot, path));
}

test("the Neon Pairs scope layout places every spec document once and keeps its text through the link rewrite", async () => {
  const layout = await neonPairsLayout();
  const frozen = await readFrozenSpecs(MaterializePaths.specs);
  const paths = frozen.documents.map((file) => file.path);
  for (const path of [...layout.upFront, ...layout.scopes.flatMap((scope) => scope.documents)]) assert.ok(paths.includes(path), path);
  const moved = frozen.documents.map((file) => ({ path: scopedPath(layout, file.path), bytes: relink(layout, file.path, file.bytes) }));
  validateLinks(moved);
  assert.ok(moved.some((file, index) => !file.bytes.equals(Buffer.from(frozen.documents[index]?.bytes ?? ""))), "Some links point into scope folders.");
  for (const [index, file] of frozen.documents.entries()) assert.deepEqual(unlink(layout, file.path, moved[index]?.bytes ?? Buffer.alloc(0)), Buffer.from(file.bytes));
  for (const record of frozen.manifest.files) {
    assert.equal(scopedMetadata(layout, record.path, record.metadata).tags.includes(UpFrontTag), layout.upFront.includes(record.path), record.path);
  }
});

test("a scope layout refuses a document in two scopes and an up-front document inside a scope", async () => {
  const layout = await neonPairsLayout();
  const [first, second] = layout.scopes;
  assert.ok(first !== undefined && second !== undefined);
  assert.throws(() => readScopeLayout({ ...layout, scopes: [first, { ...second, documents: [...second.documents, ...first.documents] }] }));
  assert.throws(() => readScopeLayout({ ...layout, upFront: [...layout.upFront, ...first.documents] }));
  assert.throws(() => readScopeLayout({ ...layout, schemaVersion: ScopeLayoutVersion + 1 }));
  assert.throws(() => readScopeLayout({ ...layout, scopes: [{ ...first, documents: ["../outside.md"] }] }));
});
