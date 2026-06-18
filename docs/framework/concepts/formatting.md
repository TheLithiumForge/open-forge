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
- flat bullet lists
- one-line route entries
- compact examples
- stable separators
- minimal decoration

Files must stay readable in plain text. They must not rely on tables when a list communicates the same structure with fewer tokens and less visual noise.

## Entry Format

Use this entry shape for compact route, constant, and index-like lists:

```text
- {entry} - {description} - #{Tag} #{Tag}
```

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

`rune:` metadata is accepted for cross-tool compatibility.

Unscoped `description` and `tags` are accepted for compatibility, but scoped metadata is preferred for Open Forge-authored routed files.

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

Only `_{category}.md` is a generated index. Other underscore-prefixed markdown files are category control files or reserved control files. They must not be regenerated as indexes and they must not appear as generated index entries.

The managed category contract exists only when Open Forge has category-level behavior to state. It contains the category meaning, category boundaries, and category axioms. It does not contain generated route entries.

User files can use any clear filename. A markdown file is indexable when it is a sibling of a generated index, does not start with `_`, and is not an overwrite file.

Open Forge must not seed generic `local.md` files. Users add local files when local shape earns a file, and those files keep their own names.

## Why

Open Forge optimizes for routing. Formatting must make routing cheap.

The one-line entry shape is less pretty than a table for some humans, but it is easier to scan in raw markdown, easier to regenerate, cheaper in tokens, and easier to review in git.

## Alignment Checks

Formatting is aligned when:

- route entries use the compact entry shape
- generated index entries use backticks around file paths
- symbolic constant entries may omit backticks when unambiguous
- scoped `open-forge:` or `rune:` frontmatter is used for indexed routed files
- direct-load files avoid unnecessary frontmatter
- tables are used only when a list would be less clear
- generated index files contain no behavior
- only `_{category}.md` files are regenerated as indexes
- `_{category}-open-forge.md` files are treated as managed category contracts
- generic `local.md` files are not seeded by Open Forge
