import assert from "node:assert/strict";
import { test } from "node:test";
import { formatCents, parseAmount } from "../src/money.ts";

test("parses whole amounts and amounts with cents", () => {
  assert.equal(parseAmount("12"), 1200);
  assert.equal(parseAmount("12.5"), 1250);
  assert.equal(parseAmount("12.05"), 1205);
  assert.equal(parseAmount("0.07"), 7);
});

test("rejects amounts it cannot represent exactly", () => {
  for (const text of ["1.234", "-3", "abc", "", "1,50"]) {
    assert.throws(() => parseAmount(text), /is not an amount/);
  }
});

test("formats cents with two decimal places", () => {
  assert.equal(formatCents(1234), "12.34");
  assert.equal(formatCents(7), "0.07");
  assert.equal(formatCents(-5), "-0.05");
});
