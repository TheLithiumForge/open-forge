---
open-forge:
  description: Current maintenance contract for the installable Analysis Memory entrypoint
  responsibility: Preserve structured candidate reasoning, evidence and assumption visibility, freshness checks, recursive scope, and source alignment
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Analysis, Reasoning, Candidate]
---

# Analysis Memory Maintenance Contract

## Source

[`src/extensions/planning/content/.agents/memory/emerging/analysis/_analysis.md`](../../../../../../../../../src/extensions/planning/content/.agents/memory/emerging/analysis/_analysis.md) is the canonical installed source for the optional Planning Extension's Analysis `entrypoint`. The repository [Analysis `entrypoint`](../../../../../../../emerging/analysis/_analysis.md) dogfoods the same authored contract and may add local generated `Entries`.

The [Emerging state contract](../../../../../framework/memory/emerging.md) defines Analysis as structured reasoning whose acceptance or final destination remains unsettled.

## Contract

- Frontmatter uses #Extension, #Memory, #Analysis, #Reasoning, #Contextual, and #Candidate to classify this optional Planning route
- Analysis keeps its question, evidence, assumptions, limits, and current conclusion visible
- Assumptions are rechecked before later work relies on the analysis
- Analysis remains contextual until a separate acceptance source establishes a durable result

## Verification

- Core-only installation verifies that the optional Analysis route is absent; Planning installation verifies that it installs, indexes, retains its full source classification, and supports recursive child routing
