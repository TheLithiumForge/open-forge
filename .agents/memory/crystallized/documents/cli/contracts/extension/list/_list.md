---
open-forge:
  description: Route the accepted read-only Extension list contracts for Installed and Available package facts
  responsibility: Route the Extension list Interface and Behavior without adding package or lifecycle authority
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, List, Interface, Behavior, ReadOnly, CurrentTruth]
---

# extension list Command

## Status And Authority

This entrypoint routes the accepted current Crystallized Interface and Behavior
contracts for read-only `open-forge extension list`. The command does not ship
yet and never mutates a workspace, package source, lifecycle document, or
generated navigation.

## Contract Roles

- [`interface.md`](interface.md) defines the exact grammar, source and section
  filters, Installed and Available projections, statuses, output, errors,
  examples, and public conformance.
- [`behavior.md`](behavior.md) defines deterministic source and lifecycle-fact
  resolution, trust handling, result formation, read-only safety, and
  conformance.
- No Technical Design file exists. The accepted CLI Architecture defines the
  shared implementation boundary. Gate 5 must prove source-generated YamlDotNet
  and STJ serialization, fixed Markdig where used, real `System.IO`, Native AOT,
  OS locking, isolated tests, and package journeys. This contract does not claim
  that implementation or proof.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->
- [Accepted technology-neutral behavior for deterministic read-only Extension list facts and lifecycle trust states](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Extension #List #Behavior #ReadOnly #Determinism #Lifecycle #CurrentTruth
- [Accepted read-only Interface for listing installed and available Extension packages from one exact source universe](interface.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Extension #List #Interface #ReadOnly #Source #Lifecycle #CurrentTruth
<!-- open-forge:generated-index:end -->
