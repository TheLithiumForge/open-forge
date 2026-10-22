---
open-forge:
  description: Current maintenance contract for the installable Observations Memory entrypoint
  responsibility: Preserve evidence-backed occurrences and patterns, recurrence behavior, promotion boundaries, recursive scope, and source alignment
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Observation, AgentLearning, Candidate]
---

# Observations Memory Maintenance Contract

## Source

[`src/extensions/observations-and-handoffs/content/.agents/memory/emerging/observations/_observations.md`](../../../../../../../../../src/extensions/observations-and-handoffs/content/.agents/memory/emerging/observations/_observations.md) is the canonical installed Observations `entrypoint` supplied by the Observations and Handoffs Extension. The repository [Observations `entrypoint`](../../../../../../../emerging/observations/_observations.md) dogfoods the same authored contract and adds repository observations through generated `Entries`.

The [Emerging state contract](../../../../../framework/memory/emerging.md) defines the observation capture threshold and recurrence boundary. The [transition contract](../../../../../framework/memory/transitions.md#capture-and-consolidation) owns consolidation and promotion.

## Contract

- The title is followed by `## What was observed, and why might it matter later?` and a short paragraph describing the folder.
- Frontmatter preserves #Extension, #Memory, #Observation, #AgentLearning, #Contextual, and #Candidate classification
- Concrete occurrences or patterns noticed in evidence are saved before handoff or closeout when they may matter later
- One reusable, surprising or costly occurrence may justify capture. Keep concrete evidence and occurrence details clear enough to verify and reuse.
- Later occurrences extend an existing Observation when scope and meaning align
- Recurrence strengthens the case for consolidation or promotion without validating or accepting the Observation by itself
- The parent Emerging catalogue supplies general review, uncertainty and transitions. This leaf keeps its own evidence and recurrence rules.
- A required observation write that is blocked is reported rather than silently discarded

## Verification

- Use the [payload integrity checks](../../../_payload.md#verification) for the Observations and Handoffs Extension source and metadata rendering.
- Compare shared source and repository rules outside Entries. Confirm the route stays on demand and is absent from Core alone.
- Manually review evidence, matching recurrence, blocked-write reporting and candidate status. Repetition and packaging checks do not establish validity or acceptance.
