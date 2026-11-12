---
open-forge:
  description: Configure the Framework and manage Extensions and Libraries
  tags: [Skill, CLI, Reference]
---

# Framework And Packages

Read this reference for an authorized installation, configuration, update, removal, Extension, or Library task. Loading the Skill does not authorize these changes. Preview each mutation and review its plan before applying. All commands accept the [shared global flags](common.md#global-flags).

## `install`

Establish or configure the Framework bundled in the running CLI. Install does not fetch a newer payload or reconcile ordinary managed divergence. Use Update for that.

| Flag                                                | Meaning                                                                                                                                             |
| --------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| `--configure`                                       | Revisit an installed setup, restore eligible missing selected defaults, or convert unedited owned files to the selected frontmatter form            |
| `--main-root <folder>`                               | Choose the context folder at first installation. Default: `--main-root .agents`. A custom folder can already hold files                              |
| `--preset <minimal\|essentials\|full-core\|custom>` | Select built-in routes. On an installed workspace, an explicit preset also requires `--configure`                                                   |
| `--frontmatter <root\|scoped>`                      | Choose where Open Forge writes file metadata: root or scoped. A fresh unattended Install uses root. Change an installed workspace with --configure. |
| `--route <id>=<add\|remove\|git-ignore>`            | Override a Custom or Minimal row. Requires `--preset custom` or `--preset minimal`. Repeat for more rows                                            |
| `--force`                                           | Permit eligible existing content to be replaced during initial management establishment. Does not update or adopt managed state                     |
| `--automatic`                                       | Suppress confirmation without adding force or choosing missing setup inputs                                                                         |
| `--dry-run`                                         | Preview selection, adoption, files, settings, and ignore changes                                                                                    |

First interactive Install offers the recommended Minimal, then Essentials, Full Core and Custom. Each built-in preset includes the one before it. Configure offers the current choices. Ordinary repeated Install retains its quiet no-op or divergence behavior.

Before route choices, first interactive Install asks `Where should Open Forge keep its context files?` unless `--main-root` supplied the folder. An unattended first installation defaults to `--main-root .agents`. The loader, human guide and ordinary categories use the selected folder. Native Skills stay in `.agents/skills/`, settings in `.agents/open-forge.json`, and ownership in `.agents/open-forge.lock.json`. The workspace-root host files point to the selected loader.

An omitted or unchanged `--main-root` keeps ordinary Install behavior. A different root on an established installation is refused before changes, including with `--configure`. Relocation is not available yet. Editing `mainRoot` in settings moves no files. Older CLIs do not support custom roots.

Later folder Include registers outside routes separately. It does not change the main root or create package ownership. Extension and Library destinations there still use explicit grants and existing source/ownership protections. Older CLIs do not maintain directory registrations, and unknown-property preservation is not downgrade compatibility.

After the preset choice, first interactive Install asks `How should Open Forge write file metadata?` with root preselected. An explicit `--frontmatter` skips the question. A fresh Install keeps an explicit preference already in settings unless the flag overrides it. Without either, fresh unattended Install uses root. Interactive Configure offers the current form first. Ordinary repeated Install and Update retain the form and never ask.

Install, Update, and Extension delivery use `frontmatter` in `.agents/open-forge.json`. A missing value means scoped. Both root and scoped frontmatter are read in every workspace. The plan shows the selected form, such as `Frontmatter: root`, or a change such as `Frontmatter: scoped -> root`.

When Install includes Memory archive and `ignoreLinksFrom` is absent from `.agents/open-forge.json`, it adds `[".agents/memory/archived"]`. An existing key, even `[]`, stays unchanged. Update leaves authored settings unchanged. Existing workspaces can add the archive entry by hand. New settings use schema version 5 for every layout. Unrelated rewrites preserve an existing schema declaration. Explicitly selecting a custom root can update it to the current schema.

A beta14 CLI ignores both ignore lists. Doctor reports an unrecognized settings version as information and continues with the keys it understands. This does not make an older CLI compatible with custom roots.

Root keys are recommended. Choose the scoped form when other tools already use top-level frontmatter keys. The interactive plan starts with `Review changes`. Plan and result recap the setup, added routes, Git-ignored contents, left-out routes, and frontmatter form. Configure also reports changed row choices and retention of existing files.

Install's headline describes the result, with the path on a separate `Workspace:` line. Explanations wrap at spaces to fit the terminal width.

| Preset       | Selected content                                                                                                                  |
| ------------ | --------------------------------------------------------------------------------------------------------------------------------- |
| `minimal`    | Directives and Crystallized Memory, with the Memory catalogue, plus the loader, `.agents/README.md` and the managed host sections |
| `essentials` | Everything in Minimal, plus Patterns, Skills, Emerging Memory and the ordinary Working route with Install Git-ignore rules        |
| `full-core`  | Every built-in category and Memory state, with no Install-owned ignore entries                                                    |
| `custom`     | Current concrete choices, or Essentials on a fresh workspace, with explicit row overrides                                         |

No built-in preset installs optional Extensions. Essentials omits Guidance, Maps, Templates, and Archived Memory. Minimal includes only Directives and Crystallized Memory. A Memory state added with `--route` brings the Memory catalogue with it.

For primary roots and file-only inclusions, Install, Update, Configure, and Extension commands keep the loader's `Entries` as the user left them. A removed root line stays removed, links to documents outside `.agents/` stay, and an empty list stays empty. A root joins the list only when the command creates its catalogue or Configure adds a route that was left out.

Ordinary Install, configure, frontmatter conversion and root Update preserve outside registrations, independent file includes, exact unlisted state and authored row bytes, conditions, aliases, mutual order and first-reaching policy. Stale targets remain authored, and these operations do not relist or restore outside anchors. Generated primary rows keep their own ordering, so absolute mixed-row positions can change. Registration still grants no package ownership or permission.

In line-mode Custom, type row edits such as `2+ 9~`, press Enter on an empty line to continue, or follow `Type cancel to stop without changes.` Existing tracked files stay tracked, and left-out defaults leave existing files routable.

Custom row IDs are `directives`, `guidance`, `maps`, `patterns`, `skills`, `templates`, `memory/working`, `memory/emerging`, `memory/crystallized`, and `memory/archived`. Identical repeated presets or row actions are idempotent. Conflicting repeats, unknown IDs, presets, or actions are invalid. Interactive Custom shows one list with a mark per row: `+` add, `~` git-ignore and `-` remove. Rows supplied by `--route` are locked.

Custom Remove omits supplied defaults and releases only their Framework management. Existing files, authored notes, and overwrite companions stay routable. Add and Remove clear only that row's owned ignore rules, preserving user rules. Malformed owned sections block changes rather than being replaced.

Git-ignore first adds or restores the ordinary route and records its policy in `.agents/open-forge.lock.json`. Install then adds anchored rules for the route's contents and whitelists its actual entrypoint in the owned `.gitignore` section. For example, `.agents/memory/working/_working.md` stays shareable while the other contents of Working are ignored. `.agents/open-forge.json` settings and the lock file stay trackable under these rules.

Index uses the lock's recorded route policy to omit private child files and nested private entrypoints from shareable `Entries`. It still maintains the whitelisted entrypoint and its parent route link. Context, Find, and References can read local private files when selected. Git-ignore does not define loading authority. The CLI neither stages files nor untracks files already committed to Git.

Dry-run, automatic, JSON, and redirected requests never ask setup questions. Noninteractive Configure requires an explicit `--preset` or `--frontmatter`. On an installed workspace, `--frontmatter` requires `--configure`. Form-only `install --configure --frontmatter <form>` keeps route choices unchanged. Ordinary unattended first Install uses Full Core and retains existing omissions. Name the preset in both a fresh installation preview and its apply request. An omitted preset uses Full Core in unattended mode, while the first interactive choice is Minimal.

Default context folder example:

```sh
open-forge install --preset essentials --dry-run
# Review the plan before applying it.
open-forge install --preset essentials --automatic
```

Custom context folder example:

```sh
open-forge install --preset essentials --main-root docs/context --dry-run
# Review the plan before applying it.
open-forge install --preset essentials --main-root docs/context --automatic
```

Use `./...` for exact workspace-relative source operands. Short IDs such as `memory/crystallized` follow the selected layout.

Use `open-forge install --configure` as the interactive way to change several setup choices. For a repeatable configuration change, preview and apply the reviewed request:

```sh
open-forge install --configure --preset custom --route guidance=add --dry-run
open-forge install --configure --preset custom --route guidance=add --automatic
```

To restore one left-out built-in category, preview `open-forge route init <category> --framework --dry-run`, review the plan, then apply the same request without `--dry-run`.

Explicit configuration restores eligible missing packaged defaults, including scaffolding after a checkout with or without a lock file. It preserves authored files, overwrite companions, narrower omissions, and unrelated selections. Private notes need a separate copy or backup.

Configure converts unedited owned delivered files to the selected form in the same reviewed plan as the settings change. Edited files and Extension files whose source is unavailable are kept and reported, while excluded, user-authored, Library, and overwrite files stay unchanged.

Install retry commands retain the request's workspace, resolved preset and frontmatter choices, Custom rows, force, and output options. Wizard answers are included even when no flag supplied them. They add `--automatic` only when confirmation was missing. A first successful install directs the user to review and commit the new files, then give their agent a normal task. If the agent tool does not read `AGENTS.md` on its own, have it read that file first.

## `update`

Align managed Framework files with the payload bundled in the CLI. Upgrade the executable first when a newer payload is wanted. Ordinary Update replaces edited or restores missing eligible managed files. Recovery bundles retain prior bytes.

Adoption counts cover the same selected files in preview and result, including metadata and navigation changes. Use the reported recovery bundle for previous content. `git diff` reviews tracked changes and cannot recover unsaved or untracked bytes. `--detail standard` shows the bundle's exact path.

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

Selection counts include the named package. For example, `2 packages, including planning.` counts Planning and Workflow Support. Standard detail lists `Packages in this selection`.

### `extension create [<stable-id>]`

Create a package scaffold under an existing local catalogue. Stable ID and destination are required to plan creation. Interactive text can ask for missing inputs.

The parent prompt asks for an existing folder and explains that the new stable-ID folder will be created inside it. The CLI never creates the parent.

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

If creation stops partway through, created files stay in the package folder. Review that folder before retrying. Create does not make a recovery bundle or install the package in a workspace.

### `extension install [<stable-id>...]`

Add reviewed optional packages and their required dependencies. Select explicit IDs, `--all`, the sole package in a one-package source, or an interactive choice.

| Flag                                   | Meaning                                                                           |
| -------------------------------------- | --------------------------------------------------------------------------------- |
| `--source <package-or-catalogue-path>` | Select the reviewed separate local source                                         |
| `--all`                                | Select every eligible package in that source                                      |
| `--force`                              | Permit eligible existing content replacement during initial installation          |
| `--allow-path <path>`                  | Record supported permission for a destination outside the primary content locations. Repeat as needed |
| `--dry-run`                            | Preview packages, dependencies, and file changes                                  |
| `--automatic`                          | Suppress prompts without selecting packages or adding force                       |

A fully excluded package is recorded as installed without copying package files. Its result names the skipped files and excluded category and suggests a Framework Route Init preview. Cancellation before application changes no files and reports no Entries updates.

Plans and receipts also name partially excluded packages and explain which files are skipped. For example, Planning's starters are skipped while Templates is left out, but its other enabled files install. The guidance previews restoring the category, then updating each affected package. Review and apply the restoration before previewing the update. Those commands keep the selected workspace and any local package source.

Use Extension Update for already managed content. Planning supplies the Decision category and Template. Essentials leaves Templates out. If those Templates are wanted, restore that category before installing Planning or `development-toolkit`. Preview, review, and apply each change:

```sh
open-forge route init templates --framework --dry-run
# Review the plan before applying it.
open-forge route init templates --framework
```

Then discover and inspect a package before previewing installation:

```sh
open-forge extension list --available
open-forge extension inspect development-toolkit
open-forge extension install development-toolkit --dry-run
# Review the plan before applying it.
open-forge extension install development-toolkit --automatic
open-forge find --tag Template
```

Continue when the results list `templates/planning/decision`. If Planning is already installed without its starters, restore Templates using the preview and apply commands above. Then preview its update, review the plan, and apply:

```sh
open-forge extension update planning --dry-run
open-forge extension update planning --automatic
```

Use the affected package's ID and the same `--source` when updating from a local catalogue.

### `extension update [<stable-id>...]`

Align selected managed packages and their dependencies with one reviewed source. Select exact managed IDs or `--all`, or choose interactively.

Accepts `--source`, `--all`, repeated `--allow-path`, `--dry-run`, and `--automatic` with the meanings above. `--all` selects managed packages represented by the source. `--prune` permits eligible retired file deletion. `--force` is accepted for compatibility and adds no authority. Ordinary Update replaces edited or restores missing eligible managed content and retains prior bytes in recovery.

### `extension remove [<stable-id>...]`

Release selected package management and delete eligible files when no retained package owns them. Select exact managed IDs or choose interactively. A dependency cannot be removed while a retained package needs it.

Accepts repeated `--allow-path`, `--dry-run`, and `--automatic`. Preview deletions and retained shared paths. Edited eligible files can be removed when their last manager is removed, with prior bytes retained in recovery. Automatic mode does not select packages or permit additional deletion.

Removal previews name every selected package, including packages with no installed files. Required-package counts use the singular `1 package` when appropriate.

The preview describes dependencies that would remain installed. Its `Next:` command applies the selected Extension removal with the same workspace and options, without `--dry-run`. Dependency-removal commands appear after that removal is applied. Retained shared-file rows name the packages that still own the file.

Removing `development-toolkit` leaves its dependency packages installed. Apply the selected removal before previewing removal of its dependencies. The applied result suggests dependency-removal commands. Preview each package you also want to remove. A dependency cannot be removed while a retained package still needs it. When no retained package requires Development, preview `open-forge extension remove development --dry-run`.

Unattended Extension operations require explicit selection or a supported `--all`, plus `--automatic` when applying a plan that needs confirmation.

## Libraries

A Library registers a shared folder and projects its files through relative file links. Source and destination remain inside the workspace. Library commands use exact management IDs, not source IDs or Extension IDs.

On-disk targets use the platform separator, while recorded paths stay portable. On Windows, Inspect, List, Doctor, and Sync report unreadable legacy links whose targets use forward slashes. Follow the reported detach command, remove only that ID from `removedLibraries` in `.agents/open-forge.json`, then attach again with the same source, projection root, and Git-ignore choice. Preview and review each operation first, supply required permissions, and resolve any reported recovery boundary. Source files stay intact. Sync and Repair do not convert legacy targets.

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
| `--allow-path <path>`        | Record supported destination permission outside the primary content locations. Repeat for required destinations and `.gitignore` |
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

## Workspace Runtime And Recovery

Install, Update, Remove and Library mutations use the workspace lock and prepare new bundles in `.open-forge-state/`. Package destinations cannot enter that reserved folder, including through links. Extension Create keeps its separate create-only destination and takes no workspace lock.

Extension Install, Update and Remove warn about drafts, damaged or invalid history and unavailable coverage. Verified bundles from earlier runs produce no warning. Current ownership, permission, target and preparation checks still apply.
