import assert from "node:assert/strict";
import { mkdir, mkdtemp, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { test } from "node:test";
import { readFrozenSpecs } from "./frozen.ts";
import { packManifest, PackFiles, writeDocumentPack } from "./document-pack.ts";
import { ScopeLayoutVersion } from "./scopes.ts";
import { SpecsFiles } from "./specs.ts";

const Fixture = "fixture-pack", Other = "other.md";
const described = { description: "A document", tags: ["Game"] };
const bytes = (text: string) => Buffer.from(text);

test("a written pack reads back as frozen specs with each document's description and tags", async () => {
  const root = await mkdtemp(join(tmpdir(), "eval-document-pack-"));
  try {
    const folder = join(root, "docs-pack"), scopes = join(root, "docs-scopes.json");
    await mkdir(folder);
    await writeFile(join(folder, SpecsFiles.overview), `# Overview\n\nRead [the other one](${Other}).\n`);
    await writeFile(join(folder, Other), "# Other\n");
    await writeFile(`${folder}.${PackFiles.metadata}`, JSON.stringify({ [SpecsFiles.overview]: described, [Other]: described }));
    await writeFile(scopes, JSON.stringify({ schemaVersion: ScopeLayoutVersion, upFront: [SpecsFiles.overview], scopes: [{ slug: "game", title: "Game", description: "Game scope", tags: ["Game"], documents: [Other] }] }));
    await writeDocumentPack({ folder, scopes, fixture: Fixture });
    const frozen = await readFrozenSpecs(folder);
    assert.equal(frozen.manifest.fixture, Fixture);
    assert.deepEqual(frozen.manifest.files.map((file) => [file.path, file.metadata]), [[SpecsFiles.overview, described], [Other, described]]);
  } finally { await rm(root, { recursive: true, force: true }); }
});

test("a pack refuses a document without metadata or a place, either the other way around, and a missing overview", () => {
  const overview: [string, Uint8Array] = [SpecsFiles.overview, bytes("# Overview\n")], other: [string, Uint8Array] = [Other, bytes("# Other\n")];
  const metadata = new Map([[SpecsFiles.overview, described], [Other, described]]);
  const placed = [SpecsFiles.overview, Other];
  assert.equal(packManifest(new Map([overview, other]), metadata, placed, Fixture).files.length, 2);
  assert.throws(() => packManifest(new Map([overview, other]), new Map([[SpecsFiles.overview, described]]), placed, Fixture));
  assert.throws(() => packManifest(new Map([overview]), metadata, placed, Fixture));
  assert.throws(() => packManifest(new Map([overview, other]), metadata, [SpecsFiles.overview], Fixture));
  assert.throws(() => packManifest(new Map([overview]), metadata, [SpecsFiles.overview, Other], Fixture));
  assert.throws(() => packManifest(new Map([other]), new Map([[Other, described]]), [Other], Fixture));
});
