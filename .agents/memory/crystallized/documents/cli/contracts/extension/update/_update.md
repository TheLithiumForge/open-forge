---
open-forge:
  description: Route the accepted Extension trusted-managed update contracts for force replacement, prune, and automatic safe reconciliation
  responsibility: Route Extension update Interface and Behavior without adding package-manager or ownership meaning
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, Update, Lifecycle, Ownership, CurrentTruth]
---

# extension update Command

## Status And Authority

This entrypoint routes the accepted current Crystallized Interface and Behavior
contracts for `open-forge extension update`. The new CLI does not ship yet.
Update reconciles trusted managed package IDs with one explicitly selected
current source. It is separate from install because source intent and managed
reconciliation are different operations.

## Contract Roles

- [`interface.md`](interface.md) defines exact source and ID selection, `--all`,
  force/prune/automatic modes, effects, results, errors, examples, and public
  conformance.
- [`behavior.md`](behavior.md) defines trusted lifecycle and Framework-anchor
  gating, dependency closure, baseline/current/intended planning, ownership,
  fingerprints, generated navigation, recovery, and conformance.
- No Technical Design file exists. The accepted CLI Architecture defines the
  shared implementation boundary. Gate 5 must prove source-generated YamlDotNet
  and STJ serialization, fixed Markdig where used, real `System.IO`, Native AOT,
  OS locking, isolated tests, and package journeys. This contract does not claim
  that implementation or proof.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->
- [Accepted technology-neutral Extension update behavior for trusted source reconciliation, force, prune, shared owners, and recovery](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Extension #Update #Behavior #Dependency #Ownership #Safety #Recovery #CurrentTruth
- [Accepted Interface for reconciling trusted Extension IDs from one exact source with force and prune boundaries](interface.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Extension #Update #Interface #Lifecycle #Ownership #Dependency #Safety #CurrentTruth
<!-- open-forge:generated-index:end -->
