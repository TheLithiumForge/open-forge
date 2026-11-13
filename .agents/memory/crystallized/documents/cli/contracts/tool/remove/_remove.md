---
open-forge:
  description: Route Tool remove inputs and behavior
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool remove

Tool remove is an operation that undoes this project's tool configuration.
Remove detaches CodeGraph from this project, keeps a package-manager installation and prints its native uninstall command.

Status: V1 design decided. Implementation waits for maintainer review of step 0.

The [Interface](interface.md) defines inputs and results. The [Behavior](behavior.md) defines execution.

## Axioms

- inherited - No local axioms. Loaded ancestor axioms still apply.

## Entries

- [Define Tool remove checks and effects](behavior.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Undo this project's tool configuration](interface.md) - #Memory #Document #CLI #Tool #CurrentTruth
