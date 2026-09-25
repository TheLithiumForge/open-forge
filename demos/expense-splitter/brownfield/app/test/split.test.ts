import assert from "node:assert/strict";
import { test } from "node:test";
import { splitEvenly } from "../src/split.ts";

test("splits evenly when the total divides exactly", () => {
  assert.deepEqual(
    splitEvenly(900, ["sam", "priya", "lee"]).map((share) => share.cents),
    [300, 300, 300],
  );
});

test("gives leftover cents to the front of the list, where the payer is", () => {
  assert.deepEqual(splitEvenly(1000, ["sam", "priya", "lee"]), [
    { person: "sam", cents: 334 },
    { person: "priya", cents: 333 },
    { person: "lee", cents: 333 },
  ]);
});

test("never loses or invents a cent", () => {
  for (let total = 0; total <= 1000; total += 37) {
    const shares = splitEvenly(total, ["a", "b", "c", "d"]);
    assert.equal(
      shares.reduce((sum, share) => sum + share.cents, 0),
      total,
    );
  }
});

test("refuses to split between nobody", () => {
  assert.throws(() => splitEvenly(100, []), /at least one person/);
});
