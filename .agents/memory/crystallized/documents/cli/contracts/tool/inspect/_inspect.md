---
open-forge:
  description: Route Tool inspect inputs and behavior
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool inspect

Tool inspect is an operation that shows one tool's full setup picture.
Inspect CodeGraph shows its local installer, retained hook and permissions, and account-wide telemetry-off action.

Status: V1 design decided. Implementation waits for maintainer review of step 0.

The [Interface](interface.md) defines inputs and results. The [Behavior](behavior.md) defines execution.

## Axioms

- inherited - No local axioms. Loaded ancestor axioms still apply.

## Entries

- [Define Tool inspect checks and effects](behavior.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Show one tool's full setup picture](interface.md) - #Memory #Document #CLI #Tool #CurrentTruth
