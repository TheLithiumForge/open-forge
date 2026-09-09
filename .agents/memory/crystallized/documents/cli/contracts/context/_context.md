---
open-forge:
  description: Current Crystallized contract set for stateless ordered context resolution, projection, and explicit link expansion
  responsibility: Route the current Context Interface, Behavior, and subordinate Technical Design
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Context, CurrentTruth]
---

# Context Command Contract Set

## Status And Authority

This routed contract set is the current Crystallized authority for the accepted
`context` meaning. The command does not ship yet; implementation and executable evidence are tracked in
[CLI Development](../../../../../working/cli-development/_cli-development.md). The sibling Interface and Behavior contracts define
the public and technology-neutral meaning. The Technical Design is subordinate
to the [CLI Architecture](../../architecture.md) and records the accepted
context-specific realization without redefining either contract.

## Contract Roles

The sibling files answer separate questions:

- [`interface.md`](interface.md) defines the complete caller-visible command
  surface and every observable result.
- [`behavior.md`](behavior.md) defines deterministic technology-neutral closure,
  graph, projection, ordering, completeness, result, read-only safety, and
  conformance mechanics without selecting implementation technology.
- [`technical-design.md`](technical-design.md) records the accepted
  context-specific realization subordinate to the CLI Architecture. It cannot
  redefine the Interface or Behavior contracts.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->
- [Current technology-neutral closure, graph, projection, result, safety, and conformance behavior for `context`](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Context #Behavior #Determinism #Graph #Projection #CurrentTruth
- [Current public interface and observable result for stateless ordered `context` resolution, projection, and explicit link expansion](interface.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Context #Interface #Route #Scope #Projection #Graph #CurrentTruth
- [Accepted context implementation design subordinate to the current CLI Architecture](technical-design.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Context #TechnicalDesign #Implementation #CurrentTruth
<!-- open-forge:generated-index:end -->
