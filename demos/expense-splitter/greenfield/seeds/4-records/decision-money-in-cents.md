---
open-forge:
  description: Every amount is a whole number of cents, and every saved expense splits exactly
  tags: [Memory, Decision, Money]
---

# Money is stored in whole cents

## Decision

Every amount is a whole number of cents, from parsing input to saving the ledger. No floating point value ever holds an amount. The ledger refuses any expense whose shares don't add up to its total.

**Applies to:** all code that parses, computes, stores, or prints money.

**Accepted by:** the maintainer, during planning.

## Context And Rationale

Splitting 10.00 three ways in floating point produces values like 3.3333333333333335. Stored balances then drift by cents nobody can explain, which breaks the one promise the tool makes.

## Alternatives And Tradeoffs

| Alternative                                  | Why it was not selected                                        |
| -------------------------------------------- | -------------------------------------------------------------- |
| Floating point with rounding at display time | The drift still accumulates in stored balances.                |
| A decimal library                            | It would work, but it adds a dependency, and cents are enough. |

## Consequences

**Benefits:** balances are exact and every expense is internally consistent.

**Costs and obligations:** every future feature must compute in cents too.

**Revisit when:** the tool needs a currency with no minor unit or more than two decimal places.

## Current Sources

- [Architecture](../documents/architecture.md#boundaries-and-invariants): the whole-cents invariant and which module parses and formats money.
- [Expense scenarios](../documents/expense-scenarios.md): the expected results, including the rejected 1.234 and the stored form.
