---
open-forge:
  description: Read workspace context and find related sources through tags and references
  tags: [Skill, CLI, Reference]
---

# Reading And Discovery

Use these read-only commands to select context before opening many files. All accept the [shared global flags](common.md#global-flags).

## Discover Connected Sources

Use `find --tag` to discover related topics and `references` to inspect authored relationships.

For example, discover applicable Directives, inspect one returned source's impact, then read its chain and one level of linked explanation:

```sh
open-forge find --tag Directive --for src/Order.cs --content metadata,headings
open-forge references directives/order-rules --direction both
open-forge context directives/order-rules --for src/Order.cs --follow-links 1
```

`directives/order-rules` is an example ID. Use an ID from the search result. Before creating a record, search for its subject and category, such as `find --tag Decision`.

## `context [<source-reference>...]`

Load startup context or selected sources with their required route context. No operands returns the loading-tag closure. The default content is `frontmatter,body`, which includes instructions. Explicit prose instructions still require their named reads.

| Flag                                   | Meaning                                                                                                                                                     |
| -------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `--for <path>`                         | Evaluate `applyTo` against a workspace-relative working file. Repeat for the complete set, including planned files                                          |
| `--additions-only`                     | Omit the startup closure. Requires an explicit source. Does not track every file read earlier                                                               |
| `--content <part[,part...]>`           | Select `metadata`, `paths`, `frontmatter`, `headings`, `body`, or `section:<heading>`. Supply one value. Escape commas and backslashes within section names |
| `--follow-links <positive-depth\|all>` | Follow contained local Markdown links breadth-first to a positive depth or all reachable links. Reports broken edges and never fetches external URLs        |

Select the scope, then apply file conditions. One supplied path must satisfy all conditions inherited along that source's route. `--for` alone does not select a hidden scope. Reading a Markdown source for context does not make its path a working file or grant edit permission.

Without known working files, exposed conditioned `#LoadNow` and `#KeepInMind` sources remain pending and return `incomplete` (exit 3). Other conditioned sources stay on demand. Resolve pending conditions before relying on those rules.

Batch related selections and use bounded links when their explanations matter:

```sh
open-forge context skills/open-forge-cli --for src/Order.cs --for web/order.ts
open-forge context directives/backend memory/crystallized --additions-only --for src/Order.cs
```

Avoid `--follow-links all` for ordinary startup. It may load related content the task does not need.

## `find`

Search Markdown tags and complete headings to find candidates, including applicable rules below unselected scopes. Search results do not load their ancestor rules. Use `context <source>` before relying on a candidate.

| Flag                           | Meaning                                                                                         |
| ------------------------------ | ----------------------------------------------------------------------------------------------- |
| `--include <source-reference>` | Bound the source inventory. Repeat to combine includes                                          |
| `--exclude <source-reference>` | Omit selected sources. Repeat as needed. Exclusions win                                         |
| `--tag <tag>`                  | Match an authored tag value. Repeat for more tags                                               |
| `--heading <heading>`          | Match a complete Markdown heading. Repeat for more headings                                     |
| `--require <all\|any>`         | Combine tag and heading predicates. Default: `all`                                              |
| `--within <part[,part...]>`    | Match within `document`, `frontmatter`, `body`, or `section:<heading>`                          |
| `--content <part[,part...]>`   | Return `metadata`, `frontmatter`, `headings`, `body`, or `section:<heading>`                    |
| `--for <path>`                 | Filter for applicable working paths. Repeat as needed. This filter is separate from `--require` |

Include and exclude each take one exact source reference per occurrence. Unresolved references prevent the search. `--within` and `--content` each take one comma-separated list. Metadata and headings help triage, but read the body before relying on a rule.

## `references <source-reference>`

Show direct incoming impact and outgoing dependencies before changing a source.

| Flag                           | Meaning                                                                                 |
| ------------------------------ | --------------------------------------------------------------------------------------- |
| `--direction <in\|out\|both>`  | Select incoming, outgoing, or both sections. Default: `both`. Values are case-sensitive |
| `--include <source-reference>` | Bound the incoming scan. Repeat to combine includes                                     |
| `--exclude <source-reference>` | Omit incoming scan sources. Repeat as needed. Exclusions win                            |

Incoming filters cannot be used with `--direction out`. References does not read target bodies, follow multiple hops, fetch URLs, or repair destinations. Use bounded `context --follow-links` to read related content.

## `route list [<source-reference>]`

List source IDs and paths to orient scope selection. `--depth=<non-negative-integer|all>` is its only command flag. Default: `1`. Use `--depth=0` for selected roots, `--depth=2` for a bounded subtree, or `--depth=all` for all descendants. The equals form is required.

## `route inspect <source-reference>`

Explain one source's route, loading, and conditions without returning its body.

| Flag               | Meaning                                                                                                                              |
| ------------------ | ------------------------------------------------------------------------------------------------------------------------------------ |
| `--for <path>`     | Explain applicability to a working path. Repeat for more paths                                                                       |
| `--matching-files` | List up to 100 existing workspace files satisfying the complete condition chain. Does not supply working paths or activate the scope |

An unrestricted source reports all files without scanning. Restricted scans use existing tracked and untracked Git files, excluding ignored untracked files, or the workspace's ignore rules when Git inventory is unavailable. They remain inside the workspace and skip symbolic links and submodules.

## `status`

Get a quick summary of installation, startup and continuity context, generated navigation, lifecycle, managed content, and recovery candidates. It has no command-specific flags and writes nothing.

## `doctor`

Diagnose workspace, routes, references, recovery, Framework files, and managed Extensions without changes. It has no command-specific flags. Use `--detail standard` for reasons or `--format json --detail full` for evidence. Run after routed changes and resolve or report findings within the task.

Continue with [route maintenance](routes.md) for edits or [Framework and packages](packages.md) for lifecycle changes.
