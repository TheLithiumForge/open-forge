---
open-forge:
  description: Current maintenance contract for the installable Decisions Memory entrypoint
  responsibility: Preserve accepted rationale, separation from current results, useful tradeoffs, recursive scope, and source alignment
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Decision, Rationale]
---

# Decisions Memory Maintenance Contract

## Source

[`src/open-forge/.agents/memory/crystallized/decisions/_decisions.md`](../../../../../../../../../src/open-forge/.agents/memory/crystallized/decisions/_decisions.md) is the canonical installed Decisions entrypoint. The repository [Decisions entrypoint](../../../../../../../crystallized/decisions/_decisions.md) dogfoods the base contract, adds repository decisions through generated entries, and adds one repository-only authoring-helper Axiom that is not part of the installable payload.

The [Crystallized state contract](../../../../../framework/memory/crystallized.md) defines the role of Decisions inside accepted durable Memory. The [accepted-state relationship contract](../../../../../framework/truth.md#current-views-decisions-and-history) defines how rationale links to current results without competing with them.

## Contract

- Frontmatter preserves #LoadNow, #Memory, #Decision, #Rationale, and #CurrentTruth classification
- Decisions preserve accepted rationale for important choices that future work may need to understand
- The chosen behavior, current record, route, code, or external state remains expressed by its authoritative source and may link back to the Decision for why
- Alternatives, tradeoffs, constraints, and consequences remain only when they provide future explanatory value
- Overlapping Decisions are consolidated, reshaped, or linked when their accepted rationale is compatible, and rationale behind a replaced choice is archived or linked
- Material divergence or competing accepted rationale is surfaced for discussion instead of being merged silently
- Accepted changes update the current result and preserve useful rationale from the previous choice without leaving two competing current outcomes
- The repository-only knowledge-role helper never enters the canonical source, and installed wording remains understandable without it
- The installed route begins empty and supports ordinary recursive scope

## Verification

- Installation and route tests verify Decisions loading, indexing, classification, and scoped route updates
- Compare canonical source and dogfood authored content outside generated `Entries`
