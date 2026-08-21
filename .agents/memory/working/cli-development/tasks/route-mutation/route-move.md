---
open-forge:
  description: Implement route move with reference, overwrite, generated-navigation, and recovery integrity
  tags: [Memory, Working, CLI, Task, Route, Move, Mutation, Contextual]
---

# Implement Route Move

## Task State

- State: Planned after Route Update and References.
- Parent: [Route Mutation Commands](_route-mutation.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/route/move/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/route/move/behavior.md).

## Expected Outcome

`route move` relocates one accepted route unit to one validated destination while
preserving route semantics, overwrite pairing, managed references, generated
navigation, lifecycle identity where applicable, and recoverability.

## Architecture

- `RouteMoveObservation` captures exact source, descendants, overwrite, references,
  destination parent, collisions, and lifecycle facts.
- `RouteMovePlan` explicitly orders destination creation, content/reference
  changes, generated projections, source removal, lifecycle update, verification,
  and recovery prerequisites.
- Reuse shared route/reference facts and mutation primitives. Keep move policy,
  reference rewrite eligibility, and effect order local.

## Evidence

Cover leaf and subtree cases allowed by contract, same-parent and cross-scope
destinations, case/Unicode aliases, source/destination physical links, collisions,
overwrite pair, incoming/outgoing references, unchanged external/unmanaged
references, dry run, lock race, partial failure at every effect boundary, recovery,
idempotent rerun, generated navigation, lifecycle, streams, exits, and AOT.

## Stop Conditions

Stop before moving a unit whose complete dependency or recovery boundary cannot
be proved, rewriting unowned references, crossing workspace containment, or
claiming atomic multi-file behavior without evidence.
