---
title: Customizing
description: Edit shipped files, keep local changes through updates with overwrite companions, and remove defaults so they stay removed.
---

# Customizing

Every installed file is yours. Add, adapt, replace, or remove the defaults as your needs change. The only question is how your change interacts with later updates.

## Three ways to change a shipped file

| You want to...                               | Do this                                   | What `open-forge update` does                                              |
| -------------------------------------------- | ----------------------------------------- | -------------------------------------------------------------------------- |
| Change the file itself                       | Edit the file directly.                   | Replaces your edited copy with the current shipped version and reports it. |
| Add a local adjustment that survives updates | Create an adjacent `{name}.overwrite.md`. | Leaves the overwrite alone.                                                |
| Stop using the file entirely                 | Remove it with `open-forge remove`.       | Keeps it removed.                                                          |

In a CLI-managed install, a direct edit to a shipped file lasts until the next update. Put a change you want to keep in an [overwrite companion](../glossary.md#overwrite-companion) instead.

An update reports every path it replaces, restores, deletes, or retains, and keeps a [recovery bundle](../glossary.md#recovery-bundle) when existing bytes need protecting. Review `open-forge update --dry-run` before applying one, and commit your edits first so `git diff` shows what changed.

## Overwrite companions

An overwrite is a user-owned file next to its base, named `{name}.overwrite.md`:

```text
.agents/memory/
  _memory.md
  _memory.overwrite.md       <- your local adjustment
```

It loads immediately after its base file and shares the base file's route, scope, and loading behavior. It isn't listed in `Entries` or selected separately.

Read it as part of the base source. Where the two answer the same question differently, the overwrite wins, but only for that corresponding content. It doesn't override unrelated files, ancestor Directives, clear user direction, or runtime safety.

Use an overwrite when the base is still right except for a small local addition, exception, or replacement. When the base no longer fits at all, edit, replace, or remove it instead. An overwrite without its base file inherits no route or loading behavior. Restore its base, move its content to its own route, or remove it.

```md title=".agents/memory/_memory.overwrite.md"
## Axioms

- Record the date and source of every Crystallized record in its first line.
```

## Removing defaults

A removed default stays removed unless you ask to restore it. With the CLI, removal is recorded so later installs and updates respect it:

```sh
open-forge remove .agents/templates --dry-run
open-forge remove planning --kind extension --dry-run
```

The CLI records [exclusions](../glossary.md#exclusion) in `.agents/open-forge.json`:

```json
{
  "schemaVersion": 2,
  "removedCategories": ["skills"],
  "removedFiles": [".agents/patterns/_patterns.md"],
  "removedDirectories": ["docs/obsolete"],
  "removedExtensions": ["planning"],
  "removedLibraries": []
}
```

Use `open-forge remove <path> --dry-run`, then apply the reviewed removal so later updates respect it. Deleting a file by hand does not record an exclusion. To restore managed content, clear every applicable exclusion and run the relevant install or update operation.

## Choosing which links to check

Add `ignoreLinksFrom` or `ignoreLinksTo` to `.agents/open-forge.json`:

```json
{
  "$schema": "https://raw.githubusercontent.com/TheLithiumForge/open-forge/main/schemas/v2/open-forge.schema.json",
  "schemaVersion": 2,
  "ignoreLinksFrom": [".agents/memory/archived"],
  "ignoreLinksTo": []
}
```

Entries name workspace-relative files or folders, not patterns. A file entry covers that file. A folder entry covers everything beneath it. Paths use the same case-insensitive comparison as `allowInstallPaths`.

`ignoreLinksFrom` skips links written inside a listed path. Links into that path from elsewhere stay checked, and Repair can fix them when an archived file moves. `ignoreLinksTo` skips links whose resolved target lies inside a listed path. Doctor, Repair, Context link expansion, Route Move and Route Remove honor these settings. References still lists every link.

Doctor shows how many links it ignored and notes listed paths that no longer exist. Route Move updates matching entries. Route Remove drops them. Both preserve ignored links as written. Install adds the archive entry when it installs Memory archive and the key is absent. An existing key, even `[]`, is your choice. Update leaves your settings unchanged. Add the entry by hand in an existing workspace.

An older CLI ignores settings keys it does not know. A beta14 CLI ignores both ignore lists. Doctor reports an unrecognized settings version as information and continues with the keys it understands.

## Files the CLI keeps for itself

| File                           | Purpose                                                                                                          | Agent context? |
| ------------------------------ | ---------------------------------------------------------------------------------------------------------------- | -------------- |
| `.agents/open-forge.lock.json` | Records files and generated navigation regions managed by the Framework and each Extension.                      | No             |
| `.agents/open-forge.json`      | Your settings, including the frontmatter form, removal exclusions, and path grants for files outside `.agents/`. | No             |

Neither file gives content any authority. They exist for file maintenance only.

The `frontmatter` setting accepts `root` or `scoped`. A missing key means scoped, and both forms remain readable in every workspace. Fresh unattended Install chooses root unless settings already declare a preference. Interactive Install asks after the preset, with root preselected, and `--frontmatter` supplies the choice without asking. Ordinary repeated Install and Update retain the effective form.

Change just the form without changing route choices:

```sh
open-forge install --configure --frontmatter root --dry-run
open-forge install --configure --frontmatter root --automatic
```

Configure converts unedited owned Framework and Extension Markdown to the selected form in the same operation as the settings change. Edited owned files and Extension files whose source is unavailable are kept unchanged and reported. Keeping them does not block the form change. Excluded files, user-authored files, Library files, and overwrite companions are never converted. Native `SKILL.md` metadata and body content, including fenced examples, stay unchanged. This conversion differs from ordinary Update, which can replace edited managed files.

New route metadata and installed Templates follow the workspace setting. `route update` edits existing metadata where it was authored. [Installation](../getting-started/installation.md#choose-the-frontmatter-form) shows both forms and the setup choices.

Compatible Skill adoption can add missing metadata and navigation to user-owned content without claiming whole-file management. See the [installation guidance](../getting-started/installation.md#with-the-cli).

## Moving and renaming

You may move, rename, or restructure routes. Moving a managed route may end automatic updates for it, but it doesn't change what the content means once loaded. A familiar folder name in a new place doesn't gain root-route behavior.

For user-owned notes, Index completes missing metadata and navigation after manual creation. Repair fixes proven renames or moves and their affected Entries. Managed files, native Skill packages and resources, overwrite companions, and excluded or private contents keep their own maintenance boundaries. Follow the reported Update or authoring action. See [manual edits](/guides/cli#after-you-edit-files-by-hand).

## Managed versus meaningful

Management and meaning are separate:

- **Management** decides which files an installer or updater may change.
- **Meaning** comes from the route a file is in and the rules loaded above it.

An Extension's files mean exactly what their route says, whether they were installed by the CLI or copied by hand.

Next: [Extensions](../extensions/index.md), or look up a term in the [Glossary](../glossary.md).
