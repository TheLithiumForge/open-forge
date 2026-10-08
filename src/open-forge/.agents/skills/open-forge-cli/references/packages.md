---
open-forge:
  description: Configure the Framework and manage Extensions and Libraries
  tags: [Skill, CLI, Reference]
---

# Framework And Packages

Read this reference for an authorized installation, configuration, update, removal, Extension, or Library task. Loading the Skill does not authorize these changes. Preview each mutation and review its plan before applying. All commands accept the [shared global flags](common.md#global-flags).

## `install`

Establish or configure the Framework bundled in the running CLI. Install does not fetch a newer payload or reconcile ordinary managed divergence. Use Update for that.

| Flag                                       | Meaning                                                                                                                         |
| ------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------- |
| `--configure`                              | Revisit an installed setup, restore eligible missing selected defaults, or convert unedited owned files to the selected frontmatter form |
| `--preset <essentials\|full-core\|custom>` | Select built-in routes. On an installed workspace, an explicit preset also requires `--configure`                               |
| `--frontmatter <root\|scoped>`              | Choose where Open Forge writes file metadata: root or scoped. A fresh unattended Install uses root. Change an installed workspace with --configure. |
| `--route <id>=<add\|remove\|git-ignore>`   | Override a Custom row. Requires `--preset custom`. Repeat for more rows                                                         |
| `--force`                                  | Permit eligible existing content to be replaced during initial management establishment. Does not update or adopt managed state |
| `--automatic`                              | Suppress confirmation without adding force or choosing missing setup inputs                                                     |
| `--dry-run`                                | Preview selection, adoption, files, settings, and ignore changes                                                                |

First interactive Install offers Essentials, Full Core, or Custom. Configure offers the current choices. Ordinary repeated Install retains its quiet no-op or divergence behavior.

After the preset choice, first interactive Install asks `How should Open Forge write file metadata?` with root preselected. An explicit `--frontmatter` skips the question. A fresh Install keeps an explicit preference already in settings unless the flag overrides it. Without either, fresh unattended Install uses root. Interactive Configure offers the current form first. Ordinary repeated Install and Update retain the form and never ask.

Install, Update, and Extension delivery use `frontmatter` in `.agents/open-forge.json`. A missing value means scoped. Both root and scoped frontmatter are read in every workspace. The plan shows the selected form, such as `Frontmatter: root`, or a change such as `Frontmatter: scoped -> root`.

| Preset       | Selected content                                                                                                              |
| ------------ | ----------------------------------------------------------------------------------------------------------------------------- |
| `essentials` | Directives, Patterns, Skills, Emerging and Crystallized Memory, plus the ordinary Working route with Install Git-ignore rules |
| `full-core`  | Every built-in category and Memory state, with no Install-owned ignore entries                                                |
| `custom`     | Current concrete choices, or Essentials on a fresh workspace, with explicit row overrides                                     |

Neither built-in preset installs optional Extensions. Essentials omits Guidance, Maps, Templates, and Archived Memory.

Custom row IDs are `directives`, `guidance`, `maps`, `patterns`, `skills`, `templates`, `memory/working`, `memory/emerging`, `memory/crystallized`, and `memory/archived`. Identical repeated presets or row actions are idempotent. Conflicting repeats, unknown IDs, presets, or actions are invalid. Interactive Custom retains supplied overrides and reviews the other rows.

Custom Remove omits supplied defaults and releases only their Framework management. Existing files, authored notes, and overwrite companions stay routable. Add and Remove clear only that row's owned ignore rules, preserving user rules. Malformed owned sections block changes rather than being replaced.

Git-ignore first adds or restores the ordinary route and records its policy in `.agents/open-forge.lock.json`. Install then adds anchored rules for the route's contents and whitelists its actual entrypoint in the owned `.gitignore` section. For example, `.agents/memory/working/_working.md` stays shareable while the other contents of Working are ignored. `.agents/open-forge.json` settings and the lock file stay trackable under these rules.

Index uses the lock's recorded route policy to omit private child files and nested private entrypoints from shareable `Entries`. It still maintains the whitelisted entrypoint and its parent route link. Context, Find, and References can read local private files when selected. Git-ignore does not define loading authority. The CLI neither stages files nor untracks files already committed to Git.

Dry-run, automatic, JSON, and redirected requests never ask setup questions. Noninteractive Configure requires an explicit `--preset` or `--frontmatter`. On an installed workspace, `--frontmatter` requires `--configure`. Form-only `install --configure --frontmatter <form>` keeps route choices unchanged. Ordinary unattended first Install uses Full Core and retains existing omissions. Preview a narrow change, then apply the reviewed request:

```sh
open-forge install --configure --preset custom --route guidance=add --dry-run
open-forge install --configure --preset custom --route guidance=add --automatic
```

Explicit configuration restores eligible missing packaged defaults, including scaffolding after a checkout with or without a lock file. It preserves authored files, overwrite companions, narrower omissions, and unrelated selections. Private notes need a separate copy or backup.

Configure converts unedited owned delivered files to the selected form in the same reviewed plan as the settings change. Edited files and Extension files whose source is unavailable are kept and reported, while excluded, user-authored, Library, and overwrite files stay unchanged.

## `update`

Align managed Framework files with the payload bundled in the CLI. Upgrade the executable first when a newer payload is wanted. Ordinary Update replaces edited or restores missing eligible managed files. Recovery bundles retain prior bytes.

- `--dry-run`: preview the complete reconciliation.
- `--automatic`: suppress confirmation without adding force or prune.
- `--prune`: allow eligible retired managed files to be deleted.
- `--force`: accepted for compatibility and adds no authority or safety bypass.

## `remove <target>`

Remove one exact path or delegate to a typed route, Extension, or Library removal.

| Flag                                       | Meaning                                                                                          |
| ------------------------------------------ | ------------------------------------------------------------------------------------------------ |
| `--kind <path\|route\|extension\|library>` | Select the target kind. Default: `path`. Use the typed kind for an ID                            |
| `--dry-run`                                | Preview the exact selected removal                                                               |
| `--automatic`                              | Run without confirmation                                                                         |
| `--allow-path <path>`                      | Grant a supported additional destination permission. Repeat as needed within the authorized task |

Removal retains eligible removed bytes in recovery data. Use [Cleanup](routes.md#cleanup) only after the user has checked the changes.

## Extensions

An Extension is an optional package of complete files. Use exact lowercase stable IDs. A local package has `extension.json` beside `content/`, with payload paths below `content/`. A catalogue contains such packages. The manifest fields are `id`, `name`, `description`, `version`, and `dependencies`.

Install and Update treat the selected package or catalogue as read-only and require it to be separate from the target workspace. Without `--source`, use the embedded catalogue. There is no network, registry, cache, or fallback lookup. Dependencies resolve within the selected source and are processed first.

### `extension list`

Inspect packages before selecting one. Command flags:

- `--installed`: show managed packages in the workspace.
- `--available`: show packages in the embedded catalogue or selected local source.
- `--source <package-or-catalogue-path>`: inspect one exact separate local source.

No section flags shows Installed and Available. Either flag selects its section, and both show both. A listed available package is not necessarily installed. Unavailable ownership is reported as unknown coverage.

### `extension inspect <stable-id>`

Check ownership, package files, dependencies, paths, and current-versus-intended content before installation or update. Its only command flag is `--source <package-or-catalogue-path>`, selecting the intended separate local source. Inspection writes nothing and executes no package content.

### `extension create [<stable-id>]`

Create a package scaffold under an existing local catalogue. Stable ID and destination are required to plan creation. Interactive text can ask for missing inputs.

| Flag                       | Meaning                                                                                         |
| -------------------------- | ----------------------------------------------------------------------------------------------- |
| `--path <catalogue-path>`  | Required existing destination parent. Only its `<stable-id>/` child is inspected or created     |
| `--name <text>`            | Override the display name, which defaults to words from the ID                                  |
| `--description <text>`     | Override `Open Forge Extension package <stable-id>.`                                            |
| `--package-version <text>` | Override the descriptive version, default `0.1.0`                                               |
| `--dependency <stable-id>` | Record a distinct non-self dependency. Repeat as needed. Creation does not resolve availability |
| `--dry-run`                | Preview the exact scaffold                                                                      |
| `--automatic`              | Disable questions and confirmation. All required inputs must be explicit                        |

Metadata overrides are nonblank and supplied once. The scaffold contains `extension.json` and `content/.agents/`. Identical existing content is a no-op. Partial, divergent, additional, or colliding content blocks creation. The parent is never created. Unattended creation requires the ID and `--path`, plus `--automatic` when applying writes.

### `extension install [<stable-id>...]`

Add reviewed optional packages and their required dependencies. Select explicit IDs, `--all`, the sole package in a one-package source, or an interactive choice.

| Flag                                   | Meaning                                                                           |
| -------------------------------------- | --------------------------------------------------------------------------------- |
| `--source <package-or-catalogue-path>` | Select the reviewed separate local source                                         |
| `--all`                                | Select every eligible package in that source                                      |
| `--force`                              | Permit eligible existing content replacement during initial installation          |
| `--allow-path <path>`                  | Record supported permission for a destination outside `.agents`. Repeat as needed |
| `--dry-run`                            | Preview packages, dependencies, and file changes                                  |
| `--automatic`                          | Suppress prompts without selecting packages or adding force                       |

Use Extension Update for already managed content. For example, discover and inspect a package before previewing installation:

```sh
open-forge extension list --available
open-forge extension inspect development-toolkit
open-forge extension install development-toolkit --dry-run
```

### `extension update [<stable-id>...]`

Align selected managed packages and their dependencies with one reviewed source. Select exact managed IDs or `--all`, or choose interactively.

Accepts `--source`, `--all`, repeated `--allow-path`, `--dry-run`, and `--automatic` with the meanings above. `--all` selects managed packages represented by the source. `--prune` permits eligible retired file deletion. `--force` is accepted for compatibility and adds no authority. Ordinary Update replaces edited or restores missing eligible managed content and retains prior bytes in recovery.

### `extension remove [<stable-id>...]`

Release selected package management and delete eligible files when no retained package owns them. Select exact managed IDs or choose interactively. A dependency cannot be removed while a retained package needs it.

Accepts repeated `--allow-path`, `--dry-run`, and `--automatic`. Preview deletions and retained shared paths. Edited eligible files can be removed when their last manager is removed, with prior bytes retained in recovery. Automatic mode does not select packages or permit additional deletion.

Unattended Extension operations require explicit selection or a supported `--all`, plus `--automatic` when applying a plan that needs confirmation.

## Libraries

A Library registers a shared folder and projects its files through relative file links. Source and destination remain inside the workspace. Library commands use exact management IDs, not source IDs or Extension IDs.

### `library list`

List registered sources and link status without scanning all source files. No command-specific flags.

### `library inspect <library-id>`

Compare every source file and destination link in one registered Library. No command-specific flags. Use before synchronization or removal when detailed impact matters.

### `library attach <library-id> <source-root>`

Register a workspace-relative source directory and create relative file links.

| Flag                         | Meaning                                                                                                      |
| ---------------------------- | ------------------------------------------------------------------------------------------------------------ |
| `--to <directory>`           | Select the workspace-relative projection root. Default: workspace root                                       |
| `--git-ignore <true\|false>` | Ignore exact projected links. `false` leaves ignore rules unchanged                                          |
| `--allow-path <path>`        | Record supported destination permission outside `.agents`. Repeat for required destinations and `.gitignore` |
| `--dry-run`                  | Preview links, registration, permissions, and ignore rules                                                   |
| `--automatic`                | Disable prompts without changing the selected targets                                                        |

Interactive omission of `--git-ignore` asks No or Yes, defaulting to No. An explicit value skips that question. Automatic, JSON, redirected, and dry-run omission means false. For unattended opt-in, include `.gitignore` permission unless a saved grant already covers it:

```sh
open-forge library attach team-rules vendor/team-rules --to docs/team --git-ignore true --allow-path docs/team --allow-path .gitignore --dry-run
```

Library ignore rules cover individual projected links in a separate owned section, including links at the workspace root or in shared folders. They preserve user and Install sections. Ignore does not untrack committed files or change context loading.

### `library sync <library-id>`

Refresh projected links from the source's complete inventory. Accepts `--dry-run`, `--automatic`, and repeated `--allow-path`. Sync remembers the Git-ignore choice and requires ignore-file permission when updating its rules.

### `library detach <library-id>`

Remove registration and projected links, preserving source files. Accepts `--dry-run`, `--automatic`, and repeated `--allow-path`. Detach and whole-Library Remove require ignore-file permission when removing owned rules.

Individual-link Remove cleans up its owned ignore rule within its removal plan. Other Libraries' rules survive. An explicitly removed ignore file stays removed. Library operations never follow links to change source targets.

For result handling or permission questions, use [shared options and results](common.md). For reference repair or retained recovery data, use [route maintenance](routes.md).
