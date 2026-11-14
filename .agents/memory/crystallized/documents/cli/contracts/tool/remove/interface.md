---
open-forge:
  description: Uninstall tool setup and remove its additions
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool remove interface

Tool remove is a command that runs native uninstall and removes Open Forge additions.
CodeGraph removes its project setup through its own uninstaller, then Open Forge removes its recorded archive exclusion.

Status: Round-2 design decided on 14 November 2026. Command implementation is pending.

## Syntax

Proposed:

```text
open-forge tool remove <id> [--yes] [--allow-path <path>...] [--dry-run] [global flags]
```

Require one known declared or locked ID, with no version selector. Repeated removal after completed cleanup is a no-op. Keep records for unfinished removal.
The help purpose is "Uninstall tool setup and remove its additions."

## Report

Report chosen or skipped native uninstall and account scope, additions to remove, conflicts to keep, kept account-wide native settings and their manual commands, shared-file choices, PATH instructions and package-manager uninstall instructions.
The [common Interface](../interface.md) defines consent, repetition and result coordinates.
The sibling [Behavior](behavior.md) defines execution.

## Example

Proposed:

```text
open-forge tool remove codegraph
```

CodeGraph removes its project setup through its own uninstaller, then Open Forge removes its recorded archive exclusion.
