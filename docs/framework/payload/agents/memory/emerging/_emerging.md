# Emerging Memory Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/memory/emerging/_emerging.md`.

The emerging memory category `entrypoint` defines how agents discover candidate memory that may be useful but is not accepted truth yet.

## Represents

Emerging memory represents candidate learning and unsettled useful material that may later crystallize, move elsewhere, or be archived.

## Contains

The installed file follows the category `entrypoint` shape defined by the [routed Markdown contract](../../../../../../.agents/memory/crystallized/documents/framework/markdown/routes.md): scoped `open-forge:` frontmatter, a title, one short definition, compact scope-specific axioms, and a final marker-bounded generated index region.

## Emerging Contract

Emerging memory contains useful material before it becomes accepted current memory.

The base payload installs `analysis/` for structured reasoning, `ideas/` for candidate possibilities, and `observations/` for noticed findings.

Child `entrypoints` and local files define their own taxonomy below this route.

## Authority Contract

Emerging memory is contextual and unapproved by default.

It may guide investigation, suggest future work, or provide evidence. It does not create #CurrentTruth, normative #Core, or external authority.

Emerging memory may explore behavior without activating it. Once accepted behavior, reusable form, guidance, capability, workflow, workspace routing, or other material should guide future work, it belongs in the matching #Core route, including user-created #Core categories and files.

## Loading Contract

The emerging memory category is relevant when current work needs useful material that is not accepted truth yet, or when current work produces candidate material.

Because this route is #KeepInMind, agents read it at task start or resume, after detected context restoration, and before a handoff or closeout. They recheck it during work only when its follow-ups may have changed.

The `entrypoint` must route agents to direct emerging memory files and child emerging memory categories whose path, description, or tags match the current request. Each selected child `entrypoint` applies the same contract recursively.

Agents load emerging memory selectively and keep unrelated candidate material out of current work. The installed parent `entrypoint` must let generated `entries` carry installed route names and descriptions instead of repeating those names in axioms.

## Promotion Contract

Emerging memory must have an obvious next state.

Accepted material moves to the matching crystallized or #Core route, external owner, or another current destination. Rejected and superseded material is archived or pruned after useful outcome or rationale is preserved.

Repeated or stale emerging material must trigger consolidation, promotion, archival, or pruning.

## Scope Contract

Nested emerging categories may group material by any useful positive scope.

Subcategories are strongly encouraged when they make emerging material easier to validate, compare, or route.

Emerging material stays outside crystallized routes until it is accepted.

## Generated Region

The final generated region uses the category `entrypoint` shape defined by the [routed Markdown contract](../../../../../../.agents/memory/crystallized/documents/framework/markdown/routes.md).

Generated `entries` list direct emerging memory files and direct child emerging memory categories. The [routed Markdown contract](../../../../../../.agents/memory/crystallized/documents/framework/markdown/routes.md) defines metadata, `entry` representation, entrypoint naming, and marker shape. The routing model governs recursive discovery and generation.

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
- includes `OrganicGrowth`, `Contextual`, `Candidate`, and `KeepInMind` in scoped `open-forge:` tags
- defines emerging memory as candidate learning
- exposes installed `analysis/`, `ideas/`, and `observations/` routes
- avoids repeating installed route names in implementation axioms
- keeps emerging memory contextual until promoted or accepted
- leaves child taxonomy to child `entrypoints` and local files
- routes candidate material selectively
- follows the #KeepInMind refresh triggers
- requires acceptance before promotion into #CurrentTruth or normative #Core
- promotes emerging memory to the route that owns its new state
- puts accepted emerging material that should guide future work in matching #Core routes
- supports recursive positive scope
- encourages subcategories that improve validation and routing
- routes only through its final generated region
- keeps compact candidate, loading, promotion, and scope axioms
- keeps the authored portion between 10 and 40 non-empty lines
