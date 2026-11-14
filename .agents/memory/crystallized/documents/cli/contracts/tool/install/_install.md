---
open-forge:
  description: Route Tool install inputs and behavior
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool install

Tool install is an operation that installs tools and runs their native setup.
For example, a teammate runs bare Install and receives the exact CodeGraph version in the lock. Named Install selects latest support.

Status: Round-2 design decided on 14 November 2026. Command implementation is pending.

The [Interface](interface.md) defines inputs and reports.
The [Behavior](behavior.md) defines execution.

## Axioms

- inherited - No local axioms. Loaded ancestor axioms still apply.

## Entries

- [Define Tool install checks and effects](behavior.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Install tools and add configuration](interface.md) - #Memory #Document #CLI #Tool #CurrentTruth
