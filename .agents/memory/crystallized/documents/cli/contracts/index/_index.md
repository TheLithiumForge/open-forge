---
open-forge:
  description: Route the current Crystallized non-shipping `index` contracts for its public interface, deterministic behavior, and accepted technical design
  responsibility: Route the current `index` contracts at the final `contracts/index/` scope while keeping the command non-shipping
  tags: [Memory, Crystallized, CLI, Release, Command, Index, Contract, CurrentTruth]
---

# Index Command Contract Set

## Status

This is the current Crystallized contract set for the non-shipping `index`
command at the final `contracts/index/` scope. Its Interface and Behavior files
define the public and technology-neutral contracts. Its Technical Design records
the accepted command-local realization under the contracts and the accepted
Architecture. The command does not ship.

The public command identity is `index`, and the complete contract set is located
under `contracts/index/`. A recognized `_index.md` entrypoint is canonicalized by
physical identity and processed exactly once, even when traversal reaches that
physical file more than once. This is a conformance requirement, not a staging
or migration rule.

The sibling contracts separate the public interface, technology-neutral
behavior, and accepted technical design. The technical design cannot change the
Interface or Behavior contracts.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->
- [Current Crystallized deterministic behavior, mutation lifecycle, safety, recovery, and conformance for `index`](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Index #Behavior #CurrentTruth
- [Current Crystallized public interface for rebuilding bounded generated `Entries` from routed topology and authored metadata](interface.md) - #Memory #Crystallized #CLI #Release #Command #Index #Interface #CurrentTruth
- [Current accepted technical design for the non-shipping `index` command](technical-design.md) - #Memory #Crystallized #CLI #Release #Command #Index #TechnicalDesign #CurrentTruth
<!-- open-forge:generated-index:end -->
