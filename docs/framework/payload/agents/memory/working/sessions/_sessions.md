# Sessions Memory Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/memory/working/sessions/_sessions.md`.

The sessions memory category entrypoint defines how agents discover raw chronological work records.

## Represents

Sessions represent contextual records of work while it happens.

They are raw memory: useful for reconstruction and extraction, but not accepted current truth.

## Contains

The installed sessions entrypoint must contain:

- scoped `open-forge:` frontmatter with description and useful tags, including `Contextual`
- a title
- one short definition of sessions
- compact raw-history, loading, extraction, and scope axioms
- a final marker-bounded generated index region

The authored portion must stay between 15 and 40 non-empty lines. Generated entries do not count toward this limit.

## Session Contract

Session files record what happened, what was tried, what changed, what remains unresolved, and where source context can be found.

They may be incomplete, noisy, or superseded. They must not become behavior, accepted truth, or a replacement for crystallized memory.

## Loading Contract

The sessions category is relevant when current work needs work history, reconstruction, extraction, audit context, or resume context.

The entrypoint must route agents to direct session files and child session categories whose path, description, or tags match the current request. Each selected child entrypoint applies the same contract recursively.

Agents load sessions selectively and prefer the narrowest session route that can answer the current question.

## Extraction Contract

Sessions must be mined for useful future material before they are treated as complete.

Useful material must move to the owner of its new state. That owner may be another #Memory route, a matching #Core route, an external system, or archived history.

## Scope Contract

Nested session categories may group raw work history by any useful positive scope.

Session organization must make origin and relevance cheap to identify.

## Generated Region

The final section must use the shared category entrypoint shape:

```md
## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
```

Generated entries list direct session files and direct child session categories. The shared formatting and routing governors own metadata extraction, entry formatting, naming, recursive discovery, marker validation, and regeneration.

## Used By

Agents use this category when they need raw work history or need to extract useful memory from past work.

Any process that writes sessions must keep them as raw contextual history until useful material is extracted.

## Why

Sessions exist so chat and work history can be preserved without pretending to be current truth.

They give Open Forge a raw trail that can later be refined into useful memory.

## Alignment Checks

The implementation is aligned when it:

- is named `_sessions.md`
- lives in `.agents/memory/working/sessions/`
- includes `Contextual` in scoped `open-forge:` tags
- defines sessions as raw chronological work records
- keeps sessions contextual rather than authoritative
- requires extraction into the route or system that owns the resulting material
- allows extracted session material to become another #Memory route, matching #Core material, or external state
- supports recursive positive scope
- routes only through its final generated region
