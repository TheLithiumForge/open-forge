# Loader

## Description

This descriptor governs `src/open-forge/.agents/loader.md`.

The loader is the mandatory Open Forge `entrypoint` after `AGENTS.md`. It defines how an agent enters an installed workspace and contains the generated registry used to select relevant root routes.

The loader is intentionally stable. It contains loading axioms, customization posture, and generated root-route navigation plus reserved load policy only. Detailed category behavior must live in the category `entrypoint` or routed concept that owns it.

## Represents

The loader represents the installed workspace loading contract and root route registry.

It is the root file and routing primitive for the installed framework.

## Contains

The installed loader contains its terms, axioms, route patterns, tag behavior, compact CLI reference, customization order, and a final marker-bounded registry of active root routes. The normative content requirements are the Alignment Checks below.

## Load Contract

The installed loader must be immediately routable after `AGENTS.md`. It must not require a secondary bootstrap file before an agent can select a root route.

Generated loader paths must be concrete and relative to the active workspace root. The active workspace root is the directory whose `AGENTS.md` selected this loader. Repository and submodule boundaries do not change that logical root. Security-sensitive CLI commands additionally reject routes that resolve outside the target through a symlink or junction.

The loader must not infer a Git repository root or require runtime path constants.

Load-policy tag semantics are governed by the routing concept. The installed loader states them as its own axioms and Tags rules so agents need no maintainer docs.

Local active truth has precedence over Open Forge defaults. Default files may still be loaded as context when useful.

When an agent selects a generated loader route, it must load the generated `entrypoint` path before exploring routed files under that route. Open Forge-authored `entrypoints` use `_{folder}.md`; compatibility `entrypoints` may use another recognized `entrypoint` name.

Before non-trivial work, the loader infers the established development state from routed current truth and the transition requested by the user, then selects the installed workflow whose Goal best covers that transition. It recommends at most one prerequisite workflow first only when a concrete missing or contradictory input would make the requested transition unreliable. Phase order is wayfinding rather than a waterfall: work may start anywhere, skip, repeat, or move backward. An explicit workflow choice or opt-out wins.

The complete routed #KeepInMind catalogue is baseline-loaded, binding follow-up context. The loader requires a full read or recheck at task start or resume, after context restoration or compaction, at meaningful phase transitions or handoffs, and before closeout. When available, `open-forge find --tag KeepInMind --bodies` is the single complete lookup. Long-running work keeps one concise active working-session checkpoint and refreshes it at the same continuity boundaries.

When the CLI is available, the loader prefers `find` for deterministic routed lookup, `chain` for inherited heading inspection, `doctor` for structural validation, `index` for regeneration, and `extend --list` for optional capability discovery. Plain-file traversal remains a complete fallback.

## Category Registry

The loader must end with this generated region:

```md
## Entries

<!-- open-forge:generated-index:start -->
- `.agents/{category}/_{category}.md` - {description} - #{Tag1} #{Tag2} ... #{TagN}
<!-- open-forge:generated-index:end -->
```

The CLI must generate one `entry` for each direct child folder under `.agents/` that contains exactly one recognized category `entrypoint`. Other folders do not become loader routes. Open Forge-authored categories use `_{category}.md`; compatibility aliases are accepted only for external or local tooling.

Descriptions and tags must derive from each category `entrypoint`. Scoped metadata is authoritative when present; the first body description and #Index are compatibility fallbacks for local categories. The generated registry must not duplicate routed files inside a category.

## Tags Contract

The installed loader must define framework tag behavior in a `## Tags` section with `### Axioms` and `### Defined Tags`.

Tag axioms must state that defined tags have framework meaning when they appear in loaded content or generated `entries`, undefined tags remain routing and search signals, entries without a load-policy tag are on-demand routes selected by the current request, tag spelling and casing are stable, and workspace-wide tag behavior belongs in the tag section.

Layer tag descriptions must define each tag positively. #Core must describe the base routing, workspace orientation, and agent primitive routes rather than defining #Core only as non-#Memory. #Memory must describe self-growing markdown memory for workspace state, AI communication, current records, historical records, and learning.

Route type tags remain routing and search signals unless defined by a loaded `entrypoint`. Their meaning must be readable from `entry` paths, descriptions, and loaded `entrypoints`.

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

Its generated registry exposes every active root route's path and meaning in the first routing file without loading inactive or unrelated routes.

## Alignment Checks

The implementation is aligned when it:

- is immediately routable after `AGENTS.md`
- uses concrete workspace-relative loader paths
- remains logically workspace-relative across repository and submodule boundaries
- states that CLI reads and writes reject external symlink or junction escapes from the target
- defines `entrypoint`, `entry`, `framework route`, `scope route`, `scoped framework route`, `slug`, and `axiom` plainly enough to understand without maintainer docs
- states that user instructions apply when safe and allowed
- defines loaded `axioms` as mandatory unless higher-priority instructions conflict
- states that required memory actions are complete only after the file is written or a blocker is reported
- states that axioms of loaded ancestor `entrypoints` apply to all routes below them, so child `entrypoints` add only scope-specific axioms
- states that material in a narrower selected scope is preferred over broader material of the same type when safe and allowed
- states that the current request controls relevance for `entries` without reserved load-policy tags
- infers the current development state and requested transition, chooses the workflow whose Goal covers it, recommends at most one evidence-backed prerequisite, treats phases as non-waterfall wayfinding, and honors explicit workflow choice or opt-out
- states that folders become routable through recognized `entrypoints`
- states that generated `entries` list sibling markdown files, direct child `entrypoints`, and supported native skill packages inside skills routes
- states that native skill package internals are loaded through `SKILL.md` relevance
- states that nested routes require `entrypoints` at every folder level
- states that `scope routes` can narrow `framework routes` through concrete `slug` folders
- states that `scoped framework routes` work only when their framework `entrypoint` exists inside the scope
- keeps route examples as shape explanations rather than generated paths
- defines reserved tag behavior in `## Tags`
- separates tag axioms from defined tags
- defines #LoadNow, #KeepInMind, #Core, #Memory, #Extension, #Contextual, and #CurrentTruth
- reads generated #LoadNow `entries` when they appear in loaded `Entries`, in listed order
- treats the complete routed #KeepInMind catalogue as baseline-loaded and binding, and rechecks every result at loader entry, context restoration, meaningful phase transitions or handoffs, and closeout
- requires a concise active working-session checkpoint for long-running work and refreshes it at the same continuity boundaries
- includes a compact CLI section for find, chain, doctor, index, and extension catalogue inspection
- keeps tool references optional and tool-assisted, never load-bearing; the framework works from plain files alone
- marks default Open Forge core `entrypoints` with #LoadNow, or #KeepInMind where standing follow-up matters
- generates `entries` for direct active root routes only
- derives descriptions and tags from category `entrypoints`
- loads a selected category `entrypoint` before its routed files
- loads overwrite companions after their base files
- states that local active truth overrides defaults
- prefers written #Memory routes over private or opaque agent memory for durable workspace state
- treats archived, historical, example, external, temporary continuation, and candidate-learning material as contextual unless restored or promoted
- states that writing files inside existing routes is normal use, child categories are added when they improve routing, ownership, or clarity, and new root routes need clear scope
- treats generated `entries` as navigation metadata plus reserved load policy that never creates authority
- keeps detailed process behavior in routed files
- keeps customization guidance minimal and diff-friendly
- stays within the 40-90 non-empty line target and moves detail into routed files or concepts before reaching 200
