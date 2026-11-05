import assert from "node:assert/strict";
import { test } from "node:test";
import type { FileBytes } from "../../materialize/files.ts";
import { ForgePreset } from "../../materialize/forge.ts";
import { PlacementRoot } from "../../materialize/placement.ts";
import { evidenceFileText, stalePages, workEvidence, WorkEvidenceFiles, type StaleStatements, type WorkEvidence } from "./work-evidence.ts";

const OldLine = "Solo play is the only mode.", NewLine = "Tournament mode joins solo play.", Word = "tournament";
const StatePage = "state.md", OtherPage = "other.md", AddedPage = "added.md", AddedTest = "tests/added.test.ts", KeptTest = "tests/kept.test.ts";
const stale: readonly StaleStatements[] = [{ document: StatePage, statements: [OldLine], current: Word }];
const file = (path: string, text: string): FileBytes => ({ path, bytes: Buffer.from(text) });
const pack = [file(StatePage, `# State\n\n${OldLine}\n`), file(OtherPage, "# Other\n\nStays as it was.\n")];
const seed = [file(KeptTest, "kept\n")];
const lines = (work: WorkEvidence, name: string): readonly string[] => work.files.find((entry) => entry.name === name)?.lines ?? [];
const run = (files: readonly FileBytes[]): WorkEvidence => workEvidence([...files, ...seed], seed, pack, stale, undefined, ForgePreset.fullCore);

test("a page is current once its old statements are gone and it mentions the work", () => {
  const pages = (text: string) => stalePages(stale, new Map([[StatePage, text]]))[0];
  assert.equal(pages(NewLine)?.current, true);
  assert.equal(pages(`${OldLine}\n${NewLine}`)?.current, false);
  assert.equal(pages(`${OldLine}\n${NewLine}`)?.remaining, 1);
  assert.equal(pages("Nothing about the feature.")?.current, false);
  assert.equal(pages("Nothing about the feature.")?.mentionsWork, false);
  assert.equal(stalePages(stale, new Map())[0]?.current, false);
});

test("the evidence shows only changed lines, new pages and removed pages", () => {
  const work = run([file(`docs/${StatePage}`, `# State\n\n${NewLine}\n`), file(`docs/${AddedPage}`, "# Added\n")]);
  const documents = lines(work, WorkEvidenceFiles.documents);
  assert.ok(documents.includes(`- ${OldLine}`) && documents.includes(`+ ${NewLine}`));
  assert.ok(documents.some((line) => line.includes(AddedPage)));
  assert.ok(documents.some((line) => line.includes(OtherPage)), "A page the run left out shows as removed.");
  assert.equal(work.staleDocs[0]?.current, true);
});

test("an unchanged project reports no changed lines and a stale state page", () => {
  const work = run(pack.map((page) => file(`docs/${page.path}`, page.bytes.toString())));
  assert.deepEqual(lines(work, WorkEvidenceFiles.documents), []);
  assert.equal(work.staleDocs[0]?.current, false);
});

test("a placed page reads like a plain one, so the evidence can't say which setup made the run", () => {
  const Setup = [PlacementRoot, "open-forge", "binding rule"];
  const placed = file(`${PlacementRoot}/memory/crystallized/${StatePage}`, `---\nopen-forge:\n  description: A page\n  tags: [Evergreen]\n---\n# State\n\nThe binding rule is in [its rule file](../../directives/a.md).\n\nSee ${PlacementRoot}/directives/a.md and ${NewLine}\n`);
  const work = run([placed, file(`${PlacementRoot}/loader.md`, "# Loader\n"), file(`${PlacementRoot}/directives/_directives.md`, "# Directives\n")]);
  for (const entry of work.files) for (const word of Setup) assert.ok(!entry.lines.some((line) => line.includes(word)), `${entry.name} ${word}`);
  assert.equal(work.staleDocs[0]?.current, true);
});

test("the run's tests appear when new or changed, and a seed test it left alone doesn't", () => {
  const tests = lines(run([file(AddedTest, "added\n")]), WorkEvidenceFiles.tests);
  assert.ok(tests.some((line) => line.includes(AddedTest)));
  assert.ok(!tests.some((line) => line.includes(KeptTest)));
});

test("an evidence file is valid JSON with one cited line per entry", () => {
  const entries = ["first", "second \"quoted\""];
  const text = evidenceFileText(entries);
  assert.deepEqual(JSON.parse(text), entries);
  assert.equal(text.split("\n")[1], JSON.stringify(entries[0]) + ",");
});
