---
open-forge:
  description: Define Tool list checks and effects
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool list behavior

Tool list behavior is the flow that lists catalogue tools and declarations.
CodeGraph and Codebase Memory appear in ID order. Rune has no V1 row.

Status: Round-2 design decided on 14 November 2026. Command implementation is pending.

## Flow

Read the compiled catalogue and readable settings. Don't invoke native probes or network queries. Keep independently readable rows if workspace facts are unavailable.
Use the [common Behavior](../behavior.md) for detection, channels, native setup and additive effects.
Keep syntax and reports in the [Interface](interface.md).

## Results

Use the common status precedence and shared schema-3 exits.
Keep actual effects, conflicts and unstarted work separate.
Dry-run changes no Tool configuration or records.

## Conformance

Required cases cover:

- Catalogue rows, no workspace, unknown fields, unreadable settings and stable ordering.
- Stable ordering and unchanged unrelated data.
- Shared text and JSON facts and global flags.
