# Crystallized Memory Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/memory/crystallized/_crystallized.md`.

The crystallized memory category entrypoint defines how agents discover accepted current memory.

## Represents

Crystallized memory represents current accepted memory within its stated scope.

It is the durable understanding the workspace accepts as current.

## Contains

The installed crystallized memory entrypoint must contain:

- scoped `open-forge:` frontmatter with description and useful tags, including `CurrentTruth` and `LoadWithParentEntrypoint`
- a title
- one short definition of crystallized memory
- compact authority, loading, consolidation, and scope axioms
- a final marker-bounded generated index region

The authored portion must stay between 15 and 45 non-empty lines. Generated entries do not count toward this limit.

## Crystallized Contract

Crystallized memory contains validated current understanding.

Child entrypoints, local files, and optional modules define their own taxonomy below this route.

## Authority Contract

Crystallized memory is accepted current memory within its stated scope.

It informs future work and still respects current user instructions, platform constraints, runtime safety, applicable `#Core` routes, and declared external sources of truth.

Crystallized memory does not automatically create operational behavior. Behavior, reusable form, guidance, capability, workflow, workspace routing, or other `#Core` material belongs in the matching `#Core` route, including user-created `#Core` categories and files.

## Loading Contract

The crystallized memory category is relevant when current work needs accepted current memory.

The entrypoint must route agents to direct crystallized memory files and child crystallized memory categories whose path, description, or tags match the current request. Each selected child entrypoint applies the same contract recursively.

Agents select the smallest crystallized route that can answer the current question.

## Consolidation Contract

Crystallized memory must keep one current truth per scope.

When new accepted memory overlaps existing crystallized memory, agents must update or reshape the existing route rather than create a second competing source.

When reality changes, crystallized memory may be rewritten, split, merged, or superseded. Superseded material must be archived or linked with enough context to understand the change.

## Scope Contract

Nested crystallized categories may organize accepted memory by any useful positive scope.

Subcategories are encouraged when they keep current truth small, discoverable, and owned by the right scope.

## Generated Region

The final section must use the shared category entrypoint shape:

```md
## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
```

Generated entries list direct crystallized memory files and direct child crystallized memory categories. The shared formatting and routing governors own metadata extraction, entry formatting, naming, recursive discovery, marker validation, and regeneration.

## Used By

Agents use this category when they need accepted durable memory for current work.

Layer 3 workflows may use crystallized memory as a durable output target.

## Why

Crystallized memory exists so future work can load concise current truth without rereading unresolved or historical material.

It keeps useful memory durable while preserving enough structure to evolve as the workspace changes.

## Alignment Checks

The implementation is aligned when it:

- is named `_crystallized.md`
- lives in `.agents/memory/crystallized/`
- includes `CurrentTruth` and `LoadWithParentEntrypoint` in scoped `open-forge:` tags
- defines crystallized memory as accepted current memory
- leaves child taxonomy to child entrypoints, local files, and optional modules
- avoids duplicate current truth
- archives or links superseded crystallized material
- keeps operational material in matching `#Core` routes, including user-created `#Core` categories and files
- supports recursive positive scope
- routes only through its final generated region
