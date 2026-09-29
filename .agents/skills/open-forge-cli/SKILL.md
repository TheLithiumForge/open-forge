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
- Check each result's status. The exit code reports it, and `--format json` names it. `completed-with-warnings` (exit 2) is usable but needs review. `incomplete` means required facts were missing. `blocked` means the CLI stopped at a boundary instead of guessing.
- Commands that ask for confirmation write nothing in a noninteractive shell and return `invalid-input`. After the dry run is reviewed and the change is within the task, run the command again with `--automatic`.
- When a result suggests a next step within the task, take it, then run the original command again.
- Do not work around a `blocked` or `incomplete` result by editing files to force the same effect. Report it with the suggested next step.
- Add `--detail standard` or `--detail full` for reasons and evidence. Use `--format json` when another tool reads the result.

## Choose A Command

### Read The Workspace

These commands change nothing.

| Command                               | Use it to                                                                                 |
| ------------------------------------- | ----------------------------------------------------------------------------------------- |
| `status`                              | Summarize startup context, navigation, installed packages, and anything needing attention |
| `context [<source>...]`               | See startup or selected context, optionally filtered by repeated `--for <path>`           |
| `route list [<source>]`               | See how a route is organized                                                              |
| `route inspect <source>`              | Explain route behavior and how supplied paths match inherited conditions                 |
| `find`                                | Find by tags or headings, with an additional path filter through `--for`                   |
| `references <source>`                 | See what links to a source with `--direction=in`, or what it links to with `out`          |
| `doctor`                              | Diagnose broken links, stale navigation, and lifecycle problems                           |
| `extension list`, `extension inspect` | See which packages exist, what each installs, and what it depends on                      |
| `library list`, `library inspect`     | See which shared Libraries are attached and whether their links are current               |

### Change Routed Files

| Command                      | Use it to                                                                         |
| ---------------------------- | --------------------------------------------------------------------------------- |
| `route init <source>`        | Create a scope or chain. Metadata needs a missing generic final target                   |
| `route create <source>`      | Add routed file with frontmatter, optional Template or repeated `--apply-to` patterns    |
| `route update <source>`      | Change description, tags, responsibility, or file conditions                     |
| `route move <source> <path>` | Move a file or category and update the links to it                                |
| `route remove <source>`      | Remove a file or category and record the removal                                  |
| `index [<source>...]`        | Rebuild generated `Entries` after files were added, renamed, or retagged by hand  |
| `repair`                     | Fix broken local links that have one safe answer                                  |

`route create`, `route init`, and `route update` accept repeated
`--apply-to <glob>` values. New declarations use a quoted-string list under
`open-forge:`. Existing frontmatter may put `applyTo` at its root or under
`open-forge:` and may use one quoted string or a list. If both locations declare
the field, their pattern sets must be equivalent. A comma separates patterns in a
string or an `--apply-to` value. A list entry stays one pattern, and `[,]` is a
literal comma. `route update` preserves the authored location. Use
`--clear-apply-to` by itself to remove the condition. It cannot be combined
with `--apply-to`.

`route init` accepts metadata options only for a missing generic final
entrypoint. They cannot update an existing final target, and `--apply-to`
cannot be combined with `--framework`.

For a new condition, preview a comma-separated value:

```sh
open-forge route create guidance/ui-rules \
  --description="UI rules" \
  --tag=Guidance \
  --apply-to "**/*.tsx,**/*.css" \
  --dry-run
```

For an existing source with a condition, preview clearing it with
`route update <source> --clear-apply-to --dry-run`.

### Change The Installation

`install`, `update`, `remove`, and `cleanup` change many files at once, and so do the `extension` and `library` commands that install, update, remove, create, attach, sync, or detach. Run them only when the user asks for that change, and show the dry-run plan first. Leave `cleanup` until the user has checked the result, because it deletes the recovery copies that updates and removals keep.

## Common Situations

- **Choose context for a change.** Supply every working path, repeating `--for`.
  A source applies when one supplied path satisfies its full inherited condition
  chain. Reading a source only for context does not add its Markdown path.
  Planned paths may not exist yet. If paths are unknown, encountered conditioned
  sources stay pending and `context` returns `incomplete` with exit 3.
  `--additions-only` still needs an explicit source operand and uses the same
  complete path set for startup and combined context.
- **A rule seems ignored.** Run `context --for src/Order.cs --for web/order.ts`
  with the complete working set. Run `route inspect <source>` with the same
  repeated `--for` inputs to explain declared and inherited conditions.
  `context` can inspect an explicit or referenced nonmatch, but it does not
  activate that source's automatic children. A condition does not activate a
  hidden ancestor or determine edit permission.
- **Looking for rules about some files.** `context --for <path>` returns only what the loading rules load for those files. `find --for <path> --tag=Directive` lists every compatible source, including ones below scopes nobody selected. Load a relevant one with `context <source> --for <path>`.
- **Before recording something.** Run `find` with the category's tag, such as `find --tag=Decision`. `--for` adds a path filter. It does not replace the tag or heading predicates used by `--require`.
- **After editing routed files by hand.** Run `index --dry-run`, then `index`, so `Entries` match the files.
- **Before moving or deleting a file.** Run `references <source> --direction=in`, then preview `route move` or `route remove`.
- **Before closeout.** When the task changed routed files, run `doctor`. Resolve what it reports, or name it in the handoff.