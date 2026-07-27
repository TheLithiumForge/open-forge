---
open-forge:
  description: Current maintenance contract for the installable Observations Memory entrypoint
  responsibility: Preserve grounded agent learning, evidence and uncertainty, continuity review, promotion boundaries, recursive scope, and source alignment
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Observation, AgentLearning, Candidate]
---

# Observations Memory Maintenance Contract

## Source

[`src/open-forge/.agents/memory/emerging/observations/_observations.md`](../../../../../../../../../src/open-forge/.agents/memory/emerging/observations/_observations.md) is the canonical installed Observations entrypoint. The repository [Observations entrypoint](../../../../../../../emerging/observations/_observations.md) dogfoods the same authored contract and adds repository findings through generated entries.

The [Emerging state contract](../../../../../framework/memory/emerging.md) defines the observation capture threshold and recurrence boundary. The [transition contract](../../../../../framework/memory/transitions.md#capture-and-consolidation) owns consolidation and promotion.

## Contract

- Frontmatter preserves #KeepInMind, #Memory, #Observation, #AgentLearning, #OrganicGrowth, #Contextual, and #Candidate classification
- Grounded findings that may matter after current context are captured before handoff or closeout, while “no observation warranted” remains valid
- Source, scope, uncertainty, supporting evidence, and current validation status remain visible
- Observations remain contextual until an appropriate authoritative source accepts, rejects, or supersedes their outcome
- One plausibly reusable, surprising, or costly occurrence may justify capture; recurrence primarily raises the case for consolidation and promotion
- A required observation write that is blocked is reported rather than silently discarded
- The installed route begins empty and supports ordinary recursive scope

## Verification

- Loading and installation closure tests verify #KeepInMind traversal, classification, generated navigation, and recursive child routing
- Compare canonical source and dogfood authored content outside generated `Entries`
