---
open-forge:
  description: Route Tool remove inputs and behavior
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool remove

Tool remove is a command that undoes Open Forge's changes for one tool and then offers the tool's own uninstall.
For example, `tool remove codegraph` removes the archive pattern from `codegraph.json` and asks whether to run CodeGraph's uninstall too.

Status: Lean design decided on 17 November 2026.

The [Interface](interface.md) defines syntax and the report.
The [Behavior](behavior.md) defines execution.

## Axioms

- inherited - No local axioms. Loaded ancestor axioms still apply.

## Entries

- [Define Tool remove effects and the uninstall question](behavior.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Remove Open Forge's changes for a tool and offer its own uninstall](interface.md) - #Memory #Document #CLI #Tool #CurrentTruth
