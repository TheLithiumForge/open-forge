---
open-forge:
  description: Current maintenance contract for the installable Handoffs Memory entrypoint
  responsibility: Preserve sealed transfer context, observable creation triggers, useful resume fields, expiration, recursive scope, and source alignment
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Handoff, AgentCommunication]
---

# Handoffs Memory Maintenance Contract

## Source

[`src/extensions/observations-and-handoffs/content/.agents/memory/working/handoffs/_handoffs.md`](../../../../../../../../../src/extensions/observations-and-handoffs/content/.agents/memory/working/handoffs/_handoffs.md) is the canonical installed Handoffs `entrypoint` supplied by the Observations and Handoffs Extension. The repository [Handoffs `entrypoint`](../../../../../../../../../.agents/memory/working/handoffs/_handoffs.md) dogfoods the same authored contract and may add local generated `Entries`.

The [Working state contract](../../../../../framework/memory/working.md) defines Handoffs as sealed boundary snapshots within temporary Working Memory.

## Contract

- The title is followed by `## What state must survive this transfer or planned resumption?` and a short paragraph describing the folder.
- Frontmatter preserves #Extension, #Memory, #Handoff, #AgentCommunication, and #Contextual classification
- Check Entries when work resumes, transfers, is delegated or interrupted, or is reviewed after a context break. Create and seal a Handoff only when an actual transfer or planned resumption needs fixed boundary state while the active record may change.
- Routine pauses, ordinary closeout, and possible future interruptions do not create a Handoff
- The intended reader or resumed work remains clear from the route, description, or content
- Handoffs remain concise and link to current work state, durable sources, documents, code, or other details instead of copying them
- Boundary status, next action, blockers, and verification state appear in the Handoff itself. A live working record may supplement but not replace the snapshot
- The snapshot remains unchanged while serving as a sealed Handoff. Later state belongs in the active working record or a new Handoff. Category transitions follow the destination's rules
- Handoffs remain contextual rather than complete history or accepted truth
- Useful material is extracted and the Handoff is archived after it no longer supports an active transfer.

## Verification

- Use the [payload integrity checks](../../../_payload.md#verification) for the Observations and Handoffs Extension source and metadata rendering.
- Compare shared source and repository rules outside Entries. Confirm Core alone has no Handoffs category.
- Review a sealed snapshot for boundary status, next action, blockers and verification evidence in the snapshot itself. Check later state is recorded elsewhere and ordinary pauses do not require Handoffs.
