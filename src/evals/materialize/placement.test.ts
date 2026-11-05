import assert from "node:assert/strict";
import { readFile, rm } from "node:fs/promises";
import { dirname, join } from "node:path";
import { test } from "node:test";
import { loadSuite } from "../benchmarks/suite.ts";
import { checkSetup } from "../config/qualify.ts";
import { generateSetup } from "../config/setups.ts";
import { evalsRoot } from "../paths.ts";
import { DocumentScaleCase } from "./brownfield-case.ts";
import { PackSetups, placementFile } from "./brownfield.ts";
import type { FileBytes } from "./files.ts";
import { ForgePreset } from "./forge.ts";
import { catalogueOf, loadPlacement, misplacedSentences, placedFiles, placedPaths, PlacementRoot, restoreDocuments, type Placement } from "./placement.ts";
import { UpFrontTag } from "./scopes.ts";

const Rungs = Object.values(ForgePreset);
interface CaseRules { readonly rules: readonly { readonly document: string; readonly sentence: string }[]; readonly superseded: readonly { readonly document: string; readonly statement: string }[] }

const placedCase = async (): Promise<{ placement: Placement; documents: readonly FileBytes[]; rules: CaseRules }> => {
  const suite = await loadSuite(DocumentScaleCase);
  const documents = suite.brownfield?.specifications.documents;
  assert.ok(documents !== undefined);
  const rules = JSON.parse(await readFile(new URL(`../benchmarks/neon-pairs/cases/${DocumentScaleCase}/rules.json`, import.meta.url), "utf8")) as CaseRules;
  return { placement: await loadPlacement(join(evalsRoot, placementFile({ id: DocumentScaleCase }))), documents, rules };
};
const byPath = (files: readonly FileBytes[]): Map<string, Uint8Array> => new Map(files.map((file) => [file.path, file.bytes]));

test("on every rung, restoring a placed project gives back the frozen pack byte for byte", async () => {
  const { placement, documents } = await placedCase();
  for (const rung of Rungs) {
    const restored = restoreDocuments(placement, rung, byPath(placedFiles(placement, rung, documents)));
    assert.equal(restored.size, documents.length, rung);
    for (const file of documents) assert.deepEqual(restored.get(file.path), file.bytes, `${rung} ${file.path}`);
  }
});

test("on every rung, each document has one place inside the categories, and nothing goes to an outside folder", async () => {
  const { placement, documents } = await placedCase();
  for (const rung of Rungs) {
    const paths = placedFiles(placement, rung, documents).map((file) => file.path);
    assert.equal(new Set(paths).size, paths.length, rung);
    assert.deepEqual([...paths].sort(), placedPaths(placement, rung).sort(), rung);
    assert.ok(paths.every((path) => path.startsWith(`${PlacementRoot}/`)), rung);
  }
  assert.equal(placement.documents.length + 1, documents.length);
});

test("each moved sentence leaves its page for its rule file, and a rule file holds its rules under Instructions", async () => {
  const { placement, documents } = await placedCase();
  for (const rung of Rungs) assert.deepEqual(misplacedSentences(placement, rung, byPath(placedFiles(placement, rung, documents))), [], rung);
  const broken = byPath(placedFiles(placement, ForgePreset.fullCore, documents));
  const [first] = placement.sentences;
  assert.ok(first !== undefined);
  broken.set(`${PlacementRoot}/${first.directive}`, Buffer.alloc(0));
  assert.notDeepEqual(misplacedSentences(placement, ForgePreset.fullCore, broken), []);
});

test("rule files come from a reading that found the graded rules among many others, and left replaced statements alone", async () => {
  const { placement, rules } = await placedCase();
  for (const rule of rules.rules) {
    const here = placement.sentences.filter((moved) => moved.document === rule.document);
    assert.ok(here.some((moved) => moved.sentence.includes(rule.sentence) || rule.sentence.includes(moved.sentence)), rule.sentence);
  }
  assert.ok(placement.sentences.length > rules.rules.length);
  for (const replaced of rules.superseded) assert.ok(!placement.sentences.some((moved) => moved.document === replaced.document && moved.sentence.includes(replaced.statement)), replaced.document);
});

test("only the overview, cross-cutting rules and rule files load by themselves, and a loading catalogue is limited to matching work", async () => {
  const { placement } = await placedCase();
  for (const rung of Rungs) {
    assert.ok(placement.documents.every((document) => !document.tags[rung]?.includes(UpFrontTag)), rung);
    for (const directive of Object.keys(placement.directiveFiles)) {
      const catalogue = catalogueOf(placement, directive, rung);
      if (catalogue.tags.includes(UpFrontTag)) assert.ok((catalogue.applyTo ?? []).length > 0, directive);
    }
  }
  assert.ok(Object.keys(placement.directiveFiles).some((directive) => !catalogueOf(placement, directive, ForgePreset.fullCore).tags.includes(UpFrontTag)));
  assert.ok(placement.readme.overview.tags.includes(UpFrontTag));
});

test("the real CLI qualifies a placed setup, with its custom category listed and every rule reachable", async () => {
  const suite = await loadSuite(DocumentScaleCase);
  const generated = await generateSetup(suite, PackSetups.minimalProper);
  try {
    assert.equal(generated.materialized?.preset, ForgePreset.minimal);
    await checkSetup(generated);
    const loader = await readFile(join(generated.folder, PlacementRoot, "loader.md"), "utf8");
    for (const root of generated.materialized?.placement?.customRoots[ForgePreset.minimal] ?? []) assert.ok(loader.includes(root), root);
  } finally { await rm(dirname(generated.folder), { recursive: true, force: true }); }
});
