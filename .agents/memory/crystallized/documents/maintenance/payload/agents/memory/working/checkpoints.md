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
- Keep one Checkpoint for an active workstream when durable sources alone cannot support resumption. It records the current state, step and next steps, and changes with the work.
- It carries #Active and #KeepInMind only while active and is refreshed after important state changes and context restoration
- It records the current goal, state, and current step, accepted decisions, evidence, unresolved questions, next steps, and durable sources when they help the work resume
- Update the Checkpoint before an actual transfer or explicitly planned resumption. Seal a transfer snapshot only when that boundary needs fixed state while the Checkpoint may continue changing. The workspace chooses its transfer record, and the optional Observations and Handoffs package supplies Handoffs for this role.
- Routine pauses and ordinary closeout do not require a Handoff
- Closeout saves durable outcomes outside the Checkpoint. When its active need ends, the Checkpoint loses its active continuity status and is archived or pruned

## Verification

- Use the [payload integrity checks](../../../_payload.md#verification) for the Planning Extension source and metadata rendering.
- Compare category rules with the repository counterpart outside Entries. Confirm Core alone has no Checkpoints category.
- Manually trace an active record through context restoration, transfer and closeout. Verify active tags expire, durable results leave the record and a sealed snapshot is conditional.
