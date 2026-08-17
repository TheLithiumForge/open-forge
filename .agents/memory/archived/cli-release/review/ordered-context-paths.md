---
open-forge:
  description: Historical ordered-context-path review record retained after Queue 14 settled
  responsibility: Preserve historical path-projection analysis without defining current `context` behavior
  tags: [Memory, Archived, Contextual, Historical, CLI, Release, PreliminaryDesign, Context, Path, Projection]
---

> **Status: Archived historical context.** Queue 14 is settled. The complete
> historical body below is preserved for context only; it does not define current
> `context` behavior or authority. Use the [current `context` contract
> set](../../../crystallized/documents/cli/contracts/context/_context.md) and [Review
> Queue](queue.md) for current meaning and
> disposition.
>
> **Origin:** `.agents/memory/working/cli-release/review/ordered-context-paths.md`.
> **Archived because:** Queue 14 integrated the path projection into `context`.

# Ordered Context Paths Preliminary Design

## Status

This file remains unaccepted preliminary analysis. The maintainer accepted
`paths` as a distinct Context content projection. Compact view emits canonical
paths in exact context order. Expanded view adds sequence, ID, layer, inclusion
category, reasons, and provenance. No new command or path-specific view flag is
needed. CLI-generated per-source data uses the separate `metadata` content part.

## User Job

Before paying to read bodies, an agent wants the exact ordered list of source
layers that a `context` invocation would emit, with enough provenance to decide
whether to request complete content. A base and overwrite remain one logical
source with two ordered physical layers.

## Accepted Direction

Keep this as a result projection of [`context`](../../../crystallized/documents/cli/contracts/context/_context.md),
not a standalone command and not a graph-path query.

The projection must reuse the exact context request and resolver:

- Startup-required closure.
- Explicit source operands in operand order.
- `--additions-only` when selected.
- Explicit link expansion at the requested depth.
- Deduplication and retained inclusion reasons.
- Base followed immediately by overwrite.
- Existing complete, attention, incomplete, invalid, blocked, failed, and
  interrupted results.

It must not reimplement loading, infer relevance, add sources, or use path order
as authority.

## Accepted Result Boundary

Each row should contain:

- Sequence number.
- Automatic source ID or null for contained linked content outside `.agents`.
- Canonical workspace-relative path.
- Base or overwrite layer.
- Inclusion reasons and inclusion category, such as startup, explicit selection,
  or link expansion.
- Requested content or section selectors retained as request metadata. They do
  not change source selection or order.

Compact human output begins with semantic result, context coverage, and path
count, followed by one canonical path per ordered physical layer. A
non-complete result adds the concise required finding and next action. Expanded
human and structured output retain the same rows, order, and complete
provenance. A result limit cannot silently truncate; an unmet requested closure
remains visibly incomplete.

## Existing-Surface Relationship

The `metadata` content part emits complete CLI-generated source blocks. The
accepted `paths` part provides the smaller operation-level ordered path list.
They reuse the same resolver but answer different projection jobs.

The global compact view renders one ordered path row per physical layer, while
the default expanded view retains identity blocks and reasons. `--content`
continues to select result facts; `--view` changes only human rendering.

## Rejected Boundaries

- A new `context paths` operation with separate selection semantics.
- Shortest paths, paths between arbitrary graph nodes, or inferred relevance.
- Receipt-based suppression or persistent context state.
- Returning only paths while hiding incomplete closure evidence.

## Remaining Conformance Question

1. How should an overwrite layer remain obvious without repeating expanded
   provenance?

## Evidence Plan

- Run the same startup, explicit-source, additions-only, link-depth, overwrite,
  and incomplete fixtures through full context, `--content=metadata`, and
  `--content=paths`.
- The candidate passes semantic parity only when source-layer identity, order,
  inclusion reasons, findings, and semantic result have zero differences from
  full context.
- Verify that compact paths reduce output bytes or tokens without hiding an
  incomplete closure, while expanded paths retain the provenance needed to
  explain every selected layer.

## Related Evidence

- [Council Synthesis](path-reference-council.md)
- [Context contract set](../../../crystallized/documents/cli/contracts/context/_context.md)
