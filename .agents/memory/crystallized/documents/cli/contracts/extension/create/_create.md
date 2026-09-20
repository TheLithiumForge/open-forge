---
open-forge:
  description: Route the accepted local Extension catalogue-scaffold creation contracts
  responsibility: Route the Extension create Interface and Behavior without conflating catalogue authoring with workspace installation
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Extension, Create, Catalogue, Mutation, CurrentTruth]
---

# extension create Command

## Status And Authority

This entrypoint routes the accepted current Crystallized Interface and Behavior
contracts for `open-forge extension create`. The new CLI does not ship yet.
Create authors a package scaffold in a catalogue and does not install it into a
workspace or write lifecycle state.

## Contract Roles

- [`interface.md`](interface.md) defines exact ID, destination and manifest
  authoring flags, deterministic defaults, global no-op, wizard/direct behavior,
  scaffold, output, statuses, errors, examples, and public conformance.
- [`behavior.md`](behavior.md) defines deterministic catalogue-parent and
  destination resolution, one scaffold plan, create-only verification, and
  conformance.
- No Technical Design file exists. The accepted CLI Architecture defines the
  shared implementation boundary. Gate 5 must prove source-generated YamlDotNet
  and STJ serialization, fixed Markdig where used, real `System.IO`, Native AOT,
  OS locking, isolated tests, and package journeys. This contract does not claim
  that implementation or proof.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Accepted technology-neutral behavior for Extension catalogue scaffold planning, application, and verification](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Extension #Create #Behavior #Catalogue #Mutation #Safety #CurrentTruth
- [Accepted Interface for creating a local Extension scaffold in a catalogue without installing it](interface.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Extension #Create #Interface #Catalogue #Mutation #CurrentTruth
