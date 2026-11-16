---
open-forge:
  description: Select which Open Forge parts to strip from routed documents and where flattened documents go
  responsibility: Define strip's syntax, parts, document set, flatten rules, preview, results, errors and the decisions awaiting the maintainer
  tags: [Memory, Crystallized, CLI, Command, Contract, Strip, Interface, CurrentTruth]
---

# strip Interface Contract

## Purpose

`strip` turns routed documents back into plain Markdown that still holds every word the user wrote. It can remove Open Forge metadata, generated `Entries` and links between documents, and it can move routed documents into one folder. The Framework stays installed. [`uninstall`](../uninstall/interface.md) accepts the same parts through `--strip` and `--flatten`, so one workspace can produce every comparison setup the evals need.

The sibling [Behavior Contract](behavior.md) defines planning, revalidation and recovery. [Issue 19](https://github.com/TheLithiumForge/open-forge/issues/19) records the request.

## Help

The purpose sentence is: Strip Open Forge metadata, generated Entries or links from routed documents, or flatten them into one folder.

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

Each `<part>` is `metadata`, `entries` or `links`. One operand may also list several parts separated by commas, such as `metadata,links`. Duplicates collapse, and the order never matters. The command runs the parts in the fixed order `metadata`, `entries`, `links`. At least one part or `--flatten` is required. An unknown part, an empty list member or a missing operand is `strip.invalid-input`.

`uninstall --strip <parts>` uses the same part grammar. Repeat `--strip` or list several parts in one value.

## Documents

Strip reads every Markdown file below the main root, the native Skills folder and each registered outside route. These are the layout's inventory roots. A file counts when its name ends in `.md`, including overwrite companions. Strip never enters `.open-forge-state/`, Git metadata or a symbolic link. A symbolic link or junction inside a root is skipped and reported as `strip.linked-file-skipped`, so a Library link never rewrites its source. Files outside these roots, such as a root `README.md` or `AGENTS.md`, are not documents and stay unchanged.

A document keeps its encoding, line endings and every byte outside the selected edits.

## Parts

### `metadata`

Removes Open Forge frontmatter keys and keeps every other key and the body bytes exactly.

- With an `open-forge:` key, strip removes that key's lines and a root `applyTo` key, because Open Forge reads `applyTo` in both forms.
- Without an `open-forge:` key, strip removes the root `description`, `tags`, `responsibility` and `applyTo` keys.
- A native `SKILL.md` keeps its frontmatter, because its `name` and `description` belong to the Skill format.

Removal takes whole lines, from the key's line to the last line of its value. When no keys remain, strip removes the whole frontmatter block, its delimiters and the line ending after the closing `---`, so the file starts with the first body byte. Comments inside a frontmatter block without keys go with it. Malformed YAML, a duplicate `open-forge` key or a removable key that shares a line with a kept key is `strip.document-unsupported` and blocks the whole run.

### `entries`

Removes the generated `Entries` section. When the section holds nothing but its heading and the generated list, strip removes the heading and the list. When the user wrote other text in the section, strip removes only the generated list and keeps the heading and that text. When the removed section ends the file, the blank lines before it go too, so the file ends with the previous line's ending. A duplicate or malformed `Entries` section is `strip.document-unsupported`.

### `links`

Turns each Markdown link to a document into its text. A link counts as a link to a document when its destination is local and one of these holds:

- it is only a fragment, such as `#play`, which points into the same document;
- its path resolves inside the workspace to a file whose name ends in `.md`, whether or not that file exists;
- its path resolves to an existing folder, which [folder links](../../shared-operation-contract.md#authored-local-destinations) read as that folder's entrypoint.

The link's text is the exact source between its brackets, so emphasis and code inside it stay. Inline, full, collapsed and shortcut reference links all count. When every use of a reference definition is stripped, its definition line goes too. Unused definitions stay.

These stay unchanged:

- external links, meaning any destination with a URI scheme;
- links to other local files, such as images, source files or data files;
- images, autolinks, code spans, fenced and indented code blocks and HTML blocks;
- local destinations that cannot be decoded, which keep their bytes.

### Comment Links

`--strip links` keeps [comment links](../shared/source-references/interface.md#comment-links) such as `<!-- @open-forge ./rules.md explains this -->` unchanged. A comment link is an explicit annotation the author chose, and removing it would mean either deleting the explanation or leaving a bare path. When `--flatten` moves a document, its comment links are rebased like other kept links.

## `--flatten <folder>`

`--flatten <folder>` moves every document below `<folder>`'s subfolders into `<folder>` itself and keeps each file name. `<folder>` is a workspace-relative path, with an optional leading `./`, to an existing folder at or below one of the inventory roots. `strip --flatten docs` moves `docs/rules/rules.md` to `docs/rules.md`.

- Entrypoints and native `SKILL.md` files stay where they are. [`uninstall`](../uninstall/interface.md#route-scaffolds) removes recognized route scaffolds first, so their folders can empty.
- Files that are not documents, such as images, stay where they are.
- A folder left empty after the moves is removed, deepest first. `<folder>` itself always stays.
- Kept links in every document are rewritten when their source or target moved, so they still resolve. The new destination is the plain relative path from the document's new folder, with the original fragment. A comment link keeps its form. A root-relative target stays root-relative, and a `./` or `../` target gets a new relative path.
- Links in files outside the document set, such as a root `README.md`, are not rewritten.

Two documents that would land on the same name, or a document that would land on a file that stays, is a collision. Names match without regard to case, so the result works on every platform. Strip reports every collision as `strip.flatten-collision` with the destination and its sources, then stops before any write. It never overwrites a file.

## Preview And Consent

`--dry-run` lists every delete, rewrite and move with the SHA-256 of its bytes before and after, and writes nothing. Applying requires plan confirmation or `--automatic`. When the session cannot prompt and `--automatic` is absent, the result is `strip.confirmation-required`. JSON never prompts. A plan with no changes is a verified no-op that takes no lock and writes no recovery data.

## Results

The command is `strip` in the shared schema-3 report. Every plan entry is an effect. A move is two effects, the created destination and the deleted source. The JSON `data` holds the complete plan at every detail level:

| Field        | Meaning                                                                                                   |
| ------------ | --------------------------------------------------------------------------------------------------------- |
| `mode`       | `dry-run` or `apply`                                                                                      |
| `strip`      | The selected parts in fixed order                                                                         |
| `flatten`    | The canonical flatten folder, or `null`                                                                   |
| `plan`       | Ordered entries with `action` (`delete`, `rewrite`, `move` or `delete-directory`), `path`, optional `to`, `reason`, `parts`, `before` and `after` hashes |
| `kept`       | Paths left in place with their reason                                                                     |
| `collisions` | Flatten destinations with their source paths                                                              |
| `verified`   | Whether every applied effect verified                                                                      |

`parts` names what changed in a rewrite or move: `metadata`, `entries`, `links`, `relink` for flatten link updates, and `managed-section` for uninstall's root hosts. Text output lists the effects. Full detail adds the hashes.

## Errors And Findings

| Code                              | Meaning                                                                                  |
| --------------------------------- | ---------------------------------------------------------------------------------------- |
| `strip.invalid-input`             | A part, flatten folder or option is invalid.                                             |
| `strip.workspace-unavailable`     | The workspace layout or settings cannot be read safely.                                  |
| `strip.document-unsupported`      | A document's frontmatter or `Entries` section cannot be edited without losing meaning.   |
| `strip.flatten-collision`         | Two documents, or a document and a kept file, would share a flattened name.              |
| `strip.linked-file-skipped`       | Information. A symbolic link or junction was left unchanged.                             |
| `strip.target-changed`            | A planned file changed before its effect.                                                |
| `strip.workspace-lock-unavailable` | The workspace mutation lease is unavailable.                                            |
| `strip.confirmation-required`     | Applying needs confirmation or `--automatic`.                                            |
| `strip.permission-declined`       | The user declined the plan.                                                              |
| `strip.recovery-unavailable`      | Recovery data could not be prepared. Nothing was changed.                                |
| `strip.write-failed`              | A planned effect failed. The recovery bundle stays.                                      |
| `strip.recovery-artifact-retained` | The changes verified, but the recovery bundle could not be removed.                     |
| `strip.interrupted`               | The run was cancelled.                                                                   |

## Examples

```sh
open-forge strip links --dry-run
open-forge strip metadata,entries --automatic
open-forge strip --flatten docs --dry-run --format json
open-forge strip metadata entries links --flatten docs --automatic
```

## Eval Harness Comparison

The demo-evals harness at `6edb7de66` strips by hand in `src/evals/materialize/`. For the Neon Pairs pack, its scoped-form documents and links give the same bytes as this contract. Where the harness disagrees with Open Forge's own rules, this contract follows Open Forge:

- `frontmatter.ts` serializes the remaining YAML again, which turns its line endings into LF and can change quoting. Strip keeps the remaining lines byte for byte.
- `frontmatter.ts` reads only the `open-forge:` form. Strip also removes root-form keys and a root `applyTo`.
- `plain-links.ts` matches links with a regular expression. It strips links inside code spans, turns an image `![alt](x.png)` into `!alt` and strips links to files that are not documents. It toggles fences on any line starting with three backticks or tildes, so a longer or mismatched fence ends a block early, and it treats indented code blocks and HTML blocks as text. It misses reference links, titles, angle-bracket destinations and labels that contain brackets. Strip reads links through the Markdown parser, so code stays code and only document links lose their targets.
- `scopes.ts` rewrites only links whose path uses word characters, dots, slashes and hyphens. Flatten rewrites every kept link whose source or target moved.

## Decisions To Confirm

These defaults are recorded for the maintainer to confirm:

- `strip` is its own command, and `uninstall` offers the same parts as options.
- Strip is opt-in. The issue's proposed `uninstall --keep` is not added, because without `--strip` everything stays.
- `--strip links` keeps comment links and rebases them on flatten.
- Fragment-only links count as links to a document, because they point into the current document. This matches the harness.
- Flatten leaves entrypoints and native `SKILL.md` files in place and never rewrites files outside the document set.
- Flatten collisions compare names without regard to case.
- Recovery uses the existing workspace removal attribution rather than a new recovery schema value.
