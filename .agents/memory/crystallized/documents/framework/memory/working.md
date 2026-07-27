---
open-forge:
  description: Current Working Memory purpose, expected expiration, authority, extraction, resumability, and shipped Handoffs and Sessions roles
  responsibility: Define what makes temporary resumability state valid and what happens when its active need ends
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, MemoryModel, Working, Contextual, Resumability]
---

# Working Memory

## State Contract

[Working Memory](../../../../working/_working.md) contains plans, current priorities, intermediate state, active sessions, handoffs, and coordination needed to continue or resume work.

Agents may freely maintain Working Memory within the task's authority. It stays current, bounded, and cheap to reread. Its defining property is that the active need is expected to end.

Working Memory is resumability context, not accepted truth. A record may be partial or change rapidly while the work it supports remains active.

## Expiration And Extraction

When the active need ends, useful material is:

- Extracted to an accepted authoritative source
- Moved to Emerging for further development
- Consolidated into another live checkpoint
- Archived when its history remains useful
- Pruned when it has no plausible future value

Expected expiration does not require premature deletion. It requires the route to remain easy to review and prevents temporary state from silently becoming permanent knowledge.

## Shipped Roles

The standard [Handoffs](../../../../working/handoffs/_handoffs.md) route provides concise static transfer notes. A new Handoff is unnecessary when a more specific route already contains complete resume context.

The standard [Sessions](../../../../working/sessions/_sessions.md) route preserves fuller chronological context, reconstruction material, and bounded active checkpoints when work may cross a context boundary.

Projects may add plans, backlogs, checkpoints, or other Working scopes when those routes earn their cost. These are customizable working roles, not additional Memory states.

## Related Current Sources

- [Memory model](model.md)
- [Memory transitions](transitions.md)
- [Working runtime maintenance](../../maintenance/payload/agents/memory/working/_working.md)
- [Accepted state and synchronization](../truth.md)
