---
open-forge:
  description: Implement route-list topology, ordering, depth, coverage, status, and next-action formation
  tags: [Memory, Working, CLI, Task, Route, List, Topology, Result, Contextual, Complete]
---

# Implement Route-List Topology And Result Formation

## Task State

- State: Complete.
- Implementer: Mastermind.
- Parent: [Route Discovery](../_route-discovery.md).

## Expected Outcome

Safe inventory becomes an immutable route graph, deterministic selected rows,
complete coverage facts, one semantic status, and exact next action without
filesystem access during projection.

## Source Placement And Components

Create `List/Shared/Topology/`:

- `RouteListRouteGraphBuilder`: creates nodes and parent/child relationships from
  recognized contained sources.
- `RouteListTopologyFacts`: immutable roots, rows, invalid relationships, and
  provenance.
- `RouteListTopologySelector`: applies source selection and depth without
  reparsing identity.
- `RouteListCoverageBuilder`: records requested boundary, effective boundary,
  confirmed inventory, omissions, and cancellation.
- `RouteListResultBuilder`: translates topology, selection, findings, and coverage
  into one valid concrete result.

Create `List/Shared/Ordering/RouteListOrdering` only when multiple cohesive
ordering consumers exist; otherwise keep ordering beside topology.

## Required Behavior

- Parent-before-child ordering with deterministic ordinal tie-breakers.
- Depth 0, 1, finite values, and all follow the public contract exactly.
- Selected explicit source and default Loader roots preserve actual identity and
  provenance.
- Orphans, duplicate IDs, ambiguous parents, invalid overwrites, and known
  inventory findings remain typed and deterministically ordered.
- Interrupted results retain safe rows and known findings but never complete
  coverage.
- Status precedence follows the route-list contract, not an exception or renderer.
- Next action appears only when contract-required and names one safe operation.

## Evidence

Pure fixed-fact Unit tests cover graph states, depth boundaries, ordering,
selection parity, coverage, every status, cancellation, finding order, next
actions, and result-construction invariants. Integration proves the same facts
from real inventory without testing presentation text.

## Accepted Evidence

- Immutable graph facts preserve authored parentage independently from Loader
  selection, including nested Loader roots, detached trees, ambiguous parents,
  routed native sources, and source-ID collisions across the complete inventory.
- Selection applies exact finite and `all` depth, parent-first ordering, relative
  and absolute depth, overwrite provenance, relevant finding boundaries,
  cancellation retention, and deterministic status precedence without filesystem
  access.
- Coverage and result builders enforce selected-root, row-count, finding,
  next-action, and all seven semantic-status invariants.
- The warning-free Release solution build, 243 Unit cases, and 80 Integration
  cases pass. Existing command-free EndToEnd evidence remains unchanged.
- Public process and Native AOT route-list evidence remain intentionally deferred
  to Presentation and Acceptance because the root still exposes no partial
  command.

## Review Record

- Bounded correctness review found Loader re-rooting, incomplete source-ID
  collision scope, overwrite-depth handling, parser use inside topology, and an
  order-sensitive root-set check. The implementation now preserves authored
  parentage, consumes full-inventory collision facts, maps overwrite findings to
  logical source depth, accepts typed selection facts without parsing, and uses
  immutable set equality.
- Local improvement review found duplicate finalization, a mixed row-building
  responsibility, and missing focused policy evidence. Finalization now has one
  owner, row formation has a cohesive local builder, and direct next-action,
  status-precedence, nested-root, and invariant cases protect the result.
- The strongest simpler alternative was to treat every Loader declaration as a
  parentless graph root. That would make selection easier but would let generated
  Loader navigation replace authored parent and absolute-depth facts, contrary to
  the route-list contract.
- After the maintainer selected a lean implementer-plus-Mastermind review model,
  the Mastermind inspected the corrected integration and accepted the focused
  build and test evidence without another broad review cycle.

## Protected Boundaries

- No parser, filesystem read, help, rendering, writer, logging, or shared generic
  graph engine.

## Stop Conditions

Stop if route topology semantics would be promoted before route inspect becomes a
real second consumer, or if a result state cannot be represented by frozen models.
