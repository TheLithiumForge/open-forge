---
open-forge:
  description: Current state, current step, and next steps for one active workstream
  tags: [Extension, Memory, Working, Checkpoint, Contextual]
---

# Checkpoints

## Where does this workstream stand, and what comes next?

A Checkpoint records the current state, current step, and next steps for one active workstream. Update it as the work changes. It is a continuation record, not history or authority by itself.

## Axioms

### Active State

- Keep one Checkpoint per active workstream when durable sources alone are not enough to resume after a pause, context restoration, or transfer.
- Tag it #Active and #KeepInMind only while it is active.
- Record only what helps work resume:
  - Current goal, state, and step.
  - Accepted decisions and evidence.
  - Unresolved questions and next steps.
  - Links to durable sources.
- Refresh the Checkpoint after an important state change and after context restoration.

### Closeout

- Update the Checkpoint before an actual transfer or explicitly planned resumption. When that boundary needs a fixed snapshot while the Checkpoint may continue to change, seal one using whatever transfer record the workspace has; the optional Observations and Handoffs package supplies Handoffs for exactly this. Routine pauses and ordinary closeout do not require one.
- At closeout, save durable outcomes outside the Checkpoint. When its active need ends, remove #Active and #KeepInMind, then archive or prune it.

## Entries

- none - No entries - #Empty
