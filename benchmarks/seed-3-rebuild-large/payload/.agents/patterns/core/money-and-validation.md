---
open-forge:
  description: Money type, parsing, and validation live in core and nowhere else
  tags: [Extension, Pattern, Core, Money, Validation]
---

# Money And Validation In Core

## Shape

- One module owns the money type (integer minor units per the money decision) with `parseAmount(string)`, `formatAmount(minor, currency)`, and `sum(amounts)`. Nothing outside core constructs money values.
- One module owns transaction validation: shape, id format, date format, category rules. Server and importer call it; they never re-implement field checks locally.
- Core has zero I/O and zero knowledge of HTTP; its functions take and return plain data.

## Why

The MVP fixed rounding in one of two math sites and drifted. A single owner per rule makes drift impossible by construction.
