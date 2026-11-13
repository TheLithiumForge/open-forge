---
open-forge:
  description: Define Tool remove checks and effects
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool remove behavior

Tool remove behavior is the flow that undoes this project's tool configuration.
Remove detaches CodeGraph from this project, keeps a package-manager installation and prints its native uninstall command.

Status: V1 design decided. Implementation waits for maintainer review of step 0.

## Flow

Read local edit records before forming removal. Restore each prior value only while current equals applied. Keep drift and its records as unfinished removal. Remove this project's Skill and conditional line with the same guards. Never uninstall a package-manager install. Ask before deleting any Open Forge-placed tool files or runtime, since other projects can use them. Delete a runtime only when no launcher uses it. Offer confirmed PATH removal only after the last tool is removed.
Use the [common Behavior](../behavior.md) for detection, channel selection, safe edits and verification.
Keep this command's public meaning in its [Interface](interface.md).

## Results

Read-only checks create no state. Writers publish only verified facts.
An independently completed effect stays in the result if a later step fails.
Use the shared semantic statuses without hiding unavailable facts or unfinished work.

## Conformance

Required cases cover:

- Prior absence and value restoration, borrowed values, drift preservation, missing local records, other-project use, refusal of shared deletion and final PATH confirmation.
- Stable ID order, no duplicate effects and unchanged unrelated bytes.
- Shared text and JSON facts, global flags and schema-3 exits.
