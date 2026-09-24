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

- Frontmatter preserves #Extension, #Memory, #Observation, #AgentLearning, #OrganicGrowth, #Contextual, and #Candidate classification
- Concrete occurrences or patterns noticed in evidence are saved before handoff or closeout when they may matter later
- One useful, surprising, or costly occurrence may justify capture
- Later occurrences extend an existing Observation when scope and meaning align
- Recurrence strengthens the case for consolidation or promotion without validating or accepting the Observation by itself
- The parent Emerging `entrypoint` supplies general review, contextual status, uncertainty, transition, and “no useful candidate” behavior instead of repeating those Axioms here
- A required observation write that is blocked is reported rather than silently discarded

## Verification

- Source/payload parity plus installation and index coverage verify Observations classification and generated navigation. Installation coverage distinguishes Core alone from the Observations and Handoffs Extension; source review confirms the on-demand Extension classification. The frozen MVP does not test or implement that behavior.
