---
title: Working with the CLI
description: Choose a command for your task, then follow the full reference or a worked flow.
---

# Working with the CLI

The `open-forge` command helps you inspect your project's context and maintain
its Markdown files. Choose a task below. The [command reference](/guides/cli)
defines every operation and option, and [Everyday flows](flows.md) gives complete
procedures. For a first walkthrough, use the
[ten-minute guide](../getting-started/ten-minute-guide.md).

Commands follow the [context folder chosen at installation](../getting-started/installation.md#choose-the-context-folder). To maintain an existing folder elsewhere in the workspace, register it as an outside route with folder Include, as [Your own docs](../getting-started/your-own-docs.md) describes.

## Start with these commands

| When you want to                               | Run                                                   |
| ---------------------------------------------- | ----------------------------------------------------- |
| Read startup context                           | [`open-forge context`](/guides/cli#context)           |
| Find a rule                                    | [`open-forge find --tag Directive`](/guides/cli#find) |
| Check links and navigation                     | [`open-forge doctor`](/guides/cli#doctor)             |
| Preview navigation updates after editing files | [`open-forge index --dry-run`](/guides/cli#index)     |

The first three only read files. Review the Index preview, then run
`open-forge index` to fill missing metadata and update generated `Entries`, the
lists that help agents find files. These are maintenance tools, not required
steps before every task.

Doctor accepts links to existing workspace folders, with or without a trailing
slash. Remove heading fragments from folder links. A missing folder named with
a trailing slash needs a manual edit, and Repair does not suggest a file for it.
Web URLs stay unchecked.

Index encodes local link paths, such as `old%20guide.md`.
Doctor also accepts readable paths: `<old guide.md>` and `café(one).md`.
Escape an entity-opening ampersand as `%26`: write `entity&amp;.md` as
`entity%26amp;.md`. Keep `#`, `%` and `?` in names escaped.
Repair writes Index's form when
correcting other spellings. Command arguments use literal source paths.
Images with invalid heading fragments need a manual edit.

After renaming or moving files by hand, preview `open-forge repair --automatic --dry-run`. Repair fixes proven links and affected Entries. When Git retains the old file's committed content, Index and Repair work in either order. Without Git, run Repair first while old Entries still record the file's metadata. Follow Doctor's next step for any remaining choice or edit.

Use `open-forge --help` for the installed command inventory and
`open-forge <command> --help` for immediate syntax and common examples. Root
help starts with an Essentials preview and installation. Command help puts
examples before its behavior notes and options.

## Which command for which job

| Task                                                       | Command reference                                                                                                                                                                                                                                                                                          |
| ---------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Catch up after creating, renaming, or moving files by hand | [After you edit files by hand](/guides/cli#after-you-edit-files-by-hand)                                                                                                                                                                                                                                   |
| Inspect the workspace                                      | [status](/guides/cli#status), [route list](/guides/cli#route-list), [route inspect](/guides/cli#route-inspect)                                                                                                                                                                                             |
| Read and discover context                                  | [context](/guides/cli#context), [find](/guides/cli#find), [references](/guides/cli#references)                                                                                                                                                                                                             |
| Add and organize knowledge                                 | [route init](/guides/cli#route-init), [route create](/guides/cli#route-create), [route update](/guides/cli#route-update), [route move](/guides/cli#route-move), [route remove](/guides/cli#route-remove)                                                                                                   |
| Use documents you already have                             | [route include](/guides/cli#route-include), [route exclude](/guides/cli#route-exclude)                                                                                                                                                                                                                     |
| Learn how the framework works                              | [help concepts](/guides/cli#help)                                                                                                                                                                                                                                                                          |
| Check and repair                                           | [index](/guides/cli#index), [doctor](/guides/cli#doctor), [repair](/guides/cli#repair), [cleanup](/guides/cli#cleanup)                                                                                                                                                                                     |
| Install, update, or remove Framework content               | [install](/guides/cli#install), [update](/guides/cli#update), [remove](/guides/cli#remove)                                                                                                                                                                                                                 |
| Manage optional Extension packages                         | [extension list](/guides/cli#extension-list), [extension inspect](/guides/cli#extension-inspect), [extension create](/guides/cli#extension-create), [extension install](/guides/cli#extension-install), [extension update](/guides/cli#extension-update), [extension remove](/guides/cli#extension-remove) |
| Share files through a Library                              | [library list](/guides/cli#library-list), [library inspect](/guides/cli#library-inspect), [library attach](/guides/cli#library-attach), [library sync](/guides/cli#library-sync), [library detach](/guides/cli#library-detach)                                                                             |

A source can be named by an ID such as `guidance`, an exact `./<workspace-path>` path, or the literal `.agents/` path form. Custom-root files keep the same short IDs.
See [source references](/guides/cli#source-references) when selecting a target.
To restore one left-out built-in category, preview
`open-forge route init <category> --framework --dry-run`, review it, then run the
same request without `--dry-run`. Use `open-forge install --configure` as the
interactive way to change several setup choices.

Index and Repair preserve protected sources, including managed files. Doctor names the managing Update or required edit. If Git times out, follow the retry command before deciding that a missing file was deleted. See [manual edits](/guides/cli#after-you-edit-files-by-hand).

## Everyday habits

Preview each mutation with `--dry-run`, review its complete plan, then apply the
same request. Check what changed and follow the result's next step. A dry run
changes no files. Commands that need confirmation accept `--automatic` for
unattended application. Route Init, Create, Update, Move, Index, and Cleanup
apply directly. See [prompts and unattended runs](/guides/cli#prompts-and-noninteractive-runs).

Use `--workspace <path>` to select another exact directory and `--detail standard`
for reasons. The [global options](/guides/cli#global-options) define shared flags.

## For agents and scripts

For an initial context batch that includes the shipped CLI Skill, run
`open-forge context skills/open-forge-cli`. Supply every known working file,
including planned files, with repeated `--for <path>`. Follow explicit read
instructions in the returned files. The Skill holds offline command-selection
and result-handling guidance. The [Context reference](/guides/cli#context)
defines file conditions, pending context, and later batches.

Use `--format json` and check the exit code in scripts. `0` means completed and
`2` means completed with warnings. The [status and exit reference](/guides/cli#status-and-exit-codes)
defines every result. Doctor returns a nonzero code for warnings or errors, so
it can check workspace health in CI.

Each directory is its own workspace, including each Git worktree. Writing commands
use its fixed `.open-forge-state/` folder, so a sandbox needs only workspace write
access. See [worktrees, sandboxes and the runtime folder](/guides/cli#worktrees-sandboxes-and-the-runtime-folder)
for lock failures, recovery stores and the bridge to older CLIs.

When an Extension or Library asks to change paths outside the primary content locations, key mode
starts focused on `Allow once`. Enter approves only this run without saving a
grant. The numbered order stays `1. Allow always`, `2. Allow once`, `3. Cancel`.
Choose `Allow always` deliberately to save a grant for later operations. Line
mode accepts `always`, `once`, or `cancel`, and an empty line cancels.

Next: [Everyday flows](flows.md).
