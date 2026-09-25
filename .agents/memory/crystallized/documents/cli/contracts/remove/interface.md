---
open-forge:
  description: Select one removal target and keep files, directories, packages or libraries removed until explicitly restored
  tags: [Memory, Document, CLI, Contract, Remove, Interface, CurrentTruth]
---

# remove Interface Contract

## Purpose

`remove` removes one selected thing from the workspace and records the choice in
`.agents/open-forge.json`. Open Forge's install, update and library synchronization
operations respect that choice. Other tools and direct file edits are outside this
guarantee.

This contract defines the root interface and persistent removal meaning. Existing
[`route remove`](../route/remove/_remove.md),
[`extension remove`](../extension/remove/_remove.md) and
[`library detach`](../library/detach/_detach.md) remain available and use the same
removal settings. Their target-specific behavior still applies where this contract
does not extend it. [Task 50](../../../../../archived/cli-development/tasks/task50-unified-remove.md)
records implementation and verification state.

## Syntax

```text
open-forge remove <target>
  [--kind <path|route|extension|library>]
  [--dry-run] [--automatic] [--allow-path <path>]
  [global flags]
```

Exactly one target is required. `--kind` defaults to `path`; it identifies the
target's namespace rather than changing the remove operation. Repeated Boolean
flags are idempotent. Repeat `--allow-path` for multiple explicit path grants;
it keeps the [shared permission meaning](../shared/workspace-permissions/interface.md).
The [global flags](../shared/global-flags/interface.md) apply unchanged.

| Kind        | Target                                     | Removal boundary                                                                                                             |
| ----------- | ------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------- |
| `path`      | Exact workspace-relative file or directory | The selected file or complete directory tree; known managed files do not require the caller to name their manager            |
| `route`     | Existing source-reference grammar          | One logical leaf, including its adjacent overwrite, or one complete category                                                 |
| `extension` | Stable Extension ID                        | One package, with shared files retained and retained dependents checked; an uninstalled ID can still be excluded             |
| `library`   | Stable local Library ID                    | One registration and its owned destination links; source files remain untouched and an unregistered ID can still be excluded |

Path input uses `/` and may begin with `./`. Rooted paths, traversal, wildcards
and the workspace root itself (`.` or `./`) are invalid targets. A bare name is a relative path unless `--kind` selects another
namespace. The command does not guess between file, package and Library IDs.

A routed Markdown leaf uses route-aware removal, including its overwrite,
supported incoming authored links and affected generated navigation. A category
directory selects its complete tree. An entrypoint **file** supplied as a path
does not silently select that tree: the command reports the category directory
or explicit route selection needed for category removal.

Ordinary files may contain any bytes. Ordinary directory removal includes support
files and native Skill files. It updates affected generated navigation but leaves
ordinary authored links unchanged. Individual known Library links can be removed
without detaching the registration; synchronization then respects the destination
file exclusion.

## Preview And Consent

`--dry-run` reports the selected target, exclusions, content and navigation effects,
and ownership changes without writing settings, files, locks or recovery state.
Applying requires the normal plan confirmation or explicit `--automatic` consent.
JSON never prompts. Detail and format options do not change deletion authority.

There is no force, recursive, batch, saved-plan or rollback mode. A directory is
already an explicit complete-tree selection. Protected workspace controls, Git
metadata, library source trees, unsafe links and uncertain required ownership
cannot be bypassed by automatic mode or a permission grant.

## Keep Removed And Restore

The authored settings contain optional, duplicate-free lists:

| Setting              | Meaning                                                             |
| -------------------- | ------------------------------------------------------------------- |
| `removedCategories`  | Root category names such as `guidance`, covering `.agents/guidance` |
| `removedFiles`       | Exact canonical workspace-relative file destinations                |
| `removedDirectories` | Canonical directories and every descendant, including future files  |
| `removedExtensions`  | Stable package IDs, including packages requested as dependencies    |
| `removedLibraries`   | Stable local Library IDs                                            |

Paths have no trailing slash, use ordinal identity and contain no wildcard or
traversal notation. Removal records established canonical spelling rather than a
physical alias. Unknown settings properties are preserved. JSON comments need not
survive the existing settings round trip.

Framework install/update, Extension install/update and Library attach/sync honor
the same path exclusions. Existing excluded files remain untouched. An explicitly
selected excluded package or Library is blocked; a required excluded dependency
blocks installation of its parent. Bulk updates skip excluded selections. Force,
prune and automatic mode do not clear exclusions. A required missing excluded
ancestor is reported instead of recreated.

To restore managed content, remove the corresponding entry from `.agents/open-forge.json`
and run the appropriate install, update, attach or sync command. Use Update to
restore a removed Core category in an installed Framework. Use Library Sync
to restore individually removed links when their registration remains. Use
Library Attach when the registration itself was removed. When several exclusions
cover a destination, clear every applicable exclusion. Merely choosing an excluded
package or using force does not express that restoration choice. Direct user edits
remain possible at all times. Ordinary user files must be recreated from their
own source or backup; clearing an exclusion does not recover deleted bytes.

Deleting a file by hand does not automatically add an exclusion. Use `remove`
when Open Forge needs to remember that the absence is intentional.

## Results

Root invocations use command `remove` in the shared schema-3 report. Existing
[statuses, exits, streams and detail rules](../shared/result-coordinates/interface.md)
apply. Target-specific data, findings, effects and recovery facts remain concrete;
route, package and Library cases retain their respective data shapes and finding
codes. Ordinary path selection and removal use the root command's own data.

Settings and ownership effects are distinct from deleted user files. Reports never
count those bookkeeping writes as additional deleted target files. A repeated
removal of a proven absent, already excluded target is an effect-free no-op. A
failure after effects reports what actually changed and where recovery evidence
remains; it does not claim the operation was undone.

## Errors And Boundaries

Domain selections retain their Route, Extension or Library finding codes. The
ordinary path operation uses these codes:

| Code | Meaning |
| --- | --- |
| `remove.invalid-input` | The target or kind is invalid. |
| `remove.protected-path` | The selection includes protected workspace state or a registered Library source. |
| `remove.entry-point-requires-route` | A file operand selects a category entrypoint. Select the category directory or use `--kind route`. |
| `remove.target-unavailable` | The selected target cannot be observed. |
| `remove.target-unsafe` | The target's physical identity, spelling or link kind is unsafe for this request. |
| `remove.target-changed` | An observed target changed before its planned effect. |
| `remove.navigation-unavailable` | Affected generated navigation cannot be safely planned. |
| `remove.settings-unavailable` | Required removal settings cannot be interpreted or safely observed. |
| `remove.ownership-unavailable` | Required ownership cannot be interpreted or safely observed. |
| `remove.workspace-lock-unavailable` | The workspace mutation lease is unavailable. |
| `remove.confirmation-required` | Applying the plan requires confirmation or `--automatic`. |
| `remove.permission-declined` | The user declined application. |
| `remove.recovery-unavailable` | Required recovery evidence could not be prepared. |
| `remove.write-failed` | A planned filesystem or settings effect failed. |
| `remove.interrupted` | Execution was cancelled. |

## Examples

```sh
open-forge remove .agents/guidance/old-note.md --dry-run
open-forge remove .agents/templates --automatic
open-forge remove docs/obsolete.txt --automatic
open-forge remove planning --kind extension --automatic
open-forge remove team-knowledge --kind library --automatic
open-forge remove guidance/old-note --kind route --automatic
```
