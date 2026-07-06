# Guidance Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/guidance/_guidance.md`.

The guidance category `entrypoint` defines how agents discover contextual advice for recurring choices, tradeoffs, and work scenarios.

## Represents

The guidance category represents established approaches whose application depends on the current context.

Guidance identifies a recurring scenario, explains a useful approach and its reasoning, and exposes the tradeoffs that may justify adaptation.

## Contains

The installed guidance category `entrypoint` must contain:

- scoped `open-forge:` frontmatter with a description and useful tags, including `Core`, `Guidance`, and `Index`
- a title
- one short definition of guidance
- compact relevance, loading, scope, and application axioms
- a final marker-bounded generated index region

The authored portion must stay between 15 and 35 non-empty lines. Generated `entries` do not count toward this limit.

## Guidance Contract

Every routed guidance file must identify a positive recurring scenario and provide enough context for an agent to determine where it applies.

Applicable guidance informs judgment. Agents use its preferred approach when it fits the current context and state the reason for a context-driven adaptation or alternative.

## Loading Contract

The guidance category is relevant when current work encounters a recurring scenario or decision that may have established guidance.

The `entrypoint` must route agents to direct guidance files and child guidance categories whose path, description, or tags match the current work. Each selected child `entrypoint` applies the same contract recursively. Agents load only guidance bodies in the current scope.

## Scope Contract

Direct guidance files provide contextual judgment available across the workspace within their stated applicability.

Nested guidance categories narrow or explicitly preserve their parent scope by domain, scenario, decision area, project area, or another positive context. Placement, descriptions, and tags must make that scope cheap to identify.

Guidance in a narrower selected scope is preferred over broader guidance when safe and allowed.

## Generated Region

The final section must use the shared category `entrypoint` shape:

```md
## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
```

Generated `entries` list direct guidance files and direct child guidance categories. The shared formatting and routing governors own metadata extraction, `entry` formatting, naming, recursive discovery, marker validation, and regeneration.

## Used By

Agents use this category when a recurring scenario may benefit from established contextual judgment.

## Why

Guidance preserves useful reasoning while leaving room for the facts and tradeoffs of the current work.

The empty default route gives each workspace room to grow its own guidance through routed files and child categories.

## Alignment Checks

The implementation is aligned when it:

- is named `_guidance.md`
- lives in `.agents/guidance/`
- includes `Core`, `Guidance`, and `Index` in scoped `open-forge:` tags
- defines guidance as contextual judgment for recurring scenarios
- selects guidance routes by visible relevance
- states how guidance is applied and adapted
- supports recursive positive scope
- prefers narrower selected guidance scopes when safe and allowed
- routes only through its final generated region
- remains empty until guidance files or child guidance categories are added
