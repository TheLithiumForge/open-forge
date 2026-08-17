---
open-forge:
  description: Historical rejected public graph capability with preserved reasoning and dissent
  responsibility: Preserve why current domain projections should precede any public graph surface
  tags: [Memory, Archived, Contextual, Historical, CLI, PreliminaryDesign, Graph, Route, Reference]
---

# Graph Views Preliminary Design

## Status

This file was archived from the
[CLI Review Queue](../cli-release/review/queue.md) on 2026-08-15 after
the maintainer rejected a public graph capability. It preserves the historical
reasoning and no longer belongs to active review. The accepted disposition is
recorded in the [Decision Agenda](../../working/cli-release/decision-agenda.md).
Focused route and reference results remain the public surfaces.

## Why A General Graph Is Not Ready

“Show the graph” does not identify:

- Node kinds.
- Edge kinds.
- Direction.
- Seed or scan universe.
- Depth and cycle behavior.
- Ordering and completeness.
- Whether generated navigation, loading, links, routes, or overwrites are mixed.

Those relationships have different Framework meanings and authoritative
sources. A generic node/edge output risks making local links look like loading,
generated Entries look authoritative, or context order look like precedence.

## Accepted Disposition

Keep the accepted per-invocation content graph as an internal shared fact model.
Expose focused domain results first:

- Context paths through `context`.
- Routed topology through the route catalogue and `route inspect`.
- Ordinary local links through the reference operation.
- Diagnosis through `doctor`.

Do not add a general query language, shortest path, transitive closure, impact
analysis, graph export, relevance score, or persistent graph index.

## Preserved Dissent

One council perspective supported a fixed one-hop typed neighbor view with an
exact seed, edge type, direction, and bound. It could save agents several calls
when they need to combine different relationship types.

A representative mixed-edge job is: “For this source, show its direct route
parent and children and every ordinary local link that points to it.” Focused
operations can answer this with a route result and an incoming-reference result.
A graph view would be useful only if combining those results repeatedly costs
enough calls or duplicated output to justify another public surface.

The opposing view is stronger for the first version: a one-hop graph still
duplicates route and reference operations, while shell composition of stable
structured outputs preserves exact meaning.

## Historical Promotion Test

The council considered the thresholds below before the maintainer rejected a
graph operation. They do not create a live graph fallback. Repeated direct-link
workflows may instead inform the bounded reference-depth question under
`CLI-D085`.

The historical graph test required all of these:

- The same mixed-edge question occurs often.
- At least three recurring mixed-edge workflows require more than two focused
  calls or repeat more than half of their identity and provenance output.
- Node and edge kinds are fixed and unambiguous.
- A one-hop bound is insufficient or demonstrably sufficient.
- Completeness, ordering, cycle, and containment behavior can be stated without
  a generic query language.
- The view can cite domain evidence instead of defining another version of the
  relationship semantics.

A one-hop prototype passes only when every node, edge, order, status, and
completeness fact matches the contributing domain results exactly and it adds no
generic `related` relation. Otherwise keep shell or agent-side composition.

## Remaining Reference Question

1. Do repeated reference workflows justify an explicit bounded depth after the
   direct one-hop operation is proven?

## Related Evidence

- [Council Synthesis](../../working/cli-release/review/path-reference-council.md)
- [Archived Discovery Family](../../archived/cli-release/review/discovery-family.md)
- [Reference Queries](../../working/cli-release/review/reference-queries.md)
- [Route Catalogue](../../working/cli-release/review/route-catalog.md)
