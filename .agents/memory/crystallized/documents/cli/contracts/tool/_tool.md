---
open-forge:
  description: Route the three tool commands that list, install and remove supported third-party tools
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool

Tool is a command group that configures a project to use supported third-party tools and helps each person install them.
For example, `open-forge tool install codegraph` installs CodeGraph with npm, runs CodeGraph's own project setup and keeps archived Open Forge memory out of its index.

Status: Lean design decided on 17 November 2026. The fuller design is parked in draft PR #32.

## Commands

The commands appear in this order:

- `list` - show the supported tools by category, with their install method and state.
- `install [id]` - install one tool and configure the project, or install the project's configured tools that are missing on this machine.
- `remove <id>` - undo Open Forge's changes for one tool, then offer the tool's own uninstall.

The bare group shows help.
There is no `inspect`, `status` or `update`. Updating means running the tool's own update or installing it again.

The [Interface](interface.md) defines inputs, consent and reports.
The [Behavior](behavior.md) defines the catalogue, detection, commands and Open Forge's changes.

## Axioms

- inherited - No local axioms. Loaded ancestor axioms still apply.

## Entries

- [Define the tool catalogue, detection, command execution and Open Forge's own changes](behavior.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Route Tool install inputs and behavior](install/_install.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Define tool IDs, consent and shared result coordinates](interface.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Route Tool list inputs and behavior](list/_list.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Route Tool remove inputs and behavior](remove/_remove.md) - #Memory #Document #CLI #Tool #CurrentTruth
