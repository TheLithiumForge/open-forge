---
open-forge:
  description: Route Tool install inputs and behavior
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool install

Tool install is an operation that installs tools and adds their project configuration.
With CodeGraph 1.6.x declared, a teammate runs bare Install and receives a working direct `codegraph` command plus project configuration.

Status: V1 design decided. Implementation waits for maintainer review of step 0.

The [Interface](interface.md) defines inputs and results. The [Behavior](behavior.md) defines execution.

## Axioms

- inherited - No local axioms. Loaded ancestor axioms still apply.

## Entries

- [Define Tool install checks and effects](behavior.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Install tools and add their project configuration](interface.md) - #Memory #Document #CLI #Tool #CurrentTruth
