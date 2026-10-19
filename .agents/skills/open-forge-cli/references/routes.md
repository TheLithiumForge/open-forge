---
open-forge:
  description: Maintain routed files, generated Entries, references, and recovery data
  tags: [Skill, CLI, Reference]
---

# Route Maintenance

Use these commands for authorized edits to routes, metadata, generated navigation, references, or recovery data. Preview each change with `--dry-run`, inspect the plan, then apply the same request without it. All commands accept the [shared global flags](common.md#global-flags).

Resolve a reported boundary before retrying. A suggested retry for a blocked Route request retains its targets and options, including metadata and Template choices. Route Init retries also keep the target and choices after an interrupted apply.

Moving or removing a category also maintains direct links to its folder and
nested folders. A missing folder link needs a manual edit, and Repair does not
offer file replacements for it.

## Metadata And Conditions

An `entrypoint` is the Markdown file that makes a folder routable. Use Route Init for missing entrypoints, Create for an ordinary routed file, and Update for an existing source.

| Metadata flag                   | Meaning                                                                                                                           |
| ------------------------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| `--description <text>`          | State why a reader should open the source                                                                                         |
| `--responsibility <text>`       | State what the source defines when that adds a useful boundary. An exact empty value omits or removes the field                   |
| `--tag <tag>`                   | Supply one descriptive tag. Repeat for an ordered list. Empty or duplicate tags are invalid                                       |
| `--apply-to <glob>`             | Declare working-file conditions. Repeat or use comma-separated expressions                                                        |
| `--template <source-reference>` | Copy an existing routed Template's body. Its metadata does not transfer, and the new body has no continuing Template relationship |

Supply description, responsibility, and Template values at most once.

`applyTo` globs match from the workspace root and are case-sensitive. `*.py` matches root files, `**/*.py` any depth, and `docs/**` a folder. A leading `!` is literal, not an exclusion. These conditions affect loading, not edit permission. [Discovery](discovery.md#context-source-reference) explains how `--for` evaluates them.

New metadata and quoted-string condition lists use the workspace's frontmatter form. Existing declarations may sit under `open-forge:` or at the frontmatter root, as a string or list. Both locations must declare equivalent pattern sets when present. List entries are atomic patterns. String expressions split at top-level commas, with `[,]` for a literal comma. Update preserves existing metadata locations. A source without Open Forge metadata receives the workspace's form.

## `route init <route-target>`

Create every missing entrypoint in an exact route chain. Targets are route IDs or exact `.agents` entrypoint paths. A missing exact path uses the canonical `_{folder-name}.md` filename. Init never creates the loader.

| Flag                                                       | Meaning                                                                                             |
| ---------------------------------------------------------- | --------------------------------------------------------------------------------------------------- |
| `--framework`                                              | Use the embedded Framework topology and entrypoint assets instead of the generic draft scaffold     |
| `--description`, `--responsibility`, `--tag`, `--apply-to` | Set metadata only on a missing generic final target. Meanings are [above](#metadata-and-conditions) |
| `--dry-run`                                                | Preview directories, entrypoints, payload, settings, and generated navigation                       |

Metadata flags apply only to a missing generic final entrypoint. They cannot update an existing final target. Do not combine `--framework` with `--description`, `--responsibility`, `--tag`, or `--apply-to`.

For an exact unscoped Core category or standard Memory state, `--framework` restores eligible missing packaged files and required missing ancestors. Other targets create a sparse scoped chain. Restoration requires an installed safe loader, preserves authored occupants, and clears only the exact selection's exclusion. Add a broader excluded ancestor explicitly first. Resolve an orphan overwrite that blocks safe navigation before retrying.

To restore omitted Patterns or Skills, preview the relevant request:

```sh
open-forge route init patterns --framework --dry-run
open-forge route init skills --framework --dry-run
```

## `route create <file-target>`

Create an ordinary Markdown file beneath an existing routable root. Missing intermediate directories and entrypoints can be created below that root. Unknown roots are refused.

Accepts `--description`, `--responsibility`, repeated `--tag`, repeated `--apply-to`, `--template`, and `--dry-run`. Use the [metadata meanings](#metadata-and-conditions). Omitted description or tags produce a warning to complete them. Create does not overwrite differing content or establish lifecycle ownership.

Planning supplies the Decision category and Template. Essentials leaves Templates out. Before installing Planning or `development-toolkit` for its Templates, preview `open-forge route init templates --framework --dry-run`, review it, then run `open-forge route init templates --framework`. After installation, continue when `open-forge find --tag Template` lists `templates/planning/decision`. If Planning is already installed without its starters, use the [restore and update procedure](packages.md#extension-install-stable-id).

For example, preview a new decision record using that available Template:

```sh
open-forge route create memory/crystallized/decisions/api-choice --description "Accepted API choice" --tag Decision --tag API --template templates/planning/decision --dry-run
```

## `route update <source-reference>`

Patch one existing source's selected metadata or complete its eligible empty body. Supply at least one operation.

Accepts `--description`, `--responsibility`, repeated `--tag`, repeated `--apply-to`, `--template`, `--clear-apply-to`, and `--dry-run`.

Repeated `--tag` replaces the complete ordered tag list, and `--apply-to` replaces the complete local condition list. `--clear-apply-to` removes local declarations from both supported locations and cannot be combined with `--apply-to`. An exact empty responsibility removes the field, while whitespace-only text is invalid.

`--template` fills an empty or whitespace-only body. Existing authored bodies stay intact with a warning. Source identity, document form, and overwrite ownership remain intact.

## `route move <source-reference> <destination-target>`

Relocate an unmanaged routed leaf or complete category, updating affected references and generated navigation. The destination is an exact workspace-relative `.agents` path under an existing routable parent and must match the source's leaf or category form.

Its only command flag is `--dry-run`. Inspect the move and all reference changes. Move does not relocate lifecycle-managed content, create a missing parent route, or overwrite a destination.

## `route remove <source-reference>`

Remove one routed file or category, record the persistent exclusion, release selected claims, detach references, and update navigation.

Eligible Framework- and Extension-managed content can be removed, including edited files. A category selection removes its whole routed category, including native and binary resources. The loader and `.agents` root stay protected. A native Skill or non-Markdown resource is not a removable routed leaf. Links, incomplete scans, unsafe references, and nested Git repositories block category removal.

- `--dry-run`: inspect dependents, deletions, exclusions, and ownership changes.
- `--automatic`: apply the reviewed plan without confirmation.

## `index [<source-reference>...]`

Index encodes local link paths, such as `old%20guide.md`.
Doctor also accepts readable paths: `<old guide.md>` and `café(one).md`.
Escape an entity-opening ampersand as `%26`: write `entity&amp;.md` as
`entity%26amp;.md`. Keep `#`, `%` and `?` in names escaped.
Repair writes Index's form when
correcting other spellings. Command arguments use literal source paths.
Images with invalid heading fragments need a manual edit.

Complete missing metadata and navigation after manual edits. You may create user-owned Markdown files directly inside routed folders without writing frontmatter or Entries. Index adds missing descriptions from the first heading, otherwise the file name, and the category's tags. It keeps bodies and supplied values. For deep notes below an existing root, it creates every missing entrypoint with metadata and direct-child Entries. Refine the prefilled values later if needed.

An entrypoint selects its subtree and direct exposing parent. A leaf includes its missing chain and direct exposing parent. No operands selects the loader and every eligible rooted entrypoint region, including missing chains. Index removes old entries and adds new ones after renames or moves. When Git retains the old file's committed content, Index and Repair work in either order. Without Git, run Repair first while old Entries still record the file's metadata.

Its only command flag is `--dry-run`. Preview a narrow rebuild before applying it:

```sh
open-forge index memory/crystallized --dry-run
open-forge index memory/crystallized
```

For an Install Git-ignore route, Index uses the policy recorded in `.agents/open-forge.lock.json` to omit private child files and nested private entrypoints from shareable `Entries`. It still maintains the whitelisted route entrypoint and its parent link. This keeps the route available to the team without publishing local private navigation. See [Install setup](packages.md#install) for how the route is recorded before its contents are ignored.

Context, Find, and References can still read local private files when selected. Ignore rules do not change their loading authority. Index rewrites the unique `## Entries` interior and removes retired generated guard comments there. Metadata completion preserves authored bytes outside inserted fields. Native Skill packages, overwrites, managed authored content, excluded paths, and recorded private contents are not completed. Index never invents a root or clears an exclusion. Invalid frontmatter stays untouched with an edit instruction, while independent valid files are indexed.

## `repair`

Correct supported local-link issues or recover an incomplete Library operation when current evidence proves the correction safe.

| Flag                                                              | Meaning                                                                                                 |
| ----------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------- |
| `--dry-run`                                                       | Preview the complete selected repair plan                                                               |
| `--automatic`                                                     | Select currently verified safe corrections. Guided choices remain unselected                            |
| `--relink <source-location> <expected-destination> <target-path>` | Select one exact current reference occurrence and its contained destination. Repeat for more selections |

Use Doctor to choose a reported target. A relink source location has the form `path@line:column`, with one-based numbers at the start of the link destination. Doctor's displayed location points to the link and cannot be copied directly as a relink location. Inspect the current source, or use Repair's reported destination location, before constructing the request. The expected destination is the exact current authored value, and the target must be an admitted reported candidate.

For `[Rule](../working/order-rule.md)` at the start of line 7, the destination begins at column 8:

```sh
open-forge repair --relink \
  ".agents/memory/crystallized/repair-note.md@7:8" \
  "../working/order-rule.md" ".agents/memory/crystallized/order-rule.md" \
  --dry-run
```

Automatic repairs can correct spelling, case, encoding, or a unique fragment while preserving the target. Explicit relinking chooses a different reported destination. Library recovery needs matching current recovery evidence, link identity, and current destination permission. It never follows or changes source targets or widens saved grants.

You may rename or move user-owned files directly, then preview `open-forge repair --automatic --dry-run` and apply with `open-forge repair --automatic`. Repair relinks a missing target when exactly one successor is proven by identical or near-identical committed content, or unique recorded metadata. It also completes eligible metadata and missing entrypoints and refreshes affected old and new Entries in that plan. When Git retains the old file's committed content, Index and Repair work in either order. Without Git, run Repair first while the stale Entries still record the file's metadata.

Missing proof or several matching files need interactive `open-forge repair` or an explicit relink. When Repair cannot ask, choose the intended target from its list and copy that target's `--relink ... --automatic` command. Preview it with `--dry-run` before applying. A filename or title match alone is a choice. A deleted file with no replacement leaves an authored link to edit or remove. Follow the reported next step, using Index for unrelated new notes, then Doctor to verify no warnings or errors remain.

Interactive Repair explains possible targets and offers `Leave this link unchanged`. Leaving broken links unchanged keeps them in the result. Library recovery offers one step for this run or leaves all recovery steps out. Its recovery counts are separate from rewritten-link counts.

Repair does not decide missing content or delete recovery artifacts. It maintains navigation only in the routes affected by selected corrections.

Repair preserves managed Framework and Extension files, native Skill packages and resources, overwrites, excluded or private contents, and unsafe sources or targets. Follow their managing Update or exact edit action. Each candidate appears once under its link with its strongest reason and copyable command. Partial results state applied repairs and remaining choices or edits. Continuations retain the explicit workspace even inside prose. A Git timeout retries the complete request before manual link edits.

## `cleanup`

Delete every recognized completed recovery bundle and draft for the selected workspace after the user has checked the changes. Its only command flag is `--dry-run`, which previews without locking or writing. Apply holds the workspace lock and rechecks each exact deletion.

Unknown, malformed, unsupported, unavailable, and unsafe artifacts remain preserved. Cleanup has no selectors, age or glob filters, force mode, or confirmation prompt. Use `status` to review recovery candidates before the preview.

Use [discovery](discovery.md) to select sources and [Framework and packages](packages.md) for installation or package management.
