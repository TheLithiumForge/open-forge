---
open-forge:
  description: Define the selection flags, strip parts, flatten rules and plan results that uninstall and export share
  responsibility: Define the shared plain-docs plan's flags, document set, parts, flatten, results, errors and the decisions awaiting the maintainer
  tags: [Memory, Crystallized, CLI, Command, Contract, Strip, Uninstall, Export, Interface, CurrentTruth]
---

# Plain-Docs Plan Interface Contract

## Purpose

[`uninstall`](../../uninstall/interface.md) and [`export`](../../export/interface.md) are one plan with two outputs. The plan takes uninstall's selection, then the strip parts, then flatten. `uninstall` applies it to the workspace. `export` writes it to a new folder and leaves the workspace unchanged. Both commands accept the same selection flags, so one workspace can produce every comparison setup the evals need.

The sibling [Behavior Contract](behavior.md) defines planning and determinism. [Issue 19](https://github.com/TheLithiumForge/open-forge/issues/19) records the request.

## Selection Flags

```text
  [--strip <metadata|frontmatter|entrypoints|links>[,...]]...
  [--flatten <folder>]
  [--remove-edited <path>]...
  [--dry-run]
```

- `--strip <parts>` selects [parts](#parts). Repeat it or list several parts separated by commas, such as `metadata,links`. Duplicates collapse, and the order never matters. An unknown part, an empty list member or a missing value is `<command>.invalid-input`.
- `--flatten <folder>` moves documents into one folder. See [`--flatten <folder>`](#--flatten-folder).
- `--remove-edited <path>` is an Authority modifier. Each occurrence names one workspace-relative file, with an optional leading `./`, that Open Forge manages: an owned Framework or Extension file, `AGENTS.md` or `CLAUDE.md`. The plan then removes that file, or that host's managed section, even when the user edited it. A path that names no managed file is `<command>.invalid-input`. Repeating a path changes nothing.
- `--dry-run` lists the plan with the SHA-256 of each file's bytes before and after, and writes nothing.

`<command>` is `uninstall` or `export`. The parts run in the fixed order `frontmatter` or `metadata`, then `entrypoints`, then `links`, then flatten. With both `frontmatter` and `metadata`, `frontmatter` wins and `metadata` adds nothing.

## Selection

The plan starts from uninstall's selection, which the [uninstall Interface](../../uninstall/interface.md#what-uninstall-removes) defines: Open Forge's own files, its managed sections in `AGENTS.md` and `CLAUDE.md`, route scaffolds and its state go, and edited managed files stay unless `--remove-edited` names them. The parts and flatten then apply to the documents that stay. Deleted files are not stripped.

## Documents

The parts read every Markdown file below the main root, the native Skills folder and each registered outside route. These are the layout's inventory roots. A file counts when its name ends in `.md`, including overwrite companions. The plan never enters `.open-forge-state/`, Git metadata or a symbolic link. A symbolic link or junction inside a root is skipped and reported as `<command>.linked-file-skipped`, so a Library link never rewrites its source. Files outside these roots, such as a root `README.md`, are not documents and keep their bytes.

Edited managed files that the selection keeps are documents like any other, because Open Forge no longer reads them. Only Markdown documents are read. Source code and other files outside this set stay untouched by every part, including the comment links they hold.

A document keeps its encoding, line endings and every byte outside the selected edits.

## Parts

### `metadata`

Removes Open Forge frontmatter keys and keeps every other key and the body bytes exactly.

- With an `open-forge:` key, the part removes that key's lines and a root `applyTo` key, because Open Forge reads `applyTo` in both forms.
- Without an `open-forge:` key, the part removes the root `description`, `tags`, `responsibility` and `applyTo` keys.
- A native `SKILL.md` keeps its Skill keys, such as `name` and `description`. Only `open-forge:`, `applyTo`, `tags` and `responsibility` go.

Removal takes whole lines, from the key's line to the last line of its value. When no keys remain, the part removes the whole frontmatter block, its delimiters and the line ending after the closing `---`, so the file starts with the first body byte. Comments inside a frontmatter block without keys go with it. Malformed YAML, a duplicate `open-forge` key or a removable key that shares a line with a kept key is `<command>.document-unsupported` and blocks the whole run.

### `frontmatter`

Removes the whole frontmatter block, Open Forge keys and every other key alike, with its delimiters and the line ending after the closing `---`, so the file starts with the first body byte. The YAML inside need not be valid. A block without a closing `---` line is `<command>.document-unsupported`. A native `SKILL.md` keeps its Skill keys and loses only Open Forge's, exactly as with `metadata`, because the Skill format needs its frontmatter.

### `entrypoints`

Deletes every entrypoint file in the document set, such as `docs/_docs.md` or `.agents/memory/_memory.md`, managed or not. Entrypoints are the only place generated `Entries` lists live, so this removes the index navigation. Any text the user wrote in an entrypoint goes with it. The loader `.agents/loader.md` is not an entrypoint. It goes as part of uninstall's selection. A folder left empty is removed, deepest first.

### `links`

Removes every link between documents. Turns each Markdown link to a document into its text. A link counts as a link to a document when its destination is local and one of these holds:

- it is only a fragment, such as `#play`, which points into the same document;
- its path resolves inside the workspace to a file whose name ends in `.md`, whether or not that file exists;
- its path resolves to an existing folder, which [folder links](../../../shared-operation-contract.md#authored-local-destinations) read as that folder's entrypoint.

The link's text is the exact source between its brackets, so emphasis and code inside it stay. A document link whose text cannot be read is `<command>.document-unsupported`. Inline, full, collapsed and shortcut reference links all count, and so do links in generated `Entries` lists. When every use of a reference definition is stripped, its definition line goes too, together with a title on the following line. Unused definitions stay.

These stay unchanged:

- external links, meaning any destination with a URI scheme;
- links to other local files, such as images, source files or data files;
- images, autolinks, code spans, fenced and indented code blocks and HTML blocks;
- local destinations that cannot be decoded, which keep their bytes.

### Comment Links

`links` also removes [comment links](../source-references/interface.md#comment-links), `@open-forge` and `@rune` alike, in the documents the plan reads. The part removes the whole comment that holds the marker, such as `<!-- @open-forge ./rules.md explains this -->`. When the comment is alone on its lines, those lines go too. A comment without its closing `-->` is `<command>.document-unsupported`. Comment links in source code stay, because code files are outside the document set.

Without `links`, comment links stay. When `--flatten` moves a document, they are rebased like other kept links.

## `--flatten <folder>`

`--flatten <folder>` moves every document below `<folder>`'s subfolders into `<folder>` itself and keeps each file name. `<folder>` is a workspace-relative path, with an optional leading `./`, to an existing folder at or below one of the inventory roots. `--flatten docs` moves `docs/rules/rules.md` to `docs/rules.md`.

- Flatten is for categories, subcategories and custom routes. Nothing stays managed after uninstall's selection, so an edited managed file it keeps is stripped and moved like any other document.
- Native `SKILL.md` files stay where they are. So do entrypoints, unless `entrypoints` deletes them. Route scaffolds go with the selection first, so their folders can empty.
- Files that are not documents, such as images, stay where they are.
- A folder left empty after the moves is removed, deepest first. `<folder>` itself always stays.
- Kept links in every document are rewritten when their source or target moved, so they still resolve. The new destination is the plain relative path from the document's new folder, with the original fragment. A comment link keeps its form. A root-relative target stays root-relative, and a `./` or `../` target gets a new relative path.
- Links in files outside the document set, such as a root `README.md`, are not rewritten.

Two documents that would land on the same name, or a document that would land on a file that stays, is a collision. Names match without regard to case, so the result works on every platform. The plan reports each collision as its own `<command>.flatten-collision` finding that names the destination and its sources, then stops before any write. It never overwrites a file.

## Results

Both commands use the shared schema-3 report with the command's own name. The JSON `data` holds the complete plan at every detail level:

| Field         | Meaning                                                                                                            |
| ------------- | ------------------------------------------------------------------------------------------------------------------ |
| `mode`        | `dry-run` or `apply`                                                                                               |
| `strip`       | The selected parts in fixed order                                                                                  |
| `flatten`     | The canonical flatten folder, or `null`                                                                            |
| `destination` | `export` only: the destination folder's full path                                                                  |
| `plan`        | Ordered entries with `action`, `path`, optional `to`, `reason`, `parts`, `before` and `after` hashes and `outcome` |
| `kept`        | Paths left in place with their reason                                                                              |
| `collisions`  | Flatten destinations with their source paths                                                                       |
| `verified`    | Whether every applied effect verified                                                                              |

`action` is `delete`, `rewrite`, `move` or `delete-directory`. Each command's Interface says what the actions mean for its output. `reason` says why an entry exists. The shared values are `strip` for a document the parts rewrite or, with `entrypoints`, delete, `flatten` for a moved document and `empty-folder` for a folder the plan leaves empty. [`uninstall`](../../uninstall/interface.md#results) adds the selection's values.

`outcome` is the run's, the same for every entry: `planned` in a preview, `done` once every effect verified, `not-started` when the run stopped before its first effect and `unknown` when it stopped after effects started. `parts` names what changed in a rewrite or move: `metadata`, `frontmatter`, `links`, `relink` for flatten link updates, and `managed-section` for root hosts. A deleted entrypoint's `parts` is `entrypoints`. Text output lists the plan. Full detail adds the hashes. Counts use singular labels for one, such as `1 file moved`.

## Shared Errors

These codes carry the command's prefix, `uninstall.` or `export.`:

| Code                    | Meaning                                                                               |
| ----------------------- | ------------------------------------------------------------------------------------- |
| `invalid-input`         | A part, flatten folder, named edited file or other input is invalid.                  |
| `workspace-unavailable` | The workspace layout or settings cannot be read safely.                               |
| `ownership-unavailable` | The ownership lock cannot be read, so the plan cannot tell what Open Forge installed. |
| `library-attached`      | A Library is attached. Detach it first.                                               |
| `target-unavailable`    | An owned target cannot be read or compared with its payload.                          |
| `edited-kept`           | Information. An edited managed file or section stays.                                 |
| `document-unsupported`  | A document's frontmatter, link or comment cannot be edited without losing meaning.    |
| `flatten-collision`     | Two documents, or a document and a kept file, would share a flattened name.           |
| `linked-file-skipped`   | Information. A symbolic link or junction was left unchanged.                          |
| `interrupted`           | The run was cancelled.                                                                |

Messages follow the shared families:

- Invalid input is `Cannot <command>: <exact problem>.` and names the rejected value.
- `flatten-collision` is `Flattening would place <n> files at <path>: <sources>. Nothing was changed.`

## Eval Stages

The evals compare three setups of the same documents. Each starts from a workspace that has Open Forge installed and the documents routed:

| Stage                | Command                                                                          |
| -------------------- | -------------------------------------------------------------------------------- |
| Organized Open Forge | None. The normal install stays.                                                  |
| Organized docs       | `open-forge export ../stage-4 --strip metadata,entrypoints`                      |
| Unorganized docs     | `open-forge export ../stage-3 --strip metadata,entrypoints,links --flatten docs` |

## Expected Bytes From The Eval Harness

Two exports of a routed workspace with scoped documents and scoped metadata have exact expected outputs. The expected bytes came from the eval harness's own output, which the test fixtures record with their origin:

- `export <destination> --strip metadata --flatten docs` writes the original documents at the `docs` root, byte for byte with their original links, and no Open Forge metadata, entrypoints or state.
- `export <destination> --strip metadata,links --flatten docs` writes the same documents without the links between them.

Where the harness's own stripping code disagrees with Open Forge's rules, this contract follows Open Forge:

- `frontmatter.ts` serializes the remaining YAML again, which turns its line endings into LF and can change quoting. `metadata` keeps the remaining lines byte for byte.
- `frontmatter.ts` reads only the `open-forge:` form. `metadata` also removes root-form keys and a root `applyTo`.
- `frontmatter.ts` rejects duplicate keys and a sequence root. `metadata` removes only Open Forge keys, so it leaves such foreign YAML byte for byte. Like the harness, it refuses malformed YAML, a missing closing `---` line and bytes that are not UTF-8.
- `plain-links.ts` matches links with a regular expression. It strips links inside code spans, turns an image `![alt](x.png)` into `!alt` and strips links to files that are not documents. It toggles fences on any line starting with three backticks or tildes, so a longer or mismatched fence ends a block early, and it treats indented code blocks and HTML blocks as text. It misses reference links, titles, angle-bracket destinations and labels that contain brackets. `links` reads links through the Markdown parser, so code stays code and only document links lose their targets.
- `scopes.ts` rewrites only links whose path uses word characters, dots, slashes and hyphens. Flatten rewrites every kept link whose source or target moved.

## Decisions

The maintainer accepted these:

- `uninstall` and `export` are one plan with two outputs and the same selection flags: `--strip`, `--flatten`, `--remove-edited` and `--dry-run`. The standalone `strip` command is gone.
- `metadata` removes only Open Forge's frontmatter keys and keeps foreign keys.
- `frontmatter` removes the whole frontmatter block. With `metadata` it wins.
- A native `SKILL.md` keeps its normal Skill frontmatter under both `metadata` and `frontmatter`. Only Open Forge's keys go.
- `entrypoints` replaces `entries` with no alias. It deletes the entrypoint files, managed ones included, the only place index links live, together with any user text in them.
- `links` removes every link between documents, including `@open-forge` and `@rune` comment links in the Markdown documents the plan reads, by removing the whole comment. Code files stay outside the document set.
- `--flatten` works in the same run as `entrypoints`.
- The fixed order is `frontmatter` or `metadata`, then `entrypoints`, then `links`, then flatten.
- The plan's own reasons are `strip` and `flatten`. The eval harness is only the source of the expected bytes, not contract vocabulary.

These defaults are recorded for the maintainer to confirm:

- Strip is opt-in. The issue's proposed `uninstall --keep` is not added, because without `--strip` everything stays.
- Fragment-only links count as links to a document, because they point into the current document. This matches the harness.
- Flatten leaves native `SKILL.md` files and entrypoints that `entrypoints` does not delete in place, and never rewrites files outside the document set.
- Flatten collisions compare names without regard to case.
