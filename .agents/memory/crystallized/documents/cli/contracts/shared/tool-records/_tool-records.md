---
open-forge:
  description: Route settings, additive lock records and per-user machine facts
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool records

Tool records are the settings, committed addition records and machine facts used by tool setup.
For example, a teammate shares an exact lock version while keeping runtime paths on their own machine.

Status: Round-2 design decided on 14 November 2026. Command implementation is pending.

The [Interface](interface.md) defines the three locations.
The [Behavior](behavior.md) defines apply, reapply and removal.

## Axioms

- inherited - No local axioms. Loaded ancestor axioms still apply.

## Entries

- [Define additive changes and removal without repository preimages](behavior.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Define supported ranges, exact locks and additive change records](interface.md) - #Memory #Document #CLI #Tool #CurrentTruth
