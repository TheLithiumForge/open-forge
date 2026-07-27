---
open-forge:
  description: Current maintenance contracts for the installable Working Memory entrypoint and its Handoffs and Sessions routes
  responsibility: Preserve bounded resumability, expected expiration, extraction, shipped child routes, recursive scope, and source alignment
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Working, Contextual]
---

# Working Memory Maintenance Contract

## Source

[`src/open-forge/.agents/memory/working/_working.md`](../../../../../../../../../src/open-forge/.agents/memory/working/_working.md) is the canonical installed Working Memory entrypoint. The repository [Working entrypoint](../../../../../../../working/_working.md) dogfoods the same authored contract and adds repository working state through generated entries.

The [Working state contract](../../../../../framework/memory/working.md) defines Working Memory through expected expiration. The [transition contract](../../../../../framework/memory/transitions.md) owns extraction and movement when active need ends.

## Contract

- Frontmatter preserves #LoadNow, #Memory, #Working, and #Contextual classification
- Working Memory remains live resumability context rather than accepted current truth
- Active material stays small, current, and cheap to replace or reread
- Useful state is extracted before stale Working Memory is archived or cleared
- Handoffs and Sessions remain the two shipped #LoadNow child routes with distinct transfer and history roles
- Recursive scopes may add plans, checkpoints, backlogs, or other temporary roles without changing Working state semantics
- The installed parent contains no workspace-specific active state

## Verification

- Installation and route tests verify Working, Handoffs, and Sessions loading, indexing, classification, and recursive scope
- Compare canonical source and dogfood authored content outside generated `Entries`

## Entries

<!-- open-forge:generated-index:start -->
- [Current maintenance contract for the installable Handoffs Memory entrypoint](handoffs.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Handoff #AgentCommunication
- [Current maintenance contract for the installable Sessions Memory entrypoint](sessions.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Session #WorkHistory
<!-- open-forge:generated-index:end -->
