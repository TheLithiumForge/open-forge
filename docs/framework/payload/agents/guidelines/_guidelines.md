# Guidelines Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/guidelines/_guidelines.md`.

The guidelines category entrypoint defines how agents discover and use contextual judgment for recurring scenarios.

## Represents

The guidelines category represents established approaches whose application depends on the current context.

A guideline identifies a recurring scenario, explains a useful approach and its reasoning, and exposes the tradeoffs that may justify adaptation.

## Contains

The installed guidelines category entrypoint must contain:

- scoped `open-forge:` frontmatter with a description and useful tags, including `Core`, `Guideline`, and `Index`
- a title
- one short definition of guidelines
- compact relevance, loading, scope, and application axioms
- a final marker-bounded generated index region

The authored portion must stay between 15 and 35 non-empty lines. Generated entries do not count toward this limit.

## Guideline Contract

Every routed guideline must identify a positive recurring scenario and provide enough context for an agent to determine where it applies.

An applicable guideline informs judgment. Agents use its preferred approach when it fits the current context and state the reason for a context-driven adaptation or alternative.

## Loading Contract

The guidelines category is relevant when current work encounters a recurring scenario or decision that may have established guidance.

The entrypoint must route agents to direct guideline files and child guideline categories whose path, description, or tags match the current work. Each selected child entrypoint applies the same contract recursively. Agents load only guideline bodies in the current scope.

## Scope Contract

Direct guideline files provide contextual judgment available across the workspace within their stated applicability.

Nested guideline categories narrow or explicitly preserve their parent scope by domain, scenario, decision area, project area, or another positive context. Placement, descriptions, and tags must make that scope cheap to identify.

Guidelines in a narrower selected scope are preferred over broader guidelines when safe and allowed.

## Generated Region

The final section must use the shared category entrypoint shape:

```md
## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
```

Generated entries list direct guideline files and direct child guideline categories. The shared formatting and routing governors own metadata extraction, entry formatting, naming, recursive discovery, marker validation, and regeneration.

## Used By

Agents use this category when a recurring scenario may benefit from established contextual judgment.

## Why

Guidelines preserve useful reasoning while leaving room for the facts and tradeoffs of the current work.

The empty core category gives each workspace room to grow its own guidance through ordinary files and recursively scoped folders.

## Alignment Checks

The implementation is aligned when it:

- is named `_guidelines.md`
- lives in `.agents/guidelines/`
- includes `Core`, `Guideline`, and `Index` in scoped `open-forge:` tags
- defines guidelines as contextual judgment for recurring scenarios
- selects guideline routes by visible relevance
- states how guidance is applied and adapted
- supports recursive positive scope
- prefers narrower selected guideline scopes when safe and allowed
- routes only through its final generated region
- remains empty until local files or optional modules add guideline content
