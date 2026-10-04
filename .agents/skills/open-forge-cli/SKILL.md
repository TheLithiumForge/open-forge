---
name: open-forge-cli
description: Use the open-forge CLI to load workspace context, find sources, maintain routes and links, and preview installation changes. Use when the command is available and the task needs Open Forge context, navigation, diagnosis, or workspace maintenance.
---

# Open Forge CLI

Open Forge is Markdown, rules, and links. The optional CLI makes reading and maintaining those files cheaper. The Skills entrypoint requires this usage guide at startup; loading it does not install the CLI or authorize an installation change.

## Start With Context

Check `open-forge --help` once to see whether the CLI is available. If it is unavailable, follow the loader and read the files directly. Use `open-forge <command> --help` for the installed version's exact interface when it differs from this guide.

Prefer `context` for initial loading and selected route chains instead of issuing a separate read for each entrypoint. With no operands it returns the loading-tag closure; the default content is `frontmatter,body`, so it includes instructions, not just a file list. Obey explicit read instructions in those files too. To include this Skill in the same initial batch:

```sh
open-forge context skills/open-forge-cli
```

When you know the working files, supply the complete set, including planned files:

```sh
open-forge context skills/open-forge-cli --for src/Order.cs --for web/order.ts
```

Select relevant scopes from the returned descriptions. Batch several selected sources in one request. After startup is already loaded, omit repeated startup content:

```sh
open-forge context directives/backend memory/crystallized/documents --additions-only --for src/Order.cs --for web/order.ts
```

These source names are examples; select IDs that exist in the workspace. `--additions-only` needs an explicit source and removes only the startup closure, not every file read earlier. Avoid `--follow-links=all` for ordinary startup: it expands referenced content whether or not the task needs it. Use a bounded depth for a selected source when its linked explanations matter.

A path filter never selects a hidden scope or grants edit permission. A source applies when at least one supplied path matches its complete inherited condition chain. Reading a Markdown file for context does not make it a working path. While paths are unknown, exposed conditioned `#LoadNow` and `#KeepInMind` entries stay pending, and `context` returns `incomplete` (exit 3); other conditioned entries remain on demand. Resolve pending paths before relying on those rules.

## Global Options

Every command accepts these flags:

| Flag                                        | Best use                                                                                                    |
| ------------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| `--workspace <path>`                        | Choose the workspace explicitly when the current directory is elsewhere.                                    |
| `--format text\|json`                       | Use text for reading and JSON for a script; still check status and exit code.                               |
| `--detail minimal\|standard\|full\|debug`   | Minimal for a quick result, standard for reasons, full for evidence, debug for diagnosing a failure.        |
| `--detail-filter error\|warning\|info\|all` | Filter diagnostic severity; repeat when needed. It does not change the operation's status.                  |
| `--help`                                    | Check exact operands, defaults, accepted values, and version-specific options before an unfamiliar command. |
| `--version`                                 | Identify the executable when output or options differ from expectations.                                    |

Name a source by ID, such as `memory/crystallized/documents`, or exact path, such as `.agents/memory/crystallized/documents/_documents.md`. Use the exact path if an ID is ambiguous. Source IDs, Extension IDs, and Library IDs identify different things.

## Read And Find

These commands write nothing. All command-specific flags are listed below; the global options also apply.

- `status`: get a quick workspace health, context, package, and recovery summary.

  Global options only.

- `context [<source>...]`: batch startup or selected context reads.

  `--for <path>` (repeat): all working files. `--additions-only`: avoid repeating startup. `--content <parts>`: request only useful content. `--follow-links <positive-depth|all>`: expand relevant linked explanations.

- `route list [<source>]`: orient yourself before selecting a scope.

  `--depth=<nonnegative-number|all>`: limit tree depth; use the equals form.

- `route inspect <source>`: explain a route, its loading, and file conditions.

  `--for <path>` (repeat): explain applicability for working files. `--matching-files`: enumerate existing matches; it does not supply working paths or activate a scope. An unrestricted source reports all files without scanning.

- `find`: locate candidates by tags or structural headings before opening them.

  `--include <source>`, `--exclude <source>` (repeat): bound the search; exclusion wins. `--tag <tag>`, `--heading <heading>` (repeat): predicates. `--require all|any`: combine predicates (default all). `--within <parts>`: where predicates match. `--for <path>` (repeat): additional applicability filter. `--content <parts>`: return useful parts of the matches.

- `references <source>`: check impact before moving or deleting a source.

  `--direction in|out|both`: incoming impact, outgoing dependencies, or both. `--include <source>`, `--exclude <source>` (repeat): bound incoming scans; invalid with out alone.

- `doctor`: diagnose broken references, stale navigation, and lifecycle problems.

  Global options only. Run after routed changes and before closeout; resolve or report findings within the task.

For `context --content`, parts are `metadata`, `paths`, `frontmatter`, `headings`, `body`, or `section:<heading>`. For `find --content`, omit `paths`. For `find --within`, use `document`, `frontmatter`, `body`, or `section:<heading>`. Each option takes one comma-separated list. Headings and metadata help triage candidates; read the body before relying on a rule.

Use `find --tag Directive --for src/Order.cs` to discover applicable rules even below unselected scopes, then `context <selected-source> --for src/Order.cs` to load the selected chain. `context --for` alone only loads what the existing selection rules expose. `--require` combines tag and heading predicates; `--for` is a separate filter.

Before creating a record, find the existing category and related records, for example `find --tag Decision`. Reuse useful sources and link to them instead of duplicating their contents.

## Change Routes And References

Preview every mutation with `--dry-run` and review the plan before applying it.

- `route init <target>`: create a missing scope or route chain, or add an omitted Core capability.

  `--framework`: use the standard scaffold. For an unscoped canonical Core root or Memory state, restore its packaged contents and necessary missing ancestors. `--description <text>`, `--responsibility <text>`, `--tag <tag>` (repeat), `--apply-to <glob>` (repeat): describe a missing generic final entrypoint. `--dry-run`: preview.

- `route create <target>`: add one routed file.

  `--description <text>`, `--responsibility <text>`, `--tag <tag>` (repeat): meaningful metadata. `--template <source>`: start from a selected Template. `--apply-to <glob>` (repeat): condition it on working files. `--dry-run`: preview.

- `route update <source>`: patch metadata or complete an eligible empty body.

  `--description <text>`, `--responsibility <text>`, `--tag <tag>` (repeat): replace those fields. `--template <source>`: complete a frontmatter-only body. `--apply-to <glob>` (repeat): replace a condition. `--clear-apply-to`: remove it. `--dry-run`: preview.

- `route move <source> <target>`: relocate a source and update references.

  `--dry-run`: inspect both the move and link changes.

- `route remove <source>`: remove a routed file or category and record its removal.

  `--dry-run`: check dependents and removals. `--automatic`: apply a reviewed selection without prompting.

- `index [<source>...]`: rebuild generated `Entries` after manual additions, renames, or metadata edits.

  `--dry-run`: inspect navigation drift; source operands bound the rebuild.

- `repair`: apply unambiguous local-link repairs or accepted Library recovery.

  `--dry-run`: preview. `--automatic`: select current safe-exact repairs. `--relink <source-location> <expected-destination> <target-path>`: select an explicit occurrence and destination.

`route init` metadata options require a missing generic final target; they cannot update an existing one. Do not combine `--apply-to` with `--framework`. Templates contribute body content only; their frontmatter does not transfer. On update, an existing authored body stays intact with a warning. Repeated update tags replace the complete ordered tag list; `--responsibility ""` removes the responsibility.

To add an omitted category, preview `route init patterns --framework --dry-run`, then apply the same request without `--dry-run`. Use `skills` for the complete bundled Skills payload or `memory/archived` for that state. Restoration clears only the exact selection's exclusion and preserves unrelated omissions and authored occupants. A broader excluded ancestor requires an explicit add first. Repair an orphan overwrite that blocks safe navigation before retrying.

`--apply-to` values may be comma-separated expressions. Canonical declarations use a quoted-string list under `open-forge:`. Existing `applyTo` may sit at the frontmatter root or under `open-forge:`, as one string or a list. Both locations must declare equivalent pattern sets if present. A list entry stays one atomic pattern, while string expressions split at top-level commas; `[,]` represents a literal comma. Update preserves the authored location. `--clear-apply-to` cannot be combined with `--apply-to`.

## Framework, Extensions, And Libraries

Run installation changes only when the user asks for them. Loading this Skill is not permission to install, update, remove, clean up, or attach anything.

- `install`: install or configure built-in Framework routes.

  `--configure`: revisit setup or restore eligible missing defaults. `--preset essentials|full-core|custom`: select the built-in routes explicitly. `--route <id>=<add|remove|git-ignore>` (repeat): override Custom rows. `--dry-run`: check selection, adoption, and proposed files. `--automatic`: apply a reviewed plan unattended. `--force`: permit eligible existing files to be replaced during initial management establishment.

- `update`: align managed Framework files with the version bundled in the CLI.

  `--dry-run`, `--automatic`; `--prune`: allow eligible retired files to be deleted. `--force`: accepted but adds no authority. Upgrade the CLI first when a newer payload is wanted.

- `remove <target>`: remove a file, folder, route, Extension, or Library.

  `--kind <kind>`: disambiguate path, route, extension, or library. `--allow-path <path>` (repeat): grant supported external-path permission. `--dry-run`, `--automatic`: review, then apply.

- `cleanup`: delete retained recovery copies after the user has checked the changes.

  `--dry-run`: review the recovery data that will be deleted.

- `extension list`: see packages before choosing one.

  `--installed`, `--available`: select which inventory to show. `--source <path>`: inspect an exact local package or catalogue.

- `extension inspect <id>`: check package files and dependencies.

  `--source <path>`: inspect the intended local source.

- `extension install [<id>...]`: add selected optional packages.

  `--source <path>`: choose a local source. `--all`: select every eligible package there. `--force`: permit eligible replacement. `--allow-path <path>` (repeat): approved destinations outside `.agents`. `--dry-run`, `--automatic`: review, then apply.

- `extension update [<id>...]`: align selected managed packages.

  `--source <path>`, `--all`, `--allow-path <path>` (repeat), `--dry-run`, `--automatic`; `--prune`: allow eligible retired files to be deleted. `--force`: accepted but adds no authority.

- `extension remove [<id>...]`: remove packages, retaining files still owned by other packages.

  `--allow-path <path>` (repeat), `--dry-run`, `--automatic`.

- `extension create [<id>]`: create a package scaffold.

  `--path <catalogue-path>`: required destination. `--name <text>`, `--description <text>`, `--package-version <text>`: package metadata. `--dependency <id>` (repeat): required packages. `--dry-run`, `--automatic`. The ID is also required in unattended runs.

- `library list`: see registered shared sources.

  Global options only.

- `library inspect <id>`: check a Library's source, projections, and current links.

  Global options only.

- `library attach <id> <source-root>`: project a shared folder through relative file links.

  `--to <directory>`: workspace-relative projection root (default workspace root). `--git-ignore <true|false>`: ignore exact projected links when the source supplies the files; false leaves ignore rules unchanged. `--allow-path <path>` (repeat), `--dry-run`, `--automatic`.

- `library sync <id>`: refresh projections from the source inventory.

  `--allow-path <path>` (repeat), `--dry-run`, `--automatic`.

- `library detach <id>`: remove registration and projected links, preserving the source.

  `--allow-path <path>` (repeat), `--dry-run`, `--automatic`.

Extension install and update treat their selected source as read-only and require it to be separate from the target workspace. Dependencies resolve within that local source. Without IDs, interactive Extension install, update, and remove offer selection; unattended runs need explicit IDs or a supported `--all`, as well as `--automatic`.

Interactive Library Attach asks whether to Git-ignore its projected links, defaulting to No. Explicit `--git-ignore true|false` skips that question. Automatic, JSON, redirected and dry-run omission means false. For unattended opt-in, include `--allow-path .gitignore` when no saved grant covers it, alongside any destination grants; preview the same request first. For example, `library attach team-rules vendor/team-rules --to docs/team --git-ignore true --allow-path docs/team --allow-path .gitignore --dry-run`.

Library ignore rules cover individual links in a separate owned section, including root destinations and shared folders. Authored and Install rules stay intact. Sync remembers the choice; Sync, Detach and whole-Library Remove require the ignore-file grant when changing that file. Individual-link Remove cleans up its owned rule within the ordinary removal plan. Other Libraries' entries survive. An explicitly removed ignore file stays removed. Ignore does not untrack committed files or change context loading.

First interactive Install offers Essentials, Full Core, or Custom. Use `install --configure` to revisit an installed setup. An explicit preset on an installed workspace also requires `--configure`. Ordinary repeated Install retains its quiet no-op or divergence behavior.

Essentials selects Directives, Patterns, Skills, Emerging and Crystallized Memory, plus the full ordinary Working route with its whole directory Git-ignored. It omits Guidance, Maps, Templates, and Archived Memory. Full Core selects every built-in category and Memory state with no Install-owned ignore entries. Neither preset installs optional Extensions. Custom starts from the current concrete choices, or Essentials when fresh.

Custom row IDs are `directives`, `guidance`, `maps`, `patterns`, `skills`, `templates`, `memory/working`, `memory/emerging`, `memory/crystallized`, and `memory/archived`. `--route` requires `--preset custom`. Identical repeated presets or actions for a row are idempotent. Conflicting repeats and unknown presets, IDs, or actions are invalid. Interactive Custom keeps supplied overrides and reviews the remaining rows.

Custom's `remove` omits supplied defaults and releases only their Framework management. Existing files, authored notes, and overwrite companions stay in place and routable. `git-ignore` adds the ordinary route and anchors its whole directory in an Install-owned `.gitignore` section. Ignored content stays indexed and readable. Already tracked files stay tracked. Add and Remove clear only that row's owned ignore pattern, preserving user rules.

Dry-run, automatic, JSON, and redirected requests never ask setup questions. Ordinary unattended first Install keeps Full Core and existing omissions. Noninteractive Configure requires an explicit preset. Preview the current choices plus a narrow override, then apply the reviewed request:

```sh
open-forge install --configure --preset custom --route guidance=add --dry-run
open-forge install --configure --preset custom --route guidance=add --automatic
```

Explicit configuration restores eligible missing packaged defaults, including ignored scaffolding after a checkout with or without a lock file. It preserves authored files, overwrite companions, narrower omissions, and unrelated selections. Missing private notes need a separate copy or backup. Configuration adds no Update, authored replacement, or deletion authority. Use `update` for managed divergence outside the additive configuration boundary.

Updates replace edited managed files and retain previous bytes in recovery bundles; removals retain removed bytes too. `--force` on update does not grant additional authority. `--allow-path` grants the CLI a write boundary, not human authorization for the action. Review the actual destination and use it only within an authorized task.

## Read The Result

Check status and exit code after each command. `completed-with-warnings` (exit 2) is usable but needs review. `incomplete` means required facts are missing; `blocked` means the CLI stopped at a boundary. Do not force the same effect through manual edits. Resolve an in-scope suggested next step and retry, or report the limitation.

Commands that need confirmation write nothing in a noninteractive shell and return `invalid-input`. After reviewing the dry run, use `--automatic` only for commands that support it and changes already within the task. More output detail or a diagnostic filter never turns an unsuccessful result into success.
