# Emerging Memory Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/memory/emerging/_emerging.md`.

The emerging memory category entrypoint defines how agents discover useful material that is not accepted truth.

## Represents

Emerging memory represents candidate learning and unsettled useful material that may later crystallize, move elsewhere, or be archived.

## Contains

The installed emerging memory entrypoint must contain:

- scoped `open-forge:` frontmatter with description and useful tags, including `Contextual` and `Candidate`
- a title
- one short definition of emerging memory
- compact candidate, loading, promotion, and scope axioms
- a final marker-bounded generated index region

The authored portion must stay between 15 and 40 non-empty lines. Generated entries do not count toward this limit.

## Emerging Contract

Emerging memory contains useful material before it becomes accepted current memory.

The base payload installs `analysis/` for structured reasoning, `ideas/` for candidate possibilities, and `observations/` for noticed findings.

Child entrypoints and local files define their own taxonomy below this route.

## Authority Contract

Emerging memory is contextual and unapproved by default.

It may guide investigation, suggest future work, or provide evidence. It is accepted only after promotion, validation, or explicit acceptance by the user or workspace process.

If emerging memory becomes operational behavior, reusable form, guidance, capability, workflow, workspace routing, or other #Core material, the material must be promoted into the matching #Core route, including user-created #Core categories and files.

If emerging memory changes external task state, the change must be promoted to the external system that owns that authority.

## Loading Contract

The emerging memory category is relevant when current work needs useful material that is not accepted truth yet.

The entrypoint must route agents to direct emerging memory files and child emerging memory categories whose path, description, or tags match the current request. Each selected child entrypoint applies the same contract recursively.

Agents load emerging memory selectively and keep unrelated candidate material out of current work. The installed parent entrypoint must let generated entries carry installed child route names and descriptions instead of repeating those names in axioms.

## Promotion Contract

Emerging memory must have an obvious next state.

It may be refined, crystallized, moved to the matching authority owner, or archived.

Repeated emerging material must trigger a suggestion to crystallize, promote, or archive it.

## Scope Contract

Nested emerging categories may group material by any useful positive scope.

Subcategories are strongly encouraged when they make emerging material easier to validate, compare, or promote.

Emerging material stays outside crystallized routes until it is accepted.

## Generated Region

The final section must use the shared category entrypoint shape:

```md
## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
```

Generated entries list direct emerging memory files and direct child emerging memory categories. The shared formatting and routing governors own metadata extraction, entry formatting, naming, recursive discovery, marker validation, and regeneration.

## Used By

Agents use this category when possible learning needs a named route before it becomes accepted memory.

Any process that writes emerging memory must preserve uncertainty until the material is accepted, promoted, or archived.

## Why

Emerging memory exists so useful signals can grow without silently becoming truth.

It gives the system a safe middle state between live work and crystallized memory.

## Alignment Checks

The implementation is aligned when it:

- is named `_emerging.md`
- lives in `.agents/memory/emerging/`
- includes `Contextual` and `Candidate` in scoped `open-forge:` tags
- defines emerging memory as candidate learning
- exposes installed `analysis/`, `ideas/`, and `observations/` routes
- avoids repeating installed child route names in implementation axioms
- keeps emerging memory contextual until promoted or accepted
- leaves child taxonomy to child entrypoints and local files
- routes candidate material selectively
- promotes operational emerging memory material into matching #Core routes, including user-created #Core categories and files
- supports recursive positive scope
- encourages subcategories that improve validation and promotion
- routes only through its final generated region
