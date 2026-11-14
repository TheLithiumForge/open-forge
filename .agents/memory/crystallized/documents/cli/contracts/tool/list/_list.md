---
open-forge:
  description: Route Tool list inputs and behavior
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool list

Tool list is an operation that lists catalogue tools and declarations.
For example, codeGraph and Codebase Memory appear in ID order. Rune has no V1 row.

Status: Round-2 design decided on 14 November 2026.

The [Interface](interface.md) defines inputs and reports.
The [Behavior](behavior.md) defines execution.

## Axioms

- inherited - No local axioms. Loaded ancestor axioms still apply.

## Entries

- [Define Tool list checks and effects](behavior.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [List catalogue tools and project declarations](interface.md) - #Memory #Document #CLI #Tool #CurrentTruth
