---
open-forge:
  description: Current maintenance contract for the installable Handoffs Memory entrypoint
  responsibility: Preserve concise transfer context, creation triggers, useful resume fields, expiration, recursive scope, and source alignment
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Handoff, AgentCommunication]
---

# Handoffs Memory Maintenance Contract

## Source

[`src/open-forge/.agents/memory/working/handoffs/_handoffs.md`](../../../../../../../../../src/open-forge/.agents/memory/working/handoffs/_handoffs.md) is the canonical installed Handoffs entrypoint. The repository [Handoffs entrypoint](../../../../../../../working/handoffs/_handoffs.md) dogfoods the same authored contract and may add local generated entries.

The [Working state contract](../../../../../framework/memory/working.md) defines Handoffs as concise static transfer notes within temporary Working Memory.

## Contract

- Frontmatter preserves #LoadNow, #Memory, #Handoff, #AgentCommunication, and #Contextual classification
- A handoff is created or updated when work is transferred, delegated, interrupted, or handed to another agent or person, unless a more specific route already contains complete resume context
- The intended receiver or resumed work remains clear from the route, description, or content
- Handoffs remain concise and link to sessions, documents, code, or other routes when detail matters
- Status, next action, blockers, relevant loaded context, and verification needs are included when they improve resumption
- Handoffs remain contextual rather than complete history or accepted truth
- Useful material is extracted and the handoff is archived after it no longer supports an active transfer
- The installed route begins empty and supports ordinary recursive scope

## Verification

- Installation and route tests verify Handoffs loading, indexing, classification, and recursive child routing
- Compare canonical source and dogfood authored content outside generated `Entries`
