# Skills Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/skills/_skills.md`.

The skills category `entrypoint` defines how agents discover skills through the standard `SKILL.md` format supported by their runtime.

## Represents

The skills category represents ordinary skills in the format understood by the active agent runtime.

A skill is a bounded ability, procedure, tool use, or resource set that helps an agent perform a specific kind of work.

## Contains

The installed file follows the shared category `entrypoint` shape owned by the formatting concept: scoped `open-forge:` frontmatter, a title, one short definition, compact scope-specific axioms, and a final marker-bounded generated index region.

## Skill Contract

Every routed skill identifies one bounded capability, its positive applicability, and the expected result of using its `SKILL.md`.

The CLI recognizes `.agents/skills/{skill-name}/SKILL.md` for routing but does not rewrite the file or define a package-internal `References` or `Entries` schema. The selected `SKILL.md` owns its metadata, instructions, resource organization, and on-demand loading. The active agent runtime owns invocation, activation, installation, and execution.

Loose Markdown files in `.agents/skills/` are not routed skills.

## Loading Contract

The skills category is relevant when current work may benefit from a reusable agent capability.

The installed `entrypoint` stays small: agents use `Entries`, follow the selected `SKILL.md`, and load its resources only when that file makes them relevant.

## Scope Contract

Direct skills describe capabilities available across the workspace within their stated applicability.

Nested skill categories narrow or explicitly preserve their parent scope by tool, domain, work type, artifact, project area, or another positive context. Placement, descriptions, and tags must make that scope cheap to identify.

Skills in a narrower selected scope are preferred over broader skills when safe and allowed.

## Generated Region

The final generated region uses the shared category `entrypoint` shape owned by the formatting concept.

Generated `entries` list direct skills and direct child skill categories. The shared formatting and routing governors own metadata extraction, `entry` formatting, naming, recursive discovery, marker validation, and regeneration.

## Used By

Agents use this category when a reusable capability may help perform the current work.

## Why

Skills make reusable capability discoverable while preserving compatibility with agent runtimes that already define their own skill mechanics.

The empty default route gives each workspace room to add only the capabilities it actually wants.

## Alignment Checks

The implementation is aligned when it:

- is named `_skills.md`
- lives in `.agents/skills/`
- includes `Core` and `Skill` in scoped `open-forge:` tags
- defines skills through their standard `SKILL.md` and runtime behavior
- selects skill routes by visible relevance
- routes `.agents/skills/{skill-name}/SKILL.md` packages without rewriting them
- keeps `SKILL.md` metadata compatible with the active agent runtime
- keeps runtime-specific invocation, activation, packaging, installation, and execution outside the category contract
- does not impose an Open Forge package-internal References or Entries format
- supports recursive positive scope
- prefers narrower selected skill scopes when safe and allowed
- routes only through its final generated region
- remains empty until skills or child skill categories are added
- keeps installed Axioms limited to selection, native behavior, and on-demand resources
- keeps the authored portion between 10 and 35 non-empty lines
- keeps the frontmatter description `Skills available through standard SKILL.md files`
