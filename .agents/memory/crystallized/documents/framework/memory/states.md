---
open-forge:
  description: Current validity, expected lifetime, authority, capture, consolidation, and historical boundaries of the four standard Memory states
  responsibility: Define what belongs in Working, Emerging, Crystallized, and Archived Memory without prescribing a mandatory transition sequence
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, MemoryModel, Working, Emerging, Crystallized, Archived]
---

# Memory States

## State Summary

| State | Purpose | Normal status | Defining property |
|---|---|---|---|
| Working | Continue or resume active work | #Contextual | Expected expiration |
| Emerging | Preserve potentially reusable but unsettled material | #Contextual | Candidate value without acceptance |
| Crystallized | Preserve accepted durable state | #CurrentTruth | Consolidated present meaning |
| Archived | Preserve useful non-current history | #Contextual and historical | No current authority |

The states are semantic contracts, not quality scores or required maturity stages.

## Working

[Working Memory](../../../../working/_working.md) contains plans, current priorities, intermediate state, active sessions, handoffs, and coordination needed to continue or resume work.

Agents may freely maintain Working Memory within the task's authority. It should stay current, bounded, and cheap to reread. Its defining property is that the active need is expected to end.

When that need ends, useful material is:

- Extracted to an accepted authoritative source
- Moved to Emerging for further development
- Consolidated into another live checkpoint
- Archived when its history remains useful
- Pruned when it has no plausible future value

The shipped Handoffs route provides concise static transfer notes. Sessions preserve fuller chronological context and reconstruction material. Projects may add plans, backlogs, checkpoints, or other Working scopes when those routes earn their cost.

## Emerging

[Emerging Memory](../../../../emerging/_emerging.md) contains useful material whose validity, acceptance, knowledge role, or final destination remains unsettled.

The shipped routes provide distinct starting roles:

- Analysis preserves structured reasoning, investigation, and comparison
- Ideas preserve possibilities, experiments, and open questions
- Observations preserve grounded findings that may become reusable learning

An explicit request to preserve or explore an idea is enough to record it. An agent records a concrete observation after one occurrence when it is plausibly reusable, surprising, or costly enough to preserve.

Before creating a parallel record, later agents search for a matching scope and meaning and extend the existing record with new evidence when one exists. Repeated independent occurrences primarily trigger consolidation and a promotion proposal; recurrence is not required for initial capture.

Emerging material keeps source, scope, evidence, and uncertainty visible. It is refined, combined, promoted, archived, rejected, or pruned as its meaning becomes clearer.

## Crystallized

[Crystallized Memory](../../../../crystallized/_crystallized.md) contains accepted durable state within its stated scope.

It keeps one coherent current representation for each distinct question and scope. When accepted state changes, agents update, split, merge, or reshape the existing authoritative source rather than creating a competing current copy.

The shipped Decisions and Documents routes are useful defaults:

- Decisions preserve accepted rationale for important choices
- Documents integrate coherent current records or route to the systems that hold them

Other accepted records may live in scoped Crystallized routes, matching #Core routes, source code, or declared external systems. The [Memory authority boundary](model.md#authority-boundary) determines the appropriate destination.

## Archived

[Archived Memory](../../../../archived/_archived.md) preserves useful context after it stops governing current work.

Before archival, useful present meaning is extracted to its current authoritative source. Archived material retains:

- Its origin
- Why it became historical
- What replaced it when a replacement exists
- Enough scope and provenance for later interpretation

Archive children may mirror former routes, group material by origin, or use another useful taxonomy. They are organizational scopes, not additional Memory states.

Archived material supports history, reconstruction, rationale, comparison, and audit. It does not regain current authority merely because it is read.

## Related Current Sources

- [Memory model](model.md)
- [Memory transitions](transitions.md)
- [Accepted state and synchronization](../truth.md)
- [Current knowledge roles](../architecture.md#current-knowledge-roles)
