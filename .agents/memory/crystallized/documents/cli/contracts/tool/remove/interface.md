---
open-forge:
  description: Uninstall tool setup and remove its additions
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool remove interface

Tool remove is a command that runs native uninstall and removes Open Forge additions.
CodeGraph removes its project setup through its own uninstaller, then Open Forge removes its recorded archive exclusion.

Status: Round-2 design decided on 14 November 2026.

## Syntax

```text
open-forge tool remove <id> [options]
```

Require one known declared or locked ID, with no version selector. An ID that is neither is refused as invalid input with the shared family sentence `Cannot remove <id>: it is not declared or locked in this project.` A completed removal deletes the declaration and lock entry, so repeating it is refused the same way. Keep records for unfinished removal.
The help purpose is "Remove project additions and offer native uninstall."

## Report

Report chosen or skipped native uninstall and account scope, additions to remove, conflicts to keep, kept account-wide native settings and their manual commands, shared-file choices, the manual step to remove the bin PATH entry after the last tool in bin, and package-manager uninstall instructions.
The [common Interface](../interface.md) defines consent, repetition and result coordinates.
The sibling [Behavior](behavior.md) defines execution.

## Example

```text
open-forge tool remove codegraph
```

CodeGraph removes its project setup through its own uninstaller, then Open Forge removes its recorded archive exclusion.
