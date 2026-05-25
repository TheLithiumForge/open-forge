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

An index file is any markdown file in that scanned area named with a leading underscore:

```text
.agents/patterns/
  _patterns.md
  open-forge.md
  local-docs.md
```

The index file reads markdown siblings in the same folder and generates entries like:

```md
- `{file}` - {description} - #{tag1} #{tag2}
```

Index files keep their text above `## Entries`. The CLI replaces the generated entries below it.

Default index files should stay boring: a short description and entries only.

Do not use overwrite files for indexes. Add or edit files in the indexed folder instead.

It skips:

- `.overwrite.md` files
- `_*.md` index files
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

`rune:` is accepted the same way for cross-tool compatibility.
