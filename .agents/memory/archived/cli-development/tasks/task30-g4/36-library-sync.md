---
open-forge:
  description: Library sync output catalogue and permission flow
  tags: [Memory, CLI, Task, Plan, G4, Library, Contextual, Archived, Historical]
---

# 36 — library sync

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md) and [04](04-interaction-system.md).

## Goal

`library sync` says which links it added and removed to match the source
folder, and why it stopped when a destination is not what it expects.

## Depends on / Blocks

- Depends on: 03, 04, 35. Lane F.
- Blocks: 37.

## Shape

Change report.

## Situations

`up-to-date`, `links-added`, `links-removed`, `both`, `dry-run`,
`unknown-id` (invalid), `changed-occupant` (blocked), `registered-link-gone`
(completed-with-warnings), `source-unreadable` (incomplete), `permission-required`,
`no-ownership-record` (info), `lock-held`, `record-invalid` (incomplete),
`write-failed-partial`, `cancelled`.

## Statuses and headlines

| Status                  | When                                                                      | Headline                                                                            | Exit | Stream |
| ----------------------- | ------------------------------------------------------------------------- | ----------------------------------------------------------------------------------- | ---: | ------ |
| completed               | nothing to change                                                         | `The <id> Library is up to date. Nothing to do.`                                    |    0 | stdout |
| completed               | changes                                                                   | `Synchronized <id>: <A> links added, <R> removed, <U> unchanged.` (omit zero parts) |    0 | stdout |
| completed-with-warnings | a registered link was restored                                          | `Synchronized <id>: <A> links added, <R> removed, <U> unchanged.` (omit zero parts) |    2 | stdout |
| completed (dry run)     | planned                                                                   | `Would synchronize <id>: <A> links to add, <R> to remove.`                          |    0 | stdout |
| completed               | no ownership record                                                       | `No ownership record exists, so <id> cannot be synchronized. Nothing was changed.`  |    0 | stdout |
| completed-with-warnings | recovery bundle retained                                                  | + family row                                                                        |    2 | stdout |
| incomplete              | source folder or a fact unreadable                                        | `<id> could not be synchronized: <limitation>. Nothing was changed.`                |    3 | stdout |
| invalid-input           | bad or unknown ID, extra operand                                          | `Cannot synchronize <ref>: <problem>.`                                              |    4 | stderr |
| blocked                 | changed destination, collision, permission, lock                          | `Cannot synchronize <id>: <reason>. Nothing was changed.`                           |    5 | stderr |
| failed                  | after effects                                                             | `Library sync stopped after <n> of <m> changes.`                                    |    1 | stderr |
| cancelled               | prompt cancelled, Ctrl+C                                                  | `Library sync was cancelled. Nothing was changed.`                                  |  130 | stderr |

## Text by level

`minimal`:

```text
Synchronized team-knowledge: 2 links added, 1 removed, 10 unchanged.
  Added    docs/new-a.md
  Added    docs/new-b.md
  Removed  docs/old.md   (its source file is gone)
  Updated the Entries section of docs/_docs.md
```

`minimal`, changed destination (stderr):

```text
Cannot synchronize team-knowledge: docs/review.md is no longer the link the Library created. Nothing was changed.
  It is now an ordinary file. Move it away or restore the link, then rerun.
Next: open-forge library inspect team-knowledge
```

`standard` adds `Workspace:`, the unchanged links as one count line, targets
per changed link, and the lock row.

`full` adds the inventory, expected states, verification and recovery facts.

## Prompts

Permission for new links outside `.agents`; plan review; `Apply these
changes? [y/N]` unless `--automatic` (added by 04).

## Findings catalogue

| Code                                         | Severity | Family                     | Message                                                                                                             | Next                              |
| -------------------------------------------- | -------- | -------------------------- | ------------------------------------------------------------------------------------------------------------------- | --------------------------------- |
| library-sync.invalid-input                   | error    | invalid-input              |                                                                                                                     |                                   |
| library-sync.confirmation-required           | error    | confirmation-required      |                                                                                                                     |                                   |
| library-sync.invalid-id                      | error    | local                      | `<value> is not a valid Library ID.`                                                                                | `open-forge library list`         |
| library-sync.unknown-id                      | error    | unknown-id                 |                                                                                                                     | `open-forge library list`         |
| library-sync.ownership-observation           | info     | ownership-observation      |                                                                                                                     |                                   |
| library-sync.record-invalid                  | warning  | local                      | `The Library section of .agents/open-forge.lock.json is invalid: <reason>.`                                         | `open-forge doctor`               |
| library-sync.record-unavailable              | warning  | lifecycle-unavailable      |                                                                                                                     |                                   |
| library-sync.record-blocked                  | error    | lifecycle-blocked          |                                                                                                                     |                                   |
| library-sync.source-root-invalid             | error    | local                      | `The source folder <path> is not a folder inside the workspace.`                                                    | none                              |
| library-sync.source-root-unavailable         | warning  | local                      | `The source folder <path> cannot be read, so nothing was changed.`                                                  | none                              |
| library-sync.source-root-blocked             | error    | local                      | `The source folder <path> resolves to an unsafe location.`                                                          | none                              |
| library-sync.inventory-incomplete            | warning  | local                      | `Some files under <source> could not be listed, so nothing was changed.`                                            | none                              |
| library-sync.mapping-unavailable             | warning  | local                      | `The link plan could not be completed: <reason>.`                                                                   | `open-forge doctor`               |
| library-sync.mapping-blocked                 | error    | local                      | `<destination path> is <an ordinary file \| a folder \| a different link> and is not the link the Library created.` | `open-forge library inspect <id>` |
| library-sync.destination-collision           | error    | local                      | `<destination path> already exists and is not a link this Library would create.`                                    | none                              |
| library-sync.retired-link-missing            | error    | local                      | `<destination path> is registered but no longer exists, so its removal cannot be confirmed.`                        | `open-forge library inspect <id>` |
| library-sync.registered-link-restored        | warning  | local                      | `<destination path> was registered but missing, so it was restored.`                                                 | none                              |
| library-sync.link-capability-unavailable     | error    | local                      | `This system cannot create the file links Libraries need.`                                                          | none                              |
| library-sync.consumer-blocked                | error    | local                      | `<path> is managed by Open Forge and cannot receive Library links.`                                                 | none                              |
| library-sync.ownership-conflict              | error    | ownership-conflict         |                                                                                                                     |                                   |
| library-sync.permission-required             | error    | permission-required        |                                                                                                                     |                                   |
| library-sync.permission-declined             | error    | permission-declined        |                                                                                                                     |                                   |
| library-sync.permission-invalid              | error    | permissions-invalid        |                                                                                                                     |                                   |
| library-sync.permission-unavailable          | warning  | permissions-unavailable    |                                                                                                                     |                                   |
| library-sync.permission-changed              | error    | permissions-changed        |                                                                                                                     |                                   |
| library-sync.permission-write-failed         | error    | permission-write-failed    |                                                                                                                     |                                   |
| library-sync.generated-navigation-blocked    | error    | generated-region-unsafe    |                                                                                                                     |                                   |
| library-sync.generated-navigation-incomplete | warning  | projection-unavailable     |                                                                                                                     |                                   |
| library-sync.lock-unavailable                | error    | workspace-lock-unavailable |                                                                                                                     |                                   |
| library-sync.recovery-unavailable            | warning  | recovery-unavailable       |                                                                                                                     |                                   |
| library-sync.recovery-retained               | warning  | recovery-artifact-retained |                                                                                                                     |                                   |
| library-sync.application-failed              | error    | write-failed               |                                                                                                                     |                                   |
| library-sync.verification-failed             | error    | verification-failed        |                                                                                                                     |                                   |
| library-sync.operation-failed                | error    | operation-failed           |                                                                                                                     |                                   |
| library-sync.interrupted                     | error    | interrupted                |                                                                                                                     |                                   |

## Effects wording

`Added <path>`, `Removed <path>  (its source file is gone)`, `Updated the
Entries section of <path>`, lock `updated`. Dry run: `Add`, `Remove`. Partial:
`added`, `removed`, `not started`, `final state unknown`.

## Counts

`linksAdded`, `linksRemoved`, `linksUnchanged`, `sectionsUpdated`.

## JSON data by level

| Level    | `data`                                                                                |
| -------- | ------------------------------------------------------------------------------------- |
| minimal  | `{ mode, id, sourceFolder, destinationFolder, permissions { ... } }` plus the effects |
| standard | + `unchanged: [ path ]`, per effect `target`                                          |
| full     | + `inventory`, `expectedStates`, `verification`, `recovery` details                   |

## References

- `src/cli/core/OpenForge.Cli.Core/Commands/Library/Sync/Shared/Rendering/*` — replaced by `Presentation/Library/Sync/`.
- Library sync interface Output and its reconciliation table (ledger only).

## Preconditions

- [ ] 03, 04, 35 merged.

## Steps

1. [ ] Write `LibrarySyncReportSelector` and `LibrarySyncDataTextRenderer`.
2. [ ] Wire the prompts per 04, adding `--automatic`.
3. [ ] Delete the old renderers.
4. [ ] Regenerate snapshots and review.
5. [ ] Three suites green.

## Acceptance

- [ ] Up-to-date `minimal` is one line.
- [ ] A blocked destination names the path and what it is now.

## Changes ledger

> The worker's own ledger was three summary bullets. The overseer rebuilt this
> from its report and the diff, as `00-conventions.md` requires.

- file layout/type: the legacy Library Sync bridge is deleted — `Presentation/Legacy/Library/Sync/Shared/Rendering/LibrarySyncLegacyPresentation.cs` (26 lines) and `LibrarySyncPresentation.cs` (131 lines). Native `Presentation/Library/Sync/` replaces them with `LibrarySyncPresentation`, `Models/LibrarySyncData` (192 lines), `Shared/Selection/LibrarySyncReportSelector` (684 lines), `Shared/Rendering/LibrarySyncDataTextRenderer` (86) and `LibrarySyncDataJsonContext`, `Shared/Wording/LibrarySyncWording` (323) and `Shared/Help/LibrarySyncHelpSections`. `CliLibraryComposer` closes the binding over it; Attach and Detach remain on the legacy bridge in the same file.
- message: the legacy presentation's status and receipt rendering is replaced by the catalogue headlines and effect rows for all 14 situations — up-to-date, links-added, links-removed, both, dry-run, unknown-id, changed-occupant, registered-link-gone, source-unreadable, permission-required, no-ownership-record, lock-held, write-failed-partial and cancelled.
- JSON member: the legacy projection is replaced by the schema v3 envelope with `data` carrying the command-owned sync facts, gated by detail level.
- snapshots: the 56 `.compact`/`.expanded` captures under `Commands/Library/Sync/__snapshots__/` are replaced by **140** captures at the four native detail levels, text and JSON, under `src/cli/tests/integration/snapshots/LibrarySyncBeforeOutputSnapshotTests/`.
- test: the legacy unit tests `Commands/Library/Sync/Shared/Rendering/LibrarySyncOutputSnapshotTests.cs` and `LibrarySyncPresentationTests.cs` are deleted and replaced by `Presentation/Library/Sync/LibrarySyncPresentationTests.cs`. `PublishedLibrarySyncProcessTests` was migrated by the worker and passes 3/3 against the installed binary.
- evidence, overseer: re-run on the merge commit, not taken from the worker's report. Unit 3,389 passed, 0 failed, 0 skipped; integration 2,234 total, 2,217 passed, 0 failed, 17 skipped; `PublishedLibrarySyncProcessTests` 3 passed; `npm run check:dotnet` exactly the five documented errors.

- status or exit: `lock-held` returned `failed` at exit 1 with `library-sync.operation-failed` -> it now raises `library-sync.lock-unavailable` as `blocked` at exit 5, as required by the maintainer ruling.
- warning: `registered-link-gone` returned `completed` at exit 0 after recreating the link -> it now also raises `library-sync.registered-link-restored` with `<destination path> was registered but missing, so it was restored.`, returning `completed-with-warnings` at exit 2.
- snapshots: the `lock-held` and `registered-link-gone` captures showed the pre-ruling statuses and findings -> they now show the ruled status, exit and finding changes.
- message: the `lock-held` headline exposed the lock manager's `IOException` cause -> it now uses the trailer-free shared lock reason, while the finding message retains the complete shared lock sentence and the underlying cause remains on `LibrarySyncFinding.Cause`.
- snapshots: only the `Synchronize_lock-held` captures were regenerated for this wording correction; no other Sync capture changed.

- status or exit: malformed or duplicate Library records now return `incomplete` / exit 3 instead of being treated as an ownership observation; malformed input is excluded from the observation shortcut.
- message: Sync lock-held headlines use the trailer-free shared lock reason, write-failed findings use the shared filesystem cause vocabulary, and the raw lock/write cause remains internal to the result/evidence path.
- snapshots: regenerated eight `Synchronize_lock-held` and seven `Synchronize_write-failed-partial` captures across text and JSON detail levels; no other Sync situation changed.

- catalogue: added `library-sync.confirmation-required` with the shared
  `confirmation-required` family; the emitted finding is `Invalid` and exits 4.

## Divergences observed

- The runtime emits `library-sync.confirmation-required` when final
  confirmation is unavailable, but the findings catalogue had no row for it.
  The shared-family row now records that existing output; no source or capture
  changed.

1. **`registered-link-gone` status disagrees with the catalogue, and the
   difference is not cosmetic.** This file says blocked at exit 5; the runtime
   returns complete at exit 0 **and creates the link**. That is a behavioural
   gap, not only a status label: the catalogue expects the command to refuse,
   and it instead repairs. Existing behaviour was preserved and no capture was
   regenerated over it. **Maintainer decision, and the higher-risk of the two.**

2. **`lock-held` status disagrees with the catalogue.** This file says blocked
   at exit 5; the runtime returns failed at exit 1 with `operation-failed` and
   an `IOException`. Every other command in this packet treats a held workspace
   lock as blocked, so this is likely a real defect rather than a catalogue
   error. Existing behaviour preserved. **Maintainer decision.**

3. **The worker left its retired capture tree in place.** All 14 situations were
   converted and the 140 native captures written, but the 56 `.compact`/
   `.expanded` files under `Commands/Library/Sync/__snapshots__/` were not
   deleted. The overseer removed them before committing. Same omission as
   [29](29-extension-create.md) divergence 1; the command packet does not ask
   for the deletion.

4. **The worker's build workaround leaked into its working tree.** Its commit,
   staged with `git add -A`, initially carried 34 files under `verify-bin/` and
   `verify-obj/` — scratch build output from working around the unreadable user
   NuGet configuration. The overseer reset the commit, removed them and
   recommitted 214 files, all under `src/cli/`. Agents may change only
   `src/cli/**` and their own task file, so nothing else belongs in a merge.
   **Durable record:** the merge recipe must check
   `git status --short | grep -vE ' (src/cli|\.agents)/'` before staging.

5. **Closeout was blocked, as expected for a worktree.** Both the task-record
   patch and `git add` were refused, with the two errors quoted in the worker's
   report. The overseer wrote this ledger and committed.

6. **The `registered-link-gone` catalogue row changed by maintainer ruling.**
   The sync operation keeps repairing an empty registered destination, but now
   reports `completed-with-warnings` at exit 2 with a warning naming the link,
   because silent success would hide that external drift was repaired while a
   changed occupant remains the separate refusal case.

7. **The `lock-held` divergence was resolved by maintainer ruling.** The
   catalogue already specified blocked/exit 5; the code now raises
   `library-sync.lock-unavailable` with blocked status, because a held
   workspace lock is expected and retryable rather than an unexpected fault.

8. **The lock-held ruling makes an existing Sync wording gap reachable.** The
   old finding message was `Library sync stopped because of an unexpected
   error: IOException (0x80070020): The filesystem operation failed.`; the new
   `library-sync.lock-unavailable` finding message is
   `IOException (0x80070020): The filesystem operation failed`, while the
   shared workspace-lock family says `Another Open Forge command holds the
   workspace lock. Nothing was changed.`. The ruling fixes status and finding
   identity, and the frozen-string rule does not choose between these messages.
   **Maintainer decision.**

9. **The lock-held wording gap is resolved without changing the catalogue
   headline or the trailer-bearing shared lock sentence.** Sync now maps every
   `LockUnavailable` finding to the generic shared reason for the headline and
   finding message; its lock manager cause remains internal on
   `LibrarySyncFinding.Cause`. The selector no longer falls back to that cause
   for a lock-held headline.

- The Sync catalogue already specified incomplete record handling, so only the situation/finding severity documentation needed alignment. No Sync catalogue headline template was changed.

## Rollback

Restore the bridge registration for library sync.
