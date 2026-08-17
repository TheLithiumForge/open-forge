---
open-forge:
  description: Current state, current step, and next steps for one active workstream
  tags: [LoadNow, Memory, Working, Checkpoint, Contextual]
---

# Checkpoints

A Checkpoint records the current state, current step, and next steps for one active workstream. Update it as the work changes. It is not history or accepted truth.

## Axioms

### Active State

- Keep one Checkpoint for an active workstream when durable sources alone are not enough to resume after a pause, context restoration, or transfer.
- Tag an active Checkpoint #Active and #KeepInMind only while it is active.
- Record only what helps the work resume:
  - Current goal, state, and current step.
  - Accepted decisions and evidence.
  - Unresolved questions and next steps.
  - Links to durable sources.
- Refresh the Checkpoint after an important state change and after context restoration.

### Closeout

- Before an actual transfer or explicitly planned resumption, update the Checkpoint. Create a Handoff only when that boundary needs a stable snapshot while the Checkpoint may continue to change. Routine pauses and ordinary closeout do not require one.
- At closeout, save durable outcomes outside the Checkpoint. When its active need ends, remove #Active and #KeepInMind, then archive or prune it.

## Entries

<!-- open-forge:generated-index:start -->
- [Concise resumption state and next action for the new Open Forge CLI release program](cli-release.md) - #Memory #Working #Checkpoint #Active #KeepInMind #CLI #Release #Contextual
<!-- open-forge:generated-index:end -->
