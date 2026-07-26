# Archived Memory Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/memory/archived/_archived.md`.

The archived memory category `entrypoint` defines how agents discover historical context after it stops being #CurrentTruth.

## Represents

Archived memory preserves historical context after material stops being #CurrentTruth.

## Contains

The installed file follows the category `entrypoint` shape defined by the [routed Markdown contract](../../../../../../.agents/memory/crystallized/documents/framework/markdown/routes.md): scoped `open-forge:` frontmatter, a title, one short definition, compact scope-specific axioms, and a final marker-bounded generated index region.

## Archived Contract

Archived memory is historical context only.

Archived memory contains memory material after useful current essence has been extracted or the material has stopped being current.

The archive must preserve enough context to answer where the material came from, why it was archived, and what replaced it when a replacement exists.

The base payload does not install default archive subroutes for every memory state. Archive child routes may use any useful taxonomy, but their organization must preserve origin and relevance.

## Authority Contract

Archived memory is not active truth.

Agents may use archived memory for history, reconstruction, rationale, or comparison. Archived material becomes current only when restored into an explicit current route.

If archived material contradicts current crystallized memory or applicable #Core routes, the current material wins unless the user chooses to restore or revise it.

## Loading Contract

The archived memory category is relevant when current work needs non-current historical context.

The `entrypoint` must route agents to direct archived memory files and child archived memory categories whose path, description, or tags match the current request. Each selected child `entrypoint` applies the same contract recursively.

Agents load archived memory only when it is relevant to the current request.

## Extraction Contract

Before moving material to `archived/`, agents extract useful current material into the correct route when safe and relevant. That route may be another #Memory route, a matching #Core route, or an external system.

The archived copy must preserve enough source context for later review without duplicating current truth.

## Scope Contract

Nested archived categories may use any taxonomy that preserves useful origin and routing context.

Archived organization must remain navigable. Use subcategories when they preserve archived meaning more clearly than a flat archive.

## Restoration Contract

Restoring archived material requires an explicit current destination.

Restored material must be validated against current workspace state before becoming current #Memory, #Core material, or external current state.

Restoration conflicts require user review or an explicit decision.

## Generated Region

The final generated region uses the category `entrypoint` shape defined by the [routed Markdown contract](../../../../../../.agents/memory/crystallized/documents/framework/markdown/routes.md).

Generated `entries` list direct archived memory files and direct child archived memory categories. The [routed Markdown contract](../../../../../../.agents/memory/crystallized/documents/framework/markdown/routes.md) defines metadata, `entry` representation, entrypoint naming, and marker shape. The routing model governs recursive discovery and generation.

## Used By

Agents use this category when they need history kept outside current truth.

Any process that writes archived memory must preserve origin context and keep the material outside current truth.

## Why

Archived memory exists so Open Forge can forget safely without deleting history.

It keeps old context available while protecting current work from stale or superseded material.

## Alignment Checks

The implementation is aligned when it:

- is named `_archived.md`
- lives in `.agents/memory/archived/`
- includes `Contextual` and `Historical` in scoped `open-forge:` tags
- defines archived memory as historical context outside #CurrentTruth
- supports archive child routes without requiring state mirrors
- preserves origin context
- requires extraction before archival when useful material remains
- allows extracted archived material to return to another #Memory route, matching #Core material, or external state
- keeps archived material below current memory authority
- requires validation before restoration
- supports recursive positive scope
- encourages navigable structure over flat accumulation
- routes only through its final generated region
- keeps compact authority, structure, loading, and restoration axioms
- keeps the authored portion between 10 and 45 non-empty lines
