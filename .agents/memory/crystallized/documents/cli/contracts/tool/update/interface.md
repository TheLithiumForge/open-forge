---
open-forge:
  description: Move tools to the newest supported range
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool update interface

Tool update is a command that moves tools to the newest supported range.
Update moves a tool to its newest supported range and rewrites its launcher to the new absolute runtime and entry paths.

Status: V1 design decided. Implementation waits for maintainer review of step 0.

## Syntax

Proposed:

```text
open-forge tool update [<id>] [--version <range>] [--channel <channel>] [--allow-download] [--allow-path <path>...] [--automatic] [--dry-run] [global flags]
```

Without an ID, update every declared tool. With an ID, require a declared tool. The default chooses the newest supported stable range. The proposed shared version and channel overrides require a named ID.
The help purpose is "Move tools to the newest supported range."

## Report

Show old and new ranges and exact versions, profile changes, account-wide replacement effects, launcher rewrites, requirements, verification and project config migration.
The [common Interface](../interface.md) defines report structure, repetition, result meanings and consent.
The sibling [Behavior](behavior.md) defines execution.

## Example

Proposed:

```text
open-forge tool update
```

Update moves a tool to its newest supported range and rewrites its launcher to the new absolute runtime and entry paths.
