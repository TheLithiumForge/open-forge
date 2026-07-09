# Working Memory Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/memory/working/_working.md`.

The working memory category `entrypoint` defines how agents discover temporary memory that helps them continue or resume active work.

## Represents

Working memory represents temporary context needed to continue or resume current work.

## Contains

The installed working memory `entrypoint` must contain:

- scoped `open-forge:` frontmatter with description and useful tags, including `Contextual` and `LoadNow`
- a title
- one short definition of working memory
- compact lifecycle, loading, freshness, and extraction axioms
- a final marker-bounded generated index region

The authored portion must stay between 10 and 40 non-empty lines. Generated `entries` do not count toward this limit.

## Working Contract

Working memory contains live context for work that is happening now or may need to resume soon.

Working memory is not accepted truth. It is resumability context. It can be wrong, partial, stale, or superseded by later work.

The base payload installs `sessions/` for raw work history and `handoffs/` for concise transfer notes. Both child `entrypoints` are default-loaded because they are bounded resumability routes.

Child `entrypoints` and local files define their own taxonomy below this route.

## Loading Contract

The working memory category is relevant when current work needs live or resumable context.

The `entrypoint` must route agents to direct working memory files and child working memory categories whose path, description, or tags match the current request. Each selected child `entrypoint` applies the same contract recursively.

Agents select the smallest current route that can answer what is happening now. The installed parent `entrypoint` must let generated `entries` carry installed route names and descriptions instead of repeating those names in axioms.

## Freshness Contract

Working memory must stay small enough to review.

When working memory becomes stale, completed, or no longer useful for resuming, agents must extract useful material before clearing or archiving it.

Useful extracted material moves to the route or system that owns its current state. That owner may be another #Memory route, a matching #Core route, or an external system.

## Scope Contract

Direct working memory files apply within their stated current work scope.

Nested working memory categories may group live context by any useful positive scope. Placement, descriptions, and tags must make the active scope cheap to identify.

Subcategories are encouraged when they prevent mixed working memory.

## Generated Region

The final section must use the shared category `entrypoint` shape:

```md
## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
```

Generated `entries` list direct working memory files and direct child working memory categories. The shared formatting and routing governors own metadata extraction, `entry` formatting, naming, recursive discovery, marker validation, and regeneration.

## Used By

Agents use this category when they need current context before deciding what to do next.

Any process that writes working memory must preserve resumability without presenting it as accepted truth.

## Why

Working memory exists because chat context decays and can disappear.

It keeps active work resumable while preserving a clear boundary between temporary state and durable memory.

## Alignment Checks

The implementation is aligned when it:

- is named `_working.md`
- lives in `.agents/memory/working/`
- includes `Contextual` and `LoadNow` in scoped `open-forge:` tags
- defines working memory as live resumability context
- exposes installed `handoffs/` and `sessions/` routes
- marks installed `handoffs/` and `sessions/` routes with #LoadNow
- avoids repeating installed route names in implementation axioms
- keeps working memory contextual rather than authoritative
- routes current work selectively
- requires stale working memory to be extracted, cleared, or archived
- allows extracted working memory to become another #Memory route, matching #Core material, or external state
- supports recursive positive scope
- encourages useful child categories over mixed working context
- routes only through its final generated region
