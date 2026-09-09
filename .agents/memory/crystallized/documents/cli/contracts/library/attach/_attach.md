---
open-forge:
  description: Route the public attach contract for registering one contained Workspace Library and projecting its complete eligible inventory
  responsibility: Route the attach Interface and Behavior Contracts without adding library lifecycle or filesystem design
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Library, Attach, Mutation, Recovery, Safety, CurrentTruth]
---

# library attach Contract Set

## Routing Boundary

This is a routing-only contract-set entrypoint for the `library attach`
operation. It exposes the local Interface and Behavior Contracts for registering
one new consumer-local library and creating its declared projections. It does
not define `library list`, `library inspect`, `library sync`, or `library detach`,
and it does not define a command-local Technical Design.

The shared [Workspace Libraries Technical Design](../../../technical-designs/workspace-libraries.md)
and [Mutation And Recovery Technical Design](../../../technical-designs/mutation-and-recovery.md)
define the accepted cross-cutting realization boundaries. This route keeps the
public and technology-neutral attach meaning together without choosing
callables or implementation structure.

## Status And Authority

This is the current Crystallized contract set for the public
`open-forge library attach` operation. The local Interface Contract owns the
syntax, input grammar, observable effects, record shape, results, examples,
non-goals, and public verification. The local Behavior Contract owns
technology-neutral fact formation, complete inventory, collision checks,
planning, preflight, application order, verification, recovery, and
conformance.

The shared [Global CLI Flags](../../shared/global-flags/interface.md),
[Shared Result Coordinates](../../shared/result-coordinates/interface.md), and
[Shared CLI Operation Contract](../../../shared-operation-contract.md) retain
their shared meanings. The [Index Behavior Contract](../../index-candidate/behavior.md)
remains authoritative for any existing generated `Entries` region that this
operation is allowed to project. The replacement CLI does not ship yet;
the active Task records implementation and executable evidence.

## Contract Roles

- [`interface.md`](interface.md) defines the complete public syntax, operand
  grammar, source-root boundary, collision rule, record schema, effects,
  statuses, output, errors, examples, non-goals, and exactly three public
  EndToEnd journeys.
- [`behavior.md`](behavior.md) defines deterministic current-fact resolution,
  complete source inventory, mapping and generated-region planning, preflight,
  dry-run, lease-bound application, typed recovery, verification, residual
  truth, and conformance.
- The linked shared technical designs define mutation and filesystem
  realization; this contract set does not add callables, dependency injection,
  a registry, JavaScript, or another implementation boundary.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->

- [Define the technology-neutral inventory, planning, application, verification, and recovery behavior for `library attach`](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Library #Attach #Behavior #Mutation #Recovery #Safety #Determinism #CurrentTruth
- [Define the exact public syntax, source boundary, collision policy, projections, record, and results for `library attach`](interface.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Library #Attach #Interface #Mutation #Recovery #Safety #CurrentTruth

<!-- open-forge:generated-index:end -->
