---
name: open-forge-cli
description: Use the open-forge CLI to load workspace context, find sources, maintain routes and links, and preview installation changes. Use when the command is available and the task needs Open Forge context, navigation, diagnosis, or workspace maintenance.
---

# Open Forge CLI

Use the optional CLI to read and maintain Open Forge's Markdown, rules, and links. Read this guide before running commands other than `context`. Reading it grants no installation or package-change authority.

Before you write workspace files or metadata, read the file formats in `.agents/README.md`, or run `open-forge help concepts` if that page was deleted.

You may create or move user-owned Markdown files directly inside routed folders. Preview and run `open-forge index` to fill missing metadata, create missing entrypoints, and refresh Entries instead of writing those by hand. After renames or moves, preview and run `open-forge repair --automatic` to repair proven links and affected navigation. When Git retains the old file's committed content, Index and Repair work in either order. Without Git, run Repair first while old Entries still record the file's metadata. Index and Repair preserve protected content, including managed files. Follow Doctor's next step for remaining choices, edits or a Git timeout retry. Read [route maintenance](references/routes.md) for the boundaries.

## Most Important Commands

| Need                                              | Start here                             |
| ------------------------------------------------- | -------------------------------------- |
| Load startup and selected route chains            | `context`                              |
| Find related material by consistent tags          | `find --tag <tag>`                     |
| Discover source IDs and route behavior            | `route list`, `route inspect`          |
| Inspect incoming impact and outgoing dependencies | `references --direction in\|out\|both` |
| Check health and diagnose problems                | `status`, `doctor`                     |
| Preview generated navigation after manual edits   | `index --dry-run`                      |

Check `open-forge --help` once for availability and groups. Use `open-forge <command> --help` for the installed version's exact syntax. If unavailable, follow the loader and maintain the files directly.

Use `open-forge help concepts` for built-in explanations and the file formats from the shipped `.agents/README.md`. It needs no workspace.

Prefer one context request over separate reads for each entrypoint:

```sh
open-forge context --for src/Order.cs --for web/order.ts
```

Supply all known working files, including planned files. Reading a source for context does not make it a working file. Omit `--for` while files are unknown, but resolve pending `applyTo` conditions before relying on those rules. Each applicable source needs one supplied path that matches its complete inherited condition chain. A path filter does not select an unexposed scope or grant edit permission.

Select relevant scopes from returned descriptions and batch their source IDs. After startup is loaded, use `context <selected-source> --additions-only` to omit that startup set. Read returned instructions and obey explicit required-read links too. The CLI follows loading tags, not prose instructions.

## Read Details When Needed

- [Reading and discovery](references/discovery.md): context, tags, headings, direct references, route inspection, and health checks. Find candidates, inspect their relationships, then load only the relevant chain or bounded linked context.
- [Route maintenance](references/routes.md): create, update, move, remove, index, repair, and cleanup. Read before changing routes or generated navigation.
- [Framework and packages](references/packages.md): install setup, update, removal, all Extension commands, and all Library commands. Read for an authorized lifecycle task.
- [Shared options and results](references/common.md): global flags, source syntax, previews, unattended execution, permission boundaries, and exit codes. Read before an unfamiliar command or when interpreting a result.

Preview each mutation with `--dry-run` and inspect its plan before applying. Run installation or package changes only when requested. `--automatic`, `--force`, and `--allow-path` add no human authorization. Check semantic status and exit code. Resolve an in-scope suggested next step or report the boundary when a command is incomplete or blocked. Do not force the same effect through manual edits.
