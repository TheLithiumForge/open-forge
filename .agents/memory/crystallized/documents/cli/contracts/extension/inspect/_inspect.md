---
open-forge:
  description: Route the accepted read-only Extension inspect contracts for one stable package identity
  responsibility: Route the Extension inspect Interface and Behavior without adding lifecycle or mutation authority
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, Inspect, Interface, Behavior, ReadOnly, CurrentTruth]
---

# extension inspect Command

## Status And Authority

This entrypoint routes the accepted current Crystallized Interface and Behavior
contracts for read-only `open-forge extension inspect`. The command does not
ship yet and never mutates a workspace, package source, lifecycle document, or
generated navigation.

## Contract Roles

- [`interface.md`](interface.md) defines the exact stable-ID grammar, source
  selection, comparison projections, statuses, output, errors, examples, and
  public conformance.
- [`behavior.md`](behavior.md) defines deterministic installed/source fact
  resolution, dependency closure, trust and source-unavailable handling, result
  formation, and read-only conformance.
- No Technical Design file exists. The accepted CLI Architecture defines the
  shared implementation boundary. Gate 5 must prove source-generated YamlDotNet
  and STJ serialization, fixed Markdig where used, real `System.IO`, Native AOT,
  OS locking, isolated tests, and package journeys. This contract does not claim
  that implementation or proof.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->
- [Accepted technology-neutral behavior for read-only Extension identity inspection and source-unavailable comparisons](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Extension #Inspect #Behavior #ReadOnly #Determinism #Lifecycle #CurrentTruth
- [Accepted read-only Interface for inspecting one Extension ID across installed, available, and three-way facts](interface.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Extension #Inspect #Interface #ReadOnly #Source #Lifecycle #CurrentTruth
<!-- open-forge:generated-index:end -->
