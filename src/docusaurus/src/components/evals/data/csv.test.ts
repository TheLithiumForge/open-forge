import assert from "node:assert/strict";
import { test } from "node:test";
import { previewBatch } from "../../../../scripts/evals/preview-fixture.ts";
import { comparisons } from "./aggregate.ts";
import { cells } from "./cells.ts";
import { resultsCsv, toCsv, type CsvValue } from "./csv.ts";

/** A small RFC 4180 reader, so the tests check what a spreadsheet would read back. */
function parse(text: string): string[][] {
  const rows: string[][] = [];
  let row: string[] = [], field = "", quoted = false;
  for (let index = 0; index < text.length; index++) {
    const char = text[index] ?? "";
    if (quoted && char === "\"" && text[index + 1] === "\"") { field += char; index++; }
    else if (char === "\"") quoted = !quoted;
    else if (!quoted && char === ",") { row.push(field); field = ""; }
    else if (!quoted && char === "\n") { row.push(field); rows.push(row); row = []; field = ""; }
    else if (quoted || char !== "\r") field += char;
  }
  return rows;
}

const Awkward = { comma: "a,b", quote: "say \"hi\"", lines: "one\ntwo", formula: "=SUM(A1)" } as const;

test("fields with commas, quotes and line breaks read back unchanged", () => {
  const values: CsvValue[] = [Awkward.comma, Awkward.quote, Awkward.lines, 12.5, null];
  const [header, row] = parse(toCsv(["a", "b", "c", "d", "e"], [values]));
  assert.equal(header?.length, values.length);
  assert.deepEqual(row, [Awkward.comma, Awkward.quote, Awkward.lines, String(values[3]), ""]);
});

test("text that a spreadsheet would run as a formula stays plain text", () => {
  const [, row] = parse(toCsv(["a"], [[Awkward.formula]]));
  assert.notEqual(row?.[0], Awkward.formula);
  assert.ok(row?.[0]?.endsWith(Awkward.formula));
});

test("the results file has a header and one row per shown cell, all the same width", () => {
  const items = comparisons(cells([previewBatch(1)]));
  const rows = parse(resultsCsv(items));
  assert.equal(rows.length, items.length + 1);
  assert.ok(rows.every((row) => row.length === rows[0]?.length));
});
