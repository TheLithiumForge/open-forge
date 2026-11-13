---
open-forge:
  description: Route the six tool setup commands and their shared contracts
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool

Tool is a command group that installs published tools and adds project configuration.
For example, a teammate runs bare Install to set up the tools declared by the project, then calls each tool directly.

Status: V1 design decided. Implementation waits for maintainer review of step 0.

## Commands

The planned commands appear in this order:

- `list` - show the catalogue.
- `inspect <id>` - show the full setup picture.
- `status [id]` - check the current installation and configuration.
- `install [id]` - install and configure one tool or all declared tools.
- `update [id]` - move one tool or all declared tools to the newest supported range.
- `remove <id>` - undo this project's changes.

The group itself shows help. The [Interface](interface.md) defines common inputs and results. The [Behavior](behavior.md) defines common checks and effects.

## Axioms

- inherited - No local axioms. Loaded ancestor axioms still apply.

## Entries

- [Define detection, channel order, safe edits and tool operation checks](behavior.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Route Tool inspect inputs and behavior](inspect/_inspect.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Route Tool install inputs and behavior](install/_install.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Define common tool command inputs, reports and consent](interface.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Route Tool list inputs and behavior](list/_list.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Route Tool remove inputs and behavior](remove/_remove.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Route Tool status inputs and behavior](status/_status.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Route Tool update inputs and behavior](update/_update.md) - #Memory #Document #CLI #Tool #CurrentTruth
