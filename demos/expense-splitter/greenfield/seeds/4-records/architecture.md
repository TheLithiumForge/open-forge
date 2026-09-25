---
open-forge:
  description: How the expense splitter is structured, what each module owns, and which rules every module keeps
  tags: [Memory, Document, Architecture]
---

# Expense Splitter Architecture

## Overview And Scope

A single Node.js command-line program in TypeScript, run directly by Node 22.18 or later, with no runtime dependencies. Data lives in one JSON file. It delivers the first useful version described in the [Vision](vision.md).

## Drivers

- Exact money: see [Money in whole cents](../decisions/decision-money-in-cents.md).
- Predictable rounding: see [The payer absorbs rounding](../decisions/decision-payer-absorbs-rounding.md).
- Nothing to install beyond Node.js.

## System Model

| Module     | Owns                                                             |
| ---------- | ---------------------------------------------------------------- |
| `money`    | Parsing and formatting amounts as whole cents                    |
| `split`    | Turning a total and a list of people into shares                 |
| `ledger`   | Loading, validating, and saving expenses, and computing balances |
| `commands` | One function per command, returning the text to print            |
| `cli`      | Reading arguments and dispatching to a command                   |

Dependencies point one way: `cli` uses `commands`, which uses `ledger`, `split`, and `money`.

## Important Flows

Adding an expense parses the amount into cents, builds the list of people with the payer first, splits the total, checks the shares, and saves the ledger. Leftover cents go one each to the people at the front of the list, so the payer always takes the first one.

## Boundaries And Invariants

- Every saved expense's shares add up exactly to its total.
- Every amount is a whole number of cents. No amount is ever held as a fractional number.

## Tradeoffs And Limits

One JSON file means no concurrent writers. That fits one person running the tool for a group.

## Related Views And Rationale

- [Vision](vision.md): the promise this structure serves.
- [Expense scenarios](expense-scenarios.md): the exact expected results for the first version.
- [Money in whole cents](../decisions/decision-money-in-cents.md) and [The payer absorbs rounding](../decisions/decision-payer-absorbs-rounding.md): why the invariants and the payer-first flow are what they are.
