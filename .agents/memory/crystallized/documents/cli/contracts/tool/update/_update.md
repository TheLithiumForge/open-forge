---
open-forge:
  description: Route Tool update inputs and behavior
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool update

Tool update is an operation that moves tools to the newest supported range.
Update moves a tool to its newest supported range and rewrites its launcher to the new absolute runtime and entry paths.

Status: V1 design decided. Implementation waits for maintainer review of step 0.

The [Interface](interface.md) defines inputs and results. The [Behavior](behavior.md) defines execution.

## Axioms

- inherited - No local axioms. Loaded ancestor axioms still apply.

## Entries

- [Define Tool update checks and effects](behavior.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Move tools to the newest supported range](interface.md) - #Memory #Document #CLI #Tool #CurrentTruth
