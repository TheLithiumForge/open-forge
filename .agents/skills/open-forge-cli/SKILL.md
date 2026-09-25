---
name: open-forge-cli
description: Use the open-forge CLI to see what loads, find sources, keep links and Entries correct, and preview workspace changes. Use when the command is available and the task needs Open Forge context, navigation, diagnosis, or changes to many workspace files.
---

# Open Forge CLI

The `open-forge` CLI reads and maintains the Open Forge files in this workspace. The files stay complete without it. Use it when it is faster or safer than working by hand: to see exactly what loads, to find sources by tag or heading, and to change routed files without breaking links or generated `Entries`.

## Start

- Check that the command is available with `open-forge --help`. When it is not, work with the files directly. Installing the CLI is the user's choice.
- Use `open-forge <command> --help` for exact arguments and options. Help describes the installed version, so follow it when it differs from this Skill.
- Commands use the current folder. Pass `--workspace <path>` to select another one.
- Name a source by its ID, such as `memory/crystallized/documents`, or by its exact path, such as `.agents/memory/crystallized/documents/_documents.md`. Use the exact path when an ID is ambiguous.

## Work Safely

- Preview every change with `--dry-run` and review the plan before applying it.
- Read the status that ends each result. `completed-with-warnings` is usable but needs review. `incomplete` means required facts were missing. `blocked` means the CLI stopped at a boundary instead of guessing.
- Follow the next step the result suggests, then run the original command again.
- Do not work around a `blocked` or `incomplete` result by editing files to force the same effect. Report it with the suggested next step.
- Add `--detail standard` or `--detail full` for reasons and evidence. Use `--format json` when another tool reads the result.

## Choose A Command

### Read The Workspace

These commands change nothing.

| Command                               | Use it to                                                                                 |
| ------------------------------------- | ----------------------------------------------------------------------------------------- |
| `status`                              | Summarize startup context, navigation, installed packages, and anything needing attention |
| `context [<source>...]`               | See the startup context, or with `--additions-only`, what selecting a scope adds          |
| `route list [<source>]`               | See how a route is organized                                                              |
| `route inspect <source>`              | Explain one source's route, scope, and loading behavior                                   |
| `find`                                | Find sources by `--tag` or `--heading`, combined with `--require=all` or `any`            |
| `references <source>`                 | See what links to a source with `--direction=in`, or what it links to with `out`          |
| `doctor`                              | Diagnose broken links, stale navigation, and lifecycle problems                           |
| `extension list`, `extension inspect` | See which packages exist, what each installs, and what it depends on                      |
| `library list`, `library inspect`     | See which shared Libraries are attached and whether their links are current               |

### Change Routed Files

| Command                      | Use it to                                                                         |
| ---------------------------- | --------------------------------------------------------------------------------- |
| `route init <source>`        | Create a scope, or a missing chain of entrypoints                                 |
| `route create <source>`      | Add a file with correct frontmatter, optionally from a Template with `--template` |
| `route update <source>`      | Change a description, tags, or responsibility                                     |
| `route move <source> <path>` | Move a file or category and update the links to it                                |
| `route remove <source>`      | Remove a file or category and record the removal                                  |
| `index [<source>...]`        | Rebuild generated `Entries` after files were added, renamed, or retagged by hand  |
| `repair`                     | Fix broken local links that have one safe answer                                  |

### Change The Installation

`install`, `update`, `remove`, and `cleanup` change many files at once, and so do the `extension` and `library` commands that install, update, remove, create, attach, sync, or detach. Run them only when the user asks for that change, and show the dry-run plan first. Leave `cleanup` until the user has checked the result, because it deletes the recovery copies that updates and removals keep.

## Common Situations

- **A rule seems ignored.** Run `context` to see whether its file loads. Then run `route inspect <source>` to see why it does not: it may be on demand, or in a scope the task did not select.
- **Before recording something.** Run `find` with the category's tag, such as `find --tag=Decision`. Update an existing record instead of adding a competing one.
- **After editing routed files by hand.** Run `index --dry-run`, then `index`, so `Entries` match the files.
- **Before moving or deleting a file.** Run `references <source> --direction=in`, then preview `route move` or `route remove`.
- **Before closeout.** When the task changed routed files, run `doctor`. Resolve what it reports, or name it in the handoff.
