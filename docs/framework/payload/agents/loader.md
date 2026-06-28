# Loader

## Description

This descriptor governs `src/open-forge/.agents/loader.md`.

The loader is the mandatory Open Forge entrypoint after `AGENTS.md`. It defines how an agent enters an installed workspace and contains the generated registry used to select relevant categories.

The loader is intentionally stable. It contains loading axioms, customization posture, and generated category navigation plus reserved load policy only. Detailed category behavior must live in the category entrypoint or routed concept that owns it.

## Represents

The loader represents the installed workspace loading contract and root category router.

It is the root file and routing primitive for the installed framework.

## Contains

The installed loader must contain:

- the rule that generated entries tagged `#LoadWithParentEntrypoint` load immediately with their parent entrypoint
- the rule that the current request controls relevance for entries without reserved load-policy tags
- the rule that a loaded markdown file includes its overwrite companion when present
- the tag behavior table used by Open Forge-authored entries
- the authority posture for local active truth, defaults, context, and candidate learning
- the customization posture: add local files first, use overwrites for light changes, edit framework files for complete behavior changes
- a final marker-bounded registry of active categories

The installed loader must stay short, concrete, and easy to diff. Target size is 40-90 non-empty lines. If it needs more than 200 non-empty lines, the design must move detail into routed files or separate concepts.

## Load Contract

The installed loader must be immediately routable after `AGENTS.md`. It must not require a secondary bootstrap file before an agent can select a category.

Generated category paths must be concrete and relative to the active workspace root. The active workspace root is the directory whose `AGENTS.md` selected this loader. Physical repository, submodule, and symlink boundaries do not change that logical root.

The loader must not infer a Git repository root or require runtime path constants.

A generated entry tagged `#LoadWithParentEntrypoint` must be loaded immediately after its parent entrypoint is loaded, in listed order. If the target is a category entrypoint, only that entrypoint is loaded first; its own entries then apply the same routing contract.

`#LoadWithParentEntrypoint` is a loading policy only. It does not create authority, scope, or precedence. It also does not search unloaded trees; nested autoload requires a loaded parent chain.

The default payload marks `directives/` and `memory/` with `LoadWithParentEntrypoint` metadata. Their generated loader entries therefore load with the loader without the loader naming those categories in axioms.

Local active truth has precedence over Open Forge defaults. Default files may still be loaded as context when useful.

When an agent selects a category, it must load the generated category entrypoint path before exploring routed files under that category. Open Forge-authored categories use `_{category}.md`; compatibility categories may use another recognized entrypoint name.

When a markdown file is loaded, its `.overwrite.md` companion must be loaded after it when present. The overwrite applies within the base file's scope.

## Category Registry

The loader must end with this generated region:

```md
## Entries

<!-- open-forge:generated-index:start -->
- `.agents/{category}/_{category}.md` - {description} - #{Tag1} #{Tag2} ... #{TagN}
<!-- open-forge:generated-index:end -->
```

The CLI must generate one entry for each direct child folder under `.agents/` that contains exactly one recognized category entrypoint. Other folders do not become loader routes. Open Forge-authored categories use `_{category}.md`; compatibility aliases are accepted only for external or local tooling.

Descriptions and tags must derive from each category entrypoint. Scoped metadata is authoritative when present; the first body description and `#Index` are compatibility fallbacks for local categories. The generated registry must not duplicate routed files inside a category.

## Authority Contract

The installed loader must state these authority axioms:

- user instructions apply when safe and allowed
- local active truth overrides Open Forge defaults
- generated entries are navigation metadata; only reserved load-policy tags affect loading
- `#LoadWithParentEntrypoint` affects loading only and does not create authority
- archived, historical, example, external, and temporary continuation material is contextual unless restored or promoted
- candidate learning is contextual, not authority
- detailed behavior belongs in the routed file or concept that owns it

## Tags Contract

The installed loader must define framework tag behavior in a `## Tags` section.

The installed `## Tags` section must contain:

- `### Axioms`
- `### Defined Tags`

Tag axioms must state that defined tags have framework meaning when they appear in loaded content or generated entries, undefined tags remain routing and search signals, entries without a load-policy tag are on-demand routes selected by the current request, tag spelling and casing are stable, and workspace-wide tag behavior belongs in the tag section.

Defined tags must include:

- `#LoadWithParentEntrypoint`
- `#Core`
- `#Memory`
- `#Extension`
- `#Contextual`
- `#CurrentTruth`

Layer tag descriptions must define each layer positively. `#Core` must describe the base routing, workspace orientation, and agent primitive routes rather than defining Core only as non-Memory.

Route type tags remain routing and search signals unless defined by a loaded entrypoint. Their meaning must be readable from entry paths, descriptions, and loaded entrypoints.

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

- is immediately routable after `AGENTS.md`
- uses concrete workspace-relative category paths
- remains valid when `.agents/` resolves through a symlink or into a submodule
- defines reserved tag behavior in `## Tags`
- separates tag axioms from defined tags
- defines `#LoadWithParentEntrypoint`, `#Core`, `#Memory`, `#Extension`, `#Contextual`, and `#CurrentTruth`
- loads generated `#LoadWithParentEntrypoint` entries in listed order
- marks default `directives/` and `memory/` entries with `#LoadWithParentEntrypoint`
- generates entries for direct active categories only
- derives descriptions and tags from category entrypoints
- loads a selected category entrypoint before its routed files
- loads overwrite companions after their base files
- states that local active truth overrides defaults
- treats generated entries as navigation metadata plus reserved load policy
- keeps detailed process behavior in routed files
- keeps customization guidance minimal and diff-friendly
