# Loader

## Description

This descriptor governs `src/open-forge/.agents/loader.md`.

The loader is the mandatory Open Forge `entrypoint` after `AGENTS.md`. It defines how an agent enters an installed workspace and contains the generated registry used to select relevant root routes.

The loader is intentionally stable. It contains loading axioms, customization posture, and generated root-route navigation plus reserved load policy only. Detailed category behavior must live in the category `entrypoint` or routed concept that owns it.

## Represents

The loader represents the installed workspace loading contract and root route registry.

It is the root file and routing primitive for the installed framework.

## Contains

The installed loader must contain:

- the rule that agents read generated `entries` tagged #LoadNow when they appear in loaded `Entries`, in listed order
- the rule that agents read generated `entries` tagged #KeepInMind immediately and recheck them before ending meaningful work
- the rule that loaded `axioms` are mandatory unless higher-priority instructions conflict
- the rule that axioms of loaded ancestor `entrypoints` apply to all routes below them, so child `entrypoints` add only scope-specific axioms
- the rule that material in a narrower selected scope is preferred over broader material of the same type when safe and allowed
- the rule that the current request controls relevance for `entries` without reserved load-policy tags
- a short terms section defining `entrypoint`, `entry`, `framework route`, `scope route`, `scoped framework route`, `slug`, and `axiom`
- the rule that a folder is routable only when it contains one recognized `entrypoint`
- the rule that `Entries` list sibling markdown files, direct child `entrypoints`, and supported native skill packages inside skills routes
- the rule that native skill package folders are routed by `SKILL.md` and their package internals are loaded only when the skill makes them relevant
- the rule that every folder in a nested route path needs its own `entrypoint`
- the rule that `scope routes` use `slug` folders with `entrypoints` before, after, or between `framework routes`
- the rule that `scoped framework routes` work only when their framework `entrypoint` exists inside the scope
- the rule that a loaded markdown file includes its overwrite companion when present
- a short route-pattern section that uses `[scope]` placeholder notation only as explanatory shape
- the tag behavior table used by Open Forge-authored `entries`
- the authority posture for local active truth, defaults, context, and candidate learning
- the preference for written #Memory routes over private or opaque agent memory for durable workspace state
- the rule that writing files inside existing routes is normal use, child categories are added when they improve routing, ownership, or clarity, and new root routes need clear scope
- the customization posture: add local files first, use overwrites for light changes, edit framework files for complete behavior changes
- a final marker-bounded registry of active root routes

The installed loader must stay short, concrete, and easy to diff. Target size is 40-90 non-empty lines. If it needs more than 200 non-empty lines, the design must move detail into routed files or separate concepts.

## Load Contract

The installed loader must be immediately routable after `AGENTS.md`. It must not require a secondary bootstrap file before an agent can select a root route.

Generated loader paths must be concrete and relative to the active workspace root. The active workspace root is the directory whose `AGENTS.md` selected this loader. Physical repository, submodule, and symlink boundaries do not change that logical root.

The loader must not infer a Git repository root or require runtime path constants.

Agents read a generated `entry` tagged #LoadNow when it appears in loaded `Entries`, in listed order. If the target is a category `entrypoint`, only that `entrypoint` is read first; its own `entries` then apply the same routing contract.

Agents read a generated `entry` tagged #KeepInMind like #LoadNow, keep its instructions active while working, and recheck it before ending meaningful work to perform the follow-ups it requires.

Load-policy tags are loading policies only. They do not create authority, scope, precedence, or a write requirement. They do not search unloaded trees; autoload requires a loaded parent chain.

The default payload tags every Open Forge core `entrypoint` with #LoadNow, or #KeepInMind where standing follow-up matters. Root `entries` therefore load with the loader, and nested core `entrypoints` load as their parent `Entries` become visible.

Local active truth has precedence over Open Forge defaults. Default files may still be loaded as context when useful.

When an agent selects a generated loader route, it must load the generated `entrypoint` path before exploring routed files under that route. Open Forge-authored `entrypoints` use `_{folder}.md`; compatibility `entrypoints` may use another recognized `entrypoint` name.

The installed loader must define its routing terms plainly enough to understand without maintainer docs. Terms must include:

- `entrypoint`
- `entry`
- `framework route`
- `scope route`
- `scoped framework route`
- `slug`
- `axiom`

The loader must explain that `entries` point to sibling markdown files, direct child `entrypoints`, and supported native skill package entrypoints.

The loader may include route patterns with `[scope]` placeholders to explain scoping shape. Those placeholders are examples only. Generated `entries` and installed routes must use concrete paths.

When a markdown file is loaded, its `.overwrite.md` companion must be loaded after it when present. The overwrite applies within the base file's scope.

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

## Authority Contract

The installed loader must state these authority axioms:

- user instructions apply when safe and allowed
- loaded `axioms` are mandatory unless higher-priority instructions conflict
- required memory actions are complete only after the file is written or a blocker is reported
- local active truth overrides Open Forge defaults
- written #Memory routes are preferred over private or opaque agent memory for durable workspace state
- axioms of loaded ancestor `entrypoints` apply to all routes below them
- writing files inside existing routes is normal use; child categories are added when they improve routing, ownership, or clarity, and new root routes need clear scope
- generated `entries` are navigation metadata; only reserved load-policy tags affect loading
- reserved load-policy tags affect loading only and do not create authority
- archived, historical, example, external, and temporary continuation material is contextual unless restored or promoted
- candidate learning is contextual, not authority
- detailed behavior belongs in the routed file or concept that owns it

## Tags Contract

The installed loader must define framework tag behavior in a `## Tags` section.

The installed `## Tags` section must contain:

- `### Axioms`
- `### Defined Tags`

Tag axioms must state that defined tags have framework meaning when they appear in loaded content or generated `entries`, undefined tags remain routing and search signals, entries without a load-policy tag are on-demand routes selected by the current request, tag spelling and casing are stable, and workspace-wide tag behavior belongs in the tag section.

Defined tags must include:

- #LoadNow
- #KeepInMind
- #Core
- #Memory
- #Extension
- #Contextual
- #CurrentTruth

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
- remains valid when `.agents/` resolves through a symlink or into a submodule
- defines `entrypoint`, `entry`, `framework route`, `scope route`, `scoped framework route`, `slug`, and `axiom`
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
- defines loaded `axioms` as mandatory
- reads generated #LoadNow `entries` when they appear in loaded `Entries`, in listed order
- rechecks loaded #KeepInMind `entries` before ending meaningful work
- marks default Open Forge core `entrypoints` with #LoadNow, or #KeepInMind where standing follow-up matters
- generates `entries` for direct active root routes only
- derives descriptions and tags from category `entrypoints`
- loads a selected category `entrypoint` before its routed files
- loads overwrite companions after their base files
- states that local active truth overrides defaults
- prefers written #Memory routes over private or opaque agent memory for durable workspace state
- distinguishes normal writes inside existing routes from routing-structure changes
- treats generated `entries` as navigation metadata plus reserved load policy
- keeps detailed process behavior in routed files
- keeps customization guidance minimal and diff-friendly
