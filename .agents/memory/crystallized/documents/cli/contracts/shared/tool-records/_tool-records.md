---
open-forge:
  description: Route portable declarations and local tool setup records
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool records

Tool records are the settings, lock, workspace edit history and machine facts used by tool setup.
For example, two teammates share a supported range and tested baseline while keeping their runtime paths on their own machines.

Status: V1 design decided. Implementation waits for maintainer review of step 0.

The [Interface](interface.md) defines the four shapes. The [Behavior](behavior.md) defines drift and publication rules.

## Axioms

- inherited - No local axioms. Loaded ancestor axioms still apply.

## Entries

- [Define config changes, drift, backups and record publication](behavior.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Define tool declarations, tested baselines and local state](interface.md) - #Memory #Document #CLI #Tool #CurrentTruth
