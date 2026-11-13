---
open-forge:
  description: Show one tool's full setup picture
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool inspect interface

Tool inspect is a command that shows one tool's full setup picture.
Inspect CodeGraph shows its local installer, retained hook and permissions, and account-wide telemetry-off action.

Status: V1 design decided. Implementation waits for maintainer review of step 0.

## Syntax

Proposed:

```text
open-forge tool inspect <id> [global flags]
```

Inspect requires exactly one catalogue ID and has no local flags.
The help purpose is "Show one tool's full setup picture."

## Report

Show identity, all channels and requirements, profiles and tested versions, minimum Open Forge version, bin folder, current PATH readiness, resolved runtime and tool paths, installer writes, Open Forge changes, telemetry and agent guidance. Missing machine paths stay absent with a reason.
The [common Interface](../interface.md) defines report structure, repetition, result meanings and consent.
The sibling [Behavior](behavior.md) defines execution.

## Example

Proposed:

```text
open-forge tool inspect codegraph
```

Inspect CodeGraph shows its local installer, retained hook and permissions, and account-wide telemetry-off action.
