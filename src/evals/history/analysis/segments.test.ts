import assert from "node:assert/strict";
import { test } from "node:test";
import { ContextClass } from "./classes.ts";
import { commandFacts } from "./command-facts.ts";
import { allocate, outputSegments, type Segment } from "./segments.ts";

const Known = new Map([["AGENTS.md", 100], ["docs/rules.md", 300], ["docs/sound.md", 200], ["src/game/rules.ts", 400]]);
const byClass = (segments: readonly Segment[]) => Object.fromEntries([...new Set(segments.map((item) => item.class))].map((type) => [type, segments.filter((item) => item.class === type).reduce((sum, item) => sum + item.chars, 0)]));
const size = (segments: readonly Segment[]) => segments.reduce((sum, item) => sum + item.chars, 0);

test("known sizes keep their share and unknown outputs split what is left", () => {
  assert.deepEqual(allocate(100, [30, null, null]), [30, 35, 35]);
  assert.deepEqual(allocate(100, [150, 50]), [75, 25]);
  assert.deepEqual(allocate(100, [150, null]), [100, 0]);
  assert.deepEqual(allocate(90, [null, null, null]), [30, 30, 30]);
});

test("a multi-file cat splits its output by each file's size", () => {
  const segments = outputSegments(commandFacts("cat AGENTS.md docs/rules.md src/game/rules.ts"), "x".repeat(800), Known);
  assert.deepEqual(byClass(segments), { [ContextClass.framework]: 100, [ContextClass.spec]: 300, [ContextClass.code]: 400 });
});

test("a glob reads every known file it names", () => {
  const segments = outputSegments(commandFacts("cat docs/*.md"), "x".repeat(500), Known);
  assert.deepEqual(segments.map((item) => [item.class, item.chars]), [[ContextClass.spec, 300], [ContextClass.spec, 200]]);
});

test("a listing with a read shares the output: the file keeps its size and the listing takes the rest", () => {
  const segments = outputSegments(commandFacts("git status --short && cat docs/rules.md"), "x".repeat(350), Known);
  assert.deepEqual(byClass(segments), { [ContextClass.search]: 50, [ContextClass.spec]: 300 });
});

test("head's file headers and open-forge context's headers split the output exactly by file", () => {
  const headed = "==> docs/rules.md <==\nrules\n\n==> src/game/rules.ts <==\ncode";
  const segments = outputSegments(commandFacts("head -n 2 docs/rules.md src/game/rules.ts"), headed, Known);
  assert.equal(size(segments), headed.length);
  assert.deepEqual(Object.keys(byClass(segments)), [ContextClass.spec, ContextClass.code]);
  const context = "=== AGENTS.md ===\nrules\n=== docs/rules.md (rules) ===\nspec";
  assert.deepEqual(Object.keys(byClass(outputSegments(commandFacts("open-forge context"), context, Known))), [ContextClass.framework, ContextClass.spec]);
});

test("search results stay search output and take each line's file type from its path", () => {
  const output = "docs/rules.md:3:best score\nsrc/game/rules.ts:9:bestScore";
  const segments = outputSegments(commandFacts("rg -n best"), output, Known);
  assert.equal(size(segments), output.length);
  assert.deepEqual(segments.map((item) => [item.class, item.fileType]), [[ContextClass.search, "md"], [ContextClass.search, "ts"]]);
});
