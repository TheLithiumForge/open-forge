---
title: Customizing
description: Edit shipped files, keep local changes through updates with overwrite companions, and remove defaults so they stay removed.
---

# Customizing

Every installed file is yours. Add, adapt, replace, or remove the defaults as your needs change. The only question is how your change interacts with later updates.

## Three ways to change a shipped file

| You want to...                               | Do this                                   | What `open-forge update` does                                                                |
| -------------------------------------------- | ----------------------------------------- | -------------------------------------------------------------------------------------------- |
| Use a different version of the file          | Edit the file directly.                   | Brings changed Framework files back to the current version and reports each one it replaces. |
| Add a local adjustment that survives updates | Create an adjacent `{name}.overwrite.md`. | Leaves the overwrite alone.                                                                  |
| Stop using the file entirely                 | Remove it with `open-forge remove`.       | Keeps it removed.                                                                            |

An update reports every path it replaces, restores, deletes, or retains, and keeps a recovery bundle when existing bytes need protecting. Still, review `open-forge update --dry-run` before applying one, and commit your edits first so `git diff` shows what changed.

## Overwrite companions

An overwrite is a user-owned file next to its base, named `{name}.overwrite.md`:

```text
.agents/memory/
  _memory.md
  _memory.overwrite.md       <- your local adjustment
```

It loads immediately after its base file and shares the base file's route, scope, and loading behavior. It isn't listed in `Entries` or selected separately.

Read it as part of the base source. Where the two answer the same question differently, the overwrite wins, but only for that corresponding content. It doesn't override unrelated files.

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

The CLI records exclusions in `.agents/open-forge.json`:

```json
{
  "schemaVersion": 1,
  "removedCategories": ["skills"],
  "removedFiles": [".agents/patterns/_patterns.md"],
  "removedDirectories": ["docs/obsolete"],
  "removedExtensions": ["planning"],
  "removedLibraries": []
}
```

Deleting a file by hand doesn't record an exclusion, so a later update may restore it. To bring removed content back, clear its exclusion and run the relevant install or update.

## Files the CLI keeps for itself

| File                           | Purpose                                                                                                   | Agent context? |
| ------------------------------ | --------------------------------------------------------------------------------------------------------- | -------------- |
| `.agents/open-forge.lock.json` | Which files the Framework and each Extension installed, so updates can tell them apart from your changes. | No             |
| `.agents/open-forge.json`      | Your settings, including removal exclusions and path grants for files outside `.agents/`.                 | No             |

Neither file gives content any authority. They exist for file maintenance only.

## Moving and renaming

You may move, rename, or restructure routes. Moving a managed route may end automatic updates for it, but it doesn't change what the content means once loaded. A familiar folder name in a new place doesn't gain root-route behavior.

## Managed versus meaningful

Management and meaning are separate:

- **Management** decides which files an installer or updater may change.
- **Meaning** comes from the route a file is in and the rules loaded above it.

An Extension's files mean exactly what their route says, whether they were installed by the CLI or copied by hand.

Continue with [Extensions](../extensions/index.md), or look up a term in the [Glossary](../glossary.md).
