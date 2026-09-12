---
open-forge:
  description: Current maintenance contract for the installable Handoffs Memory entrypoint
  responsibility: Preserve sealed transfer context, observable creation triggers, useful resume fields, expiration, recursive scope, and source alignment
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Handoff, AgentCommunication]
---

# Handoffs Memory Maintenance Contract

## Source

[`src/open-forge/.agents/memory/working/handoffs/_handoffs.md`](../../../../../../../../../src/open-forge/.agents/memory/working/handoffs/_handoffs.md) is the canonical installed Handoffs `entrypoint`. The repository [Handoffs `entrypoint`](../../../../../../../working/handoffs/_handoffs.md) dogfoods the same authored contract and may add local generated `Entries`.

The [Working state contract](../../../../../framework/memory/working.md) defines Handoffs as sealed boundary snapshots within temporary Working Memory.

## Contract

- Frontmatter preserves #LoadNow, #Memory, #Handoff, #AgentCommunication, and #Contextual classification
- A Handoff is created and sealed only when an actual transfer or explicitly planned resumption needs a stable boundary snapshot while the active Checkpoint may continue to change
- Routine pauses, ordinary closeout, and possible future interruptions do not create a Handoff
- The intended reader or resumed work remains clear from the route, description, or content
- Handoffs remain concise and link to current work state, durable sources, documents, code, or other details instead of copying them
- Boundary status, next action, blockers, and verification state appear in the Handoff itself. A live Checkpoint may supplement but not replace the snapshot
- The snapshot remains unchanged while serving as a sealed Handoff. Later state belongs in the active Checkpoint or a new Handoff. Category transitions follow the destination's rules
- Handoffs remain contextual rather than complete history or accepted truth
- Useful material is extracted and the handoff is archived after it no longer supports an active transfer

## Verification

- Installation and `route` tests verify Handoffs loading, indexing, classification, and recursive child routing
