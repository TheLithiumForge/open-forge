---
open-forge:
  description: Current maintenance contract for the installable Checkpoints Memory entrypoint
  responsibility: Preserve current active-workstream state, active continuity status, restoration refresh, closeout, recursive scope, and source alignment
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Checkpoint, Resumability]
---

# Checkpoints Memory Maintenance Contract

## Source

[`src/extensions/planning/content/.agents/memory/working/checkpoints/_checkpoints.md`](../../../../../../../../../src/extensions/planning/content/.agents/memory/working/checkpoints/_checkpoints.md) is the canonical installed source for the optional Planning Extension's Checkpoints `entrypoint`. The repository [Checkpoints `entrypoint`](../../../../../../../working/checkpoints/_checkpoints.md) dogfoods the same authored contract and may add local generated `Entries`.

The [Working state contract](../../../../../framework/memory/working.md) defines Checkpoints as current state for one active workstream that any future reader can use. The [transition contract](../../../../../framework/memory/transitions.md) owns durable-outcome integration and expiration.

## Contract

- Frontmatter uses #Extension, #Memory, #Working, #Checkpoint, and #Contextual to classify this optional Planning route
- A Checkpoint records the current state, current step, and next steps for one active workstream and is updated as the work changes
- It carries #Active and #KeepInMind only while active and is refreshed after important state changes and context restoration
- It records the current goal, state, and current step, accepted decisions, evidence, unresolved questions, next steps, and durable sources when they help the work resume
- Before an actual transfer or explicitly planned resumption, the Checkpoint is updated. A Handoff is created only when that boundary needs a stable snapshot while the Checkpoint may continue to change
- Routine pauses and ordinary closeout do not require a Handoff
- Closeout saves durable outcomes outside the Checkpoint, removes active continuity status, then archives or prunes the expired Checkpoint

## Verification

- Core-only installation verifies that the optional Checkpoints route is absent; Planning installation verifies that it installs, indexes, retains its full source classification and active-status wording, supports recursive scope, and preserves user-owned legacy Sessions
