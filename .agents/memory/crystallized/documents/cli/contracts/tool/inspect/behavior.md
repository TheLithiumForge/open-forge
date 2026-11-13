---
open-forge:
  description: Define Tool inspect checks and effects
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool inspect behavior

Tool inspect behavior is the flow that shows one tool's full setup picture.
Inspect CodeGraph shows its local installer, retained hook and permissions, and account-wide telemetry-off action.

Status: V1 design decided. Implementation waits for maintainer review of step 0.

## Flow

Read catalogue and available local records, then perform bounded detection and declared read-only config checks. Show missing and unavailable facts separately. Never create state or fetch updates.
Use the [common Behavior](../behavior.md) for detection, channel selection, safe edits and verification.
Keep this command's public meaning in its [Interface](interface.md).

## Results

Read-only checks create no state. Writers publish only verified facts.
An independently completed effect stays in the result if a later step fails.
Use the shared semantic statuses without hiding unavailable facts or unfinished work.

## Conformance

Required cases cover:

- Both origins, every requirement field, missing runtime, unavailable files and the complete installer-effects disclosure.
- Stable ID order, no duplicate effects and unchanged unrelated bytes.
- Shared text and JSON facts, global flags and schema-3 exits.
