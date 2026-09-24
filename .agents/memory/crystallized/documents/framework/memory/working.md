---
open-forge:
  description: Current Working Memory purpose, expected expiration, authority, extraction, resumability, and optional Checkpoints and Handoffs roles
  responsibility: Define what makes temporary resumability state valid and what happens when its active need ends
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, MemoryModel, Working, Resumability]
---

# Working Memory

## State Contract

[Working Memory](../../../../../../src/open-forge/.agents/memory/working/_working.md) contains plans, current priorities, intermediate state, checkpoints, handoffs, and coordination needed to continue or resume work.

Agents may freely maintain Working Memory within the task's authority. It stays current, bounded, and cheap to reread. Its defining property is that the active need is expected to end.

Working Memory does not establish acceptance by itself. Most records are contextual, partial, or changing. It may preserve an explicitly accepted temporary choice when the source, scope, and expected expiration are clear. That scoped acceptance does not make the choice durable or suitable for Crystallized Memory.

## Expiration And Extraction

When the active need ends, useful material is:

- Extracted to an accepted authoritative source
- Moved to Emerging for further development
- Consolidated into another live checkpoint
- Archived when its history remains useful
- Pruned when it has no plausible future value

Expected expiration does not require premature deletion. It requires the `route` to remain easy to review and prevents temporary state from silently becoming permanent knowledge.

## Optional Roles

The optional [Checkpoints](../../../../../../src/extensions/planning/content/.agents/memory/working/checkpoints/_checkpoints.md) `route` from the Planning Extension keeps the current state, current step, and next steps of one active workstream in a form any future reader can use. An active Checkpoint carries #Active and #KeepInMind only while active, and is refreshed after material state changes and restoration.

The optional [Handoffs](../../../../../../src/extensions/observations-and-handoffs/content/.agents/memory/working/handoffs/_handoffs.md) `route` from the Observations and Handoffs Extension provides sealed snapshots for actual transfers and explicitly planned resumptions after context boundaries. Create one only when the boundary state must remain stable while the active Checkpoint may continue to change. The snapshot records its boundary status directly and remains unchanged while serving as a Handoff. Later state belongs in the Checkpoint or a new Handoff. When the material changes category, its destination's rules apply. Routine pauses and ordinary closeout do not require one.

These Extension routes are not Core defaults. Projects may add plans, backlogs, history, or other Working scopes when those `routes` earn their cost. These are customizable working roles, not additional Memory states.

## Related Current Sources

- [Memory model](model.md)
- [Memory transitions](transitions.md)
- [Working runtime maintenance](../../maintenance/payload/agents/memory/working/_working.md)
- [Accepted state and synchronization](../truth.md)
