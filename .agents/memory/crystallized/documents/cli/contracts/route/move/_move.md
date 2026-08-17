---
open-forge:
  description: Routing-only entrypoint for the accepted current `route move` contract
  responsibility: Route the `route move` Interface and Behavior Contracts without defining their command meaning
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Route, Move, Mutation, Reference, CurrentTruth]
---

# route move Command Contract Set

## Status And Authority

This is the accepted current Crystallized entrypoint for `route move`.
The command does not ship yet; implementation and executable proof remain
pending Gate 5. Its sibling [Interface Contract](interface.md) defines what a
caller may enter and observe. Its sibling [Behavior Contract](behavior.md)
defines the deterministic, technology-neutral operation behind that interface.
This entrypoint does not duplicate either contract and no Route Technical Design
is needed for this operation.

The command accepts one eligible ordinary unmanaged logical leaf or one eligible
ordinary unmanaged category. A category is selected through one recognized
entrypoint source reference and is processed as one complete operation, not as a
batch of leaf moves.

## Contract Roles

- [`interface.md`](interface.md) is the caller-visible Interface Contract.
- [`behavior.md`](behavior.md) is the technology-neutral Behavior Contract
  behind that interface.
- The shared [Global CLI Flags](../../shared/global-flags/interface.md), [CLI
  Source References](../../shared/source-references/interface.md), and [Index
  contracts](../../index/interface.md) retain authority for the
  shared meanings consumed by this operation.

Generated `Entries` provide navigation and help routing only. They are not a
second source of command meaning or selected-category authority.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->
- [Accepted current technology-neutral resolution, effects, safety, recovery, and conformance for `route move`](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Route #Move #Behavior #Mutation #Reference #Safety #Recovery #CurrentTruth
- [Accepted current public interface for moving one eligible unmanaged routed leaf or complete routed category](interface.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Route #Move #Interface #Mutation #Reference #Safety #CurrentTruth
<!-- open-forge:generated-index:end -->
