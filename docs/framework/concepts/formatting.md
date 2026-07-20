# Formatting

## Description

This descriptor governs markdown formatting conventions across the installable Open Forge payload.

Formatting is part of the framework contract because agents route through these files repeatedly. Files must be easy to scan, easy to diff, and cheap to load.

## Represents

Formatting represents the shared markdown shape for Open Forge files.

It is a cross-cutting concept. It applies to descriptors, category `entrypoints`, generated index regions, routed files, and compact concept files.

## Contains

Open Forge markdown files must prefer:

- short headings
- short paragraphs
- line-based lists
- one-line route `entries`
- compact examples
- stable separators
- minimal decoration

Files must stay readable in plain text. They must use lists instead of tables when a list communicates the same structure with fewer tokens and less visual noise.

User-facing Open Forge files state the positive current contract in complete natural language. Avoid compressed fragments, dash-delimited asides, and mentions of rejected prototypes that make readers wonder whether the old shape still matters. Isolated one-sentence definitions and single-sentence bullets may omit terminal periods; multi-sentence prose keeps normal punctuation.

When prose introduces a following list, keep the introducer as prose followed by the list, or give the child list an explicit subheading or true nested-list structure. Do not express an introducer and the items it introduces as adjacent peer bullets.

## Entry Format

Compact route and generated index `entries` must use this shape:

```md
- [Description](relative/path.md) - #Tag1 #Tag2 ... #TagN
```

Generated `entries` list direct routed files and direct child category `entrypoints`:

```md
- [Route file](alpha.md) - #Route
- [Repository workspace routes](repos/_repos.md) - #Repository #Workspace
- [Summarize content](summarize/SKILL.md) - #Skill
```

The link label is the decision-grade description. The link destination is the complete path from the Markdown file containing the link to the routed target. Standard relative-link resolution therefore works in editors, renderers, graph tools, and plain Markdown-aware agents without an Open Forge-specific path convention.

Loader `entries` follow the same rule. Because the loader lives at `.agents/loader.md`, its direct category links use destinations such as `workspace/_workspace.md`, not `.agents/workspace/_workspace.md`. A category `entrypoint` links to its direct files and child `entrypoints` relative to its own folder. Parent category `entrypoints` stay at one folder boundary, and a child folder becomes visible through its own `_{folder}.md` `entrypoint`.

Use one `entry` per line. `Entries` must not wrap.

Authored `Required Routes` lines use the same one-line link shape. Their link label explains why the workflow needs the target, their destination resolves relative to the workflow file, and their suffix includes useful routing tags with at least the target primitive type:

```md
## Required Routes

- [Implementation capability](../skills/implementation/SKILL.md) - #Skill #Implementation
```

The generated empty-state sentinel `- none - No entries - #Empty` is the only `Entries` line without a link destination.

## Tags

Tags must add compact routing information rather than repeat words without adding meaning.

Useful tags identify the primitive type, domain, work type, topic, technology, artifact, lifecycle, or another selection signal. Use as many tags as the `entry` needs and no tags that do not improve routing.

```md
- [Database migration requirements](migrations.md) - #Directive #Database #Migration
```

Paths and descriptions must keep critical scope readable. Tags reinforce route selection, but tags alone must not create directive authority. Direct root files are workspace-wide because the root directive route is baseline-loaded; child directive scope is selected before its body is opened.

Use normal words when naming, defining, or explaining the local concept itself. Use tags when the text points to routed ownership, classification, promotion, load policy, truth status, or search/reference targets. For example, a memory `entrypoint` says "Memory records state"; a promotion rule can say "move to #Core".

Open Forge-authored payload tags use singular PascalCase concept names by default. Built-in route type tags are #Directive, #Pattern, #Guidance, #Skill, #Workflow, and #Workspace.

Layer classification tags are singular: #Core, #Memory, and #Extension. These tags classify where material belongs; they do not create authority by themselves.

Reserved load-policy tags are different from normal classification tags. Open Forge currently reserves #LoadNow and #KeepInMind, which are governed by `docs/framework/concepts/routing.md` and defined in the installed loader.

## Links And Backticks

Use Markdown links for routed destinations and other clickable references:

```md
- [Repository workspace routes](repositories.md) - #Workspace
```

Use backticks for commands, code literals, defined Open Forge terms, and concrete paths discussed as text rather than used as link destinations.

The CLI may read the legacy backtick entry shape during migration, including legacy `Required Routes`, which retain workspace-root-relative resolution. Generated output and newly authored entries use containing-file-relative Markdown links. Parser compatibility is not a second canonical authoring format.

Tags must stay bare, including in prose, so graph and search tools can recognize them.

Backtick defined Open Forge terms when precision matters, especially `entrypoint`, `entry`, `Entries`, `framework route`, `scope route`, `scoped framework route`, and `slug`. Do not backtick headings. Use normal words when the text means the ordinary English word rather than the framework term.

## Frontmatter

Open Forge-authored category `entrypoints` and indexed routed files must use scoped metadata:

```yaml
---
open-forge:
  description: Local workspace routes
  tags: [LoadNow, Core, Workspace]
---
```

Open Forge-authored files use only `open-forge:` scoped metadata.

The index generator accepts `rune:` scoped metadata in external files for cross-tool compatibility. It also accepts unscoped `description` and `tags` in external files.

Direct-load files that are not discovered through indexes do not need frontmatter unless another tool needs it.

Standard skill files such as `SKILL.md` use the metadata required by their runtime. Open Forge index generation can read a root `description` from those files and must not require `open-forge:` metadata in them.

## Category Entrypoints

A routed folder is represented by one category `entrypoint`:

```text
_{category}.md
```

`{category}` is the category folder name. For `.agents/patterns/`, the category `entrypoint` is `_patterns.md`.

Open Forge-authored categories must use `_{category}.md`.

For cross-tool compatibility, the CLI also accepts these category `entrypoint` names:

- `_index.md`
- `index.md`
- `_references.md`
- `references.md`

Compatibility aliases are CLI input only. Open Forge framework descriptors, payload files, and examples must use the canonical name. A folder must contain exactly one recognized category `entrypoint`; generation must stop before writing when multiple candidates exist.

A category `entrypoint` contains:

- scoped metadata when Open Forge authors the file
- a title
- one short category description
- compact category meaning, boundaries, or axioms when the category requires them
- a final `## Entries` section
- one generated index region

Category rules belong before `## Entries`. The authored portion must contain only stable category-level meaning and axioms. Detailed behavior, guidance, patterns, and user content belong in routed files.

An `## Axioms` section in a local or user-created category is optional. Missing, empty, `inherited`, or `none` all mean that the category adds no local axioms; loaded ancestor axioms remain active. When a sentinel is used, it must not be mixed with substantive local axioms.

Installed category `entrypoints` must be understandable without governance descriptors. Any rule required for agent behavior must appear in installed payload content, not only in `docs/framework/`.

The authored portion of an Open Forge category `entrypoint` must stay between 5 and 80 non-empty lines. Generated `entries` do not count toward this limit.

## Primitive Contracts

A direct directive file declares #LoadNow in its metadata and exactly one non-empty level-2 `## Axioms` section. It does not declare `## Applies To`; the active directive route already established scope before loading the file. `inherited` and `none` are category-entrypoint sentinels, not direct directive contents. Operational conditions may appear inside an Axiom without making the loaded directive optional.

A workflow starts with `## Mode`, followed in order by `## Goal`, `## Required Routes`, `## Constraints`, `## Steps`, `## Loop`, `## Outputs`, and `## Completion`. Goal may include one optional `- helpful before: ...` item naming prior work that would improve the result. The item is advisory, never a route dependency or blocker. Mode is exactly `linear` or `iterative`. Constraints is always present and uses `- none` when no workflow-specific invariant applies. Every workflow seeks its Goal; goal-seeking is not a separate mode. Every complete recipe declares exactly one of `PhaseDiscovery`, `PhaseDefinition`, `PhasePlanning`, `PhaseDelivery`, or `PhaseVerification` in frontmatter; phase tags are routing wayfinding, not extra body sections or mandatory chronology.

## Scope Route Slugs

A `scope route` is a routed folder used to narrow meaning or ownership for routes below it.

A `slug` is the stable folder segment used to create that route.

`Slug` folders must use concrete, stable names. Prefer lowercase kebab-case unless an existing external name requires another stable spelling.

A `slug` folder becomes a `scope route` only when it contains exactly one recognized `entrypoint`. Use the same canonical shape:

```text
mobile-app/
  _mobile-app.md
```

Every folder in a visible route chain must have its own `entrypoint`. A deep file below a folder without an `entrypoint` is not reachable through generated routing.

Route-template placeholders such as `[scope]`, `[route]`, or `[state]` are maintainer and CLI notation only. Installed payload paths and generated `entries` must contain concrete folder names, not placeholders.

Display names belong in titles, descriptions, or frontmatter. They must not require hidden metadata to explain a `slug` or `scope route`.

## Generated Index Region

The final section of every category `entrypoint` and the loader must use this shape:

```md
## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
```

The CLI owns only the content between the markers. Index generation must preserve all content outside the markers.

In category `entrypoints`, the generated region contains direct routed files, direct child category `entrypoints`, and standard `SKILL.md` files inside skills routes. In the loader, it contains direct active category `entrypoints`. Generated regions contain navigation metadata plus reserved load policy only. Generated `entries` never define instructions, behavior, or authority.

When the markers are absent from a legacy category `entrypoint` with a final `## Entries` section, the CLI must migrate that section. When the heading and markers are all absent, the CLI must append the complete generated section.

Generation must stop without writing when markers are incomplete, duplicated, reversed, detached from `## Entries`, or followed by authored content.

## Indexed Files

User files can use any clear filename.

A direct markdown file is indexable when it uses `*.md`, including underscore-prefixed names, except:

- the folder's recognized category `entrypoint`
- canonical and compatibility `entrypoint` names
- runtime skill entrypoint names such as `SKILL.md` and `Skill.md`
- `.overwrite.md` companions

Inside `.agents/skills/`, direct loose markdown files are not indexable skill routes. A direct child folder is indexable as a skill when it contains exactly one supported entrypoint such as `SKILL.md`. The generated route points to that file. Other files in the skill folder remain skill resources and are not indexed unless the folder also defines normal Open Forge child categories.

A child folder is indexable when it contains exactly one recognized category `entrypoint`. The CLI must not create category `entrypoints` for folders that have not explicitly opted into routing.

## Why

Open Forge optimizes for routing. Formatting must make routing cheap.

One category `entrypoint` gives agents category meaning and navigation without an extra mandatory file. The bounded generated region keeps machine output obvious and prevents index generation from replacing authored content.

The one-line `entry` shape is easier to scan in raw markdown, easier to regenerate, cheaper in tokens, and easier to review in git than a table.

## Alignment Checks

Formatting is aligned when:

- `entries` use the compact one-line Markdown-link shape with a tag suffix
- tags add useful routing information with minimal text
- Open Forge-authored payload tags use singular PascalCase concept names by default
- layer tags classify material without creating authority
- reserved load-policy tags are documented before use
- generated link targets are concrete and relative to the Markdown file containing them
- loader link targets resolve from `.agents/loader.md` without repeating the `.agents/` prefix
- `Required Routes` use the same link shape and carry useful tags, including at least the target primitive type
- legacy backtick entries remain parser-compatible during migration but are not emitted or newly authored
- category generated `entries` include direct routed files and direct child category `entrypoints`
- skills category generated `entries` include standard `SKILL.md` skills
- loader generated `entries` include direct active category `entrypoints`
- Open Forge-authored indexed files use only scoped `open-forge:` frontmatter
- `SKILL.md` files keep runtime-compatible metadata
- direct-load files avoid unnecessary frontmatter
- tables are used only when a list would be less clear
- Open Forge-authored categories use `_{category}.md`
- compatibility `entrypoint` names are accepted only by the CLI
- each routed folder contains at most one recognized `entrypoint` name
- `scope route` `slugs` use concrete stable names and matching `entrypoints`
- route-template placeholders stay out of installed payload paths and generated `entries`
- category contracts stay before the generated region
- direct directive files carry #LoadNow, contain one substantive level-2 Axioms section, and have no `Applies To` gate
- directive scope is visible on the route selection surface before the directive body is opened
- complete workflow recipes declare exactly one recognized primary phase tag without adding phase sections or physical routing layers
- installed category `entrypoints` do not depend on governance-only context
- user-facing prose states positive current behavior in complete natural language
- prose that introduces a following list is not represented as an adjacent peer bullet to its children
- generated `entries` stay inside the required markers
- only marker-bounded content is regenerated
