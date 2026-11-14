---
open-forge:
  description: Define Tool inspect checks and effects
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool inspect behavior

Tool inspect behavior is the flow that shows one tool's complete setup picture.
Inspect Codebase Memory discloses its account-wide installer, native Skills and hooks, and watcher defaults.

Status: Round-2 design decided on 14 November 2026.

## Flow

Read catalogue and local records, then run bounded version and declared read-only checks. Show missing and unavailable facts separately. Don't install, configure or fetch latest.
Use the [common Behavior](../behavior.md) for detection, channels, native setup and additive effects.
Keep syntax and reports in the [Interface](interface.md).

## Results

Use the common status precedence and shared schema-3 exits.
Keep actual effects, conflicts and unstarted work separate.
Dry-run changes no Tool configuration or records.

## Conformance

Required cases cover:

- Missing paths, failed probes, every requirement field, native account scope and separate Open Forge additions.
- Stable ordering and unchanged unrelated data.
- Shared text and JSON facts and global flags.
