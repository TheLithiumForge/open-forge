---
open-forge:
  description: Routing-only entrypoint for the accepted `route init` command contracts
  responsibility: Route `route init` help to its Interface and Behavior Contracts
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Route, Init, Entrypoint, CurrentTruth]
---

# route init Command Contract Set

## Status And Authority

This is the accepted current Crystallized entrypoint for `route init`. The
command does not ship yet; implementation and executable proof remain pending
Gate 5. Its sibling Interface and Behavior Contracts define command meaning.
This entrypoint does not duplicate their detail.

## Contract Roles

- [`interface.md`](interface.md) is the caller-visible Interface Contract.
- [`behavior.md`](behavior.md) is the technology-neutral Behavior Contract
  behind that interface.
- The shared [Global CLI Flags Interface Contract](../../shared/global-flags/interface.md), [CLI Source References Interface Contract](../../shared/source-references/interface.md),
  and the [Index Interface Contract](../../index/interface.md) remain
  authoritative at their own scopes. These contracts are linked here instead of
  copied in full.
- The linked Framework routing and Markdown sources remain authoritative for
  Framework meaning. This set does not replace those sources.

This set contains no command-local Technical Design file. The two sibling
contracts define the accepted current command contract without selecting
implementation technology.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->
- [Accepted current technology-neutral resolution, planning, effects, safety, recovery, and conformance for `route init`](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Route #Init #Entrypoint #Behavior #CurrentTruth
- [Accepted current public interface and observable result for recursively initializing missing route entrypoints](interface.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Route #Init #Entrypoint #Interface #CurrentTruth
<!-- open-forge:generated-index:end -->
