import assert from "node:assert/strict";
import { test } from "node:test";
import { join } from "node:path";
import { evalsRoot } from "../paths.ts";
import { documentMetadata } from "./frontmatter.ts";
import { NeonPairsOverload } from "./delivery.ts";
import { loadOverload, NoteKind, NoteRoutes, noteBytes, noteHashes, notePath } from "./overload.ts";
import { stripFrontmatter } from "./strip.ts";

test("the Neon Pairs notes load with every kind, mix into docs or sit on their Memory route, and keep every word", async () => {
  const overload = await loadOverload(join(evalsRoot, NeonPairsOverload));
  assert.deepEqual(new Set(overload.notes.map((note) => note.kind)), new Set(Object.values(NoteKind)));
  for (const note of overload.notes) {
    assert.equal(notePath(note, false, "docs"), `docs/${note.file}`);
    assert.equal(notePath(note, true, "docs"), `${NoteRoutes[note.kind]}/${note.file}`);
    assert.deepEqual(noteBytes(note, false), note.bytes);
    assert.equal(documentMetadata(noteBytes(note, true)).description, note.description);
    assert.deepEqual(stripFrontmatter(noteBytes(note, true)), note.bytes);
  }
  assert.ok(NoteRoutes[NoteKind.decision].includes("crystallized") && [NoteKind.analysis, NoteKind.idea, NoteKind.observation].every((kind) => NoteRoutes[kind].includes("emerging")));
  assert.equal(new Set(noteHashes(overload, true, "docs").map((note) => note.path)).size, overload.notes.length);
});
