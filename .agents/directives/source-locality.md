---
open-forge:
  description: Keep behavior and its directly related source, contracts, tests, fixtures, and support together at the narrowest useful scope
  tags: [LoadNow, Core, Directive, Source, Locality, Contract, Testing, Structure]
---

# Source Locality

## Instructions

- Locality is binding. Keep one behavior and the source, contracts, tests,
  fixtures, and focused support that directly describe or verify it together in
  the narrowest useful subject, feature, command, or component scope.
- Do not separate related files merely because they have different artifact
  types, such as interface, behavior, implementation, or test. Split files by a
  clear responsibility while preserving their shared local scope.
- When a language or build system makes separate test projects or roots
  materially cleaner, tests may live in a separate physical tree that mirrors
  the production feature or capability paths one-to-one. Keep fixtures and
  support at the nearest mirrored scope. Place complete system tests at the
  system boundary. Do not link test files into production folders merely to
  simulate locality.
- Promote a capability to a shared parent only after multiple real consumers
  need the same meaning. Use the
  [Nearest Shared Scope](../patterns/software/source-locality/nearest-shared-scope.md)
  Pattern for that placement.
- A selected language or product scope may require an explicit support folder at
  the narrowest owner even before promotion. The replacement CLI uses this
  specialization through its C# and CLI Directives. That folder communicates the
  owner and support boundary; it does not by itself prove wider reuse or permit
  promotion.
- Preserve authority and lifecycle boundaries. Locality keeps related material
  easy to find; it does not merge current, candidate, generated, or historical
  meaning.
