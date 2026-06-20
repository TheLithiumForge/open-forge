# Open Forge CLI

The Open Forge CLI is intentionally small. It installs the released framework files, updates `AGENTS.md`, and rebuilds generated indexes.

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
- rebuild generated indexes
- leave user-added files outside managed paths alone

There is no wizard. The command installs the current release payload. If you want a different local shape, install first, then edit or add files. The framework is plain markdown for exactly this reason.

## index

```sh
open-forge index
open-forge index {target-folder}
```

`index` rebuilds generated index files.

When `.agents/` exists, the CLI scans `.agents/`. Otherwise it scans the target folder.

An index file is named `_{folder-name}.md`:

```text
.agents/patterns/
  _patterns.md
  _patterns-open-forge.md
  local-docs.md
```

The index file reads direct markdown route files and direct child indexes:

```md
- `{file}` - {description} - #{tag1} #{tag2} ... #{tagN}
- `{folder/_folder.md}` - {description} - #Index
```

Child folders are routed through their own `_{folder-name}.md` index. Parent indexes stay at one folder boundary.

Index files keep their text above `## Entries`. The CLI replaces the generated entries below it.

Default index files should stay boring: a short description and entries only.

Change index output by adding, editing, moving, or removing route files and child indexes in the indexed folder.

The index generator reads:

- direct `*.md` route files, including underscore-prefixed files such as `_{name}-open-forge.md`
- direct child indexes named `_{folder-name}.md`

Reserved index filenames in the indexed folder are:

- `_{folder-name}.md`
- `_index.md`
- `index.md`

The index generator ignores tool paths:

- `.git/`
- `.obsidian/`
- `node_modules/`

Metadata comes from frontmatter:

```md
---
description: Local documentation patterns
tags: [Docs, Pattern]
---
```

Nested metadata works too:

```md
---
open-forge:
  description: Local documentation patterns
  tags: [Docs, Pattern]
---
```

The CLI also accepts `rune:` scoped metadata in user-added route files for cross-tool compatibility. Open Forge-authored files use `open-forge:` metadata.
