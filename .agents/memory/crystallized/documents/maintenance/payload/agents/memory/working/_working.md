---
open-forge:
  description: Current maintenance contract for the Core Working Memory entrypoint and its optional Extension-provided Checkpoints and Handoffs routes
  responsibility: Preserve active resume state, expected expiration, durable-result extraction, standard nested roles, recursive scope, and source alignment
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Working]
---

# Working Memory Maintenance Contract

## Source

[`src/open-forge/.agents/memory/working/_working.md`](../../../../../../../../../src/open-forge/.agents/memory/working/_working.md) is the canonical installed Working Memory `entrypoint`. The repository [Working `entrypoint`](../../../../../../../working/_working.md) dogfoods the same authored contract and adds repository working state through generated `Entries`.

The [Working state contract](../../../../../framework/memory/working.md) defines Working Memory through expected expiration. The [transition contract](../../../../../framework/memory/transitions.md) owns extraction and movement when active need ends.

## Contract

- Frontmatter preserves #LoadNow, #Memory, #Working, and #Contextual classification
- Working Memory does not establish acceptance by itself
- An explicitly accepted temporary choice may remain in Working when its source, scope, and expected expiration are clear
- Active material stays small, current, and easy to replace or reread
- Useful results are saved before stale Working Memory is archived or cleared
- Checkpoints and Handoffs retain their distinct active-state and transfer roles within Working Memory. Their routes are provided by optional Extensions and remain linked through the maintenance `Entries`.
- Recursive scopes may add plans, history, backlogs, or other temporary roles without changing Working state semantics
- The installed parent contains no workspace-specific active state

## Verification

- Core installation and `route` tests verify Working loading and classification plus legacy user-owned Sessions preservation. Selected Extension installation tests verify the optional Checkpoints and Handoffs routes, indexing, classification, and recursive scope.

## Entries

- [Current maintenance contract for the installable Checkpoints Memory entrypoint](checkpoints.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Checkpoint #Resumability
- [Current maintenance contract for the installable Handoffs Memory entrypoint](handoffs.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Handoff #AgentCommunication
