---
open-forge:
  description: Take Open Forge back out of a workspace, keep the files the user wrote and optionally strip what remains
  responsibility: Define uninstall's syntax, removal boundary, kept files, state removal, strip options, results, errors and the decisions awaiting the maintainer
  tags: [Memory, Crystallized, CLI, Command, Contract, Uninstall, Interface, Lifecycle, CurrentTruth]
---

# uninstall Interface Contract

## Purpose

`uninstall` removes the Framework and Extension files Open Forge installed and still recognizes, its managed sections in `AGENTS.md` and `CLAUDE.md`, the route scaffolds it wrote and its state. Files the user wrote stay. Managed files the user edited stay too, unless the user names them. With [`--strip`](../strip/interface.md#parts) and [`--flatten`](../strip/interface.md#--flatten-folder) it also turns the remaining documents into plain Markdown, so one command builds an eval comparison setup from a full Open Forge workspace.

The sibling [Behavior Contract](behavior.md) defines planning and application. [Issue 19](https://github.com/TheLithiumForge/open-forge/issues/19) records the request.

## Help

The purpose sentence is: Remove Open Forge from this workspace and keep the files you wrote.

```text
open-forge uninstall [--strip <parts>]... [--flatten <folder>] [--remove-edited <path>]... [--dry-run] [--automatic]
```

Follow the [shared help template](../shared/global-flags/interface.md#--help).

## Syntax

```text
open-forge uninstall
  [--strip <metadata|entries|links>[,...]]...
  [--flatten <folder>]
  [--remove-edited <path>]...
  [--dry-run] [--automatic]
  [global flags]
```

`uninstall` takes no operands. `--strip` and `--flatten` keep their [strip meaning](../strip/interface.md). `--remove-edited` is an Authority modifier. Each occurrence names one workspace-relative file, with an optional leading `./`, that Open Forge manages: an owned Framework or Extension file, `AGENTS.md` or `CLAUDE.md`. Uninstall then removes that file, or that host's managed section, even when the user edited it. A path that names no managed file is `uninstall.invalid-input`. Repeating a path changes nothing.

## What Uninstall Removes

Uninstall compares each owned target with the running CLI's payload, using the same comparison as `status` and `update`.

- **Framework and Extension files** that still match their payload are deleted. A file shared by several packages goes once.
- **Managed sections** in `AGENTS.md` and `CLAUDE.md` that still match their payload are removed, together with the blank lines that separated them from the user's text. The user's own text in those files stays. When nothing else is left, the file is deleted.
- **Route scaffolds** are entrypoints Open Forge wrote for a route, such as `docs/_docs.md` from `route include`. See [Route Scaffolds](#route-scaffolds).
- **State** is `.agents/open-forge.lock.json`, `.agents/open-forge.json` and the runtime folder `.open-forge-state/`. The runtime folder is the plan's last entry and goes when it holds only its lock and ignore files. When it holds recovery data, is or holds a link, or cannot be read, it stays and the result reports `uninstall.runtime-retained` with the real reason. Retained recovery data adds the `cleanup` next action.
- **Folders** left empty by these deletions are removed, deepest first. A folder that still holds any file stays.

Only ownership records make a file Open Forge's. Without them uninstall removes nothing, even a file whose bytes match the payload or the scaffold shape. Missing targets need no effect. A target that cannot be read or compared is `uninstall.target-unavailable` and blocks the run, because uninstall cannot tell whether the user edited it.

## What Stays

- Every file Open Forge does not own, including notes the user added to Framework folders and overwrite companions.
- Owned files and managed sections the user edited. Each one is reported as `uninstall.edited-kept`, an information finding naming the `--remove-edited` option.
- Generated `Entries` sections in documents that stay. Add `--strip entries` to remove them.
- Git-ignore rules. This pass leaves the Install and Library sections of `.gitignore` in place.

Libraries project links into the workspace and record them in the lock. While a Library is attached, uninstall stops with `uninstall.library-attached` and the `library detach` next action, so no projected link loses its record.

## Route Scaffolds

An entrypoint that no package owns is a route scaffold when it holds nothing but what Open Forge writes for a new route. After its Open Forge metadata and generated `Entries` section are set aside, its non-blank lines must be exactly:

1. one level-1 heading;
2. optionally, its metadata description followed by a period;
3. the `## Axioms` heading;
4. the inherited marker `- inherited - No local axioms; loaded ancestor axioms remain active.`

Its frontmatter may hold only Open Forge keys. Any other line, key or section means the user wrote in it, so it stays and is stripped like other documents.

## Strip And Flatten

`--strip` and `--flatten` apply the [strip parts](../strip/interface.md#parts) to the documents that stay. Deleted files are not stripped. Strip and flatten run after the removals are planned, so a flatten folder can empty once its scaffold goes. Strip's document set, link rules, collision rules and result fields apply unchanged, with `uninstall` finding codes.

## Preview And Consent

`--dry-run` lists every delete, rewrite and move with SHA-256 hashes and writes nothing. JSON output lists the same plan. Applying requires plan confirmation or `--automatic`. When the session cannot prompt and `--automatic` is absent, the result is `uninstall.confirmation-required`. JSON and redirected output never prompt. A workspace with nothing to remove and nothing to strip is a verified no-op that reports `Nothing to do.`

## Results

The command is `uninstall` in the shared schema-3 report, with the [strip result fields](../strip/interface.md#results). Plan entries add these `reason` values: `framework`, `extension`, `root-host`, `scaffold` and `state`. The runtime folder's entry is `delete-directory` with reason `state`. `kept` lists edited files with reason `edited`, and a runtime folder that stays with reason `recovery-data`, `link` or `unreadable`.

## Errors And Boundaries

Uninstall uses every [strip code](../strip/interface.md#errors-and-boundaries) with the `uninstall.` prefix, plus its own codes:

| Code                                    | Meaning                                                                                                    |
| --------------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| `uninstall.invalid-input`               | Same meaning as `strip.invalid-input`.                                                                     |
| `uninstall.workspace-unavailable`       | Same meaning as `strip.workspace-unavailable`.                                                             |
| `uninstall.document-unsupported`        | Same meaning as `strip.document-unsupported`.                                                              |
| `uninstall.flatten-collision`           | Same meaning as `strip.flatten-collision`.                                                                 |
| `uninstall.flatten-managed`             | Same meaning as `strip.flatten-managed`, counting only files that stay managed after the planned removals. |
| `uninstall.linked-file-skipped`         | Same meaning as `strip.linked-file-skipped`.                                                               |
| `uninstall.target-changed`              | Same meaning as `strip.target-changed`.                                                                    |
| `uninstall.target-changed-during-apply` | Same meaning as `strip.target-changed-during-apply`.                                                       |
| `uninstall.workspace-lock-unavailable`  | Same meaning as `strip.workspace-lock-unavailable`.                                                        |
| `uninstall.confirmation-required`       | Same meaning as `strip.confirmation-required`.                                                             |
| `uninstall.permission-declined`         | Same meaning as `strip.permission-declined`.                                                               |
| `uninstall.recovery-unavailable`        | Same meaning as `strip.recovery-unavailable`.                                                              |
| `uninstall.write-failed`                | Same meaning as `strip.write-failed`.                                                                      |
| `uninstall.recovery-artifact-retained`  | Same meaning as `strip.recovery-artifact-retained`.                                                        |
| `uninstall.interrupted`                 | Same meaning as `strip.interrupted`.                                                                       |
| `uninstall.ownership-unavailable`       | The ownership lock cannot be read, so uninstall cannot tell what Open Forge installed.                     |
| `uninstall.library-attached`            | A Library is attached. Detach it first.                                                                    |
| `uninstall.target-unavailable`          | An owned target cannot be read or compared with its payload.                                               |
| `uninstall.edited-kept`                 | Information. An edited managed file or section stays.                                                      |
| `uninstall.runtime-retained`            | `.open-forge-state/` stays. The message names why: recovery data, a link, or a read or removal failure.    |

## Examples

```sh
open-forge uninstall --dry-run
open-forge uninstall --automatic
open-forge uninstall --strip metadata,entries,links --automatic
open-forge uninstall --strip metadata,entries,links --flatten docs --automatic
open-forge uninstall --remove-edited .agents/loader.md --dry-run --format json
```

## Accepted Decisions

The maintainer accepted these answers to the issue's questions:

- **State removal is a plan step.** `.open-forge-state/` is the plan's last entry, shown by `--dry-run` and reported in the effects. When it must stay, that is a `kept` entry with its reason.
- **Files changed during apply** stop the run as `target-changed-during-apply` with the `doctor` next action. A change found before effects start stays `target-changed`.
- **A second run is idempotent.** Once the ownership records are gone, uninstall changes nothing. Matching bytes never establish ownership.
- **Edited managed files stay by default.** Uninstall never asks about them one by one. `--remove-edited <path>` names each one to remove.
- **Managed sections go, user text stays.** Uninstall removes only Open Forge's own sections in `AGENTS.md` and `CLAUDE.md`, keeps the user's text and deletes a host only when nothing else is left.
- **`strip` is its own command,** and its parts are also options on `uninstall`.

## Decisions To Confirm

These defaults are recorded for the maintainer to confirm:

- **Route scaffolds count as installed files.** Uninstall removes an unowned entrypoint only when it matches the scaffold shape above.
- **Libraries block.** A later pass may detach them as part of uninstall.
- **Git-ignore sections stay** in this pass.
