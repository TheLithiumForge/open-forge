---
open-forge:
  description: Accepted current shared source-reference Interface and Behavior contract set
  responsibility: Route the accepted source-reference contract files while keeping their Interface and Behavior roles separate
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Source, Reference, CurrentTruth]
---

# CLI Source References Contract Set

## Status And Authority

This routed set is the accepted current Crystallized authority for shared CLI
source references. It defines current behavior for the new CLI and does not ship
yet. Implementation and executable evidence are tracked in
[CLI Development](../../../../../../working/cli-development/_cli-development.md).
This entrypoint provides navigation only. Consumers link directly to the
Interface for public definitions and to the Behavior file for technology-neutral
resolution and conformance.

## Contract Roles

- [`interface.md`](interface.md) is the current Crystallized authority for the
  complete caller-visible public contract and observable results.
- [`behavior.md`](behavior.md) is the current Crystallized authority for
  technology-neutral resolution, invariants, safety, result formation, and
  conformance for the Interface Contract.
- No Technical Design file is created for this shared contract set.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->
- [Accepted current technology-neutral resolution and conformance for shared CLI source references](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Source #Reference #Behavior #CurrentTruth
- [Accepted current caller-visible contract for shared CLI source references](interface.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Source #Reference #Interface #CurrentTruth
<!-- open-forge:generated-index:end -->
