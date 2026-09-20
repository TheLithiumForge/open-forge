---
open-forge:
  description: Permanent route for the shared operation-specific source-universe filter contracts
  responsibility: Route the shared Interface and Behavior contracts for repeatable source-universe selection
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Shared, Source, Universe, Filter, CurrentTruth]
---

# Shared Source-Universe Filters

## Status And Authority

This is the permanent Crystallized route for the shared source-universe filter
contract set. The command contracts that explicitly apply it remain
non-shipping. Implementation and executable evidence are tracked in
[CLI Development](../../../../../../working/cli-development/_cli-development.md). The
accepted implementation boundaries are defined by the [CLI
Architecture](../../../architecture.md), while this route remains
technology-neutral.

`--include` and `--exclude` are not global flags. They are reusable,
operation-specific source-universe selection flags. A consuming command must
explicitly declare their applicability; an unrelated command rejects them as
unknown rather than accepting them as no-ops.

The [Interface Contract](interface.md) owns the caller-visible spellings,
selection, expansion, identity, safety, error, and selector-reporting meaning.
The [Behavior Contract](behavior.md) owns technology-neutral resolution,
expansion, set formation, safety, determinism, and conformance. The shared
[CLI Source References](../source-references/interface.md) contracts remain
the related authority for the reusable source-reference vocabulary.

## Contract Roles

- [`interface.md`](interface.md) defines what an applicable consumer accepts
  and what shared selector facts it must preserve for reporting.
- [`behavior.md`](behavior.md) defines how a conforming consumer resolves,
  expands, composes, and verifies the effective logical source set without
  selecting an implementation library or module.

## Scope Boundary

The shared contract does not define one universal default source universe,
which commands apply the flags, a universal coverage rule, or an output-row
schema. Each consuming command owns those decisions and links here for the
shared filter meaning.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Technology-neutral resolution, expansion, set formation, safety, determinism, and conformance for shared source-universe filters](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Shared #Source #Universe #Filter #Behavior #Determinism #CurrentTruth
- [Shared caller-visible contract for operation-specific source-universe selection](interface.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Shared #Source #Universe #Filter #Interface #CurrentTruth
