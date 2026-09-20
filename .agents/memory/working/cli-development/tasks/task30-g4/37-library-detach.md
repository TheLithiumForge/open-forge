---
open-forge:
  description: Library detach output catalogue and permission flow
  tags: [Memory, Working, CLI, Task, Plan, G4, Library, Contextual, Active]
---

# 37 — library detach

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md) and [04](04-interaction-system.md).

## Goal

`library detach` says which links it removed, that the source files were
kept, and that the registration is gone.

## Depends on / Blocks

- Depends on: 03, 04, 36. Lane F, last.
- Blocks: 40.

## Shape

Change report. Source-independent: an unreadable source folder is not a
reason to stop.

## Situations

`detached`, `detached-no-links`, `dry-run`, `unknown-id` (invalid),
`registered-link-gone` (blocked), `changed-occupant` (blocked),
`destination-protected` (blocked),
`permission-required`, `no-ownership-record` (info), `lock-held`,
`record-invalid` (incomplete),
`write-failed-partial`, `cancelled`.

## Statuses and headlines

| Status                  | When                                                   | Headline                                                                                    | Exit | Stream |
| ----------------------- | ------------------------------------------------------ | ------------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | detached                                               | `Detached <id>: removed <N> links under <destination>. Source files in <source> were kept.` |    0 | stdout |
| completed               | registration with no links                             | `Detached <id>. It had no links.`                                                           |    0 | stdout |
| completed (dry run)     | planned                                                | `Would detach <id>: remove <N> links under <destination>.`                                  |    0 | stdout |
| completed               | no ownership record                                    | `No ownership record exists, so <id> cannot be detached. Nothing was changed.`              |    0 | stdout |
| completed-with-warnings | recovery bundle retained                               | + family row                                                                                |    2 | stdout |
| incomplete              | record, Entries or recovery unreadable                 | `<id> could not be detached: <limitation>. Nothing was changed.`                            |    3 | stdout |
| invalid-input           | bad or unknown ID, extra operand                       | `Cannot detach <ref>: <problem>.`                                                           |    4 | stderr |
| blocked                 | a registered link is gone or changed, permission, lock | `Cannot detach <id>: <reason>. Nothing was changed.`                                        |    5 | stderr |
| failed                  | after effects                                          | `Library detach stopped after <n> of <m> links were removed.`                               |    1 | stderr |
| cancelled               | prompt cancelled, Ctrl+C                               | `Library detach was cancelled. Nothing was changed.`                                        |  130 | stderr |

## Text by level

`minimal`:

```text
Detached team-knowledge: removed 12 links under docs. Source files in shared/team were kept.
  Updated the Entries section of docs/_docs.md
```

`minimal`, registered link gone (stderr):

```text
Cannot detach team-knowledge: docs/review.md is registered but no longer exists, so its removal cannot be confirmed. Nothing was changed.
Next: open-forge library inspect team-knowledge
```

`standard` adds `Workspace:`, every removed link as a row, and the lock row
(`.agents/open-forge.lock.json  registration removed`).

`full` adds expected states, verification and recovery facts.

## Prompts

Permission when links outside `.agents` are removed; plan review listing
every link; `Remove the <N> links listed above? [y/N]` unless `--automatic`
(added by 04).

## Findings catalogue

| Code                                           | Severity | Family                     | Message                                                                                                 | Next                              |
| ---------------------------------------------- | -------- | -------------------------- | ------------------------------------------------------------------------------------------------------- | --------------------------------- |
| library-detach.invalid-input                   | error    | invalid-input              |                                                                                                         |                                   |
| library-detach.confirmation-required           | error    | confirmation-required      |                                                                                                         |                                   |
| library-detach.invalid-id                      | error    | local                      | `<value> is not a valid Library ID.`                                                                    | `open-forge library list`         |
| library-detach.unknown-id                      | error    | unknown-id                 |                                                                                                         | `open-forge library list`         |
| library-detach.ownership-observation           | info     | ownership-observation      |                                                                                                         |                                   |
| library-detach.record-invalid                  | warning  | local                      | `The Library section of .agents/open-forge.lock.json is invalid: <reason>.`                             | `open-forge doctor`               |
| library-detach.record-unavailable              | warning  | lifecycle-unavailable      |                                                                                                         |                                   |
| library-detach.record-blocked                  | error    | lifecycle-blocked          |                                                                                                         |                                   |
| library-detach.registered-link-missing         | error    | local                      | `<path> is registered but no longer exists, so its removal cannot be confirmed.`                        | `open-forge library inspect <id>` |
| library-detach.mapping-blocked                 | error    | local                      | `<path> is <an ordinary file \| a folder \| a different link> and is not the link the Library created.`; when the occupant cannot be classified, `<path> could not be classified as a supported destination occupant.` | `open-forge library inspect <id>` |
| library-detach.destination-protected           | error    | local                      | `<path> is protected, owned by the source, or registered to another Library.`                          | `open-forge library list`         |
| library-detach.mapping-unavailable             | warning  | local                      | `<path> could not be checked, so nothing was changed.`                                                  | `open-forge doctor`               |
| library-detach.link-capability-unavailable     | error    | local                      | `This system cannot handle the file links Libraries use.`                                               | none                              |
| library-detach.consumer-blocked                | error    | local                      | `<path> is managed by Open Forge and cannot be changed by detach.`                                      | none                              |
| library-detach.ownership-conflict              | error    | ownership-conflict         |                                                                                                         |                                   |
| library-detach.permission-required             | error    | permission-required        |                                                                                                         |                                   |
| library-detach.permission-declined             | error    | permission-declined        |                                                                                                         |                                   |
| library-detach.permission-invalid              | error    | permissions-invalid        |                                                                                                         |                                   |
| library-detach.permission-unavailable          | warning  | permissions-unavailable    |                                                                                                         |                                   |
| library-detach.permission-changed              | error    | permissions-changed        |                                                                                                         |                                   |
| library-detach.permission-write-failed         | error    | permission-write-failed    |                                                                                                         |                                   |
| library-detach.generated-navigation-blocked    | error    | generated-region-unsafe    |                                                                                                         |                                   |
| library-detach.generated-navigation-incomplete | warning  | projection-unavailable     |                                                                                                         |                                   |
| library-detach.lock-unavailable                | error    | workspace-lock-unavailable |                                                                                                         |                                   |
| library-detach.recovery-unavailable            | warning  | recovery-unavailable       |                                                                                                         |                                   |
| library-detach.recovery-retained               | warning  | recovery-artifact-retained |                                                                                                         |                                   |
| library-detach.application-failed              | error    | write-failed               |                                                                                                         |                                   |
| library-detach.verification-failed             | error    | verification-failed        |                                                                                                         |                                   |
| library-detach.operation-failed                | error    | operation-failed           |                                                                                                         |                                   |
| library-detach.interrupted                     | error    | interrupted                |                                                                                                         |                                   |

## Effects wording

`Removed <N> links under <folder>` at `minimal`; rows `<path>  link removed`
at `standard`; `Updated the Entries section of <path>`; lock `registration
removed`. Dry run: `Would remove`. Partial: `removed`, `not started`, `final
state unknown`.

## Counts

`linksRemoved`, `sectionsUpdated`.

## JSON data by level

| Level    | `data`                                                                                                       |
| -------- | ------------------------------------------------------------------------------------------------------------ |
| minimal  | `{ mode, id, sourceFolder, destinationFolder, registrationRemoved: bool, permissions { ... } }` plus effects |
| standard | per effect `target`                                                                                          |
| full     | + `expectedStates`, `verification`, `recovery` details                                                       |

## References

- `src/cli/core/OpenForge.Cli.Core/Commands/Library/Detach/Shared/Rendering/*` — replaced by `Presentation/Library/Detach/`.
- Library detach interface Output (ledger only).

## Preconditions

- [ ] 03, 04, 36 merged.

## Steps

1. [ ] Write `LibraryDetachReportSelector` and `LibraryDetachDataTextRenderer`.
2. [ ] Wire the prompts per 04, adding `--automatic`.
3. [ ] Delete the old renderers and the remaining `Commands/Library/Shared/Rendering/*`.
4. [ ] Regenerate snapshots and review.
5. [ ] Three suites green.

## Acceptance

- [ ] `minimal` says the source files were kept.
- [ ] No `Record before change`, `Source location`, `Inside workspace` lines at any level.

## Changes ledger

- file layout/type: the legacy Library Detach renderers under `Presentation/Legacy/Library/Detach/` are deleted; native `Presentation/Library/Detach/` owns the data model, selector, text renderer, JSON context, wording, help and presentation. `CliLibraryComposer` closes the binding over it, leaving only Attach on the legacy bridge in that file.
- message: `Library detach completed.` and the `Status: complete` line are replaced by the catalogue headlines for the detached, no-links, dry-run, blocked, partial, no-record and cancelled outcomes.
- message: the confirmation was `Apply these changes? [y/N]`; it is now `Remove the <N> links listed above? [y/N]` unless `--automatic`, which is the wording this file's `Prompts` section requires. The code, not the catalogue, was wrong. Help and examples keep `--automatic`.
- JSON member: the legacy identity, record, projection, plan and application objects are replaced by command-owned `mode`, `id`, folders, `registrationRemoved`, `permissions`, `effects`, `expectedStates`, `verification` and `recovery`.
- status or exit: legacy shared status rendering is replaced by native status, stream and exit coordinates.
- effects/counts: the legacy plan and application graph is replaced by link, section and registration receipts, with `linksRemoved` and `sectionsUpdated`.
- text: full-level facts rendered raw enum states; they now render as sentence-form verification and recovery facts, per the shared wording rules.
- snapshots: the 44 legacy captures under `Commands/Library/Detach/__snapshots__/` are replaced by **120** captures at the four native detail levels, text and JSON, across 12 situations, under `src/cli/tests/integration/snapshots/LibraryDetachBeforeOutputSnapshotTests/`; this change adds the 10 `destination-protected` captures.
- test: legacy detach snapshots and callers are migrated to native unit, integration and published-process assertions. `PublishedLibraryDetachProcessTests` was migrated by the worker and passes 3/3 against the installed binary.
- test fixture: the shared fixture cleaned up the lock unconditionally; cleanup is now conditional for the no-op detach cases.
- evidence, overseer: re-run on the merge commit, not taken from the worker's report. Unit 3,374 passed, 0 failed, 0 skipped; integration 2,228 total, 2,211 passed, 0 failed, 17 skipped; `PublishedLibraryDetachProcessTests` and `PublishedLibrarySyncProcessTests` 3 passed each, the second because both commands share `LibraryPermissionTestPrompt`; `npm run check:dotnet` exactly the five documented errors.

- status or exit: `lock-held` returned `failed` at exit 1 with `library-detach.operation-failed` -> it now raises `library-detach.lock-unavailable` as `blocked` at exit 5, as required by the maintainer ruling.
- snapshots: the `lock-held` captures showed the failed/exit-1 operation-failed result -> they now show the blocked/exit-5 lock-unavailable result, as required by the maintainer ruling.
- message: the `lock-held` headline repeated the complete shared lock sentence -> it now uses the trailer-free shared lock reason, leaving exactly one `Nothing was changed.` supplied by the catalogue template; the finding message remains unchanged.
- snapshots: only the `Detach_lock-held` captures were regenerated for this wording correction; no other Detach capture changed.

- status or exit: malformed or duplicate Library records now return `incomplete` / exit 3 instead of blocked / exit 5; the existing incomplete status row already described record unreadability.
- message: Detach lock-held headlines use the trailer-free shared lock reason, and write-failed findings use the shared filesystem cause vocabulary; the finding's complete lock sentence remains unchanged.
- snapshots: regenerated eight `Detach_lock-held` and seven `Detach_write-failed-partial` captures across text and JSON detail levels; no other Detach situation changed.
- result/finding: `mapping-blocked` findings carried no occupant kind -> findings carry a command-local kind projected from the inspected destination leaf, and presentation renders that fact.
- selector: every `mapping-blocked` finding previously went through a throwing occupant-kind fallback -> known enum values render `an ordinary file`, `a folder` or `a different link`, while null kinds render the planner cause as a sentence.
- catalogue: `library-detach.confirmation-required` had no row -> the row now classifies the emitted shared-family code and leaves its message and next-action cells empty, as the shared family supplies them.
- test: no assertion covered all three occupant alternatives -> detach integration checks the carried kind for ordinary files, folders and both relative and absolute different links, while presentation checks each literal message.
- snapshots: the existing 11 Detach situations were expected to retain their bytes -> the update-enabled snapshot run rewrote no captures; `changed-occupant` already uses an ordinary file, while folder and different-link wording is covered by focused tests.
- edge case: unsupported, special and unresolved leaf states had no occupant kind -> the planner carries a command-local unclassified-cause sentence and the selector renders it without throwing or guessing one of the three supported kinds.
- rendering: one planner-level protected/source-owned/other-Library conflict carried no occupant kind -> it now uses `destination-protected`, whose selector renders the destination path and dedicated next action; the `mapping-blocked` cause fallback remains for genuinely unclassifiable occupants.

- status or exit: a planner-level protected/source-owned/other-Library conflict returned `library-detach.mapping-blocked` -> it now returns `library-detach.destination-protected`, blocked at exit 5.
- message: the protected conflict used `The Library destination is protected, source-owned, or registered to another Library.` -> it now renders `<path> is protected, owned by the source, or registered to another Library.`.
- action: the protected finding previously suggested inspecting the detached Library -> it now suggests `open-forge library list`.
- stop: the proposed `physical-alias` split is not reachable from a valid detach result. `LibraryRegistrationSet.Create` rejects duplicate portable destination keys before the planner, `HasDestinationAlias` only repeats that logical-key check, and the detach planning input carries no physical identity fact. No dead code or catalogue row was added; the situation is reported below.

## Divergences observed

1. **`lock-held` returns failed/exit 1 instead of the catalogue's blocked/exit 5
   — and [36](36-library-sync.md) reports the identical divergence.** Two
   independent workers found the same shape in two Library commands, so this is
   one shared defect rather than two catalogue errors. Every other command in
   the packet treats a held workspace lock as blocked. Existing behaviour was
   preserved in both lanes. **Maintainer decision, and it settles both.**

2. **`record-invalid` returns blocked/exit 5 instead of the catalogue's
   incomplete/exit 3.** Existing behaviour preserved. **Maintainer decision.**

3. **`mapping-blocked` cannot say what the occupant is.** The catalogue expects
   the row to distinguish a file, a folder or a different link; the existing
   result does not carry that fact. **Contract decision:** either the result
   gains it, or the row drops the distinction. Same shape as
   [25](25-route-move.md) divergence 4, where `reference-unsafe` is specified to
   render coordinates the result does not carry.

4. **`confirmation-required` is emitted but absent from this file's findings
   table.** A finding code that reaches the user with no catalogue row is
   unevidenced wording, the same defect as [19](19-references.md) divergence 2.
   **Maintainer decision:** add the row, or change the code.

5. **`interrupted-after-effects` reports a cause this file does not describe.**
   The existing cause differs from the catalogue's cancellation case. Native
   output was left unchanged. **Maintainer decision.**

6. **The worker ran the cross-command caller search and reported the result.**
   It recorded that no external test reader used the removed detach JSON
   members. This is the check added to the packet after two commands merged with
   a broken shared caller, and this is the first lane to perform it
   unprompted and state the outcome.

7. **The worker left its retired capture tree in place.** All 11 situations were
   converted and the 110 native captures written, but the 44 legacy files were
   not deleted; the overseer removed them before committing. Fourth lane in a
   row, after [29](29-extension-create.md), [36](36-library-sync.md) and
   [22](22-route-init.md).

8. **Merge conflict in a shared fixture, resolved by the overseer.**
   `Commands/Library/Shared/Permissions/LibraryPermissionTestPrompt.cs` is edited
   by every Library lane. Sync and Detach each removed their own legacy import
   and added their own native one, which git could not reconcile. Resolved on
   what the merged bodies call: `LibraryAttachLegacyPresentation` stays because
   Attach is still legacy; both `LibrarySync*` and `LibraryDetach*` native
   imports are kept; both legacy Sync and legacy Detach imports are dropped.
   Build and both published classes confirm it. **The Attach lane will conflict
   here too** and must be resolved the same way, after which the file has no
   legacy import left.

9. **The prescribed restore hit NuGet access and audit restrictions.** An
   offline restore plus a no-restore build succeeded. Same root cause as
   [24](24-route-update.md) divergence 3. No product divergence, and nothing
   leaked into the merge.

9. **The `lock-held` divergence was resolved by maintainer ruling.** The
   catalogue already specified blocked/exit 5; the code now raises
   `library-detach.lock-unavailable` with blocked status, because a held
   workspace lock is expected and retryable rather than an unexpected fault.

10. **The lock-held ruling makes an existing Detach wording composition
    reachable.** The old headline was `Library detach stopped after 0 of 1
    links were removed.`; the new headline is `Cannot detach team-knowledge:
    Another Open Forge command holds the workspace lock. Nothing was changed.
    Nothing was changed.`, while the catalogue template has one final
    `Nothing was changed.`. The frozen-string rule does not authorize changing
    this native composition. **Maintainer decision.**

11. **The lock-held wording composition is resolved without changing the
    catalogue headline or the trailer-bearing shared lock sentence.** Detach
    now uses the trailer-free shared reason only for the blocked headline;
    finding details continue to use the complete shared lock message.

12. **The `mapping-blocked` result was missing the destination occupant kind.**
    The Framework no-follow observation already distinguishes ordinary files,
    directories and relative/absolute links. Detach now projects those states
    into its result finding and the selector renders the three catalogue
    alternatives without cause-text or projection inference.

13. **The `confirmation-required` finding had no catalogue row.** The emitted
    message is `Library detach needs confirmation, and this session cannot ask.`;
    the row now classifies it as `confirmation-required` / `invalid-input` and
    leaves its message and next-action cells empty because the shared family
    supplies them; the code still supplies the existing
    `open-forge library detach --automatic` action.

14. **The observer also has leaf states outside the three-way catalogue.** A
    `ReparsePoint` has no immediate managed link target, `Special` is a device,
    and `Unknown` may represent an unresolved parent, so those states cannot be
    rendered honestly as an ordinary file, folder or different link. Detach
    leaves `OccupantKind` null and the planner supplies an explicit
    unclassified-cause sentence for the selector. **Maintainer decision:**
    preserve the distinction in the result and report the classification failure
    rather than inventing a kind. The ordinary-file, directory and link states
    are distinct and covered by this change; no such state appeared in
    regenerated captures.

15. **`mapping-blocked` overloaded three situations:** a non-Library occupant, a
    protected/source-owned/other-Library destination, and two destinations that
    resolve to the same physical path. The local catalogue message described only
    the first. The maintainer ruling is implemented for the reachable protected
    conflict: it now uses `destination-protected`, with its own message and next
    action; the null leaf fallback remains only for unclassifiable occupants.
    The physical-alias situation is not reachable from the validated detach
    result model: duplicate logical destination keys are rejected by
    `LibraryRegistrationSet.Create`, the existing alias helper does not resolve
    physical paths, and no physical identity is carried into the planner. It is
    therefore reported as a stop rather than given a dead code or catalogue row.

- The Detach catalogue already had the correct incomplete status headline. Its record-invalid finding severity was amended from error to warning to match the ruled status; no output template was changed.

## Rollback

Restore the bridge registration for library detach.
