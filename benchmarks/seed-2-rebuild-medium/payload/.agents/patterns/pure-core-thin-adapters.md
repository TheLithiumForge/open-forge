---
open-forge:
  description: Keep domain logic in pure modules; keep filesystem, clock, and terminal at the edges
  tags: [Extension, Pattern, Architecture, Testability]
---

# Pure Core, Thin Adapters

## Shape

- Pure core: entry construction, id generation (injected randomness), window resolution (injected "now" and zone), correction resolution, grouping/formatting for reports. No filesystem, clock, or process access.
- Thin adapters: NDJSON file reader/appender, argv parsing, stdout/stderr printing, entry point wiring.

The clock and the local timezone are inputs to the core, never read inside it — the date-handling analysis explains why this is load-bearing here, not stylistic.

## When To Diverge

State the reason when skipping; for this project the window math must stay pure regardless.
