---
open-forge:
  description: Define Tool list checks and effects
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool list behavior

Tool list behavior is the flow that lists catalogue tools and project declarations.
List returns CodeGraph and Codebase Memory in ID order. Rune has no V1 row.

Status: V1 design decided. Implementation waits for maintainer review of step 0.

## Flow

Read the compiled catalogue and readable settings. Do not run version probes or fetch registry data. Keep independently readable rows if workspace facts are unavailable.
Use the [common Behavior](../behavior.md) for detection, channel selection, safe edits and verification.
Keep this command's public meaning in its [Interface](interface.md).

## Results

Read-only checks create no state. Writers publish only verified facts.
An independently completed effect stays in the result if a later step fails.
Use the shared semantic statuses without hiding unavailable facts or unfinished work.

## Conformance

Required cases cover:

- An empty declaration set, no workspace, unreadable settings, origin labels and stable ordering.
- Stable ID order, no duplicate effects and unchanged unrelated bytes.
- Shared text and JSON facts, global flags and schema-3 exits.
