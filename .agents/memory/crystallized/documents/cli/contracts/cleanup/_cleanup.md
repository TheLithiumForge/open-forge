---
open-forge:
  description: Route the accepted non-shipping root cleanup contracts for recognized Open Forge transient and recovery artifacts
  responsibility: Route cleanup's public Interface and technology-neutral Behavior without adding artifact or implementation detail
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Cleanup, Mutation, Recovery, Safety, CurrentTruth]
---

# Cleanup Command Contract Set

## Routing Boundary

This is a routing-only contract-set entrypoint for the direct root `cleanup`
command. It exposes the local Interface and Behavior Contracts for one
operand-free cleanup operation. It does not define a wizard, an artifact
selection surface, or a Technical Design.

## Status And Authority

This is the accepted current Crystallized contract set for the non-shipping root
`cleanup` operation. The local Interface Contract defines what a caller may
enter and observe. The local Behavior Contract defines deterministic catalogue
formation, deletion, safety, recovery, and conformance. The new CLI does not
ship yet; implementation and executable proof remain pending Gate 5.

## Contract Roles

- [`interface.md`](interface.md) defines the complete public syntax, recognized
  catalogue boundary, observable effects, statuses, output, errors, examples,
  non-goals, and public verification.
- [`behavior.md`](behavior.md) defines technology-neutral current-fact
  resolution, complete planning, preflight, deletion, verification, the narrow
  monotonic recovery exception, result formation, and conformance.
- No command-local Technical Design file is needed. The [CLI
  Architecture](../../architecture.md) defines the accepted shared structured
  result, process-status, source, package, runtime, filesystem, and recovery
  realization. This contract remains the authority for cleanup's observable
  meaning.

The [historical Decision Agenda](../../../../../archived/cli-release/decision-agenda-2026-08-21.md) records the accepted D017A
direction. The detailed local contracts remain the authorities for cleanup's
public and technology-neutral meanings.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->
- [Accepted technology-neutral catalogue, deletion, safety, monotonic recovery, and conformance for cleanup](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Cleanup #Behavior #Mutation #Recovery #Safety #Determinism #CurrentTruth
- [Accepted non-shipping public interface for operand-free cleanup of recognized Open Forge transient and recovery artifacts](interface.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Cleanup #Interface #Mutation #Recovery #Safety #CurrentTruth
<!-- open-forge:generated-index:end -->
