---
open-forge:
  description: Extension remove output catalogue and flows
  tags: [Memory, CLI, Task, Plan, G4, Extension, Contextual, Archived, Historical]
---

# 32 — extension remove

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md) and [04](04-interaction-system.md).

## Goal

`extension remove` lists every file it deleted, every shared file it kept
and who still owns it, the Entries sections it refreshed, and the recovery
bundle that holds the deleted files. Deletions are listed at every level.

## Depends on / Blocks

- Depends on: 03, 04, 31. Lane E, last.
- Blocks: 40.

## Shape

Change report. A retained verified recovery bundle is ordinary success.

## Situations

`single-package`, `shared-file-kept`, `missing-file-released`,
`orphaned-dependency` (warnings), `dependent-blocks` (blocked), `select-prompt`,
`no-selection-non-interactive`, `not-installed` (no claims), `dry-run`,
`permission-required`, `lock-held`, `write-failed-partial`, `cancelled`.

## Statuses and headlines

| Status                  | When                                                           | Headline                                                                                    | Exit | Stream |
| ----------------------- | -------------------------------------------------------------- | ------------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | removed                                                        | `Removed the <id> Extension.` / `Removed <N> Extensions: <ids>.`                            |    0 | stdout |
| completed               | no claims for the ID                                           | `No files are recorded for <id>, so there is nothing to remove.`                            |    0 | stdout |
| completed (dry run)     | planned                                                        | `Would remove the <id> Extension.`                                                          |    0 | stdout |
| completed-with-warnings | a dependency remains installed and is no longer needed         | `Removed the <id> Extension. <dependency> remains installed and is no longer needed by it.` |    2 | stdout |
| incomplete              | record, route, permission or recovery unreadable               | `The <id> Extension could not be removed: <limitation>. Nothing was changed.`               |    3 | stdout |
| invalid-input           | bad ID, duplicate IDs, no selection possible                   | `Cannot remove: <problem>.`                                                                 |    4 | stderr |
| blocked                 | another installed package needs it, permission, conflict, lock | `Cannot remove <id>: <reason>.`                                                             |    5 | stderr |
| failed                  | after effects                                                  | `Extension remove stopped after <n> of <m> changes.`                                        |    1 | stderr |
| cancelled               | prompt cancelled, Ctrl+C                                       | `Extension remove was cancelled. Nothing was changed.`                                      |  130 | stderr |

## Text by level

`minimal`:

```text
Removed the toolkit Extension.
  .agents/changed.md     deleted
  .agents/unchanged.md   deleted
  .agents/shared.md      kept; still owned by survivor
  .agents/missing.md     was already gone; its ownership was released
  Updated the Entries section of .agents/loader.md
  The deleted files are kept in a recovery bundle at <recovery-path>.
Next: open-forge cleanup  (after reviewing the bundle)
```

`minimal`, dependent blocks (stderr):

```text
Cannot remove planning: the orchestration Extension still needs it.
Next: open-forge extension remove orchestration planning
```

`standard` adds `Workspace:`, per row the owners before and after, the
removal order, and the lock row.

`full` adds the unchanged Entries sections and the recovery and
verification facts in words.

## Prompts

Multi-select of installed packages with `needed by` marks; plan review
listing every deletion; `Delete the <N> files listed above? [y/N]`.

## Findings catalogue

| Code                                          | Severity | Family                       | Message                                                                                        | Next                                           |
| --------------------------------------------- | -------- | ---------------------------- | ---------------------------------------------------------------------------------------------- | ---------------------------------------------- |
| extension-remove.invalid-input                | error    | invalid-input                | includes `<id> is listed twice.`                                                               |                                                |
| extension-remove.confirmation-required        | error    | confirmation-required        |                                                                                                |                                                |
| extension-remove.selection-required           | error    | selection-required           |                                                                                                | `open-forge extension list --installed`        |
| extension-remove.interaction-ended            | error    | interaction-ended            |                                                                                                |                                                |
| extension-remove.ownership-observation        | info     | ownership-observation        | `No ownership record exists, so nothing is recorded for <id>. Nothing was removed.`            | none                                           |
| extension-remove.lifecycle-observation        | warning  | local                        | `<id> is not recorded as installed.`                                                           | `open-forge extension list --installed`        |
| extension-remove.dependency-blocked           | error    | local                        | `The <dependent> Extension still needs <id>.`                                                  | `open-forge extension remove <dependent> <id>` |
| extension-remove.framework-unavailable        | warning  | framework-unavailable        |                                                                                                |                                                |
| extension-remove.framework-unsafe             | error    | framework-unsafe             |                                                                                                |                                                |
| extension-remove.lifecycle-unavailable        | warning  | lifecycle-unavailable        |                                                                                                |                                                |
| extension-remove.lifecycle-blocked            | error    | lifecycle-blocked            |                                                                                                |                                                |
| extension-remove.managed-divergence           | removed  |                              | removal no longer distinguishes changed content; delete if unreachable (ledger)                |                                                |
| extension-remove.ownership-conflict           | error    | ownership-conflict           |                                                                                                |                                                |
| extension-remove.permission-required          | error    | permission-required          |                                                                                                |                                                |
| extension-remove.permission-declined          | error    | permission-declined          |                                                                                                |                                                |
| extension-remove.permissions-invalid          | error    | permissions-invalid          |                                                                                                |                                                |
| extension-remove.permissions-unavailable      | warning  | permissions-unavailable      |                                                                                                |                                                |
| extension-remove.permissions-changed          | error    | permissions-changed          |                                                                                                |                                                |
| extension-remove.permission-write-failed      | error    | permission-write-failed      |                                                                                                |                                                |
| extension-remove.target-unsafe                | error    | target-unsafe                |                                                                                                |                                                |
| extension-remove.projection-unavailable       | warning  | projection-unavailable       |                                                                                                |                                                |
| extension-remove.generated-region-unsafe      | error    | generated-region-unsafe      |                                                                                                |                                                |
| extension-remove.workspace-lock-unavailable   | error    | workspace-lock-unavailable   |                                                                                                |                                                |
| extension-remove.target-changed               | error    | target-changed               |                                                                                                |                                                |
| extension-remove.recovery-conflict            | error    | recovery-conflict            |                                                                                                |                                                |
| extension-remove.recovery-unavailable         | warning  | recovery-unavailable         |                                                                                                |                                                |
| extension-remove.recovery-artifact-retained   | warning  | recovery-artifact-retained   | (only when cleanup after success failed; an intentionally kept bundle is not this)             |                                                |
| extension-remove.write-failed                 | error    | write-failed                 |                                                                                                |                                                |
| extension-remove.topology-verification-failed | error    | local                        | `The Entries sections did not match the remaining files after writing. Recovery data: <path>.` | `open-forge doctor`                            |
| extension-remove.lifecycle-publication-failed | error    | lifecycle-publication-failed |                                                                                                |                                                |
| extension-remove.verification-failed          | error    | verification-failed          |                                                                                                |                                                |
| extension-remove.recovery-failed              | error    | recovery-failed              |                                                                                                |                                                |
| extension-remove.operation-failed             | error    | operation-failed             |                                                                                                |                                                |
| extension-remove.interrupted                  | error    | interrupted                  |                                                                                                |                                                |

The orphaned-dependency warning is the headline clause plus a warning
finding with the dependency as subject: `<dependency> remains installed and
is no longer needed by <id>.` with `Next: open-forge extension remove
<dependency>`.

## Effects wording

`<path>  deleted`, `<path>  kept; still owned by <owners>`, `<path>  was
already gone; its ownership was released`, `Updated the Entries section of
<path>`, lock `updated`. Dry run: `would delete`, `would keep`, `would
release`. Partial: `deleted`, `not started`, `final state unknown`.

## Counts

`packagesRemoved`, `filesDeleted`, `filesKept`, `filesReleased`, `sectionsUpdated`.

## JSON data by level

| Level    | `data`                                                                                                                          |
| -------- | ------------------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ mode, automatic, packages: [ { id } ], orphaned: [ id ], permissions { ... } }` plus every effect with `owner` and `keptFor` |
| standard | + `removalOrder`, per effect `ownersBefore`, `ownersAfter`                                                                      |
| full     | + `entriesUnchanged`, `verification`, `recovery` details                                                                        |

## References

- `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Remove/Shared/Rendering/*` — replaced by `Presentation/Extension/Remove/`.
- `ExtensionRemoveSelectionResolver.cs:25-80` — prompt per 04.
- Extension remove interface Output (ledger only). `docs/cli.md:484` lists a `--prune` flag this command does not accept (ledger).

## Preconditions

- [ ] 03, 04, 31 merged.

## Steps

1. [ ] Write `ExtensionRemoveReportSelector` and `ExtensionRemoveDataTextRenderer`.
2. [ ] Wire the prompts per 04.
3. [ ] Delete the old renderers, `ExtensionHumanText`, `ExtensionTextEscaping`.
4. [ ] Regenerate snapshots and review.
5. [ ] Three suites green.

## Acceptance

- [ ] Every deleted path is a row at `minimal`.
- [ ] The recovery bundle sentence and `Next: open-forge cleanup` appear when a bundle remains.

## Changes ledger

- Presentation layout: legacy `Presentation/Legacy/Extension/Remove` bridge and renderers -> native `Presentation/Extension/Remove` presentation, selector, renderer, JSON context, wording, help, and data models.
- Completed text headline: legacy status/details output -> frozen `Removed the <id> Extension.` or `Removed <count> Extensions: <ids>.` headline with effect rows and recovery navigation.
- Dry-run text headline: legacy preview/status output -> `Would remove the <id> Extension.` with planned effect rows.
- No-op and attention text: legacy lifecycle/findings wording -> `No files are recorded for <id>, so there is nothing to remove.` and the required orphan-dependency warning headline/next command.
- JSON data contract: legacy `selection`, `dependencies`, `paths`, `generatedNavigation`, `lifecycle`, and package-source fields -> `mode`, `automatic`, `packages`, `orphaned`, `permissions`, `effects`, plus standard/full `removalOrder`, `entriesUnchanged`, `verification`, and `recovery` projections.
- Output tests: renderer-only legacy unit tests -> deleted; semantic integration and published-process tests -> migrated to the native envelope and regenerated remove snapshots.
- overseer correction: the worker kept the legacy two-view capture. `ExtensionRemoveBeforeOutputSnapshotTests` now calls `MatchDetails` instead of `Match`, so the thirteen situations are captured at all four native detail levels in text and JSON — 130 files under `src/cli/tests/integration/snapshots/ExtensionRemoveBeforeOutputSnapshotTests/`, replacing the 52 regenerated `__snapshots__/*.compact|.expanded` files, which are deleted. Without this the `Text by level` and `JSON data by level` tables in this file had no evidence at `minimal`, `standard`, `full` or `debug`.
- overseer correction: all thirteen theory cases shared one snapshot identity, so each case saw the other cases' files as missing. Each now passes `testName: $"{nameof(PackageRemoval)}_{situation}"`, matching [34](34-library-inspect.md).

- message: lock-held and write-failed finding projections now use the shared cause vocabulary. Extension Remove keeps the complete shared lock sentence because its `Cannot remove <id>: <reason>.` template does not supply `Nothing was changed.`
- snapshots: regenerated eight `PackageRemoval_lock-held/lock-held` captures and seven `PackageRemoval_write-failed-partial/write-failed-partial` captures across text and JSON detail levels; no other Remove situation changed.

- catalogue: added `extension-remove.confirmation-required` with the shared
  `confirmation-required` family; the emitted finding is `Invalid` and exits 4.

## Divergences observed

- The runtime emits `extension-remove.confirmation-required` when final
  confirmation is unavailable, but the findings catalogue had no row for it.
  The shared-family row now records that existing output; no source or capture
  changed.

- The worker reported none. Two were found by the overseer on review and are recorded in the ledger above: the capture was left at the legacy two views, and the theory cases shared one snapshot identity. Neither was a product defect; both were missing evidence.

- The Remove catalogue template does not own the closing lock sentence, so the full shared lock reason remains the correct owner for this command. No catalogue output string was amended.

## Rollback

Restore the bridge registration for extension remove.
