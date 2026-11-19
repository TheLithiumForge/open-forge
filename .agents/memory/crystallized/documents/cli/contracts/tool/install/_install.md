---
open-forge:
  description: Route Tool install inputs and behavior
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool install

Tool install is a command that installs a tool and configures the project to use it, or installs a project's configured tools on a new machine.
For example, a teammate clones a project that uses CodeGraph and runs bare `open-forge tool install`.

Status: Lean design decided on 17 November 2026.

The [Interface](interface.md) defines syntax and the plan.
The [Behavior](behavior.md) defines execution.

## Axioms

- inherited - No local axioms. Loaded ancestor axioms still apply.

## Entries

- [Define Tool install checks and effects](behavior.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Install a tool and configure the project, or install configured tools that are missing](interface.md) - #Memory #Document #CLI #Tool #CurrentTruth
