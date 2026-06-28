# Open Forge CLI

The Open Forge CLI is intentionally small. It installs the released framework files, updates `AGENTS.md`, and rebuilds generated index regions.

The distributed CLI runs on Node.js.

## Commands

```sh
open-forge install [target]
open-forge index [target]
```

If `target` is omitted, the current directory is used.

## install

```sh
open-forge install
open-forge install {target-folder}
```

`install` is idempotent.

Running it will:

- create or update `AGENTS.md`
- append the Open Forge block if missing
- replace only the Open Forge block if it already exists
- overwrite Open Forge managed files with the same name
- rebuild generated index regions
- leave user-added files outside managed paths alone

There is no wizard. The command installs the current release payload. If you want a different local shape, install first, then edit or add files. The framework is plain markdown for exactly this reason.

## index

```sh
open-forge index
open-forge index {target-folder}
```

`index` rebuilds the loader category registry and the generated regions inside category entrypoints.

When `.agents/` exists, the CLI scans `.agents/`. Otherwise it scans the target folder.

When `loader.md` exists at the scan root, the CLI generates one loader entry for every direct child folder that contains one recognized category entrypoint. Loader descriptions and tags come from the category entrypoint, preferring supported metadata and falling back to its first body description and `#Index`.

In an installed workspace, loader paths are concrete and relative to the target folder, such as `.agents/workspace/_workspace.md`. The target folder is the logical workspace root even when `.agents/` is a symlink or its contents come from a submodule. The CLI does not derive routing roots from Git boundaries.

Nested categories stay behind their parent category entrypoint. Folders without a matching entrypoint do not become loader routes.

Open Forge-authored category entrypoints are named `_{folder-name}.md`:

```text
.agents/patterns/
  _patterns.md
  local-docs.md
```

The category entrypoint contains stable category meaning followed by a generated region. The generated region reads direct markdown route files and direct child category entrypoints:

```md
- `{file}` - {description} - #{Tag1} #{Tag2} ... #{TagN}
- `{folder/_folder.md}` - {description} - #Index
```

Child folders are routed through their own `_{folder-name}.md` category entrypoint. Parent entrypoints stay at one folder boundary.

For cross-tool compatibility, the CLI also recognizes these entrypoint names:

- `_index.md`
- `index.md`
- `_references.md`
- `references.md`

Open Forge itself uses only `_{folder-name}.md`. A folder must contain exactly one recognized entrypoint name. If multiple candidates exist, the CLI stops before changing any generated region.

The generated region is bounded explicitly:

```md
## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
```

The CLI replaces only the content between the markers. It preserves frontmatter and category content above the region.

When a legacy category entrypoint or loader has a final `## Entries` section containing only generated list entries, the CLI adds the markers automatically. If the heading is absent, the CLI appends the complete section. Malformed or non-final markers stop generation without changing the file.

Change index output by adding, editing, moving, or removing route files and child category entrypoints in the indexed folder.

The index generator reads:

- direct `*.md` route files, including underscore-prefixed routed files
- direct child category entrypoints named `_{folder-name}.md`

Reserved filenames in an indexed folder are:

- `_{folder-name}.md`
- `_index.md`
- `index.md`
- `_references.md`
- `references.md`

The index generator ignores tool paths:

- `.git/`
- `.obsidian/`
- `node_modules/`

Metadata comes from frontmatter:

```md
---
description: Local documentation patterns
tags: [Doc, Pattern]
---
```

Nested metadata works too:

```md
---
open-forge:
  description: Local documentation patterns
  tags: [Doc, Pattern]
---
```

The CLI also accepts `rune:` scoped metadata in user-added route files for cross-tool compatibility. Open Forge-authored files use `open-forge:` metadata.
