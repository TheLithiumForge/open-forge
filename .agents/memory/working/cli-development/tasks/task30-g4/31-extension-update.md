---
open-forge:
  description: Extension update output catalogue and flows
  tags: [Memory, Working, CLI, Task, Plan, G4, Extension, Contextual, Active]
---

# 31 — extension update

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md) and [04](04-interaction-system.md).

## Goal

`extension update` says which files it replaced, restored, created or
deleted for each package, which files from an older version it kept, and
where the previous content went. Fingerprints stay at `full`.

## Depends on / Blocks

- Depends on: 03, 04, 30. Lane E.
- Blocks: 32.

## Shape

Change report. Same relation wording as [13](13-update.md).

## Situations

`up-to-date`, `files-replaced`, `new-version-with-new-files`, `retired-kept`,
`retired-pruned`, `all-packages`, `select-prompt`, `no-selection-non-interactive`,
`permission-required`, `ownership-unknown` (warnings), `dry-run`,
`source-unreadable`, `lock-held`, `write-failed-partial`, `cancelled`.

## Statuses and headlines

| Status                  | When                                                                  | Headline                                                                                                 | Exit | Stream |
| ----------------------- | --------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | nothing to change                                                     | `The <id> Extension is up to date. Nothing to do.` / `All <N> Extensions are up to date. Nothing to do.` |    0 | stdout |
| completed               | changes                                                               | `Updated the <id> Extension to <version>.` / `Updated <N> Extensions.`                                   |    0 | stdout |
| completed (dry run)     | planned                                                               | `Would update the <id> Extension to <version>.`                                                          |    0 | stdout |
| completed-with-warnings | retired files kept, ownership cannot be established for a selected ID | headline + rows                                                                                          |    2 | stdout |
| incomplete              | source, Framework, record, permission or recovery unreadable          | `The <id> Extension could not be updated: <limitation>. Nothing was changed.`                            |    3 | stdout |
| invalid-input           | bad ID, `--all` with IDs, no selection possible                       | `Cannot update: <problem>.`                                                                              |    4 | stderr |
| blocked                 | permission, conflict, overlap, lock, target changed                   | `Cannot update <id>: <reason>.`                                                                          |    5 | stderr |
| failed                  | after effects                                                         | `Extension update stopped after <n> of <m> changes.`                                                     |    1 | stderr |
| cancelled               | prompt cancelled, Ctrl+C                                              | `Extension update was cancelled. Nothing was changed.`                                                   |  130 | stderr |

## Text by level

`minimal`:

```text
Updated the development Extension to 0.2.0.
  .agents/workflows/review.md         replaced (new content in this version)
  .agents/workflows/development.md    replaced (you had changed it)
  .agents/workflows/checklist.md      created (new in this version)
  .agents/workflows/old.md            kept; no longer part of the package
  Previous content: git diff
Next: open-forge extension update development --prune --dry-run  (preview deleting the kept file)
```

`standard` adds `Workspace:`, the source, unchanged files as a count, the
Entries sections updated, the dependency closure, and the grant.

`full` adds both SHA-256 values per changed path and the verification and
recovery facts in words.

## Prompts

Multi-select of installed packages when no ID and no `--all`; Permission;
plan review; Confirm; `Delete the <K> files listed above? [y/N]` under
`--prune`.

## Findings catalogue

| Code                                          | Severity | Family                       | Message                                                                                        | Next                                    |
| --------------------------------------------- | -------- | ---------------------------- | ---------------------------------------------------------------------------------------------- | --------------------------------------- |
| extension-update.invalid-input                | error    | invalid-input                | includes `--all cannot be combined with package IDs.`                                          |                                         |
| extension-update.confirmation-required        | error    | confirmation-required        |                                                                                                |                                         |
| extension-update.selection-required           | error    | selection-required           |                                                                                                | `open-forge extension list --installed` |
| extension-update.interaction-ended            | error    | interaction-ended            |                                                                                                |                                         |
| extension-update.source-unavailable           | warning  | local                        | `The source <path> could not be read.`                                                         | none                                    |
| extension-update.source-invalid               | error    | local                        | `<path> is not a valid package or package folder: <reason>.`                                   | fix by hand                             |
| extension-update.source-overlap               | error    | local                        | `<path> is inside the workspace and cannot be used as a source.`                               | none                                    |
| extension-update.source-identity-conflict     | error    | local                        | `The source at <path> does not contain <id>, which is what is installed.`                      | `open-forge extension inspect <id>`     |
| extension-update.framework-unavailable        | warning  | framework-unavailable        |                                                                                                |                                         |
| extension-update.framework-unsafe             | error    | framework-unsafe             |                                                                                                |                                         |
| extension-update.lifecycle-unavailable        | warning  | lifecycle-unavailable        |                                                                                                |                                         |
| extension-update.lifecycle-blocked            | error    | lifecycle-blocked            |                                                                                                |                                         |
| extension-update.lifecycle-observation        | warning  | local                        | `<id> is not recorded as installed, so it was not updated.`                                    | `open-forge extension list --installed` |
| extension-update.managed-divergence           | removed  |                              | ordinary update replaces changed files; delete the member if it is unreachable (ledger)        |                                         |
| extension-update.ownership-conflict           | error    | ownership-conflict           |                                                                                                |                                         |
| extension-update.permission-required          | error    | permission-required          |                                                                                                |                                         |
| extension-update.permission-declined          | error    | permission-declined          |                                                                                                |                                         |
| extension-update.permissions-invalid          | error    | permissions-invalid          |                                                                                                |                                         |
| extension-update.permissions-unavailable      | warning  | permissions-unavailable      |                                                                                                |                                         |
| extension-update.permissions-changed          | error    | permissions-changed          |                                                                                                |                                         |
| extension-update.permission-write-failed      | error    | permission-write-failed      |                                                                                                |                                         |
| extension-update.target-unsafe                | error    | target-unsafe                |                                                                                                |                                         |
| extension-update.projection-unavailable       | warning  | projection-unavailable       |                                                                                                |                                         |
| extension-update.generated-region-unsafe      | error    | generated-region-unsafe      |                                                                                                |                                         |
| extension-update.workspace-lock-unavailable   | error    | workspace-lock-unavailable   |                                                                                                |                                         |
| extension-update.target-changed               | error    | target-changed               |                                                                                                |                                         |
| extension-update.recovery-conflict            | error    | recovery-conflict            |                                                                                                |                                         |
| extension-update.recovery-unavailable         | warning  | recovery-unavailable         |                                                                                                |                                         |
| extension-update.recovery-artifact-retained   | warning  | recovery-artifact-retained   |                                                                                                |                                         |
| extension-update.write-failed                 | error    | write-failed                 |                                                                                                |                                         |
| extension-update.topology-verification-failed | error    | local                        | `The Entries sections did not match the installed files after writing. Recovery data: <path>.` | `open-forge doctor`                     |
| extension-update.lifecycle-publication-failed | error    | lifecycle-publication-failed |                                                                                                |                                         |
| extension-update.verification-failed          | error    | verification-failed          |                                                                                                |                                         |
| extension-update.recovery-failed              | error    | recovery-failed              |                                                                                                |                                         |
| extension-update.operation-failed             | error    | operation-failed             |                                                                                                |                                         |
| extension-update.interrupted                  | error    | interrupted                  |                                                                                                |                                         |

The retired-kept warning has no own code today; it is the `kept` effect row
plus the headline clause. If a code exists in the result for it, map it to
`warning` and the row.

## Effects wording

As [13](13-update.md), with `(new content in this version)` and `no longer
part of the package`.

## Counts

`packagesUpdated`, `filesReplaced`, `filesRestored`, `filesCreated`,
`filesDeleted`, `filesKept`, `filesUnchanged`, `sectionsUpdated`.

## JSON data by level

| Level    | `data`                                                                                                                           |
| -------- | -------------------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ mode, force, prune, automatic, source { kind, path }, packages: [ { id, from, to } ], previousContent, permissions { ... } }` |
| standard | + `unchanged: [ path ]`, `sections: [ path ]`, `dependencies`                                                                    |
| full     | + per effect `before`, `after`, `relation`, `verification`, `recovery` details                                                   |

## References

- `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Update/Shared/Rendering/*` — replaced by `Presentation/Extension/Update/`.
- Extension update interface Output (ledger only).

## Preconditions

- [ ] 03, 04, 30 merged.

## Steps

1. [ ] Write `ExtensionUpdateReportSelector` and `ExtensionUpdateDataTextRenderer`.
2. [ ] Wire the prompts per 04.
3. [ ] Delete the old renderers.
4. [ ] Regenerate snapshots and review.
5. [ ] Three suites green.

## Acceptance

- [ ] No fingerprint below `full`.
- [ ] Every replaced, restored, created, deleted and kept file is a row at `minimal`.

## Changes ledger

> The first run on this subtask returned zero changes. It stopped on the frozen
> wording conflict in divergence 1 and never built, because the sandbox cannot
> read the user NuGet configuration. The overseer built the worktree, told it the
> conflict was accepted and escalated, and relaunched it on the same thread; the
> resumed run delivered the whole subtask.

- file layout/type: the legacy Extension Update bridge and its five renderers are deleted; native `Presentation/Extension/Update/` owns the data model, selector, text renderer, JSON context, wording and help sections. `CliExtensionComposer` closes the binding over it.
- message: output moved to the catalogue headlines, with per-path action and relation rows, previous-content guidance, verification, recovery and grant details, and the dry-run `No files were changed.` footer.
- JSON member: schema v3 native data now carries package versions; `standard` adds dependency, unchanged and section data; `full` adds per-effect facts.
- status or exit: migrated to the shared semantic statuses, root effects and detail-aware streams. The `Status:` line is gone.
- flag/help: native help sections added for selection, authority, permissions, results and global options.
- result facts: installed and source versions, and a request-selection projection, were added so the output can state what it actually acted on.
- test: callers migrated; the 60 legacy captures are replaced by **150** captures across 15 situations at four detail levels in text and JSON. `PublishedExtensionUpdateProcessTests` was migrated by the worker and passes 3/3 against the installed binary.
- test, overseer: `ExtensionUpdateResultContractTests.ResultSnapshotsFactsAndOrdersFindings` asserted that the rendered text contains `.agents/_index.md` at both `minimal` and `standard`. The native output never renders the Entries section **path** at any level; it publishes a **count**. The assertion now reads `Assert.Equal(view >= CliDetail.Standard, text.Contains("1 Entries section updated", ...))`, which is what `Text by level` means by "standard adds ... the Entries sections updated" and what `Counts` carries as `sectionsUpdated`. See divergence 2.
- evidence, overseer: re-run on the merge commit, not taken from the worker's report. Unit 3,255 passed, 0 failed, 0 skipped; integration 2,208 total, 2,191 passed, 0 failed, 17 skipped; `PublishedExtensionUpdateProcessTests` 3 passed; `PublishedSchema3ProcessTests` 28 passed, checked because this lane reshaped `data`; `npm run check:dotnet` exactly the five documented errors.

- message: `write-failed` finding messages now use the shared filesystem-access cause vocabulary; raw exception evidence remains in the existing full/debug evidence paths. The lock-held user-facing projection was already generic.
- snapshots: regenerated the seven `PackageUpdate_write-failed-partial/write-failed-partial` native captures across text and JSON detail levels; no `PackageUpdate_lock-held` capture changed.

- catalogue: added `extension-update.confirmation-required` with the shared
  `confirmation-required` family; the emitted finding is `Invalid` and exits 4.

## Divergences observed

- The runtime emits `extension-update.confirmation-required` when final
  confirmation is unavailable, but the findings catalogue had no row for it.
  The shared-family row now records that existing output; no source or capture
  changed.

1. **The `--all` wording conflict, shared with [30](30-extension-install.md).**
   This file asks for `--all cannot be combined with package IDs.`; the code
   emits `Explicit Extension IDs and --all cannot be combined.` The string lives
   in five call sites across both commands — `ExtensionUpdateBinding.cs:173`,
   `ExtensionUpdateOperation.cs:49`, `ExtensionInstallBinding.cs:159`,
   `ExtensionInstallOperationFactory.cs:91` and
   `ExtensionInstallSelectionResolver.cs:111`. Nothing was changed.
   **Maintainer decision, and one decision settles both commands.**

2. **The worker reported unit `0 failed` while its own contract test was
   failing.** The overseer reproduced the failure **in the worker's own
   worktree**, so it was not introduced by the merge: the report was simply
   wrong. The defect: the worker rewrote
   `ExtensionUpdateResultContractTests`, correctly changing the headline
   assertion from `Status: blocked` to `Cannot update toolkit:`, but carried the
   legacy `.agents/_index.md` path assertion through unchanged — and flattened
   the level gating that the legacy test used for a sibling assertion
   (`Assert.Equal(view >= CliDetail.Standard, ...)`). Probing the renderer at all
   three levels showed the Entries section appears as `1 Entries section updated`
   at `standard` and `full`, and not at all at `minimal`; the path appears
   nowhere. The assertion was corrected to the count rather than deleted, so the
   test still proves the Entries update is reported. **This is an observable
   output change worth confirming**: the legacy renderer listed the updated
   Entries section by path, the native one reports only a count.

3. **`extension-update.managed-divergence` is marked `removed` in the findings
   table, but this file's own `Text by level` example requires the behaviour it
   produces.** The table row reads `removed`, with the note "delete the member if
   it is unreachable". The member is **reachable**:
   `ExtensionUpdateReconciler.cs:454` emits it whenever a retired managed target
   is preserved because `--prune` was not passed, and the native wording renders
   it as `Retired file was kept`. The `minimal` example in this same file shows
   exactly that outcome:

   ```text
   .agents/workflows/old.md            kept; no longer part of the package
   Next: open-forge extension update development --prune --dry-run  (preview deleting the kept file)
   ```

   **Overseer decision: kept, deliberately.** The deletion instruction is
   conditional on the member being unreachable, and it is not; removing it would
   silently drop a warning the catalogue's own example specifies. The defect is
   the contradiction between the findings table and the text example.
   **Maintainer decision:** give the code a proper findings row describing
   "retired file kept", or rename it and retire the `managed-divergence` name.

4. **Blocked outcomes suppress synthetic navigation effects**, so a blocked
   report carries no effects at all. Recorded by the worker as an intentional
   choice; no catalogue rule covers it. **Confirm.**

5. **Source-unreadable selection now projects the request selection into the
   result facts**, so the selected identity survives into the output. Without
   it the report could not say which package it had been asked about.

6. **Optional `from` and `to` version facts are sourced from installed ownership
   and the source package**, rather than being left null, so version transitions
   render truthfully.

7. **The prescribed solution build was blocked** by unauthorized access to
   `%APPDATA%\NuGet\NuGet.Config`; serial no-restore project builds succeeded.
   Same root cause as [24](24-route-update.md) divergence 3. This is what
   defeated the first run entirely.

- The affected lock-held situation retains its raw lock cause only in existing internal evidence; its user-facing capture remained byte-identical. No catalogue template was changed.

## Rollback

Restore the bridge registration for extension update.
