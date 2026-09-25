---
open-forge:
  description: Extension create output catalogue and input prompts
  tags: [Memory, CLI, Task, Plan, G4, Extension, Contextual, Archived, Historical]
---

# 29 — extension create

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md) and [04](04-interaction-system.md).

## Goal

`extension create` says where the scaffold is and what to edit next, and
asks for the ID and folder in a terminal when they are missing.

## Depends on / Blocks

- Depends on: 03, 04, 28. Lane E.
- Blocks: 30.

## Shape

Change report. `--workspace` is a no-op; the package folder is the subject.

## Situations

`created`, `created-with-metadata`, `dry-run`, `already-present` (no-op),
`destination-has-other-content` (blocked), `missing-id-non-interactive`,
`prompted-id-and-path`, `invalid-id`, `catalogue-unreadable` (incomplete),
`write-failed-partial`, `cancelled`.

## Statuses and headlines

| Status              | When                                                                           | Headline                                                                | Exit | Stream |
| ------------------- | ------------------------------------------------------------------------------ | ----------------------------------------------------------------------- | ---: | ------ |
| completed           | created                                                                        | `Created the <id> Extension scaffold at <folder>`                       |    0 | stdout |
| completed           | identical scaffold present                                                     | `The <id> scaffold at <folder> already matches. Nothing to do.`         |    0 | stdout |
| completed (dry run) | planned                                                                        | `Would create the <id> Extension scaffold at <folder>`                  |    0 | stdout |
| incomplete          | folder unreadable                                                              | `The scaffold could not be created: <limitation>. Nothing was changed.` |    3 | stdout |
| invalid-input       | missing ID or folder outside a terminal, invalid ID, bad version               | `Cannot create the Extension: <problem>.`                               |    4 | stderr |
| blocked             | folder inside the workspace's `.agents`, destination has other content, unsafe | `Cannot create <id> at <folder>: <reason>.`                             |    5 | stderr |
| failed              | after effects                                                                  | `Extension create stopped after <n> of <m> files.`                      |    1 | stderr |
| cancelled           | prompt cancelled, end of input                                                 | `Extension create was cancelled. Nothing was changed.`                  |  130 | stderr |

## Text by level

`minimal`:

```text
Created the my-tools Extension scaffold at packages/my-tools
  packages/my-tools/extension.json
  packages/my-tools/content/.agents/
  Edit extension.json, then add files under content/.agents/.
```

`minimal`, missing ID outside a terminal (stderr):

```text
Cannot create the Extension: no ID was given, and this session cannot ask.
Next: open-forge extension create <id> --path <folder>
```

`standard` adds the manifest values written (`name`, `description`,
`version`, `dependencies`).

`full` adds the manifest content verbatim.

## Prompts

Text input for the ID (`Extension ID (lowercase, digits and hyphens):`) and
the folder (`Package folder:`) when missing, then plan review and
`Create these files? [y/N]` unless `--automatic`.

## Findings catalogue

| Code                                   | Severity | Family              | Message                                                                                                                                                                                                                                                                                     | Next                        |
| -------------------------------------- | -------- | ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------- |
| extension-create.invalid-input         | error    | local               | `<value> is not a valid Extension ID. Use lowercase letters, digits and hyphens.` / `--package-version <value> is not a valid version.` / `no ID was given, and this session cannot ask.` / `no --path was given, and this session cannot ask.` / `--dependency <value> is not a valid ID.` | corrected command           |
| extension-create.confirmation-required | error    | confirmation-required |                                                                                                                                                                                                                                                                                     |                             |
| extension-create.catalogue-unavailable | warning  | local               | `<folder> could not be read.`                                                                                                                                                                                                                                                               | none                        |
| extension-create.catalogue-unsafe      | error    | local               | `<folder> cannot be used: <it is inside the workspace's .agents \| it resolves to an unsafe location>.`                                                                                                                                                                                     | choose another folder       |
| extension-create.destination-collision | error    | local               | `<folder>/<id> already exists with different content.`                                                                                                                                                                                                                                      | choose another ID or folder |
| extension-create.destination-changed   | error    | target-changed      |                                                                                                                                                                                                                                                                                             |                             |
| extension-create.application-failed    | error    | write-failed        | (no recovery bundle: `Extension create stopped after <n> of <m> files. Created files were left in place.`)                                                                                                                                                                                  | none                        |
| extension-create.verification-failed   | error    | verification-failed | (no recovery bundle)                                                                                                                                                                                                                                                                        |                             |
| extension-create.interrupted           | error    | interrupted         |                                                                                                                                                                                                                                                                                             |                             |

## Effects wording

`<path>  created` / `Would create <path>`; partial: `created`, `not started`.

## Counts

`filesCreated`, `directoriesCreated`.

## JSON data by level

| Level    | `data`                                                         |
| -------- | -------------------------------------------------------------- |
| minimal  | `{ mode, id, folder, packagePath, manifestPath, contentPath }` |
| standard | + `manifest { name, description, version, dependencies }`      |
| full     | + `manifestContent`                                            |

## References

- `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Create/Shared/Rendering/*` — replaced by `Presentation/Extension/Create/`.
- `ExtensionCreateRequestResolver.cs:100-175` prompts — per 04.
- Extension create interface Output (ledger only).

## Preconditions

- [ ] 03, 04, 28 merged.

## Steps

1. [ ] Write `ExtensionCreateReportSelector`.
2. [ ] Wire the text-input prompts and confirmation per 04.
3. [ ] Delete the old renderers.
4. [ ] Regenerate snapshots and review.
5. [ ] Three suites green.

## Acceptance

- [ ] `minimal` names the manifest and the content folder and says what to edit.
- [ ] End of input at a prompt is `cancelled`, not `invalid-input` (record the current classification change in the ledger).

## Changes ledger

- file layout/type: the legacy Extension Create renderers under `Presentation/Legacy/Extension/Create/` (6 files) are deleted; native `Presentation/Extension/Create/` owns the data model, selector, text renderer, JSON context, wording and help. `CliExtensionComposer` closes the binding over it.
- message: the legacy status, mode and package summary lines are replaced by the catalogue headlines for the created, dry-run, exact no-op, incomplete, invalid, blocked, failed and cancelled outcomes.
- message: `Scaffold: <n> intended; <n> applied` is gone; the manifest and content paths, preview rows, partial outcomes and the edit-next instruction are rendered as rows beneath the headline.
- JSON member: the legacy result graph is replaced by `mode`, `id`, `folder`, `packagePath`, `manifestPath`, `contentPath`, plus level-gated `manifest` and `manifestContent`.
- counts: the legacy effect summary is replaced by `filesCreated` and `directoriesCreated`.
- interaction: the legacy confirmation rendering is replaced by a native preview and confirmation over `ExtensionCreateData`. The command asks for its id and path through `CliPrompts.TextAsync` and confirms through `PlanConfirmation`, and the `prompted-id-and-path` situation captures that flow.
- status or exit: prompt end-of-input or a declined confirmation now produce the exact cancelled message, exit 130, and no writes.
- snapshots: the 44 `.compact`/`.expanded` captures under `Commands/Extension/Create/__snapshots__/` are replaced by **110** captures at the four native detail levels, text and JSON, across 11 situations, under `src/cli/tests/integration/snapshots/ExtensionCreateBeforeOutputSnapshotTests/`.
- test: legacy assertions are migrated to the native report. `PublishedExtensionCreateProcessTests` was migrated by the worker and passes 3/3 against the installed binary.
- evidence, overseer: re-run on the merge commit, not taken from the worker's report. Unit 3,410 passed, 0 failed, 0 skipped; integration 2,244 total, 2,227 passed, 0 failed, 17 skipped; `ExtensionCreateBeforeOutputSnapshotTests` 11 passed; `PublishedExtensionCreateProcessTests` 3 passed; `npm run check:dotnet` exactly the five documented errors.

- catalogue: added `extension-create.confirmation-required` with the shared
  `confirmation-required` family; the emitted finding is `Invalid` and exits 4.

## Divergences observed

- The runtime emits `extension-create.confirmation-required` when final
  confirmation is unavailable, but the findings catalogue had no row for it.
  The shared-family row now records that existing output; the finding is
  `Invalid` and exits 4. No source or capture changed.

1. **The worker left its retired capture tree in place.** All three call sites
   were converted to `MatchDetails` and the 110 native captures were written,
   but the 44 `.compact`/`.expanded` files under
   `Commands/Extension/Create/__snapshots__/` were not deleted, leaving them
   unreferenced on disk. The overseer removed them before committing. The
   command packet never asks for this deletion, so whether a lane cleans up
   after itself is currently chance: [24](24-route-update.md) removed its 64
   files unprompted. **Durable record:** the packet should require it, and
   [40](40-verification.md) should assert per command that a native
   `snapshots/<Command>BeforeOutputSnapshotTests/` tree exists and the legacy
   `Commands/<Command>/__snapshots__` tree does not.

2. **Version and dependency validation is unspecified.** The worker reports that
   the catalogue does not say how a malformed version or an unresolvable
   dependency in the created manifest should be classified. Native behaviour was
   left unchanged. **Maintainer decision.**

3. **Creating inside the workspace's own `.agents` is unspecified.** The
   catalogue gives no safety rule for a target path inside `.agents`. Native
   behaviour was left unchanged. **Maintainer decision.**

4. **Completed-effect wording is unspecified.** The catalogue does not give the
   row wording for an effect that completed, as distinct from one planned or
   partially applied. Native behaviour was left unchanged. **Maintainer
   decision.**

5. **`folder` and `packagePath` are both published and their split is
   undocumented.** The JSON carries both members; the catalogue does not say
   which is authoritative for the created package's location, nor how they
   differ. Native output was left unchanged. **Maintainer decision.**

6. **Closeout was blocked, as expected for a worktree.** Both `git add` and the
   task-record edit were refused, with the two errors quoted in the worker's
   report. No workaround was attempted. The overseer wrote this ledger and
   committed.

## Rollback

Restore the bridge registration for extension create.
