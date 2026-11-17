---
open-forge:
  description: Route Tool remove inputs and behavior
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool remove

Tool remove is an operation that runs native uninstall and removes Open Forge additions.
For example, CodeGraph removes its project setup through its own uninstaller, then Open Forge removes its recorded archive exclusion.

Status: Round-2 design decided on 14 November 2026.

The [Interface](interface.md) defines inputs and reports.
The [Behavior](behavior.md) defines execution.

## Axioms

- inherited - No local axioms. Loaded ancestor axioms still apply.

## Entries

- [Define Tool remove checks and effects](behavior.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Uninstall tool setup and remove its additions](interface.md) - #Memory #Document #CLI #Tool #CurrentTruth
