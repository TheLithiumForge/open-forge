import assert from "node:assert/strict";
import { test } from "node:test";
import { validateLinks } from "./markdown.ts";
import { safeRelative } from "./files.ts";

const rules = { path: "rules.md", bytes: Buffer.from("# Rules\n## Card names\n## Card names\n") };
const page = (link: string) => ({ path: "README.md", bytes: Buffer.from(`[Read](${link})\n`) });

test("inline, reference, duplicate heading and encoded file links resolve", () => {
  for (const link of ["rules.md#card-names", "rules.md#card-names-1", "rul%65s.md", "https://example.invalid/rules"]) validateLinks([page(link), rules]);
  validateLinks([{ path: "README.md", bytes: Buffer.from("[Read][rules]\n[rules]: rules.md#card-names\n```md\n[Ignored](missing.md)\n```\n") }, rules]);
});

test("missing headings, escaped paths and absolute links fail", () => {
  for (const link of ["rules.md#missing", "../rules.md", "/rules.md", "%2e%2e/rules.md", "rules.md#%zz", "missing.md"]) assert.throws(() => validateLinks([page(link), rules]));
  for (const path of ["../file", "a/../../file", "a\\file", "a//file", "file:bad", "."]) assert.throws(() => safeRelative(path));
});
