---
open-forge:
  description: Route the accepted Extension ownership-release and bounded removal contracts
  responsibility: Route Extension remove Interface and Behavior without adding arbitrary cleanup or Framework removal authority
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, Remove, Ownership, Prune, CurrentTruth]
---

# extension remove Command

## Status And Authority

This entrypoint routes the accepted current Crystallized Interface and Behavior
contracts for `open-forge extension remove`. The new CLI does not ship yet.
Remove is an explicit package-ownership release operation. It does not remove a
package source and has no Framework equivalent.

## Contract Roles

- [`interface.md`](interface.md) defines exact managed-ID selection, human
  Keep-as-unmanaged/Delete choice, `--prune`, automatic behavior, effects,
  statuses, output, errors, examples, and public conformance.
- [`behavior.md`](behavior.md) defines trusted ownership and route resolution,
  dependency and shared-owner checks, removal planning, generated navigation,
  recovery, verification, and conformance.
- No Technical Design file exists. The accepted CLI Architecture defines the
  shared implementation boundary. Gate 5 must prove source-generated YamlDotNet
  and STJ serialization, fixed Markdig where used, real `System.IO`, Native AOT,
  OS locking, isolated tests, and package journeys. This contract does not claim
  that implementation or proof.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->
- [Accepted technology-neutral Extension removal behavior for ownership release, shared owners, same-request prune, and recovery](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Extension #Remove #Behavior #Ownership #Prune #Safety #Recovery #CurrentTruth
- [Accepted Interface for releasing selected Extension ownership with safe final-owner removal and same-request prune](interface.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Extension #Remove #Interface #Ownership #Prune #Safety #CurrentTruth
<!-- open-forge:generated-index:end -->
