---
open-forge:
  description: Route Tool status inputs and behavior
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool status

Tool status is an operation that checks tool versions, configuration and launch readiness.
A teammate with a declared but missing CodeGraph gets bare Install as the next step, with no files written.

Status: V1 design decided. Implementation waits for maintainer review of step 0.

The [Interface](interface.md) defines inputs and results. The [Behavior](behavior.md) defines execution.

## Axioms

- inherited - No local axioms. Loaded ancestor axioms still apply.

## Entries

- [Define Tool status checks and effects](behavior.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Check tool versions, configuration and launch readiness](interface.md) - #Memory #Document #CLI #Tool #CurrentTruth
