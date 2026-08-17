---
open-forge:
  description: Historical rejected route-neighborhood capability whose structural result moved to route list
  responsibility: Preserve why route-neighborhood paths do not need a standalone operation
  tags: [Memory, Archived, Contextual, Historical, CLI, PreliminaryDesign, Route, Neighborhood, Path]
---

# Route Neighborhood Preliminary Design

## Status

This file was archived from the
[CLI Review Queue](../cli-release/review/queue.md) on 2026-08-15 after
the maintainer rejected a separate neighborhood capability. It preserves the
historical reasoning and no longer belongs to active review. The accepted
disposition is recorded in the
[Decision Agenda](../../working/cli-release/decision-agenda.md). `route list`
owns bounded structural neighborhood output.

## User Job

Given one routed source, an agent wants its ordered parent chain and direct
children as paths without loading rules, content, measurements, or a complete
workspace catalogue.

## Current Coverage

[`route inspect`](../../crystallized/documents/cli/contracts/route/inspect/_inspect.md)
already reports the selected source's chain, depth, parent, direct child count,
descendant count, loading behavior, and detailed profile. The accepted
[Route Catalogue](../../working/cli-release/review/route-catalog.md) capability
reports actual subtree paths by depth; its defaults remain open.

## Accepted Disposition

Do not add `route neighbors` or `discover neighborhood` as a standalone
operation in the first version.

Use these boundaries:

- `route inspect <source>` for one source's detailed profile and exact parent
  chain.
- `route list <source> --depth=<non-negative-integer|all>` for actual child and
  bounded descendant rows.

The global compact view keeps `route inspect` token-friendly without adding a
path-specific neighborhood projection. `route list` includes the selected source
at relative depth `0` and its requested descendants. It does not add the
selected source's parent chain to that subtree result.

## Historical Alternatives

### Standalone Neighborhood Operation

This is easy to find but duplicates route catalogue and inspect. The maintainer
rejected it rather than retaining an evidence-gated fallback.

### Route Catalogue Only

This is the smallest surface. A source plus depth `1` answers direct structural
neighborhood questions; route inspect remains the provenance operation.

## Remaining Route-List Decisions

The rejected neighborhood capability adds no separate open decision. The
remaining `route list` defaults and filters are exactly those recorded under
`CLI-D084`: operand-free root selection, omitted depth, and first-version
metadata filters.

## Evidence Plan

- Run fixed one-hop tasks for parent chain, direct children, and depth-1
  descendants using `route inspect` and the proposed `route list` result.
- Verify that `route list <source> --depth=1` and `route inspect` provide every
  expected path and relation for the fixed tasks without a separate neighborhood
  surface.
- Fixtures must include routed leaves, entrypoints, detached entrypoints,
  overwrite operands, and structurally ambiguous routes.

## Related Evidence

- [Council Synthesis](../../working/cli-release/review/path-reference-council.md)
- [Route Catalogue](../../working/cli-release/review/route-catalog.md)
- [Route Inspect contract set](../../crystallized/documents/cli/contracts/route/inspect/_inspect.md)
