# Open Forge CLI

The Open Forge CLI is intentionally small. It installs the released framework files, installs local extension overlays, updates `AGENTS.md`, and rebuilds generated index regions.

The distributed CLI runs on Node.js.

## Commands

```sh
open-forge install [target]
open-forge extend --list
open-forge extend <extension-source-or-id> [target]
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
- update `scoped framework route` `entrypoints` recognized by path shape
- rebuild generated index regions
- leave user-added files outside managed paths alone

There is no wizard. The command installs the current release payload. If you want a different local shape, install first, then edit or add files. The framework is plain markdown for exactly this reason.

The current CLI indexes any explicit route chain whose folders have `entrypoints`. It does not scaffold `scope routes` from templates yet. Create the scope folders and `entrypoints` yourself for now, then run `open-forge index`.

When `scoped framework routes` already exist, `install` updates their framework `entrypoints` from the current framework wording. It identifies them by concrete path shape and canonical `entrypoint` filename, not by hidden version metadata. It does not create missing scope `entrypoints` yet.

Examples of `scoped framework routes` recognized by the current implementation:

```text
.agents/memory/[scope]/crystallized/_crystallized.md
.agents/memory/[scope]/crystallized/decisions/_decisions.md
.agents/memory/[scope]/crystallized/documents/_documents.md
.agents/memory/[scope]/crystallized/[scope]/documents/_documents.md
.agents/workflows/implementation/directives/_directives.md
```

`[scope]` means a concrete `slug` folder with its own `entrypoint`, not a literal folder name. These examples assume each intermediate scope folder is already visible through its own `entrypoint`.

These match `framework route` shapes such as:

```text
memory/.../crystallized/_crystallized.md
memory/.../crystallized/.../decisions/_decisions.md
memory/.../crystallized/.../documents/_documents.md
.../directives/_directives.md
```

A local `scope route` such as `.agents/patterns/[scope]/react/_react.md` is not updated as a `scoped framework route` unless it matches a known `framework route` shape.

Manual edits to framework files are visible in git diffs after install. Prefer sibling files, child routes, or `.overwrite.md` companions for durable local customization.

## extend

```sh
open-forge extend {extension-source}
open-forge extend {bundled-extension-id}
open-forge extend {extension-source} {target-folder}
open-forge extend {bundled-extension-id} {target-folder}
open-forge extend --list
```

`extend` installs an extension overlay into the target and rebuilds generated index regions.

The extension can come from:

- a local folder shaped like the files it should add to the workspace
- a bundled first-party Open Forge extension shipped with the CLI package

Local overlay example:

```text
my-extension/
  .agents/
    patterns/
      react/
        _react.md
        components.md
```

Running `open-forge extend my-extension {target-folder}` copies those files into `{target-folder}`. Markdown files preserve matching marked local blocks when the target file already exists. Other files are copied over directly.

Bundled first-party extensions live in the CLI package under this source shape:

```text
src/extensions/{extension-id}/
  payload/
    .agents/
      ...
```

Use `open-forge extend --list` to show bundled extensions available in the installed CLI package. Use `open-forge extend {extension-id}` to install one.

This is an MVP dogfooding command. It does not provide an external registry, manifest contract, wizard, preview, uninstall, or dependency model yet. Build or select the overlay intentionally, run `extend`, then inspect the git diff.

## index

```sh
open-forge index
open-forge index {target-folder}
```

`index` rebuilds the loader category registry and the generated regions inside category `entrypoints`.

When `.agents/` exists, the CLI scans `.agents/`. Otherwise it scans the target folder.

When `loader.md` exists at the scan root, the CLI generates one loader `entry` for every direct child folder that contains one recognized category `entrypoint`. Loader descriptions and tags come from the category `entrypoint`, preferring supported metadata and falling back to its first body description and #Index.

In an installed workspace, loader paths are concrete and relative to the target folder, such as `.agents/workspace/_workspace.md`. The target folder is the logical workspace root even when `.agents/` is a symlink or its contents come from a submodule. The CLI does not derive routing roots from Git boundaries.

Nested categories stay behind their parent category `entrypoint`. Folders without a matching `entrypoint` do not become loader routes.

Open Forge-authored category `entrypoints` are named `_{folder-name}.md`:

```text
.agents/patterns/
  _patterns.md
  local-docs.md
```

The category `entrypoint` contains stable category meaning followed by a generated region. The generated region reads direct markdown route files and direct child category `entrypoints`:

```md
- `{file}` - {description} - #{Tag1} #{Tag2} ... #{TagN}
- `{folder/_folder.md}` - {description} - #Index
```

Child folders are routed through their own `_{folder-name}.md` category `entrypoint`. Parent `entrypoints` stay at one folder boundary.

`scope routes` use the same rule. A `scope route` is a concrete `slug` folder with its own `entrypoint`. Every folder in the visible route chain needs its own `entrypoint`:

```text
.agents/memory/
  _memory.md
  [scope]/
    _[scope].md
    crystallized/
      _crystallized.md
      decisions/
        _decisions.md
      documents/
        _documents.md

.agents/guidance/
  _guidance.md
  [scope]/
    _[scope].md
    cross-platform-apps.md
```

Open Forge does not require a folder named `projects`, `scope`, `domain`, or `team`. Use those only when they improve your local routing.

Scope placement changes meaning:

```text
.agents/memory/crystallized/[scope]/decisions/
.agents/memory/[scope]/crystallized/decisions/
```

The first means `[scope]` is inside crystallized memory. The second means `[scope]` owns its own memory states. Both are valid when every folder has an `entrypoint` and the `entrypoint` descriptions make the scope clear.

For cross-tool compatibility, the CLI also recognizes these `entrypoint` names:

- `_index.md`
- `index.md`
- `_references.md`
- `references.md`

Open Forge itself uses only `_{folder-name}.md`. A folder must contain exactly one recognized `entrypoint` name. If multiple candidates exist, the CLI stops before changing any generated region.

The generated region is bounded explicitly:

```md
## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
```

The CLI replaces only the content between the markers. It preserves frontmatter and category content above the region.

When a legacy category `entrypoint` or loader has a final `## Entries` section containing only generated list `entries`, the CLI adds the markers automatically. If the heading is absent, the CLI appends the complete section. Malformed or non-final markers stop generation without changing the file.

Change index output by adding, editing, moving, or removing route files and child category `entrypoints` in the indexed folder.

The index generator reads:

- direct `*.md` route files, including underscore-prefixed routed files
- direct child category `entrypoints` named `_{folder-name}.md`

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
