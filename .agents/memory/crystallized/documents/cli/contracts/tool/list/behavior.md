---
open-forge:
  description: Define what Tool list reads
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool list behavior

Tool list behavior is the read-only flow behind `tool list`.
For example, it runs `codegraph --version` to show the installed version.

Status: Lean design decided on 17 November 2026.

## Flow

Read the project settings and each tool's integration files. Run each tool's detection command. Report the result.
List writes nothing and runs no install, setup or uninstall command.
Unreadable settings or integration files don't stop the list. The list adds a warning that names the file, shows the affected state as not configured or not integrated, and reports `incomplete`.
Use the [common Behavior](../behavior.md) for detection and integration state.
