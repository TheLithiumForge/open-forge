# Workspace Index

## Description

This descriptor governs `src/open-forge/.agents/workspace/_workspace.md`.

The workspace index is the generated index for workspace route files.

## Represents

The workspace index represents category navigation for workspace routes.

It is a generated index file. Its responsibility is category navigation only.

## Contains

The installed workspace index must contain:

- a title
- one short description
- `## Entries`
- generated entries for direct workspace route files and direct child workspace indexes

The installed workspace index must stay dull. Its content is limited to the index prefix and generated entries.

When no indexable workspace route files exist, the generated entries section must contain:

```md
- none - No entries - #Empty
```

## Entry Contract

Generated entries must use this shape:

```md
- `{file}` - {description} - #{Tag1} #{Tag2} ... #{TagN}
```

Child folders must route through child indexes:

```md
- `area.md` - Workspace route file - #Workspace
- `repos/_repos.md` - Repository workspace route index - #Workspace #Index
```

Direct markdown files in the indexed folder are indexable route entries, including underscore-prefixed files such as `_workspace-open-forge.md`. Direct child folders are indexable when they contain their own `_{folder}.md` child index.

Reserved index filenames are `_workspace.md`, `_index.md`, and `index.md`. Overwrite companions use `.overwrite.md` and stay outside generated entries.

## Used By

The loader loads this file first after constants.

Agents use it to discover workspace route files before deciding which route files are relevant to the current request.

## Why

The workspace index exists so the loader can route cheaply without loading every workspace route file.

It keeps Open Forge route-based and lets users add their own workspace route files with any clear filename.

## Alignment Checks

The implementation is aligned when it:

- is named `_workspace.md`
- lives in `{forgePath}/workspace/`
- contains generated navigation only
- lists direct workspace route files and direct child workspace indexes
- includes `_workspace-open-forge.md` as an indexed managed category contract
- uses compact one-line entries
