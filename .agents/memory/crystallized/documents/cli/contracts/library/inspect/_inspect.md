---
open-forge:
  description: Route the accepted read-only Library inspect contracts for one exact library ID
  responsibility: Route Library inspect Interface and Behavior without adding mutation or source-reference authority
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Library, Inspect, Interface, Behavior, ReadOnly, CurrentTruth]
---

# library inspect Command

## Status And Authority

This entrypoint routes the accepted current Crystallized Interface and Behavior
Contracts for read-only `open-forge library inspect`. Inspect explains one
record, inventories its source root's complete eligible ordinary-file set, and
compares that inventory with the exact registered projection.

The command does not ship yet. It is stateless and read-only: it does not
acquire a lock, create recovery state, write a record or link, or invoke a
mutation command.

The [Workspace Libraries Technical Design](../../../technical-designs/workspace-libraries.md)
defines the shared realization boundary. The [Library group entrypoint](../_library.md)
defines group help and the exact command order only.

## Contract Roles

- [`interface.md`](interface.md) defines the exact Library-ID grammar, strict
  record subject, complete inventory result, comparison relations, statuses,
  output, errors, examples, and exactly three public EndToEnd journeys.
- [`behavior.md`](behavior.md) defines technology-neutral subject resolution,
  eligible inventory, exact comparison, result formation, safety, and lower-tier
  conformance.

## Axioms

- inherited - No local axioms; loaded ancestor entrypoints remain active.

## Entries

<!-- open-forge:generated-index:start -->

- [Accepted technology-neutral behavior for complete Library inventory and exact projection comparison](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Library #Inspect #Behavior #ReadOnly #Determinism #CurrentTruth
- [Accepted public interface for complete Library inventory and exact projection comparison](interface.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Library #Inspect #Interface #ReadOnly #Workspace #CurrentTruth

<!-- open-forge:generated-index:end -->
