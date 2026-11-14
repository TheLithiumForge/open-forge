---
open-forge:
  description: Route the six published tool setup operations
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool

Tool is a command group that installs published tools and adds configuration.
For example, bare Install restores teammates' exact locked versions.

Status: Round-2 design decided on 14 November 2026. Command implementation is pending.

## Commands

The planned commands appear in this order:

- `list` - show the catalogue.
- `inspect <id>` - show the complete setup picture.
- `status [id]` - check versions and additions.
- `install [id[@version]]` - run native setup and add configuration.
- `update [id[@version]]` - choose the latest or an explicit supported version.
- `remove <id>` - run native uninstall, then remove additions.

The bare group shows help.
The [Interface](interface.md) defines tool specs, `--yes` and results.
The [Behavior](behavior.md) defines common checks.

## Axioms

- inherited - No local axioms. Loaded ancestor axioms still apply.

## Entries

- [Define published channels, native setup and additive effects](behavior.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Route Tool inspect inputs and behavior](inspect/_inspect.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Route Tool install inputs and behavior](install/_install.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Define tool specs, yes consent and shared result coordinates](interface.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Route Tool list inputs and behavior](list/_list.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Route Tool remove inputs and behavior](remove/_remove.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Route Tool status inputs and behavior](status/_status.md) - #Memory #Document #CLI #Tool #CurrentTruth
- [Route Tool update inputs and behavior](update/_update.md) - #Memory #Document #CLI #Tool #CurrentTruth
