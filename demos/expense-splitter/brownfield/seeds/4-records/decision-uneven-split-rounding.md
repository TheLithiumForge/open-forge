---
open-forge:
  description: Percentage splits keep whole cents and give leftover cents to the payer first
  tags: [Memory, Decision, Money, Rounding, Split]
---

# Percentage splits keep whole cents and payer-first leftovers

## Decision

A percentage split works in whole cents only. Each person's share is their percentage of the total, rounded down to a whole cent. The cents left over go one at a time to the payer first, if the payer is in the split, and then to everyone else in the order given.

**Applies to:** every uneven split, including any future split by shares.

**Accepted by:** the maintainer, when percentage splits were planned.

## Context And Rationale

The app already keeps every amount in whole cents, because floating point made balances drift by cents nobody could explain. Even splits already give the leftover cents to the payer, who is listed first. Percentage splits must follow both rules, or the same expense would round differently depending on how it was entered.

## Alternatives And Tradeoffs

| Alternative                                             | Why it was not selected                                                                           |
| ------------------------------------------------------- | ------------------------------------------------------------------------------------------------- |
| Compute shares with floating point and round at the end | It reintroduces the drift the cents rule removed, and the shares may not add up to the total.     |
| Give leftover cents to the largest percentage           | It is fair in a different way, but it contradicts the payer rule that even splits already follow. |
| Give leftover cents in list order, ignoring the payer   | It is simpler, but the same people would round differently between even and uneven splits.        |

## Consequences

**Benefits:** shares always add up to the total, and rounding is predictable.

**Costs and obligations:** the split code needs the payer, not just the list of people.

**Revisit when:** the app adds a second currency or a currency without cents.

## Current Sources

- [Percentage split scenarios](../documents/percentage-split-scenarios.md): the exact expected results, including where leftover cents go.
- Once the feature ships, the [README](../../../../README.md) documents `--percent`, and the split tests pin the rounding.
