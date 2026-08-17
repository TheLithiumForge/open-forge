---
open-forge:
  description: Permanent route for accepted shared CLI contract sets
  responsibility: Route accepted shared contracts while keeping their Interface and Behavior roles separate
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Shared, CurrentTruth]
---

# Shared CLI Contracts

## Status And Authority

This is the permanent Crystallized route for shared CLI contract sets. The
[Global CLI Flags](global-flags/_global-flags.md), [CLI Source References](source-references/_source-references.md),
and [Shared Source-Universe Filters](source-universe-filters/_source-universe-filters.md)
child entrypoints route their accepted current Interface and Behavior contracts.
These contracts define current behavior for the new CLI; they do not ship yet.
Implementation and executable proof remain pending Gate 5. The [CLI
Architecture](../../architecture.md) defines the accepted shared implementation
boundaries; these contracts remain technology-neutral.
The shared source-universe filter contract is a permanent shared contract set,
but it is not a global-flag contract: its flags apply only where a consuming
command explicitly declares them.

`.agents/memory/crystallized/documents/cli/contracts/shared/` is the accepted
permanent location.
Command consumers link directly to an Interface for public meaning and to a
Behavior file for technology-neutral resolution and conformance.

## Contract Roles

- [`global-flags/`](global-flags/_global-flags.md) routes the accepted shared
  global-flag Interface and Behavior files.
- [`source-references/`](source-references/_source-references.md) routes the
  accepted shared source-reference Interface and Behavior files.
- [`source-universe-filters/`](source-universe-filters/_source-universe-filters.md)
  routes the accepted shared operation-specific source-universe filter
  Interface and Behavior files. Its `--include` and `--exclude` flags are not
  global flags; a consumer must explicitly declare their applicability.
- Each Interface file defines its complete caller-visible public contract. Each
  Behavior file defines technology-neutral resolution, invariants, safety, result
  formation, and conformance for that Interface.
- No command-local Technical Design file is created for these shared contract
  sets. Their implementation boundaries are defined by the CLI Architecture.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->
- [Accepted current shared global-flag Interface and Behavior contract set](global-flags/_global-flags.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Global #Flag #CurrentTruth
- [Accepted current shared source-reference Interface and Behavior contract set](source-references/_source-references.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Source #Reference #CurrentTruth
- [Permanent route for the shared operation-specific source-universe filter contracts](source-universe-filters/_source-universe-filters.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Shared #Source #Universe #Filter #CurrentTruth
<!-- open-forge:generated-index:end -->
