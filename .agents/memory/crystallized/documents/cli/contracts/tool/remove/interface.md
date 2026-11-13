---
open-forge:
  description: Undo this project's tool configuration
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool remove interface

Tool remove is a command that undoes this project's tool configuration.
Remove detaches CodeGraph from this project, keeps a package-manager installation and prints its native uninstall command.

Status: V1 design decided. Implementation waits for maintainer review of step 0.

## Syntax

Proposed:

```text
open-forge tool remove <id> [--allow-path <path>...] [--automatic] [--dry-run] [global flags]
```

Remove requires exactly one declared or locally recorded tool ID. A repeated removal with no remaining records or changes is a successful no-op for a known catalogue ID.
The help purpose is "Undo this project's tool configuration."

## Report

Show each prior value to restore, drift to keep, settings and lock removal, guidance removal, package-manager uninstall instructions and optional shared-file and final PATH removal choices.
The [common Interface](../interface.md) defines report structure, repetition, result meanings and consent.
The sibling [Behavior](behavior.md) defines execution.

## Example

Proposed:

```text
open-forge tool remove codegraph
```

Remove detaches CodeGraph from this project, keeps a package-manager installation and prints its native uninstall command.
