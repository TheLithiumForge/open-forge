---
open-forge:
  description: Current maintenance contract for the installable Documents Memory entrypoint
  responsibility: Preserve complete current explanations, external-source delegation, routed destination authority, recursive scope, and source alignment
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Record]
---

# Documents Memory Maintenance Contract

## Source

[`src/extensions/project-documents/content/.agents/memory/crystallized/documents/_documents.md`](../../../../../../../../../src/extensions/project-documents/content/.agents/memory/crystallized/documents/_documents.md) is the canonical installed Documents `entrypoint` supplied by the Project Documents Extension. The repository [Documents `entrypoint`](../../../../../_documents.md) dogfoods the same authored contract and adds repository current documents through generated `Entries`.

The [Crystallized state contract](../../../../../framework/memory/crystallized.md) defines Documents as a shipped Crystallized role. The [accepted-state relationship contract](../../../../../framework/truth.md#current-views-decisions-and-history) defines how current documents integrate accepted meaning.

## Contract

- Frontmatter preserves #Extension, #Memory, #Document, #Record, and #CurrentTruth classification
- Documents explain accepted current knowledge that needs one coherent view, or link to the system that contains it
- A document that assigns a subject to another authoritative source follows that source for its detail
- A routed destination still defines its own detail. The containing document does not become a competing copy
- Copy-ready creation sources belong in [Templates](../../../../../framework/primitives/templates.md), while Documents retain accepted current content
- Existing current documents are reshaped when accepted meaning changes instead of accumulating parallel current views
- Documents are arranged top-down: top-level Documents give the overview and summarize the narrower Documents they link to, and each narrower Document explains its own part and links back up
- A change to a narrower Document also updates the summaries above it that no longer match. This top-down arrangement is the Extension's convention, not a Core requirement: Core only requires that detail live in the narrowest defining source and that accepted changes reach it

## Verification

- Installation and `route` tests verify that Core alone does not provide Documents and that the Project Documents Extension supplies its loading, indexing, classification, and managed reconciliation through scopes
