# Formatting

## Description

This descriptor governs markdown formatting conventions across the installable Open Forge payload.

Formatting is part of the framework contract because agents route through these files repeatedly. Files must be easy to scan, easy to diff, and cheap to load.

## Represents

Formatting represents the shared markdown shape for Open Forge files.

It is a cross-cutting concept. It applies to descriptors, category entrypoints, generated index regions, routed files, and compact concept files.

## Contains

Open Forge markdown files must prefer:

- short headings
- short paragraphs
- line-based lists
- one-line route entries
- compact examples
- stable separators
- minimal decoration

Files must stay readable in plain text. They must use lists instead of tables when a list communicates the same structure with fewer tokens and less visual noise.

## Entry Format

Compact route and generated index entries must use this shape:

```text
- {entry} - {description} - #{Tag1} #{Tag2} ... #{TagN}
```

Generated entries list direct routed files and direct child category entrypoints:

```text
- `alpha.md` - Route file - #Route
- `repos/_repos.md` - Child category entrypoint - #Index
```

Generated entries keep the full relative path in backticks. Parent category entrypoints stay at one folder boundary. A child folder becomes visible through its own `_{folder}.md` entrypoint.

Use one entry per line. Entries must not wrap.

## Tags

Tags must add compact routing information rather than repeat words without adding meaning.

Useful tags identify the primitive type, domain, work type, topic, technology, artifact, lifecycle, or another selection signal. Use as many tags as the entry needs and no tags that do not improve routing.

```text
- `migrations.md` - Database migration requirements - #Directive #Database #Migration
```

Paths and descriptions must keep critical scope readable. Tags reinforce and describe scope, but tags alone must not make a directive workspace-wide, mandatory, or active.

Open Forge-authored payload tags use singular PascalCase concept names by default. Built-in route type tags are `#Directive`, `#Pattern`, `#Guideline`, `#Skill`, `#Workflow`, and `#Workspace`.

Layer classification tags are singular: `#Core`, `#Memory`, and `#Extension`. These tags classify where material belongs; they do not create authority by themselves.

Reserved load-policy tags are different from ordinary classification tags. Open Forge currently reserves only `#LoadWithParentEntrypoint`, which is governed by `docs/framework/concepts/routing.md` and defined in the installed loader.

## Backticks

Use backticks when an entry is a concrete filename, path, command, or code literal:

```text
- `repositories.md` - Repository workspace routes - #Workspace
```

Generated entries must keep backticks around paths because each path is a lookup target.

## Frontmatter

Open Forge-authored category entrypoints and indexed routed files must use scoped metadata:

```yaml
---
open-forge:
  description: Local workspace routes
  tags: [OpenForge, Core, Workspace, Index]
---
```

Open Forge-authored files use only `open-forge:` scoped metadata.

The index generator accepts `rune:` scoped metadata in external files for cross-tool compatibility. It also accepts unscoped `description` and `tags` in external files.

Direct-load files that are not discovered through indexes do not need frontmatter unless another tool needs it.

## Category Entrypoints

A routed folder is represented by one category entrypoint:

```text
_{category}.md
```

`{category}` is the category folder name. For `.agents/patterns/`, the category entrypoint is `_patterns.md`.

Open Forge-authored categories must use `_{category}.md`.

For cross-tool compatibility, the CLI also accepts these category entrypoint names:

- `_index.md`
- `index.md`
- `_references.md`
- `references.md`

Compatibility aliases are CLI input only. Open Forge framework descriptors, payload files, and examples must use the canonical name. A folder must contain exactly one recognized category entrypoint; generation must stop before writing when multiple candidates exist.

A category entrypoint contains:

- scoped metadata when Open Forge authors the file
- a title
- one short category description
- compact category meaning, boundaries, or axioms when the category requires them
- a final `## Entries` section
- one generated index region

Category rules belong before `## Entries`. The authored portion must contain only stable category-level meaning and axioms. Detailed behavior, guidance, patterns, and user content belong in routed files.

Installed category entrypoints must be understandable without governance descriptors. Any rule required for agent behavior must appear in installed payload content, not only in `docs/framework/`.

The authored portion of an Open Forge category entrypoint must stay between 5 and 80 non-empty lines. Generated entries do not count toward this limit.

## Generated Index Region

The final section of every category entrypoint and the loader must use this shape:

```md
## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
```

The CLI owns only the content between the markers. Index generation must preserve all content outside the markers.

In category entrypoints, the generated region contains direct routed files and direct child category entrypoints. In the loader, it contains direct active category entrypoints. Generated regions contain navigation metadata plus reserved load policy only. Generated entries never define instructions, behavior, or authority.

When the markers are absent from a legacy category entrypoint with a final `## Entries` section, the CLI must migrate that section. When the heading and markers are all absent, the CLI must append the complete generated section.

Generation must stop without writing when markers are incomplete, duplicated, reversed, detached from `## Entries`, or followed by authored content.

## Indexed Files

User files can use any clear filename.

A direct markdown file is indexable when it uses `*.md`, including underscore-prefixed names, except:

- the folder's recognized category entrypoint
- canonical and compatibility entrypoint names
- `.overwrite.md` companions

A child folder is indexable when it contains exactly one recognized category entrypoint. The CLI must not create category entrypoints for folders that have not explicitly opted into routing.

## Why

Open Forge optimizes for routing. Formatting must make routing cheap.

One category entrypoint gives agents category meaning and navigation without an extra mandatory file. The bounded generated region keeps machine output obvious and prevents index generation from replacing authored content.

The one-line entry shape is easier to scan in raw markdown, easier to regenerate, cheaper in tokens, and easier to review in git than a table.

## Alignment Checks

Formatting is aligned when:

- entries use the compact one-line shape
- tags add useful routing information with minimal text
- Open Forge-authored payload tags use singular PascalCase concept names by default
- layer tags classify material without creating authority
- reserved load-policy tags are documented before use
- generated paths use backticks
- generated paths are concrete and workspace-root-relative
- category generated entries include direct routed files and direct child category entrypoints
- loader generated entries include direct active category entrypoints
- Open Forge-authored indexed files use only scoped `open-forge:` frontmatter
- direct-load files avoid unnecessary frontmatter
- tables are used only when a list would be less clear
- Open Forge-authored categories use `_{category}.md`
- compatibility entrypoint names are accepted only by the CLI
- each routed folder contains at most one recognized entrypoint name
- category contracts stay before the generated region
- installed category entrypoints do not depend on governance-only context
- generated entries stay inside the required markers
- only marker-bounded content is regenerated
