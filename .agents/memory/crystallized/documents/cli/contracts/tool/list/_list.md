---
open-forge:
  description: Route Tool list inputs and behavior
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool list

Tool list is a command that shows the supported tools by category, with each one's install method and state.
For example, a teammate runs it to see that CodeGraph is configured for the project but not installed on their machine.

Status: Lean design decided on 17 November 2026.

The [Interface](interface.md) defines syntax and the report.
The [Behavior](behavior.md) defines what it reads.

## Axioms

- inherited - No local axioms. Loaded ancestor axioms still apply.

## Entries

- [Define what Tool list reads](behavior.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Show supported tools by category with their install method and state](interface.md) - #Memory #Document #CLI #Tool #CurrentTruth
