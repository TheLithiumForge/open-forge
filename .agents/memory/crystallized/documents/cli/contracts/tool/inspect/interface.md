---
open-forge:
  description: Show one tool's full setup picture
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool inspect interface

Tool inspect is a command that shows one tool's complete setup picture.
Inspect Codebase Memory discloses its account-wide installer, native Skills and hooks, and watcher defaults.

Status: Round-2 design decided on 14 November 2026.

## Syntax

```text
open-forge tool inspect <id> [global flags]
```

Require exactly one known catalogue ID. There are no local flags.
The help purpose is "Show one tool's full setup picture."

## Report

Report identity, channels, client and runtime requirements, supported profiles, bin and manager-global-bin readiness, runtime and tool paths, native setup and uninstall vectors, declared writes, Open Forge additions and telemetry.
The [common Interface](../interface.md) defines consent, repetition and result coordinates.
The sibling [Behavior](behavior.md) defines execution.

## Example

```text
open-forge tool inspect codebase-memory
```

Inspect Codebase Memory discloses its account-wide installer, native Skills and hooks, and watcher defaults.
