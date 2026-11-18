---
open-forge:
  description: Take Open Forge back out of a workspace, keep the files the user wrote and optionally strip what remains
  responsibility: Define uninstall's syntax, removal boundary, kept files, state removal, strip options, results, errors and the decisions awaiting the maintainer
  tags: [Memory, Crystallized, CLI, Command, Contract, Uninstall, Interface, Lifecycle, CurrentTruth]
---

# uninstall Interface Contract

## Purpose

`uninstall` removes the Framework and Extension files Open Forge installed and still recognizes, its managed sections in `AGENTS.md` and `CLAUDE.md`, route scaffolds and its state. Files the user wrote stay. Managed files the user edited stay too, unless the user names them. It shares the [plain-docs plan](../shared/plain-docs/interface.md) and its selection flags with [`export`](../export/interface.md), so `--strip` and `--flatten` also turn the remaining documents into plain Markdown. `uninstall` applies the plan in place. `export` writes the same plan to a new folder and leaves the workspace unchanged.

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
  [--strip <metadata|frontmatter|entrypoints|links>[,...]]...
  [--flatten <folder>]
  [--remove-edited <path>]...
  [--dry-run] [--automatic]
  [global flags]
```

`uninstall` takes no operands. `--strip`, `--flatten`, `--remove-edited` and `--dry-run` are the [shared selection flags](../shared/plain-docs/interface.md#selection-flags). `--automatic` applies the plan without a confirmation prompt.

## What Uninstall Removes

Uninstall compares each owned target with the running CLI's payload, using the same comparison as `status` and `update`.

- **Framework and Extension files** that still match their payload are deleted. A file shared by several packages goes once.
- **Managed sections** in `AGENTS.md` and `CLAUDE.md` that still match their payload are removed, together with the blank lines that separated them from the user's text. The user's own text in those files stays. When nothing else is left, the file is deleted.
- **Route scaffolds** are entrypoints that hold nothing but what Open Forge writes for a route, such as `docs/_docs.md` from `route include`. See [Route Scaffolds](#route-scaffolds).
- **State** is `.agents/open-forge.lock.json`, `.agents/open-forge.json` and the runtime folder `.open-forge-state/`. The runtime folder is the plan's last entry and goes when it holds only its lock and ignore files. When it holds recovery data, is or holds a link, or cannot be read, it stays and the result reports `uninstall.runtime-retained` with the real reason. Retained recovery data adds the `cleanup` next action.
- **Folders** left empty by these deletions are removed, deepest first. A folder that still holds any file stays.

Only ownership records make a file Open Forge's, with one exception: while they exist, an unowned entrypoint with the [route scaffold](#route-scaffolds) shape counts as installed. Without them uninstall removes nothing, even a file whose bytes match the payload or the scaffold shape. Missing targets need no effect. A target that cannot be read or compared is `uninstall.target-unavailable` and blocks the run, because uninstall cannot tell whether the user edited it.

## What Stays

- Every file Open Forge does not own, including notes the user added to Framework folders and overwrite companions.
- Owned files and managed sections the user edited. Each one is reported as `uninstall.edited-kept`, an information finding naming the `--remove-edited` option.
- Entrypoints that are not route scaffolds, with their generated `Entries` sections. Add `--strip entrypoints` to delete them.
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

`--strip` and `--flatten` apply the [shared parts and flatten](../shared/plain-docs/interface.md#parts) to the documents that stay. Deleted files are not stripped. Strip and flatten run after the removals are planned, so a flatten folder can empty once its scaffold goes.

## Preview And Consent

`--dry-run` lists every delete, rewrite and move with SHA-256 hashes and writes nothing. JSON output lists the same plan. Each action changes the workspace: `delete` deletes the file, `rewrite` rewrites it in place, `move` moves it and `delete-directory` removes a folder left empty. Applying requires plan confirmation or `--automatic`. When the session cannot prompt and `--automatic` is absent, the result is `uninstall.confirmation-required`. JSON and redirected output never prompt. A workspace with nothing to remove and nothing to strip is a verified no-op that reports `Nothing to do.`

## Results

The command is `uninstall` in the shared schema-3 report, with the [plain-docs result fields](../shared/plain-docs/interface.md#results). Every plan entry is an effect. A move is two effects, the created destination and the deleted source. Plan entries add these `reason` values: `framework`, `extension`, `root-host`, `scaffold` and `state`. The runtime folder's entry is `delete-directory` with reason `state`. `kept` lists edited files with reason `edited`, and a runtime folder that stays with reason `recovery-data`, `link` or `unreadable`.

## Errors And Boundaries

Uninstall uses the [shared plain-docs codes](../shared/plain-docs/interface.md#shared-errors) with their shared meanings. It also uses every shared code with its own prefix: `uninstall.invalid-input`, `uninstall.workspace-unavailable`, `uninstall.ownership-unavailable`, `uninstall.library-attached`, `uninstall.target-unavailable`, `uninstall.edited-kept`, `uninstall.document-unsupported`, `uninstall.flatten-collision`, `uninstall.linked-file-skipped`, `uninstall.interrupted`. Its own codes are:

| Code                                    | Meaning                                                                                                 |
| --------------------------------------- | ------------------------------------------------------------------------------------------------------- |
| `uninstall.target-changed`              | A planned file changed before the first effect. Nothing was changed.                                    |
| `uninstall.target-changed-during-apply` | A planned file changed after effects started. The run stopped part way.                                 |
| `uninstall.workspace-lock-unavailable`  | The workspace mutation lease is unavailable.                                                            |
| `uninstall.confirmation-required`       | Applying needs confirmation or `--automatic`.                                                           |
| `uninstall.permission-declined`         | The user declined the plan.                                                                             |
| `uninstall.recovery-unavailable`        | Recovery data could not be prepared. Nothing was changed.                                               |
| `uninstall.write-failed`                | A planned effect failed. The recovery bundle stays, and the next action is `doctor`.                    |
| `uninstall.recovery-artifact-retained`  | The changes verified, but the recovery bundle could not be removed.                                     |
| `uninstall.runtime-retained`            | `.open-forge-state/` stays. The message names why: recovery data, a link, or a read or removal failure. |

Messages follow the shared families:

- A missing confirmation is `Uninstall needs confirmation, and this session cannot ask.`
- `target-changed` is `<path> changed after the plan was made. Nothing was changed.`
- `target-changed-during-apply` is `<path> changed while changes were being written. Stopped after <n> of <m> changes.` with the `doctor` next action.
- An interruption after the first effect ends with `Stopped after <n> of <m> changes.`

## Examples

```sh
open-forge uninstall --dry-run
open-forge uninstall --automatic
open-forge uninstall --strip metadata,entrypoints --automatic
open-forge uninstall --strip metadata,entrypoints,links --flatten docs --automatic
open-forge uninstall --remove-edited .agents/loader.md --dry-run --format json
```

## Accepted Decisions

The maintainer accepted these answers to the issue's questions:

- **State removal is a plan step.** `.open-forge-state/` is the plan's last entry, shown by `--dry-run` and reported in the effects. When it must stay, that is a `kept` entry with its reason.
- **Files changed during apply** stop the run as `target-changed-during-apply` with the `doctor` next action. A change found before effects start stays `target-changed`.
- **A second run is idempotent.** Once the ownership records are gone, uninstall changes nothing. Matching bytes never establish ownership, except for route scaffolds below.
- **Route scaffolds count as installed files.** While ownership records exist, uninstall removes an unowned entrypoint that matches the scaffold shape above, whoever wrote it.
- **Edited managed files stay by default.** Uninstall never asks about them one by one. `--remove-edited <path>` names each one to remove.
- **Managed sections go, user text stays.** Uninstall removes only Open Forge's own sections in `AGENTS.md` and `CLAUDE.md`, keeps the user's text and deletes a host only when nothing else is left.
- **Uninstall and export are one plan with two outputs,** with the same selection flags. The standalone `strip` command is gone.

## Decisions To Confirm

These defaults are recorded for the maintainer to confirm:

- **Libraries block.** A later pass may detach them as part of uninstall.
- **Git-ignore sections stay** in this pass.
