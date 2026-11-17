---
open-forge:
  description: Route Tool inspect inputs and behavior
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool inspect

Tool inspect is an operation that shows one tool's complete setup picture.
For example, inspect Codebase Memory discloses its account-wide installer, native Skills and hooks, and watcher defaults.

Status: Round-2 design decided on 14 November 2026.

The [Interface](interface.md) defines inputs and reports.
The [Behavior](behavior.md) defines execution.

## Axioms

- inherited - No local axioms. Loaded ancestor axioms still apply.

## Entries

- [Define Tool inspect checks and effects](behavior.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Show one tool's full setup picture](interface.md) - #Memory #Document #CLI #Tool #CurrentTruth
