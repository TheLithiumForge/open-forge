---
open-forge:
  description: Route the accepted read-only Library list contracts for bounded record and registered-link observations
  responsibility: Route Library list Interface and Behavior without adding source-inventory or mutation authority
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Library, List, Interface, Behavior, ReadOnly, CurrentTruth]
---

# library list Command

## Status And Authority

This entrypoint routes the accepted current Crystallized Interface and Behavior
Contracts for read-only `open-forge library list`. The command reports only the
consumer-local Library record, each recorded source-root state, and bounded
no-follow observations of registered destination links. It does not enumerate a
source root's eligible files.

`library list` is stateless and read-only. It does not acquire a lock, create
recovery state, write a record or link, or invoke `library inspect`.

The [Workspace Libraries Technical Design](../../../technical-designs/workspace-libraries.md)
defines the shared realization boundary. The [Library group entrypoint](../_library.md)
defines group help and command order only.

## Contract Roles

- [`interface.md`](interface.md) defines the public grammar, strict record
  shape, bounded result, status meanings, output, errors, examples, and exactly
  three public EndToEnd journeys.
- [`behavior.md`](behavior.md) defines technology-neutral record resolution,
  source-root checks, registered-link observation, result formation, safety,
  and lower-tier conformance.

## Axioms

- inherited - No local axioms; loaded ancestor entrypoints remain active.

## Entries

<!-- open-forge:generated-index:start -->
- [Accepted technology-neutral behavior for bounded read-only Library list observations](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Library #List #Behavior #ReadOnly #Determinism #CurrentTruth
- [Accepted public interface for bounded read-only Library list observations](interface.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Library #List #Interface #ReadOnly #CurrentTruth
<!-- open-forge:generated-index:end -->
