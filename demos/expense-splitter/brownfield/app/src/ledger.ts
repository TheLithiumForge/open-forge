import { existsSync, readFileSync, writeFileSync } from "node:fs";
import type { Share } from "./split.ts";

export interface Expense {
  readonly description: string;
  readonly paidBy: string;
  readonly totalCents: number;
  readonly shares: readonly Share[];
}

export interface Ledger {
  readonly expenses: readonly Expense[];
}

export interface Balance {
  readonly person: string;
  readonly cents: number;
}

const EMPTY_LEDGER: Ledger = { expenses: [] };

function isLedger(value: unknown): value is Ledger {
  return typeof value === "object" && value !== null && "expenses" in value && Array.isArray(value.expenses);
}

export function loadLedger(file: string): Ledger {
  if (!existsSync(file)) {
    return EMPTY_LEDGER;
  }
  const parsed: unknown = JSON.parse(readFileSync(file, "utf8"));
  if (!isLedger(parsed)) {
    throw new Error(`${file} is not an expense ledger.`);
  }
  return parsed;
}

export function saveLedger(file: string, ledger: Ledger): void {
  writeFileSync(file, `${JSON.stringify(ledger, null, 2)}\n`);
}

// Every saved expense must be split exactly: the shares add up to the total.
export function addExpense(ledger: Ledger, expense: Expense): Ledger {
  const sharedCents = expense.shares.reduce((sum, share) => sum + share.cents, 0);
  if (sharedCents !== expense.totalCents) {
    throw new Error(`The shares add up to ${sharedCents} cents, not ${expense.totalCents}.`);
  }
  return { expenses: [...ledger.expenses, expense] };
}

// A positive balance means the person is owed money. A negative one means they owe it.
export function balances(ledger: Ledger): Balance[] {
  const totals = new Map<string, number>();
  const change = (person: string, cents: number): void => {
    totals.set(person, (totals.get(person) ?? 0) + cents);
  };
  for (const expense of ledger.expenses) {
    change(expense.paidBy, expense.totalCents);
    for (const share of expense.shares) {
      change(share.person, -share.cents);
    }
  }
  return [...totals].map(([person, cents]) => ({ person, cents })).sort((a, b) => b.cents - a.cents);
}
