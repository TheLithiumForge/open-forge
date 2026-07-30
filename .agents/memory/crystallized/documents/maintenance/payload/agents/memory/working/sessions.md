---
open-forge:
  description: Current maintenance contract for the installable Sessions Memory entrypoint
  responsibility: Preserve raw work history, fallback capture, bounded active checkpoints, extraction, archival, recursive scope, and source alignment
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Session, WorkHistory]
---

# Sessions Memory Maintenance Contract

## Source

[`src/open-forge/.agents/memory/working/sessions/_sessions.md`](../../../../../../../../../src/open-forge/.agents/memory/working/sessions/_sessions.md) is the canonical installed Sessions `entrypoint`. The repository [Sessions `entrypoint`](../../../../../../../working/sessions/_sessions.md) dogfoods the same authored contract and may add local generated `Entries`.

The [Working state contract](../../../../../framework/memory/working.md) defines Sessions as raw chronological context within temporary Working Memory.

## Contract

- Frontmatter preserves #LoadNow, #Memory, #Session, #WorkHistory, and #Contextual classification
- Sessions preserve what happened, useful source pointers, and unresolved state without presenting raw history as accepted truth
- Useful work context without a clearer destination may enter a Session first and be reclassified later
- One bounded #Active #KeepInMind checkpoint is maintained only when work may cross a context boundary, pause before completion, or require a handoff
- The active checkpoint tracks the current goal, applicable phase or stage, accepted decisions, evidence, unresolved questions, and next action. It is refreshed after material changes or detected context restoration.
- Closeout or transfer extracts durable results, removes active continuity status, and archives the checkpoint instead of leaving stale baseline context

## Verification

- Installation and `route` tests verify Sessions loading, indexing, classification, active-checkpoint compatibility, and recursive child routing
