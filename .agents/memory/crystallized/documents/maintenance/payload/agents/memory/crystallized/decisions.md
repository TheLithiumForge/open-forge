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

- Frontmatter uses #Extension, #Memory, #Decision, #Rationale, and #CurrentTruth to classify this optional Planning route
- Decisions record important accepted choices and why they were made
- The source for the current behavior, record, route, code, or external state defines the result and may link back to the Decision for why
- Alternatives, tradeoffs, constraints, and consequences remain only when they provide future explanatory value
- Overlapping Decisions are consolidated, reshaped, or linked when their reasoning agrees. Reasoning behind a replaced choice is archived or linked
- Important disagreement or competing accepted reasoning is surfaced for discussion instead of being merged silently
- Accepted changes update the current result and preserve useful rationale from the previous choice without leaving two competing current outcomes
- The repository-only knowledge-role helper stays in the user-owned overwrite. It never enters the canonical source, and installed wording remains understandable without it.

## Verification

- Core-only installation verifies that the optional Decisions route is absent; Planning installation verifies that it installs, indexes, retains its full source classification, and reconciles through scopes
- Verify that the repository-only overwrite remains adjacent and loadable
