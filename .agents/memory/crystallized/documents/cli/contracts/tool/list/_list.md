---
open-forge:
  description: Route Tool list inputs and behavior
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool list

Tool list is an operation that lists catalogue tools and project declarations.
List returns CodeGraph and Codebase Memory in ID order. Rune has no V1 row.

Status: V1 design decided. Implementation waits for maintainer review of step 0.

The [Interface](interface.md) defines inputs and results. The [Behavior](behavior.md) defines execution.

## Axioms

- inherited - No local axioms. Loaded ancestor axioms still apply.

## Entries

- [Define Tool list checks and effects](behavior.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [List catalogue tools and project declarations](interface.md) - #Memory #Document #CLI #Tool #CurrentTruth
