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

- Frontmatter uses #Extension, #Memory, #Analysis, #Contextual, and #Candidate to classify this optional Planning route
- Analysis keeps its question, evidence, assumptions, limits, and current conclusion visible
- Assumptions are rechecked before later work relies on the analysis. Exploratory Analysis may change while its question is open, and experimental use does not seal or accept it.
- Analysis remains contextual until a separate acceptance source establishes a durable result. Check Entries when prior reasoning may help.

- When Analysis is handed off for execution, preserve the version used. Later implementation findings and execution evidence belong in the active working record.

## Verification

- Use the [payload integrity checks](../../../_payload.md#verification) for the Planning Extension source and metadata rendering.
- Compare category rules with the repository counterpart outside Entries. Confirm Core alone has no Analysis category.
- Manually check a record's evidence, assumptions and uncertainty, then verify an execution handoff preserves its consumed version and leaves later findings in Working.
