---
open-forge:
  description: Route the public detach contract for removing one complete registered Workspace Library projection without touching its source
  responsibility: Route the detach Interface and Behavior Contracts without adding library lifecycle or filesystem design
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Library, Detach, Mutation, Recovery, Safety, CurrentTruth]
---

# library detach Contract Set

## Routing Boundary

This is a routing-only contract-set entrypoint for the `library detach`
operation. It exposes the local Interface and Behavior Contracts for removing
one complete registered library projection and its consumer record. It does
not define `library list`, `library inspect`, `library attach`, or `library
sync`, and it does not define a command-local Technical Design.

The shared [Workspace Libraries Technical Design](../../../technical-designs/workspace-libraries.md)
and [Mutation And Recovery Technical Design](../../../technical-designs/mutation-and-recovery.md)
define accepted cross-cutting realization boundaries. This route keeps
detach's public and technology-neutral meaning together without choosing
callables or implementation structure.

## Status And Authority

This is the current Crystallized contract set for the public
`open-forge library detach` operation. The local Interface Contract owns the
syntax, identity and source-independence boundary, observable effects, record
shape, results, output, errors, examples, non-goals, and exactly three public
EndToEnd journeys. The local Behavior Contract owns deterministic record and
mapping resolution, all-or-nothing preflight, generated-region planning,
application order, verification, typed recovery, residual truth, and
conformance.

The shared [Global CLI Flags](../../shared/global-flags/interface.md),
[Shared Result Coordinates](../../shared/result-coordinates/interface.md), and
[Shared CLI Operation Contract](../../../shared-operation-contract.md) retain
their shared meanings. The [Index Behavior Contract](../../index-candidate/behavior.md)
remains authoritative for any existing generated `Entries` region that this
operation is allowed to project. The merged native CLI is the current delivery;
the active Task records implementation and executable evidence.

## Contract Roles

- [`interface.md`](interface.md) defines the complete public syntax, operand
  grammar, source-independent mapping boundary, record effects, statuses,
  output, errors, examples, non-goals, and exactly three public EndToEnd
  journeys.
- [`behavior.md`](behavior.md) defines deterministic record facts, exact
  registered-link checks, all-or-nothing planning, generated projection,
  preflight, dry-run, lease-bound application, typed recovery, verification,
  residual truth, and conformance.
- The linked shared technical designs define mutation and filesystem
  realization; this contract set does not add callables, dependency injection,
  a registry, JavaScript, or another implementation boundary.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Define the technology-neutral record checks, all-or-nothing planning, application, verification, and recovery behavior for `library detach`](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Library #Detach #Behavior #Mutation #Recovery #Safety #Determinism #CurrentTruth
- [Define the exact public syntax, source-independent link checks, all-or-nothing effects, record, and results for `library detach`](interface.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Library #Detach #Interface #Mutation #Recovery #Safety #CurrentTruth
