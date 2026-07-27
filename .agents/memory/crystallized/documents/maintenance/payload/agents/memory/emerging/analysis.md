---
open-forge:
  description: Current maintenance contract for the installable Analysis Memory entrypoint
  responsibility: Preserve structured candidate reasoning, evidence and assumption visibility, freshness checks, recursive scope, and source alignment
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Analysis, Reasoning, Candidate]
---

# Analysis Memory Maintenance Contract

## Source

[`src/open-forge/.agents/memory/emerging/analysis/_analysis.md`](../../../../../../../../../src/open-forge/.agents/memory/emerging/analysis/_analysis.md) is the canonical installed Analysis entrypoint. The repository [Analysis entrypoint](../../../../../../../emerging/analysis/_analysis.md) dogfoods the same authored contract and may add local generated entries.

The [Emerging state contract](../../../../../framework/memory/emerging.md) defines Analysis as structured reasoning whose acceptance or final destination remains unsettled.

## Contract

- Frontmatter preserves #LoadNow, #Memory, #Analysis, #Reasoning, #Contextual, and #Candidate classification
- Analysis keeps its question, evidence, assumptions, limits, and current conclusion visible
- Assumptions are rechecked before later work relies on the analysis
- Analysis remains contextual until a separate acceptance source establishes a durable result
- The installed route begins empty and supports ordinary recursive scope

## Verification

- Installation and route tests verify Analysis loading, indexing, classification, and recursive child routing
- Compare canonical source and dogfood authored content outside generated `Entries`
