# Patterns Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/patterns/_patterns.md`.

The patterns category entrypoint defines how agents discover and apply concrete reusable shapes for inspectable work.

## Represents

The patterns category represents established arrangements for code, files, naming, placement, boundaries, APIs, documents, and other inspectable results.

A pattern has stable relationships and variable contents. It governs recognizable form within its positive scope.

## Contains

The installed patterns category entrypoint must contain:

- scoped `open-forge:` frontmatter with a description and useful tags
- a title
- one short definition of patterns
- compact relevance, loading, scope, and application axioms
- a final marker-bounded generated index region

The authored portion must stay between 15 and 35 non-empty lines. Generated entries do not count toward this limit.

## Pattern Contract

Every routed pattern must define a concrete, recognizable shape and identify enough positive context for an agent to determine where it applies.

An applicable pattern is the established default shape for its scope. A different shape requires a deliberate reason.

## Loading Contract

The patterns category is relevant when current work creates, changes, or reviews an inspectable result.

The entrypoint must route agents to direct pattern files and child pattern categories whose path, description, or tags match the current work. Each selected child entrypoint applies the same contract recursively. Agents load only pattern bodies in the current scope.

## Scope Contract

Direct pattern files describe reusable shapes available across the workspace within their stated applicability.

Nested pattern categories narrow or explicitly preserve their parent scope by technology, domain, artifact, project area, or another positive context. Placement, descriptions, and tags must make that scope cheap to identify.

Patterns in a narrower selected scope are preferred over broader patterns when safe and allowed. Unresolved conflicts must be reported.

## Generated Region

The final section must use the shared category entrypoint shape:

```md
## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
```

Generated entries list direct pattern files and direct child pattern categories. The shared formatting and routing governors own metadata extraction, entry formatting, naming, recursive discovery, marker validation, and regeneration.

## Used By

Agents use this category when work may have an established inspectable shape.

## Why

Patterns make structural consistency visible, and selective routing limits loading to relevant pattern bodies.

The empty core category gives each workspace room to grow its own patterns through ordinary files and recursively scoped folders.

## Alignment Checks

The implementation is aligned when it:

- is named `_patterns.md`
- lives in `.agents/patterns/`
- defines patterns as concrete recognizable shapes
- selects pattern routes by visible relevance
- treats applicable patterns as established defaults
- supports recursive positive scope
- prefers narrower selected pattern scopes when safe and allowed
- routes only through its final generated region
- remains empty until local files or optional modules add pattern content
