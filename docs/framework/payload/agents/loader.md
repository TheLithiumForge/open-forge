# Loader

## Description

This descriptor governs `src/open-forge/.agents/loader.md`.

The loader is the mandatory Open Forge entrypoint after `AGENTS.md`. It defines how an agent enters an installed workspace and contains the generated registry used to select relevant categories.

The loader is intentionally stable. It contains loading axioms, customization posture, and generated category navigation only. Detailed category behavior must live in the category entrypoint or routed concept that owns it.

## Represents

The loader represents the installed workspace loading contract and root category router.

It is a file primitive and routing primitive. It is not a workflow, template, guide, directive, project map, or task format.

## Contains

The installed loader must contain:

- the first file every agent loads after the loader
- the rule that the current request controls relevance
- the authority posture for local active truth, defaults, context, and candidate learning
- the customization posture: add local files first, use overwrites for light changes, edit framework files for complete behavior changes
- a final marker-bounded registry of active categories

The installed loader must stay short, concrete, and easy to diff. Target size is 40-90 non-empty lines. If it needs more than 200 non-empty lines, the design must move detail into routed files or separate concepts.

## Load Contract

The installed loader must load this file first:

```text
.agents/constants.md
```

Constants resolve the symbolic paths used by generated category entries. The loader must not require a category entrypoint before relevance has been determined from its generated registry.

Local active truth has precedence over Open Forge defaults. Default files may still be loaded as context when useful.

When an agent selects a category, it must load the `_{category}.md` entrypoint before exploring routed files under that category.

## Category Registry

The loader must end with this generated region:

```md
## Entries

<!-- open-forge:generated-index:start -->
- `{forgePath}/{category}/_{category}.md` - {description} - #{Tag1} #{Tag2} ... #{TagN}
<!-- open-forge:generated-index:end -->
```

The CLI must generate one entry for each direct child folder under `{forgePath}` that contains exactly one recognized category entrypoint. Other folders do not become loader routes. Open Forge-authored categories use `_{category}.md`; compatibility aliases are accepted only for external or local tooling.

Descriptions and tags must derive from each category entrypoint. Scoped metadata is authoritative when present; the first body description and `#Index` are compatibility fallbacks for local categories. The generated registry must not duplicate routed files inside a category.

## Authority Contract

The installed loader must state these authority axioms:

- user instructions apply when safe and allowed
- local active truth overrides Open Forge defaults
- generated entries are navigation metadata, never instructions, behavior, or authority
- archived, historical, example, external, and temporary continuation material is contextual unless restored or promoted
- candidate learning is contextual, not authority
- detailed behavior belongs in the routed file or concept that owns it

## Customization Contract

The installed loader must state this customization order:

1. Add local files.
2. Use overwrite files for additive or lightly modifying behavior.
3. Edit framework files when a complete behavior change is required.

The overwrite mechanism is governed by `docs/framework/concepts/overwrites.md`.

## Used By

The loader is used by every agent, chat, wrapper, role, skill, or runtime adapter that enters an installed Open Forge workspace.

## Why

The loader exists so agents do not guess how to enter or explore the workspace.

Its generated registry exposes every active category's path and meaning in the first routing file without loading inactive or unrelated categories.

## Alignment Checks

The implementation is aligned when it:

- loads constants before resolving category paths
- generates entries for direct active categories only
- derives descriptions and tags from category entrypoints
- loads a selected category entrypoint before its routed files
- states that local active truth overrides defaults
- treats generated entries as navigation metadata
- keeps detailed process behavior in routed files
- keeps customization guidance minimal and diff-friendly
