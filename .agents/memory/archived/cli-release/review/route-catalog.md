---
open-forge:
  description: Historical route-catalogue review record retained after Queue 13 settled
  responsibility: Preserve historical route-catalogue analysis without defining current `route list` behavior
  tags: [Memory, Archived, Contextual, Historical, CLI, Release, PreliminaryDesign, Discovery, Route, Catalogue, Depth]
---

> **Status: Archived historical context.** Queue 13 is settled. The complete
> historical body below is preserved for context only; it does not define current
> `route list` behavior or authority. Use the [current `route list` contract
> set](../../../crystallized/documents/cli/contracts/route/list/_list.md) and [Review
> Queue](queue.md) for current meaning and
> disposition.
>
> **Origin:** `.agents/memory/working/cli-release/review/route-catalog.md`.
> **Archived because:** Queue 13 settled the current `route list` contract.

# Route Catalogue Preliminary Design

## Status

The maintainer accepted the read-only `route list` capability, global compact
and expanded human views, and complete shared structured result. This contextual
record remains unaccepted preliminary analysis and keeps the unresolved default
depth, operand-free root set, and exact metadata-filter questions visible.

## User Job

An agent needs one atomic answer to questions such as:

- Which routed material exists in this workspace?
- Which routes exist below Patterns?
- What is available one, two, or all structural levels below this route?
- Which exact IDs and paths can be passed to `route inspect` or `context`?

This job closes a real gap. `find` is flat and includes unrouted Markdown.
`route inspect` profiles one subject and reports counts but not complete child
paths. Generated Entries expose only direct navigation and may be stale.

## Accepted Core Direction

Add one read-only route operation that enumerates authored topology:

```text
open-forge route list [source-reference]
  [--depth=<non-negative-integer|all>]
  [global flags]
```

The operation and structural-depth dimension are accepted. The bullets below
separate accepted behavior from candidate defaults that remain unresolved under
`CLI-D084`:

- **Open default:** with no source, the current candidate selects every root
  route. The exact operand-free root set remains under `CLI-D084`.
- With one routed entrypoint source, enumerate that exact route and its routed
  descendants.
- Include the selected root itself at relative depth `0`.
- **Open default:** omitted `--depth` is currently proposed as `1`; depth `0`
  remains the smaller alternative. Explicit `all` selects the complete
  descendant closure.
- Include entrypoints and ordinary routed leaves.
- Include a routed native source such as `SKILL.md` as a routed leaf when its
  source contract establishes route metadata.
- Exclude unrouted Markdown and overwrite companions as independent rows.
- Resolve from current authored filesystem topology and source contracts, not
  from trusting generated Entries.

Depth counts route ancestry edges. It is not filesystem depth or heading depth.
A `scope` is where a route narrows the applicability of following content. It is
not a file, row, or filter; use route depth for this list.

## Subject Resolution

- A routed entrypoint selects itself and its descendants through the requested
  relative depth.
- A routed leaf selects only itself because it has no routed children.
- A valid overwrite path resolves to its base logical source. The overwrite is
  not an independent route row.
- An explicitly selected detached entrypoint may list its complete local routed
  subtree without claiming a Loader-rooted route.
- The Loader operand is invalid. Operand-free selection uses the root-set default
  chosen under `CLI-D084`; it never treats the Loader as a route row.
- An existing supported but unrouted source is invalid for route listing. Use
  `find` to discover it or `route inspect` to report its not-routed identity.
- An unknown reference is invalid. An unsafe reference or structurally ambiguous
  route is blocked. Interactive source-ID selection may choose a physical source,
  but it cannot repair ambiguous route meaning.

## Result Shape

Each row should expose enough information to choose the next operation without
reading the body:

- Automatic source ID.
- Canonical workspace-relative path.
- Parent source ID or visible indentation.
- Absolute route depth and relative depth from the selected root.
- Entrypoint or routed-leaf kind.
- Authored description.
- Authored tags, clearly labelled as metadata.
- Direct child count when applicable.

Order is parent before child, then deterministic canonical route/path order.

Compact human output begins with semantic result, route coverage, and row count,
then uses a token-friendly route tree or row list. A non-complete result adds the
concise required finding and next action. Expanded human output adds
descriptions, tags, kinds, depth, child counts, and provenance. Machine output
uses the shared structured result and retains every field regardless of the
selected human view.

The operation reuses the accepted semantic results:

- `complete` when the requested roots and depth were fully enumerated, including
  a complete empty result after any accepted exact filtering.
- `attention` for a non-blocking identity or authored-form finding that does not
  weaken route coverage.
- `incomplete` when confirmed rows are safe but requested route coverage could
  not be completed.
- `invalid` for invalid input or subject kind.
- `blocked` when a safe workspace, source, or route boundary cannot be
  established.
- `failed` and `interrupted` under their shared meanings.

No limit may silently look complete.

## Open Filter Options

One candidate first-version boundary accepts only exact structural selection and
depth. It displays descriptions and tags but does not filter by them or infer
topic relevance. This boundary is not accepted yet.

Two exact metadata filters remain plausible but unaccepted:

- Match a route row's own authored tag.
- Match literal text in route ID, path, description, or tags.

The stricter alternative keeps all tag and heading predicates in `find`. Under
that boundary:

```text
open-forge route list patterns/open-forge/cli
```

answers a structural CLI subtree question, while:

```text
open-forge find --tag=CLI
```

answers which Markdown sources carry the exact CLI tag. Neither claims to find
everything semantically related to CLI.

## Rejected Boundaries

- Fuzzy, semantic, or relevance matching.
- Inherited-tag or inferred-scope filtering.
- Trusting current generated Entries as the route inventory.
- Turning `route inspect` into an `--all` collection mode.
- Returning unrouted files as route rows.
- A general route graph query language.

## Remaining Review Questions

1. Is the candidate default depth `1` useful, or should omission select only
   the roots at relative depth `0`?
2. Should the operand-free form select every root route or a narrower explicit
   standard root set?
3. Is one exact own-tag filter worth including, or should tag selection remain
   exclusively in `find`?
4. Should literal metadata matching be deferred until real use proves it useful?

## Evidence Needed

- Build fixtures with stale Entries, detached trees, routed leaves, overwrite
  paths, unrouted sources, identity collisions, and incomplete metadata. The
  candidate passes semantic coverage only with zero missing or extra route rows
  and no false `complete` result.
- Run cold-start tasks for Patterns, CLI material, and unfamiliar custom scopes.
  The candidate proves distinct value when each task can select the next route
  in one catalogue call instead of recursive Entry reads or repeated inspection.
- Record route count, output size, and elapsed time on small, representative,
  and deliberately large workspaces. Any proposed default bound must return an
  explicit incomplete result when it prevents requested coverage.
- Compare with `find` plus repeated `route inspect` calls. Reject duplicate
  metadata filters that do not remove a call or improve completeness.

## Related Evidence

- [Council Synthesis](path-reference-council.md)
- [Archived Discovery Family](discovery-family.md)
- [Route Inspect contract set](../../../crystallized/documents/cli/contracts/route/inspect/_inspect.md)
- [Find contract set](../../../crystallized/documents/cli/contracts/find/_find.md)
