---
open-forge:
  description: Accepted current shared global-flag Interface and Behavior contract set
  responsibility: Route the accepted global-flag contract files while keeping their Interface and Behavior roles separate
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Global, Flag, CurrentTruth]
---

# Global CLI Flags Contract Set

## Status And Authority

This routed set is the accepted current Crystallized authority for shared global
flags. The flags define current behavior for the new CLI and do not ship yet.
Implementation and executable evidence are tracked in
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
- [Accepted current technology-neutral resolution and conformance for shared global CLI flags](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Global #Flag #Behavior #CurrentTruth
- [Accepted current caller-visible contract for shared global CLI flags](interface.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Global #Flag #Interface #CurrentTruth
<!-- open-forge:generated-index:end -->
