---
open-forge:
  description: Route the accepted current `find` contracts for deterministic source inventory and typed tag or heading discovery
  responsibility: Route the accepted current `find` contracts while keeping the command non-shipping
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Find, Discovery, Tag, Heading, CurrentTruth]
---

# Find Command Contract Set

## Status And Authority

This folder is the routed Crystallized contract set for the accepted current
non-shipping `find` command. Its Interface and Behavior files are the current
authorities for public and technology-neutral meaning. Its Technical Design
records the accepted implementation design and remains subordinate to those
contracts and the [CLI Architecture](../../architecture.md). The command does not
ship.

## Contract Roles

The sibling files answer separate questions:

- [`interface.md`](interface.md) defines the public command surface and every
  observable result.
- [`behavior.md`](behavior.md) defines deterministic semantics, coverage,
  result formation, and the read-only guarantee without selecting technology.
- [`technical-design.md`](technical-design.md) records accepted .NET Native AOT
  implementation choices and evidence boundaries. It cannot redefine the other
  two contracts.

The shared [Global CLI Flags Interface Contract](../shared/global-flags/interface.md),
[CLI Source References Interface Contract](../shared/source-references/interface.md),
and [Shared Source-Universe Filters Interface Contract](../shared/source-universe-filters/interface.md)
with its related [Behavior Contract](../shared/source-universe-filters/behavior.md)
remain in the shared contract scope. Find explicitly applies the shared source-universe
filter contract for `--include` and `--exclude`; those flags are not global, and
unrelated commands reject them as unknown. The find contracts link to these
shared contracts rather than copying their full definitions.

## Entries

- [Accepted current technology-neutral operation flow, coverage mechanics, and conformance for `find`](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Find #Behavior #Discovery #Determinism #Coverage #CurrentTruth
- [Complete accepted current public surface and observable result for deterministic `find` discovery](interface.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Find #Interface #Discovery #Tag #Heading #Filter #Projection #CurrentTruth
- [Accepted current implementation design for deterministic `find` discovery](technical-design.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Find #TechnicalDesign #Implementation #Traceability #CurrentTruth
