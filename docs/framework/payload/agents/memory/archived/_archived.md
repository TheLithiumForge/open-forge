# Archived Memory Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/memory/archived/_archived.md`.

The archived memory category entrypoint defines how agents discover memory preserved for context but no longer current.

## Represents

Archived memory represents historical context after material stops being current.

## Contains

The installed archived memory entrypoint must contain:

- scoped `open-forge:` frontmatter with description and useful tags, including `Contextual` and `Historical`
- a title
- one short definition of archived memory
- compact authority, structure, loading, and restoration axioms
- a final marker-bounded generated index region

The authored portion must stay between 15 and 45 non-empty lines. Generated entries do not count toward this limit.

## Archived Contract

Archived memory is historical context only.

Archived memory contains memory material after useful current essence has been extracted or the material has stopped being current.

The archive must preserve enough context to answer where the material came from, why it was archived, and what replaced it when a replacement exists.

The base payload does not install default archive subroutes for every memory state. Archive routes may be global or scoped anywhere their owning route makes historical context clearer, but their organization must preserve origin and relevance.

## Authority Contract

Archived memory is not active truth.

Agents may use archived memory for history, reconstruction, rationale, or comparison. Archived material becomes current only when restored into an explicit current route.

If archived material contradicts current crystallized memory or applicable #Core routes, the current material wins unless the user chooses to restore or revise it.

## Loading Contract

The archived memory category is relevant when current work needs non-current historical context.

The entrypoint must route agents to direct archived memory files and child archived memory categories whose path, description, or tags match the current request. Each selected child entrypoint applies the same contract recursively.

Agents load archived memory only when it is relevant to the current request.

## Extraction Contract

Before moving material to `archived/`, agents extract useful current material into the correct route when safe and relevant.

The archived copy must preserve enough source context for later review without duplicating current truth.

## Scope Contract

Nested archived categories may use any taxonomy that preserves useful origin and routing context.

Archived organization must remain navigable. Agents must create subcategories when they preserve archived meaning more clearly than a flat archive.

## Restoration Contract

Restoring archived material requires an explicit current destination.

Restored material must be validated against current workspace state before becoming working, emerging, or crystallized memory.

Restoration conflicts require user review or an explicit decision.

## Generated Region

The final section must use the shared category entrypoint shape:

```md
## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
```

Generated entries list direct archived memory files and direct child archived memory categories. The shared formatting and routing governors own metadata extraction, entry formatting, naming, recursive discovery, marker validation, and regeneration.

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
- defines archived memory as historical context only
- supports global or scoped archive routes without requiring state mirrors
- preserves origin context
- requires extraction before archival when useful material remains
- keeps archived material below current memory authority
- requires validation before restoration
- supports recursive positive scope
- encourages navigable structure over flat accumulation
- routes only through its final generated region
