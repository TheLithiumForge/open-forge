---
open-forge:
  description: Current maintenance contract for the installable Decisions Memory entrypoint
  responsibility: Preserve accepted rationale, separation from current results, useful tradeoffs, recursive scope, and source alignment
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Decision, Rationale]
---

# Decisions Memory Maintenance Contract

## Source

[`src/extensions/planning/content/.agents/memory/crystallized/decisions/_decisions.md`](../../../../../../../../../src/extensions/planning/content/.agents/memory/crystallized/decisions/_decisions.md) is the canonical installed source for the optional Planning Extension's Decisions `entrypoint`. The repository [Decisions `entrypoint`](../../../../../../decisions/_decisions.md) dogfoods the base contract and adds repository decisions through generated `Entries`. A user-owned [overwrite](../../../../../../decisions/_decisions.overwrite.md) adds the repository-only knowledge-role helper without changing the installable payload.

The [Crystallized state contract](../../../../../framework/memory/crystallized.md) defines the role of Decisions inside accepted durable Memory. The [accepted-state relationship contract](../../../../../framework/truth.md#current-views-decisions-and-history) defines how rationale links to current results without competing with them.

## Contract

- The title is followed by `## What was chosen, why, and what follows from the choice?` and a short paragraph describing the folder.
- Frontmatter uses #Extension, #Memory, #Decision, and #CurrentTruth to classify this Planning route.
- Decisions record important accepted choices, why they were made and which applicable authority accepted them. Unaccepted proposals stay in candidate routes.
- The source for the current behavior, record, route, code, or external state defines the result and may link back to the Decision for why
- Alternatives, tradeoffs, constraints and consequences remain only when they provide future explanatory value. Keep one choice or a tightly related group per record and split unrelated choices.
- Overlapping Decisions are consolidated, reshaped, or linked when their reasoning agrees. Reasoning behind a replaced choice is archived or linked.
- Important disagreement or competing accepted reasoning is surfaced for discussion instead of being merged silently
- Current-result updates follow the [loader](../../loader.md) and [Memory](../_memory.md) rules. Decisions preserve useful rationale without becoming a competing exact specification.
- The repository-only knowledge-role helper stays in the user-owned overwrite. It never enters the canonical source, and installed wording remains understandable without it.

## Verification

- Use the [payload integrity checks](../../../_payload.md#verification) for the Planning Extension source and metadata rendering.
- Compare the packaged and repository category rules outside Entries. Verify the repository-only overwrite remains adjacent and its helper does not enter the shipped source.
- Review accepted authority, focused rationale and links to current results manually. Confirm Core alone has no Decisions category.
