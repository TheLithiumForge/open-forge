---
open-forge:
  description: Write a copy of the project as it would look after uninstall to a new folder, optionally stripped and flattened, and leave the project unchanged
  responsibility: Define export's syntax, destination, copied files, output meaning, results and errors
  tags: [Memory, Crystallized, CLI, Command, Contract, Export, Interface, CurrentTruth]
---

# export Interface Contract

## Purpose

`export` writes a copy of the project as it would look after [`uninstall`](../uninstall/interface.md) to a destination folder. It shares the [plain-docs plan](../shared/plain-docs/interface.md) and its selection flags with `uninstall`, so `--strip` and `--flatten` shape the copy the same way. The project itself never changes.

The sibling [Behavior Contract](behavior.md) defines how the copy is written. [Issue 19](https://github.com/TheLithiumForge/open-forge/issues/19) records the request.

## Help

The purpose sentence is: Write a copy of this project without Open Forge to a new folder.

```text
open-forge export <destination> [--strip <parts>]... [--flatten <folder>] [--remove-edited <path>]... [--dry-run]
```

Follow the [shared help template](../shared/global-flags/interface.md#--help).

## Syntax

```text
open-forge export <destination>
  [--strip <metadata|frontmatter|entrypoints|links>[,...]]...
  [--flatten <folder>]
  [--remove-edited <path>]...
  [--dry-run]
  [global flags]
```

`<destination>` is required. A relative path resolves against the current directory. The [selection flags](../shared/plain-docs/interface.md#selection-flags) keep their shared meaning. `export` has no `--automatic` and never asks for confirmation, because nothing in the project changes.

## Destination

The destination must be a new folder or an empty one. Outside the project is recommended. A destination inside the project is allowed, and the copy then leaves the destination itself out, so nothing copies into itself. These are `export.invalid-input` and nothing is written:

- the destination is a file;
- the destination is a folder that already holds files or folders;
- the destination is the project folder or contains it.

The message names the destination and the problem, and the next step is to choose a new or empty folder.

## What The Copy Holds

The copy starts from the project's files:

- In a Git work tree, Git's own file list: tracked files plus untracked files that Git does not ignore.
- Without Git, the same walk that [comment discovery](../shared/source-references/interface.md#comment-links) uses: every file except `.git` and nested Git repositories, applying the workspace's `.gitignore` rules.
- Never `.open-forge-state/`, a symbolic link, a junction or anything inside them. A link is reported as `export.linked-file-skipped`.

The plan then shapes the copy. In the copy, its actions mean:

- `delete`: the file is left out of the copy. Uninstall's selection leaves out Open Forge's own files and state, and `entrypoints` leaves out every entrypoint, managed ones included.
- `rewrite`: the file is copied with its edits, such as a root host without Open Forge's managed section or a stripped document.
- `move`: the file is copied to its flattened path with its edits.
- Every other listed file is copied byte for byte.

Edited managed files stay in the copy, as uninstall keeps them, unless `--remove-edited` names them. The copy holds files only. A folder appears only when it holds a copied file, so `delete-directory` entries add nothing and are not listed.

## Results

The command is `export` in the shared schema-3 report, with the [plain-docs result fields](../shared/plain-docs/interface.md#results). `data.destination` is the destination's full path. The plan lists only the files the copy changes: left out, rewritten or moved. Counts add `files copied` for files written byte for byte. Effects describe the copy: a rewrite is a rewritten file and a move is a created file at its new path. A left-out file has no effect.

A preview says how many files the copy would hold. A completed run says how many it wrote. The next action after a preview is the same command without `--dry-run`.

## Errors And Boundaries

`export` uses the [shared plain-docs codes](../shared/plain-docs/interface.md#shared-errors) with their shared meanings. It also uses every shared code with its own prefix: `export.invalid-input`, `export.workspace-unavailable`, `export.ownership-unavailable`, `export.library-attached`, `export.target-unavailable`, `export.edited-kept`, `export.document-unsupported`, `export.flatten-collision`, `export.linked-file-skipped`, `export.interrupted`. Its own code is:

| Code                  | Meaning                                                                                   |
| --------------------- | ----------------------------------------------------------------------------------------- |
| `export.write-failed` | Writing the copy stopped part way. The message names the destination to delete and rerun. |

`export.interrupted` after writing started also names the destination to delete. Both messages end with `Delete <destination> and run the same command again.`, and the next action is that command.

## Examples

```sh
open-forge export ../plain-docs --dry-run
open-forge export ../stage-4 --strip metadata,entrypoints
open-forge export ../stage-3 --strip metadata,entrypoints,links --flatten docs
```

## Decisions

The maintainer accepted these:

- `export` replaces the standalone `strip` command. It writes the project as `uninstall` would leave it to a new or empty folder and never changes the project.
- The copy starts from Git's file list, tracked plus untracked and not ignored, or without Git the same `.gitignore`-aware walk comment discovery uses, and never `.open-forge-state/`.
- The destination may be inside the project, which is allowed but not recommended. It must be new or empty, and it must not be a file or contain the project.
- No confirmation prompt, recovery bundle, lease or interruption tracking. A run that stops part way names the destination to delete and rerun.

These defaults are recorded for the maintainer to confirm:

- An attached Library blocks export as it blocks uninstall, with the `library detach` next action.
- Symbolic links and junctions are left out of the copy and reported, rather than copied as links or followed.
