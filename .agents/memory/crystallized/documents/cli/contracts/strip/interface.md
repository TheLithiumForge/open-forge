---
open-forge:
  description: Select which Open Forge parts to strip from routed documents and where flattened documents go
  responsibility: Define strip's syntax, parts, document set, flatten rules, preview, results, errors and the decisions awaiting the maintainer
  tags: [Memory, Crystallized, CLI, Command, Contract, Strip, Interface, CurrentTruth]
---

# strip Interface Contract

## Purpose

`strip` turns routed documents back into plain Markdown. It can remove Open Forge metadata or whole frontmatter blocks, delete entrypoints and remove links between documents, and it can move routed documents into one folder. The Framework stays installed. [`uninstall`](../uninstall/interface.md) accepts the same parts through `--strip` and `--flatten`, so one workspace can produce every comparison setup the evals need.

The sibling [Behavior Contract](behavior.md) defines planning, revalidation and recovery. [Issue 19](https://github.com/TheLithiumForge/open-forge/issues/19) records the request.

## Help

The purpose sentence is: Strip Open Forge metadata, frontmatter, entrypoints or links from routed documents, or flatten them into one folder.

```text
open-forge strip [<part>...] [--flatten <folder>] [--dry-run] [--automatic]
```

Follow the [shared help template](../shared/global-flags/interface.md#--help).

## Syntax

```text
open-forge strip [<part>...]
  [--flatten <folder>]
  [--dry-run] [--automatic]
  [global flags]
```

Each `<part>` is `metadata`, `frontmatter`, `entrypoints` or `links`. One operand may also list several parts separated by commas, such as `metadata,links`. Duplicates collapse, and the order never matters. The command runs the parts in the fixed order `frontmatter` or `metadata`, then `entrypoints`, then `links`, then flatten. With both `frontmatter` and `metadata`, `frontmatter` wins and `metadata` adds nothing. At least one part or `--flatten` is required. An unknown part, an empty list member or a missing operand is `strip.invalid-input`.

`uninstall --strip <parts>` uses the same part grammar. Repeat `--strip` or list several parts in one value.

## Documents

Strip reads every Markdown file below the main root, the native Skills folder and each registered outside route. These are the layout's inventory roots. A file counts when its name ends in `.md`, including overwrite companions. Strip never enters `.open-forge-state/`, Git metadata or a symbolic link. A symbolic link or junction inside a root is skipped and reported as `strip.linked-file-skipped`, so a Library link never rewrites its source. Files outside these roots, such as a root `README.md` or `AGENTS.md`, are not documents and stay unchanged.

Strip leaves the files Open Forge manages unchanged, meaning the owned Framework and Extension files, so the installed Framework keeps working. The one exception is `entrypoints`, which deletes managed entrypoints too. `uninstall` strips the edited managed files it keeps, because Open Forge no longer reads them.

Only Markdown documents are read. Source code and other files outside this set stay untouched by every part, including the comment links they hold.

A document keeps its encoding, line endings and every byte outside the selected edits.

## Parts

### `metadata`

Removes Open Forge frontmatter keys and keeps every other key and the body bytes exactly.

- With an `open-forge:` key, strip removes that key's lines and a root `applyTo` key, because Open Forge reads `applyTo` in both forms.
- Without an `open-forge:` key, strip removes the root `description`, `tags`, `responsibility` and `applyTo` keys.
- A native `SKILL.md` keeps its frontmatter, because its `name` and `description` belong to the Skill format.

Removal takes whole lines, from the key's line to the last line of its value. When no keys remain, strip removes the whole frontmatter block, its delimiters and the line ending after the closing `---`, so the file starts with the first body byte. Comments inside a frontmatter block without keys go with it. Malformed YAML, a duplicate `open-forge` key or a removable key that shares a line with a kept key is `strip.document-unsupported` and blocks the whole run.

### `frontmatter`

Removes the whole frontmatter block, Open Forge keys and every other key alike, with its delimiters and the line ending after the closing `---`, so the file starts with the first body byte. The YAML inside need not be valid. A native `SKILL.md` keeps its frontmatter, as with `metadata`. A block without a closing `---` line is `strip.document-unsupported`.

### `entrypoints`

Deletes every entrypoint file in the document set, such as `docs/_docs.md` or `.agents/memory/_memory.md`. Entrypoints are the only place generated `Entries` lists live, so this removes the index navigation. Any text the user wrote in an entrypoint goes with it. Unlike the other parts, it also deletes managed entrypoints. The loader `.agents/loader.md` is not an entrypoint and stays. A folder left empty is removed, deepest first.

### `links`

Removes every link between documents. Turns each Markdown link to a document into its text. A link counts as a link to a document when its destination is local and one of these holds:

- it is only a fragment, such as `#play`, which points into the same document;
- its path resolves inside the workspace to a file whose name ends in `.md`, whether or not that file exists;
- its path resolves to an existing folder, which [folder links](../../shared-operation-contract.md#authored-local-destinations) read as that folder's entrypoint.

The link's text is the exact source between its brackets, so emphasis and code inside it stay. A document link whose text cannot be read is `strip.document-unsupported`. Inline, full, collapsed and shortcut reference links all count. When every use of a reference definition is stripped, its definition line goes too, together with a title on the following line. Unused definitions stay.

These stay unchanged:

- external links, meaning any destination with a URI scheme;
- links to other local files, such as images, source files or data files;
- images, autolinks, code spans, fenced and indented code blocks and HTML blocks;
- local destinations that cannot be decoded, which keep their bytes.

### Comment Links

`links` also removes [comment links](../shared/source-references/interface.md#comment-links), `@open-forge` and `@rune` alike, in the Markdown documents strip reads. Strip removes the whole comment that holds the marker, such as `<!-- @open-forge ./rules.md explains this -->`. When the comment is alone on its lines, those lines go too. A comment without its closing `-->` is `strip.document-unsupported`. Comment links in source code stay, because code files are outside strip's document set.

Without `links`, comment links stay. When `--flatten` moves a document, they are rebased like other kept links.

## `--flatten <folder>`

`--flatten <folder>` moves every document below `<folder>`'s subfolders into `<folder>` itself and keeps each file name. `<folder>` is a workspace-relative path, with an optional leading `./`, to an existing folder at or below one of the inventory roots. `strip --flatten docs` moves `docs/rules/rules.md` to `docs/rules.md`.

- Flatten is for categories, subcategories and custom routes, whose files the user owns. `strip --flatten` refuses a folder that holds any file Open Forge manages, entrypoints and native `SKILL.md` files included, as `strip.flatten-managed`, and nothing is written. Strip never edits a managed file, so flattening around one would leave Open Forge's own navigation pointing at moved documents. Entrypoints the same run deletes with `entrypoints` never cause this refusal. `uninstall --flatten` never refuses this way: nothing stays managed after it, so an edited managed file it keeps is stripped and moved like any other document.
- Native `SKILL.md` files stay where they are. So do entrypoints, unless `entrypoints` deletes them. [`uninstall`](../uninstall/interface.md#route-scaffolds) removes recognized route scaffolds first, so their folders can empty.
- Files that are not documents, such as images, stay where they are.
- A folder left empty after the moves is removed, deepest first. `<folder>` itself always stays.
- Kept links in every document are rewritten when their source or target moved, so they still resolve. The new destination is the plain relative path from the document's new folder, with the original fragment. A comment link keeps its form. A root-relative target stays root-relative, and a `./` or `../` target gets a new relative path.
- Links in files outside the document set, such as a root `README.md`, are not rewritten.

Two documents that would land on the same name, or a document that would land on a file that stays, is a collision. Names match without regard to case, so the result works on every platform. Strip reports each collision as its own `strip.flatten-collision` finding that names the destination and its sources, then stops before any write. It never overwrites a file.

## Preview And Consent

`--dry-run` lists every delete, rewrite and move with the SHA-256 of its bytes before and after, and writes nothing. Applying requires plan confirmation or `--automatic`. When the session cannot prompt and `--automatic` is absent, the result is `strip.confirmation-required`. JSON and redirected output never prompt. A plan with no changes is a verified no-op that takes no lock, writes no recovery data and reports `Nothing to do.`

## Results

The command is `strip` in the shared schema-3 report. Every plan entry is an effect. A move is two effects, the created destination and the deleted source. The JSON `data` holds the complete plan at every detail level:

| Field        | Meaning                                                                                                                                                                |
| ------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `mode`       | `dry-run` or `apply`                                                                                                                                                   |
| `strip`      | The selected parts in fixed order                                                                                                                                      |
| `flatten`    | The canonical flatten folder, or `null`                                                                                                                                |
| `plan`       | Ordered entries with `action` (`delete`, `rewrite`, `move` or `delete-directory`), `path`, optional `to`, `reason`, `parts`, `before` and `after` hashes and `outcome` |
| `kept`       | Paths left in place with their reason                                                                                                                                  |
| `collisions` | Flatten destinations with their source paths                                                                                                                           |
| `verified`   | Whether every applied effect verified                                                                                                                                  |

`reason` says why an entry exists. Strip's own values are `strip` for a document the parts rewrite in place or, with `entrypoints`, delete, `flatten` for a moved document and `empty-folder` for a folder the plan leaves empty. [`uninstall`](../uninstall/interface.md#results) adds its own values.

`outcome` is the run's, the same for every entry: `planned` in a preview, `done` once every effect verified, `not-started` when the run stopped before its first effect and `unknown` when it stopped after effects started. A stopped run's finding says how many changes it made, and its recovery bundle holds the prior bytes. `parts` names what changed in a rewrite or move: `metadata`, `frontmatter`, `links`, `relink` for flatten link updates, and `managed-section` for uninstall's root hosts. A deleted entrypoint's `parts` is `entrypoints`. Text output lists the effects. Full detail adds the hashes. Counts use singular labels for one, such as `1 file moved`.

## Errors And Boundaries

| Code                                | Meaning                                                                                                                                                                      |
| ----------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `strip.invalid-input`               | A part, flatten folder or option is invalid.                                                                                                                                 |
| `strip.workspace-unavailable`       | The workspace layout, settings or ownership lock cannot be read safely.                                                                                                      |
| `strip.document-unsupported`        | A document's frontmatter, link or comment cannot be edited without losing meaning.                                                                                           |
| `strip.flatten-collision`           | Two documents, or a document and a kept file, would share a flattened name.                                                                                                  |
| `strip.flatten-managed`             | `strip --flatten` was given a folder that holds a file Open Forge manages and the run keeps, entrypoints and native `SKILL.md` files included. `uninstall` never reports it. |
| `strip.linked-file-skipped`         | Information. A symbolic link or junction was left unchanged.                                                                                                                 |
| `strip.target-changed`              | A planned file changed before the first effect. Nothing was changed.                                                                                                         |
| `strip.target-changed-during-apply` | A planned file changed after effects started. The run stopped part way.                                                                                                      |
| `strip.workspace-lock-unavailable`  | The workspace mutation lease is unavailable.                                                                                                                                 |
| `strip.confirmation-required`       | Applying needs confirmation or `--automatic`.                                                                                                                                |
| `strip.permission-declined`         | The user declined the plan.                                                                                                                                                  |
| `strip.recovery-unavailable`        | Recovery data could not be prepared. Nothing was changed.                                                                                                                    |
| `strip.write-failed`                | A planned effect failed. The recovery bundle stays, and the next action is `doctor`.                                                                                         |
| `strip.recovery-artifact-retained`  | The changes verified, but the recovery bundle could not be removed.                                                                                                          |
| `strip.interrupted`                 | The run was cancelled.                                                                                                                                                       |

Messages follow the shared families, with `strip` or `uninstall` as the command:

- Invalid input is `Cannot <command>: <exact problem>.` and names the rejected value.
- A missing confirmation is `<Command> needs confirmation, and this session cannot ask.`
- `flatten-collision` is `Flattening would place <n> files at <path>: <sources>. Nothing was changed.`
- `flatten-managed` is `<folder> holds <path>, which Open Forge manages. Flatten a category, subcategory or custom route instead. Nothing was changed.`
- `target-changed` is `<path> changed after the plan was made. Nothing was changed.`
- `target-changed-during-apply` is `<path> changed while changes were being written. Stopped after <n> of <m> changes.` with the `doctor` next action.
- An interruption after the first effect ends with `Stopped after <n> of <m> changes.`

## Examples

```sh
open-forge strip links --dry-run
open-forge strip metadata,entrypoints --automatic
open-forge strip --flatten docs --dry-run --format json
open-forge strip frontmatter entrypoints links --flatten docs --automatic
```

## Eval Stages

The evals compare three setups of the same documents. Each is one command on a workspace that has Open Forge installed and the documents routed:

| Stage                | Command                                                                  |
| -------------------- | ------------------------------------------------------------------------ |
| Organized Open Forge | None. The normal install stays.                                          |
| Organized docs       | `open-forge uninstall --strip metadata,entrypoints`                      |
| Unorganized docs     | `open-forge uninstall --strip metadata,entrypoints,links --flatten docs` |

## Expected Bytes From The Eval Harness

Two uninstall runs over a routed workspace with scoped documents and scoped metadata have exact expected outputs. The expected bytes came from the eval harness's own output, which the test fixtures record with their origin:

- `uninstall --strip metadata --flatten docs` leaves the original documents at the `docs` root, byte for byte with their original links, and no Open Forge metadata, entrypoints or state.
- `uninstall --strip metadata,links --flatten docs` leaves the same documents without the links between them.

Where the harness's own stripping code disagrees with Open Forge's rules, this contract follows Open Forge:

- `frontmatter.ts` serializes the remaining YAML again, which turns its line endings into LF and can change quoting. Strip keeps the remaining lines byte for byte.
- `frontmatter.ts` reads only the `open-forge:` form. Strip also removes root-form keys and a root `applyTo`.
- `frontmatter.ts` rejects duplicate keys and a sequence root. Strip removes only Open Forge keys, so it leaves such foreign YAML byte for byte. Like the harness, it refuses malformed YAML, a missing closing `---` line and bytes that are not UTF-8.
- `plain-links.ts` matches links with a regular expression. It strips links inside code spans, turns an image `![alt](x.png)` into `!alt` and strips links to files that are not documents. It toggles fences on any line starting with three backticks or tildes, so a longer or mismatched fence ends a block early, and it treats indented code blocks and HTML blocks as text. It misses reference links, titles, angle-bracket destinations and labels that contain brackets. Strip reads links through the Markdown parser, so code stays code and only document links lose their targets.
- `scopes.ts` rewrites only links whose path uses word characters, dots, slashes and hyphens. Flatten rewrites every kept link whose source or target moved.

## Decisions

The maintainer accepted that `strip` is its own command and that `uninstall` offers the same parts as options. The maintainer also accepted that strip's own plan reasons are `strip` and `flatten`, that `--flatten` is for categories, subcategories and custom routes, that `strip --flatten` refuses a folder holding any managed file, entrypoints included, while `uninstall --flatten` never does, and that the eval harness is only the source of the expected bytes, not contract vocabulary.

The maintainer accepted the part set:

- `metadata` removes only Open Forge's frontmatter keys and keeps foreign keys.
- `frontmatter` removes the whole frontmatter block. With `metadata` it wins.
- `entrypoints` replaces `entries` with no alias. It deletes the entrypoint files, the only place index links live, together with any user text in them.
- `links` removes every link between documents, including `@open-forge` and `@rune` comment links in the Markdown documents strip reads, by removing the whole comment. Code files stay outside the document set.
- `--flatten` works in the same run as `entrypoints`. Entrypoints the plan deletes no longer make it refuse. Native `SKILL.md` files and other managed files still do.
- The fixed order is `frontmatter` or `metadata`, then `entrypoints`, then `links`, then flatten.

These defaults are recorded for the maintainer to confirm:

- Strip is opt-in. The issue's proposed `uninstall --keep` is not added, because without `--strip` everything stays.
- `strip` leaves managed Framework and Extension files unchanged, so the Framework keeps working, except that `entrypoints` deletes managed entrypoints too. `uninstall` strips the edited managed files it keeps.
- `frontmatter` keeps a native `SKILL.md` file's frontmatter, as `metadata` does, because the Skill format needs it.
- Fragment-only links count as links to a document, because they point into the current document. This matches the harness.
- Flatten leaves native `SKILL.md` files and entrypoints that `entrypoints` does not delete in place, and never rewrites files outside the document set.
- Flatten collisions compare names without regard to case.
- Recovery uses the existing workspace removal attribution rather than a new recovery schema value.
