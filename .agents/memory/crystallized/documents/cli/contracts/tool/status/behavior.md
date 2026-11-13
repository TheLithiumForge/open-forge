---
open-forge:
  description: Define Tool status checks and effects
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool status behavior

Tool status behavior is the flow that checks tool versions, configuration and launch readiness.
A teammate with a declared but missing CodeGraph gets bare Install as the next step, with no files written.

Status: V1 design decided. Implementation waits for maintainer review of step 0.

## Flow

Run bounded version and native read probes. Compare current config values with desired and last applied values. A missing declared tool gives the next step `open-forge tool install`. Self-updates inside support offer `open-forge tool install <id>` to reapply. Outside support warns without downgrading.
Use the [common Behavior](../behavior.md) for detection, channel selection, safe edits and verification.
Keep this command's public meaning in its [Interface](interface.md).

## Results

Read-only checks create no state. Writers publish only verified facts.
An independently completed effect stays in the result if a later step fails.
Use the shared semantic statuses without hiding unavailable facts or unfinished work.

## Conformance

Required cases cover:

- Self-updates inside and outside support, absent declarations, missing tools, config conflicts, missing guidance and unavailable observations.
- Stable ID order, no duplicate effects and unchanged unrelated bytes.
- Shared text and JSON facts, global flags and schema-3 exits.
