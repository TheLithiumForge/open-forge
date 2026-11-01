import assert from "node:assert/strict";
import { test } from "node:test";
import { IgnoreFile, ignoredBy } from "./ignore.ts";

test("frozen ignore rules handle rooted paths, directory names, globs, escapes and negation", () => {
  const ignores = ignoredBy([{ path: IgnoreFile, content: "# comment\n/root-only/\ncache/\n*.log\n!keep.log\nsrc/**/generated?.[jt]s\n\\#literal\n\\!literal\nspace\\ \n" }]);
  for (const path of ["root-only/output", "cache/output", "src/cache/output", "debug.log", "src/generated1.ts", "src/deep/generated2.js", "#literal", "!literal", "space "]) assert.equal(ignores(path), true, path);
  for (const path of ["src/root-only/output", "cache", "keep.log", "src/keep.log", "space", "src/generated12.ts"]) assert.equal(ignores(path), false, path);
});

test("nested ignore files override parent patterns but cannot reopen an ignored parent", () => {
  const ignores = ignoredBy([{ path: IgnoreFile, content: "*.log\nblocked/\n" }, { path: `src/${IgnoreFile}`, content: "!keep.log\n" }, { path: `blocked/${IgnoreFile}`, content: "!keep.log\n" }]);
  assert.equal(ignores("src/keep.log"), false);
  assert.equal(ignores("other/keep.log"), true);
  assert.equal(ignores("blocked/keep.log"), true);
});
