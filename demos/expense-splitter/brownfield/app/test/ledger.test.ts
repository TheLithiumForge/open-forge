import assert from "node:assert/strict";
import { test } from "node:test";
import { addExpense, balances } from "../src/ledger.ts";
import type { Ledger } from "../src/ledger.ts";

const dinner = {
  description: "Dinner",
  paidBy: "sam",
  totalCents: 1000,
  shares: [
    { person: "sam", cents: 334 },
    { person: "priya", cents: 333 },
    { person: "lee", cents: 333 },
  ],
};

test("works out who owes whom", () => {
  const ledger = addExpense({ expenses: [] }, dinner);
  assert.deepEqual(balances(ledger), [
    { person: "sam", cents: 666 },
    { person: "priya", cents: -333 },
    { person: "lee", cents: -333 },
  ]);
});

test("refuses an expense whose shares don't add up to the total", () => {
  const empty: Ledger = { expenses: [] };
  assert.throws(() => addExpense(empty, { ...dinner, totalCents: 1001 }), /add up to 1000 cents, not 1001/);
});
