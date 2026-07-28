---
open-forge:
  description: Current maintenance contract for the installable Documents Memory entrypoint
  responsibility: Preserve durable accepted records, external-source delegation, routed destination authority, recursive scope, and source alignment
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Record]
---

# Documents Memory Maintenance Contract

## Source

[`src/open-forge/.agents/memory/crystallized/documents/_documents.md`](../../../../../../../../../src/open-forge/.agents/memory/crystallized/documents/_documents.md) is the canonical installed Documents `entrypoint`. The repository [Documents `entrypoint`](../../../../../../../crystallized/documents/_documents.md) dogfoods the same authored contract and adds repository current documents through generated `Entries`.

The [Crystallized state contract](../../../../../framework/memory/crystallized.md) defines Documents as a shipped Crystallized role. The [accepted-state relationship contract](../../../../../framework/truth.md#current-views-decisions-and-history) defines how current documents integrate accepted meaning.

## Contract

- Frontmatter preserves #LoadNow, #Memory, #Document, #Record, and #CurrentTruth classification
- Documents contain durable accepted long-form records or `routes` to the authoritative systems that contain them
- A document that delegates a subject to another authoritative source defers to that source
- A routed destination retains authority for its detailed truth rather than turning the containing document into a competing copy
- Copy-ready creation sources belong in [Templates](../../../../../framework/primitives/templates.md), while Documents retain accepted current content
- Existing current documents are reshaped when accepted meaning changes instead of accumulating parallel current views

## Verification

- Installation and `route` tests verify Documents loading, indexing, classification, and managed reconciliation through scopes
