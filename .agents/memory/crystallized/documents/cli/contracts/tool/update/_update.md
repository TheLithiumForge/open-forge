---
open-forge:
  description: Route Tool update inputs and behavior
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool update

Tool update is an operation that updates tools through their published channel.
For example, update replaces the active version and rewrites absolute launchers while recording the new exact version.

Status: Round-2 design decided on 14 November 2026.

The [Interface](interface.md) defines inputs and reports.
The [Behavior](behavior.md) defines execution.

## Axioms

- inherited - No local axioms. Loaded ancestor axioms still apply.

## Entries

- [Define Tool update checks and effects](behavior.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Update tools to a supported version](interface.md) - #Memory #Document #CLI #Tool #CurrentTruth
