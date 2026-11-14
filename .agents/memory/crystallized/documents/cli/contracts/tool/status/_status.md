---
open-forge:
  description: Route Tool status inputs and behavior
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool status

Tool status is an operation that checks versions and additive configuration.
For example, a missing declared CodeGraph produces bare Install as the next step without creating Tool state.

Status: Round-2 design decided on 14 November 2026. Command implementation is pending.

The [Interface](interface.md) defines inputs and reports.
The [Behavior](behavior.md) defines execution.

## Axioms

- inherited - No local axioms. Loaded ancestor axioms still apply.

## Entries

- [Define Tool status checks and effects](behavior.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Check tool versions, configuration and launch readiness](interface.md) - #Memory #Document #CLI #Tool #CurrentTruth
