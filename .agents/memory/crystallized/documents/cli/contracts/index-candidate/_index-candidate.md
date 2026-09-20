---
open-forge:
  description: Route the current Crystallized non-shipping `index` contracts for its public interface, deterministic behavior, and accepted technical design
  responsibility: Route the current `index` contracts in candidate `contracts/index-candidate/` staging while keeping the command non-shipping
  tags: [Memory, Crystallized, CLI, Release, Command, Index, Contract, CurrentTruth]
---

# Index Command Contract Set

## Status

This is the current Crystallized contract set for the non-shipping `index`
command, currently staged under `contracts/index-candidate/`. Its Interface and
Behavior files define the public and technology-neutral contracts. Its Technical
Design records the accepted command-local realization under the contracts and the
accepted Architecture. The command does not ship. Candidate staging remains
until a separate accepted route migration moves the contracts to their final
compatibility-name paths.

The public command identity is `index`, and the candidate contract set is located
under `contracts/index-candidate/`. The candidate `_index-candidate.md`
entrypoint remains staged even though the replacement `index` command has proved
the required physical-identity behavior for the final compatibility-name path
`contracts/index/_index.md`. That final compatibility entrypoint is
canonicalized by physical identity and processed exactly once, even when
traversal reaches that physical file more than once. This is a conformance
requirement, not authorization for the separate route migration.

The sibling contracts separate the public interface, technology-neutral
behavior, and accepted technical design. The technical design cannot change the
Interface or Behavior contracts.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Current Crystallized deterministic behavior, mutation lifecycle, safety, recovery, and conformance for `index`](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Index #Behavior #CurrentTruth
- [Current Crystallized public interface for rebuilding bounded generated `Entries` from routed topology and authored metadata](interface.md) - #Memory #Crystallized #CLI #Release #Command #Index #Interface #CurrentTruth
- [Current accepted technical design for the non-shipping `index` command](technical-design.md) - #Memory #Crystallized #CLI #Release #Command #Index #TechnicalDesign #CurrentTruth
