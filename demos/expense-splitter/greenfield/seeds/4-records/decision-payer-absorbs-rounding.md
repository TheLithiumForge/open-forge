---
open-forge:
  description: Leftover cents from a split go to the payer, who is always listed first
  tags: [Memory, Decision, Money, Rounding]
---

# The payer absorbs rounding

## Decision

When a total doesn't divide evenly, the leftover cents go one each to the people at the front of the split, and the payer is always listed first. In practice the payer pays the extra cent.

**Applies to:** every split that can leave cents over.

**Accepted by:** the maintainer, during planning.

## Context And Rationale

Someone has to pay 3.34 when 10.00 is split three ways. Giving the extra cent to the payer is predictable, needs no stored state, and the payer chose the expense.

## Alternatives And Tradeoffs

| Alternative                          | Why it was not selected                                                       |
| ------------------------------------ | ----------------------------------------------------------------------------- |
| Rotate the extra cent between people | It needs stored state and the same expense would split differently over time. |
| Give it to whoever is typed first    | The result would depend on typing order rather than on who paid.              |

## Consequences

**Benefits:** the same expense always splits the same way.

**Costs and obligations:** every kind of split needs to know who paid.

**Revisit when:** users ask for a different fairness rule.

## Current Sources

- [Architecture](../documents/architecture.md#important-flows): the payer-first order when an expense is added.
- [Expense scenarios](../documents/expense-scenarios.md): the exact shares when cents are left over.
