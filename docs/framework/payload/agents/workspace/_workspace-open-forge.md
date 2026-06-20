# Workspace Category Contract

## Description

This descriptor governs `src/open-forge/.agents/workspace/_workspace-open-forge.md`.

The workspace category contract defines what workspace route files represent and how agents use the workspace route category.

## Represents

The workspace category represents discoverable workspace paths, ownership boundaries, and route meaning.

It is the bridge between the loader and the actual files a workspace wants agents to find.

## Contains

The installed workspace category contract must contain:

- scoped `open-forge:` frontmatter with description and tags for generated indexes
- the purpose of workspace route files
- the rule that `_workspace.md` is loaded before route files
- the rule that route files point to destinations and the destination owns detailed truth
- the default Open Forge route entries for installed framework paths
- the distinction between agent-owned and human-owned paths

The installed workspace category contract must stay compact. Target size is 20-80 non-empty lines.

## Route Contract

Workspace route files contain path entries.

Path entries must use this shape:

```md
- {path} - {description} - #{Tag1} #{Tag2} ... #{TagN}
```

Use backticks when `{path}` is a concrete relative path or filename. Backticks may be omitted for symbolic constants such as `{forgePath}` and `{docsPath}` when the entry is unambiguous.

A route entry tells an agent where to look. The target file or folder owns the detailed truth.

## Required Routes

The installed workspace category contract must route these installed areas:

- `{forgePath}/` - agent-owned Open Forge files
- `{forgePath}/constants.md` - root path constants
- `{forgePath}/loader.md` - agent loading entrypoint
- `{forgePath}/workspace/_workspace.md` - generated workspace route index
- `{forgePath}/workspace/_workspace-open-forge.md` - workspace category contract
- `{docsPath}/` - human-facing documentation root
- `{docsPath}/directives/` - human-reviewed local rules
- `{docsPath}/guides/` - human-facing guidance

It may route future Open Forge categories as their active implementation files are added.

## Customization Contract

Users add workspace route files under `{forgePath}/workspace/` with clear names that match the local workspace shape.

Agents must load `_workspace.md` before choosing route files. Agents load `_workspace-open-forge.md` when they need to understand workspace route meaning or maintain workspace route files.

## Used By

The loader, agents, skills, and future category files use the workspace category contract when they need to resolve where workspace knowledge lives.

## Why

The workspace category exists so Open Forge does not hardcode one project layout.

It gives agents a small routing surface while letting each workspace define its own real paths.

## Alignment Checks

The implementation is aligned when it:

- is named `_workspace-open-forge.md`
- stays separate from `_workspace.md`
- defines workspace routes as navigation, not full truth
- includes only active installed routes
- keeps user route filenames open-ended
