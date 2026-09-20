---
open-forge:
  description: Route the public sync contract for reconciling one registered Workspace Library with its complete current inventory
  responsibility: Route the sync Interface and Behavior Contracts without adding library lifecycle or filesystem design
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Library, Sync, Mutation, Recovery, Safety, CurrentTruth]
---

# library sync Contract Set

## Routing Boundary

This is a routing-only contract-set entrypoint for the `library sync`
operation. It exposes the local Interface and Behavior Contracts for
reconciling one registered library's projections with one complete current
source inventory. It does not define `library list`, `library inspect`,
`library attach`, or `library detach`, and it does not define a command-local
Technical Design.

The shared [Workspace Libraries Technical Design](../../../technical-designs/workspace-libraries.md)
and [Mutation And Recovery Technical Design](../../../technical-designs/mutation-and-recovery.md)
define accepted cross-cutting realization boundaries. This route keeps sync's
public and technology-neutral meaning together without choosing callables or
implementation structure.

## Status And Authority

This is the current Crystallized contract set for the public
`open-forge library sync` operation. The local Interface Contract owns the
syntax, identity and source-availability boundary, observable reconciliation,
record shape, results, output, errors, examples, non-goals, and exactly three
public EndToEnd journeys. The local Behavior Contract owns deterministic
current-fact resolution, complete inventory, set reconciliation, collision
checks, planning, preflight, application order, verification, recovery,
residual truth, and conformance.

The shared [Global CLI Flags](../../shared/global-flags/interface.md),
[Shared Result Coordinates](../../shared/result-coordinates/interface.md), and
[Shared CLI Operation Contract](../../../shared-operation-contract.md) retain
their shared meanings. The [Index Behavior Contract](../../index-candidate/behavior.md)
remains authoritative for any existing generated `Entries` region that this
operation is allowed to project. The merged native CLI is the current delivery;
the active Task records implementation and executable evidence.

## Contract Roles

- [`interface.md`](interface.md) defines the complete public syntax, operand
  grammar, source and record availability, reconciliation effects, statuses,
  output, errors, examples, non-goals, and exactly three public EndToEnd
  journeys.
- [`behavior.md`](behavior.md) defines deterministic complete inventory,
  current-versus-registered path reconciliation, collision policy, generated
  projection, preflight, dry-run, lease-bound application, typed recovery,
  verification, residual truth, and conformance.
- The linked shared technical designs define mutation and filesystem
  realization; this contract set does not add callables, dependency injection,
  a registry, JavaScript, or another implementation boundary.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Define the technology-neutral inventory, reconciliation, planning, application, verification, and recovery behavior for `library sync`](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Library #Sync #Behavior #Mutation #Recovery #Safety #Determinism #CurrentTruth
- [Define the exact public syntax, complete-inventory requirement, reconciliation effects, record, and results for `library sync`](interface.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Library #Sync #Interface #Mutation #Recovery #Safety #CurrentTruth
