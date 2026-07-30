---
open-forge:
  description: Money is integer minor units, never floating point, everywhere
  tags: [Extension, Memory, Decision, Money, CurrentTruth]
---

# Money: Integer Minor Units

## Decision

All amounts are integers in minor units (cents). No floating-point number ever holds a money value — not in parsing, storage, transport, math, or formatting. Parsing user input like `12.30` converts directly from the string to `1230` without an intermediate float.

## Rationale

The MVP's float summary was off by one cent once, and once was enough — a finance tool that is almost right is worthless. Integer cents make every sum exact.

## Consequences

- The core package owns parse/format/sum for money; server and CLI never do money math ad hoc.
- API payloads carry `amountMinor` as a JSON integer plus `currency`.
- Division (if ever needed) must define its rounding rule explicitly; none is needed for this rebuild.
- Transaction date is a `YYYY-MM-DD` string of what the user meant; created-at is a separate UTC instant. Month filtering uses the transaction date only.
