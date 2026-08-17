---
open-forge:
  description: Historical reference-query review record retained after Queue 15 settled
  responsibility: Preserve historical reference-query analysis without defining current `references` behavior
  tags: [Memory, Archived, Contextual, Historical, CLI, Release, PreliminaryDesign, Reference, Link, Incoming, Outgoing]
---

> **Status: Archived historical context.** Queue 15 is settled. The complete
> historical body below is preserved for context only; it does not define current
> `references` behavior or authority. Use the [current `references` contract
> set](../../../crystallized/documents/cli/contracts/references/_references.md) and
> [Review Queue](queue.md) for current meaning
> and disposition.
>
> **Origin:** `.agents/memory/working/cli-release/review/reference-queries.md`.
> **Archived because:** Queue 15 settled the current `references` contract.

# Reference Queries Preliminary Design

## Status

The maintainer accepted one `references` operation that returns separate incoming
and outgoing sections at direct depth one. This contextual design refines details
that remain open under `CLI-D085`. The file remains unaccepted preliminary
analysis; it does not reopen the accepted operation, default inclusion of both
directions, or one-hop boundary.

## User Jobs

- What ordinary local Markdown references leave this source?
- Which declared source universe contains ordinary links to this target?
- Where is each reference authored, and did it resolve safely?

Flat `find`, route topology, and context body output do not report these jobs.
Incoming lookup in particular requires a reverse scan with an explicit
completeness boundary.

## Accepted Core And Open Details

Design one read-only operation for one selected source:

```text
open-forge references <source-reference>
  [--direction=<incoming|outgoing|both>]
  [scan-scope]
  [global flags]
```

The shown direction flag and scan-scope syntax are candidates, not accepted
spellings. Omission returns both directions, grouped separately in human output
and typed separately in structured output. A direction filter only trims the
returned and required work; it does not choose a different operation. The first
version is direct and one hop only. It inspects ordinary local Markdown links,
not route, loading, overwrite, or generated-navigation edges.

The source reference names the source whose outgoing links are read and the
target whose incoming links are found. The result preserves separate direction,
ordering, scan-boundary, and completeness facts even though one invocation may
return both sections.

Outgoing results preserve document and occurrence order. Incoming results use a
declared scan universe and deterministic source-path/location order. The first
useful incoming universe may be all eligible `.agents` Markdown sources, matching
the known bounded `find` universe. Broader workspace Markdown or route-scoped
universes require an explicit later decision.

## Human Views And Levels

The global expanded view is the default. Both human views keep incoming and
outgoing sections separate and organize traversed results by reference depth.
The direct first version therefore has one level.

Compact view returns a token-friendly list. Coverage belongs to each requested
direction so a complete outgoing read cannot hide an incomplete incoming scan:

```text
result=complete  references=3

Outgoing — coverage=complete  references=1
  Level 1
    .agents/memory/_memory.md

Incoming — coverage=complete  references=2  scan=.agents-markdown
  Level 1
    .agents/directives/security.md
    .agents/memory/project-a/_project-a.md
```

A non-complete compact result includes the concise required finding and next
action before any safe reference rows. Each requested section still states its
own coverage and, for incoming results, scan universe. If one requested section
is incomplete, the invocation is incomplete even when the other section is
complete. A direction excluded by an explicit filter is absent and not evaluated;
it is never represented as complete or incomplete.

With the default `both` request, aggregate reference coverage is complete only
when both the outgoing read and declared incoming scan are complete. If either
requested direction has incomplete coverage, aggregate coverage is incomplete
even when the other direction completed. A blocked, failed, or interrupted
direction also prevents a complete invocation result. No requested section can
mask another section's status. Exact precedence between simultaneous
non-complete statuses belongs in the later shared result contract. An explicit
direction filter removes the other direction from both work and completeness
accounting.

Expanded view shows authored direction, locations, resolution, and provenance:

```text
Outgoing — Level 1
  .agents/loader.md:96 -> .agents/directives/_directives.md
    resolved: directives
    status: complete

Incoming — Level 1
  .agents/directives/security.md:8 -> .agents/loader.md
    status: complete
```

Structured output retains complete occurrences, direction, depth, parent
occurrence when applicable, locations, raw destination, resolved target, and
per-direction coverage and status regardless of human view. Incoming coverage
also retains its exact scan universe and inspected-source evidence. The
invocation result derives from every requested direction rather than one
aggregate row count or undifferentiated coverage field.

If bounded depth is accepted later, compact output remains grouped by numbered
levels. Expanded output may render a tree, but every child must retain the exact
occurrence that reached it so cycles, duplicate paths, and several parents do not
become a misleading single-parent hierarchy.

## Reference Facts

Each occurrence should expose:

- Source ID when available and canonical source path.
- Source location.
- Raw authored destination.
- Resolved target ID when available and canonical target path.
- Fragment when present.
- Incoming or outgoing direction.
- Resolution and containment status.

Each requested section exposes its own completeness. The incoming section also
exposes the declared reverse-scan universe. Outgoing completion does not make an
incomplete incoming scan or aggregate invocation look complete.

Contained local targets outside `.agents` may have a null Open Forge ID. External
HTTP or HTTPS references may be reported as unchecked outgoing facts but are
never fetched. Broken, unsupported, unsafe, or ambiguous references remain
visible; they are not silently omitted.

## Overwrite Layers

- A valid overwrite operand resolves to its base logical source under the shared
  source-reference contract.
- Outgoing inspection reads the base layer first and then its overwrite layer.
  Each link occurrence retains the physical layer and path where it was authored.
- A link that targets an overwrite path resolves to the complete logical target
  while retaining the targeted physical layer as evidence.
- An orphan or ambiguous overwrite is never an independent source or target. It
  produces the applicable incomplete or blocked evidence.

## Boundaries

- A local reference never becomes route membership, loading, authority, or
  semantic relatedness.
- The operation does not load target bodies into context.
- No transitive closure, impact analysis, network checking, diagnosis catalogue,
  repair, or persistent reverse index belongs in the first version. A later
  bounded reference depth is preferable to a graph command only if repeated
  workflows prove direct references insufficient and define direction, cycles,
  ordering, and completeness precisely.
- Complete empty incoming results are valid only after the declared scan
  universe completed.
- Reached bounds use existing incomplete or blocked result meaning rather than a
  silent row cap.

The operation reuses the accepted seven semantic results. `complete` includes a
complete empty edge set. `attention` retains safe non-blocking authored-form or
identity findings. `incomplete` means safe edges are available but the declared
scan or resolution coverage could not finish. Invalid input is `invalid`; an
unsafe or unresolved operation boundary is `blocked`; unexpected failure and
caller cancellation remain `failed` and `interrupted`.

## Relationship With Existing Commands

`context --follow-links` adds reachable target bodies to selected context. This
operation reports edge-only facts without content expansion. `doctor` remains
the complete diagnosis and recommendation operation. The accepted separate
repair/fix operation requires its own mutation authority; its name remains open.

## Remaining Review Questions

1. Should the initial incoming scan universe be all eligible `.agents` Markdown,
   an exact route subtree, or an explicit required choice?
2. Should outgoing results include external unchecked references by default?
3. Is `--direction=incoming|outgoing|both` clearer than two Boolean trim flags?

## Evidence Plan

- Build fixtures for base and overwrite layers, duplicate links, fragments,
  missing targets, external links, cycles, aliases, path escapes, unsupported
  syntax, and targets outside `.agents` but inside the workspace.
- The candidate passes semantic coverage only when every expected occurrence,
  source location, physical layer, resolved target, and status matches the
  hand-authored fixture and no incomplete scan reports `complete`.
- Compare incoming lookup with a manual workspace search. The operation proves
  useful when it replaces that search with one declared complete scan and a
  typed result.
- Record elapsed time and inspected-source count for the `.agents` universe and
  representative route-scoped candidates before accepting a default scan scope.

## Related Evidence

- [Council Synthesis](path-reference-council.md)
- [Context Interface Contract](../../../crystallized/documents/cli/contracts/context/interface.md#explicit-link-expansion)
- [Historical Local Reference Contract](../../cli-v2/documents/contracts/local-references.md)
