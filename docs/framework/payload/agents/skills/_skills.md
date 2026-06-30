# Skills Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/skills/_skills.md`.

The skills category entrypoint defines how agents discover bounded reusable capabilities without redefining any agent runtime's native skill format.

## Represents

The skills category represents reusable agent capabilities with clear applicability and expected results.

A skill packages a bounded ability, procedure, tool use, or reference set that helps an agent perform a specific kind of work.

## Contains

The installed skills category entrypoint must contain:

- scoped `open-forge:` frontmatter with a description and useful tags, including `Core`, `Skill`, and `Index`
- a title
- one short definition of skills
- compact relevance, loading, scope, and runtime-boundary axioms
- a final marker-bounded generated index region

The authored portion must stay between 15 and 35 non-empty lines. Generated entries do not count toward this limit.

## Skill Contract

Every routed skill must identify one bounded capability, its positive applicability, and the expected result of using it.

Skill files may contain steps, required inputs, tool requirements, output expectations, examples, or references when those details are needed to use the capability reliably.

Open Forge routes skills. The active agent runtime owns skill invocation, activation, packaging, installation, and execution.

## Loading Contract

The skills category is relevant when current work may benefit from a reusable agent capability.

The entrypoint must route agents to direct skill files and child skill categories whose path, description, or tags match the current work. Each selected child entrypoint applies the same contract recursively. Agents load only skill bodies in the current scope.

## Scope Contract

Direct skill files describe capabilities available across the workspace within their stated applicability.

Nested skill categories narrow or explicitly preserve their parent scope by tool, domain, work type, artifact, project area, or another positive context. Placement, descriptions, and tags must make that scope cheap to identify.

Skills in a narrower selected scope are preferred over broader skills when safe and allowed.

## Generated Region

The final section must use the shared category entrypoint shape:

```md
## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
```

Generated entries list direct skill files and direct child skill categories. The shared formatting and routing governors own metadata extraction, entry formatting, naming, recursive discovery, marker validation, and regeneration.

## Used By

Agents use this category when a reusable capability may help perform the current work.

## Why

Skills make reusable capability discoverable while preserving compatibility with agent runtimes that already define their own skill mechanics.

The empty core category gives each workspace room to add only the capabilities it actually wants.

## Alignment Checks

The implementation is aligned when it:

- is named `_skills.md`
- lives in `.agents/skills/`
- includes `Core`, `Skill`, and `Index` in scoped `open-forge:` tags
- defines skills as bounded reusable agent capabilities
- selects skill routes by visible relevance
- keeps runtime-specific invocation, activation, packaging, installation, and execution outside the category contract
- supports recursive positive scope
- prefers narrower selected skill scopes when safe and allowed
- routes only through its final generated region
- remains empty until skill files or child skill categories are added
