---
open-forge:
  description: Define Tool update checks and effects
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool update behavior

Tool update behavior is the flow that moves tools to the newest supported range.
Update moves a tool to its newest supported range and rewrites its launcher to the new absolute runtime and entry paths.

Status: V1 design decided. Implementation waits for maintainer review of step 0.

## Flow

Check drift before replacing executable bytes. Resolve and disclose the exact supported target. Use the existing channel unless a valid override is given. Update the active installation, rewrite generated launchers and migrate only values equal to the last applied values. Verify before recording the new baseline. A newer upstream release alone never implies that a newer Open Forge catalogue supports it.
Use the [common Behavior](../behavior.md) for detection, channel selection, safe edits and verification.
Keep this command's public meaning in its [Interface](interface.md).

## Results

Read-only checks create no state. Writers publish only verified facts.
An independently completed effect stays in the result if a later step fails.
Use the shared semantic statuses without hiding unavailable facts or unfinished work.

## Conformance

Required cases cover:

- No available update, newest range selection, known newer catalogue, unknown support, self-updated tool, changed requirements, config migration and partial completion.
- Stable ID order, no duplicate effects and unchanged unrelated bytes.
- Shared text and JSON facts, global flags and schema-3 exits.
