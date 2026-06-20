# Formatting

## Description

This descriptor governs markdown formatting conventions across the installable Open Forge payload.

Formatting is part of the framework contract because agents route through these files repeatedly. The files must be easy to scan, easy to diff, and cheap to load.

## Represents

Formatting represents the shared markdown shape for Open Forge files.

It is a cross-cutting concept. It applies to descriptors, installable route files, managed category contracts, generated indexes, user route files, and compact concept files.

## Contains

Open Forge markdown files must prefer:

- short headings
- short paragraphs
- line-based lists
- one-line route entries
- compact examples
- stable separators
- minimal decoration

Files must stay readable in plain text. They must not rely on tables when a list communicates the same structure with fewer tokens and less visual noise.

## Entry Format

Use this entry shape for compact route, constant, and index-like lists:

```text
- {entry} - {description} - #{Tag1} #{Tag2} ... #{TagN}
```

Generated index entries list direct route files and direct child indexes:

```text
- `alpha.md` - Route file - #Route
- `repos/_repos.md` - Child route index - #Index
```

Generated index entries keep the full relative file path in backticks. Child folders become visible through their own `_{folder}.md` index. Parent indexes stay at one folder boundary.

This shape is preferred because it is:

- visually scannable
- easy for agents to parse
- cheap in tokens
- stable under generated updates
- easy to diff line by line

Use one entry per line. Do not wrap entries unless the description becomes unreadable.

## Backticks

Use backticks when the entry is a concrete filename, path, command, or code literal:

```text
- `_workspace-open-forge.md` - Open Forge workspace route contract - #OpenForge #Workspace
```

Backticks may be omitted for short symbolic entries when the format is already unambiguous:

```text
- {repoRoot} - Root of the installed workspace - #Constant
```

Generated index entries must keep backticks around the generated file path because the path is the lookup target.

## Frontmatter

Indexed files should use scoped metadata:

```yaml
---
open-forge:
  description: Local workspace routes
  tags: [Workspace, Local]
---
```

Open Forge-authored routed files use only `open-forge:` scoped metadata.

The index generator accepts `rune:` scoped metadata in external route files for cross-tool compatibility.

Unscoped `description` and `tags` are accepted for compatibility in external route files.

Direct-load files that are not discovered through indexes do not need frontmatter unless another tool needs it.

Generated index files do not need frontmatter.

## Index Files

Index files must stay dull.

They contain:

- a title
- one short description
- `## Entries`
- generated entries

They do not contain behavior, recommendations, examples, or overwrite companions.

## Category Files

Routed categories use this filename shape:

- `_{category}.md` - generated category index
- `_{category}-open-forge.md` - Open Forge managed category contract
- `{name}.md` - user, workspace, or routed content file

`{category}` is the category folder name. For `{forgePath}/patterns/`, the generated index is `_patterns.md` and the managed category contract is `_patterns-open-forge.md`.

`_{category}.md` is the generated index for a category. `index.md` and `_index.md` are reserved index aliases. Other underscore-prefixed markdown files are indexable files when they are direct files under the indexed folder.

The managed category contract exists when Open Forge has category-level behavior to state. It contains the category meaning, category boundaries, and category axioms. Generated route entries belong in `_{category}.md`.

User files can use any clear filename. A markdown file is indexable when it is a direct route file in a generated index folder and uses `*.md`, including underscore-prefixed names, except generated index files, `_index.md`, `index.md`, and `.overwrite.md` companions.

A child folder is indexable from its parent when the child folder contains its own `_{folder}.md` index.

## Why

Open Forge optimizes for routing. Formatting must make routing cheap.

The one-line entry shape is less pretty than a table for some humans, but it is easier to scan in raw markdown, easier to regenerate, cheaper in tokens, and easier to review in git.

## Alignment Checks

Formatting is aligned when:

- route entries use the compact entry shape
- generated index entries use backticks around file paths
- generated index entries include direct route files and direct child indexes
- symbolic constant entries may omit backticks when unambiguous
- scoped `open-forge:` frontmatter is used for Open Forge-authored indexed routed files
- direct-load files avoid unnecessary frontmatter
- tables are used only when a list would be less clear
- generated index files contain no behavior
- only `_{category}.md` files are regenerated as indexes
- `_{category}-open-forge.md` files are indexed managed category contracts
