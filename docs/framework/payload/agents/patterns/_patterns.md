# Patterns Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/patterns/_patterns.md`.

The patterns category `entrypoint` defines how agents discover and apply concrete reusable shapes for code, files, APIs, documents, and other inspectable work.

## Represents

The patterns category represents established arrangements for code, files, naming, placement, boundaries, APIs, documents, and other inspectable results.

A pattern has stable relationships and variable contents. It makes repeated work consistent across modules, projects, repositories, or other scopes and easier to review.

## Contains

The installed file follows the category `entrypoint` shape defined by the [routed Markdown contract](../../../../../.agents/memory/crystallized/documents/framework/markdown/routes.md): scoped `open-forge:` frontmatter, a title, one short definition, compact scope-specific axioms, and a final marker-bounded generated index region.

## Pattern Contract

Every routed pattern must define a concrete, recognizable shape and identify enough positive context for an agent to determine where it applies.

An applicable pattern is the established default shape for its scope. A different shape requires a deliberate reason, preserving consistency without forcing a shape that does not fit.

## Loading Contract

The patterns category is relevant when current work creates, changes, or reviews an inspectable result.

The `entrypoint` must route agents to direct pattern files and child pattern categories whose path, description, or tags match the current work. Each selected child `entrypoint` applies the same contract recursively. Agents load only pattern bodies in the current scope.

## Scope Contract

Direct pattern files describe reusable shapes available across the workspace within their stated applicability.

Nested pattern categories narrow or explicitly preserve their parent scope by technology, domain, artifact, project area, or another positive context. Placement, descriptions, and tags must make that scope cheap to identify.

Patterns in a narrower selected scope are preferred over broader patterns when safe and allowed. Unresolved conflicts must be reported.

## Generated Region

The final generated region uses the category `entrypoint` shape defined by the [routed Markdown contract](../../../../../.agents/memory/crystallized/documents/framework/markdown/routes.md).

Generated `entries` list direct pattern files and direct child pattern categories. The [routed Markdown contract](../../../../../.agents/memory/crystallized/documents/framework/markdown/routes.md) defines metadata, `entry` representation, entrypoint naming, and marker shape. The routing model governs recursive discovery and generation.

## Used By

Agents use this category when work may have an established inspectable shape.

## Why

Patterns make structural consistency visible, and selective routing limits loading to relevant pattern bodies.

The empty default route gives each workspace room to grow its own patterns through routed files and child categories.

## Alignment Checks

The implementation is aligned when it:

- is named `_patterns.md`
- lives in `.agents/patterns/`
- includes `Core` and `Pattern` in scoped `open-forge:` tags
- defines patterns as concrete recognizable shapes that support consistency and review
- selects pattern routes by visible relevance
- treats applicable patterns as established defaults
- supports recursive positive scope
- prefers narrower selected pattern scopes when safe and allowed
- routes only through its final generated region
- remains empty until pattern files or child pattern categories are added
- keeps compact relevance, loading, scope, and application axioms
- keeps the authored portion between 10 and 35 non-empty lines
