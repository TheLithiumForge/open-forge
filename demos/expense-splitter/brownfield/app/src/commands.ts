import { addExpense, balances, loadLedger, saveLedger } from "./ledger.ts";
import { formatCents, parseAmount } from "./money.ts";
import { splitEvenly } from "./split.ts";

export interface AddOptions {
  readonly description: string;
  readonly amount: string;
  readonly paidBy: string;
  readonly with: readonly string[];
}

// The payer always comes first, then everyone else in the order given.
function participants(paidBy: string, others: readonly string[]): string[] {
  return [paidBy, ...others.filter((person) => person !== paidBy)];
}

export function add(file: string, options: AddOptions): string {
  const totalCents = parseAmount(options.amount);
  const shares = splitEvenly(totalCents, participants(options.paidBy, options.with));
  const ledger = addExpense(loadLedger(file), { description: options.description, paidBy: options.paidBy, totalCents, shares });
  saveLedger(file, ledger);
  const split = shares.map((share) => `${share.person} ${formatCents(share.cents)}`).join(", ");
  return `Added "${options.description}" (${formatCents(totalCents)}): ${split}`;
}

export function list(file: string): string {
  const { expenses } = loadLedger(file);
  if (expenses.length === 0) {
    return "No expenses yet.";
  }
  return expenses.map((expense) => `${expense.description}  ${formatCents(expense.totalCents)}  paid by ${expense.paidBy}`).join("\n");
}

export function showBalances(file: string): string {
  const rows = balances(loadLedger(file));
  if (rows.length === 0) {
    return "Everyone is settled up.";
  }
  return rows.map(({ person, cents }) => `${person}  ${formatCents(cents)}  ${cents >= 0 ? "is owed" : "owes"}`).join("\n");
}
