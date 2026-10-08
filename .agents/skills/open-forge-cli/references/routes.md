---
open-forge:
  description: Maintain routed files, generated Entries, references, and recovery data
  tags: [Skill, CLI, Reference]
---

# Route Maintenance

Use these commands for authorized edits to routes, metadata, generated navigation, references, or recovery data. Preview each change with `--dry-run`, inspect the plan, then apply the same request without it. All commands accept the [shared global flags](common.md#global-flags).

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

Metadata flags cannot update an existing final target. Do not combine `--apply-to` with `--framework`.

For an exact unscoped Core category or standard Memory state, `--framework` restores eligible missing packaged files and required missing ancestors. Other targets create a sparse scoped chain. Restoration requires an installed safe loader, preserves authored occupants, and clears only the exact selection's exclusion. Add a broader excluded ancestor explicitly first. Resolve an orphan overwrite that blocks safe navigation before retrying.

To restore omitted Patterns or Skills, preview the relevant request:

```sh
open-forge route init patterns --framework --dry-run
open-forge route init skills --framework --dry-run
```

## `route create <file-target>`

Create an ordinary Markdown file beneath an existing routable root. Missing intermediate directories and entrypoints can be created below that root. Unknown roots are refused.

Accepts `--description`, `--responsibility`, repeated `--tag`, repeated `--apply-to`, `--template`, and `--dry-run`. Use the [metadata meanings](#metadata-and-conditions). Omitted description or tags produce a warning to complete them. Create does not overwrite differing content or establish lifecycle ownership.

For example, preview a new decision record using a Template available in the workspace:

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

- `--dry-run`: inspect dependents, deletions, exclusions, and ownership changes.
- `--automatic`: apply the reviewed plan without confirmation.

## `index [<source-reference>...]`

Rebuild generated `Entries` after manual additions, renames, or metadata changes. An entrypoint selects its subtree and direct exposing parent. A leaf selects its direct exposing parent. No operands selects the loader and every reachable entrypoint region.

Its only command flag is `--dry-run`. Preview a narrow rebuild before applying it:

```sh
open-forge index memory/crystallized --dry-run
open-forge index memory/crystallized
```

For an Install Git-ignore route, Index uses the policy recorded in `.agents/open-forge.lock.json` to omit private child files and nested private entrypoints from shareable `Entries`. It still maintains the whitelisted route entrypoint and its parent link. This keeps the route available to the team without publishing local private navigation. See [Install setup](packages.md#install) for how the route is recorded before its contents are ignored.

Context, Find, and References can still read local private files when selected. Ignore rules do not change their loading authority. Index rewrites only the unique `## Entries` interior and removes retired generated guard comments there. It does not format whole files or modify overwrite companions.

## `repair`

Correct supported local-link issues or recover an incomplete Library operation when current evidence proves the correction safe.

| Flag                                                              | Meaning                                                                                                 |
| ----------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------- |
| `--dry-run`                                                       | Preview the complete selected repair plan                                                               |
| `--automatic`                                                     | Select currently verified safe corrections. Guided choices remain unselected                            |
| `--relink <source-location> <expected-destination> <target-path>` | Select one exact current reference occurrence and its contained destination. Repeat for more selections |

Use Doctor's occurrence coordinates and possible target when selecting a relink. Automatic repairs can correct spelling, case, encoding, or a unique fragment while preserving the target. Explicit relinking chooses a different reported destination. Library recovery needs matching current recovery evidence, link identity, and current destination permission. It never follows or changes source targets or widens saved grants.

Repair does not author content, rebuild navigation, or delete recovery artifacts.

## `cleanup`

Delete every recognized completed recovery bundle and draft for the selected workspace after the user has checked the changes. Its only command flag is `--dry-run`, which previews without locking or writing. Apply holds the workspace lock and rechecks each exact deletion.

Unknown, malformed, unsupported, unavailable, and unsafe artifacts remain preserved. Cleanup has no selectors, age or glob filters, force mode, or confirmation prompt. Use `status` to review recovery candidates before the preview.

Use [discovery](discovery.md) to select sources and [Framework and packages](packages.md) for installation or package management.
