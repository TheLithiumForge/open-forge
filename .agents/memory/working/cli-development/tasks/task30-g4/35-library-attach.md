---
open-forge:
  description: Library attach output catalogue and permission flow
  tags: [Memory, Working, CLI, Task, Plan, G4, Library, Contextual, Active]
---

# 35 — library attach

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md) and [04](04-interaction-system.md).

## Goal

`library attach` says which Library it registered and which links it
created, asks for permission when links go outside `.agents`, and reports a
partial run honestly with the recovery path.

## Depends on / Blocks

- Depends on: 03, 04, 34. Lane F.
- Blocks: 36.

## Shape

Change report.

## Situations

`attached-inside-agents`, `attached-outside-with-flag`, `permission-prompt`,
`permission-required-non-interactive`, `empty-source`, `duplicate-id` (blocked),
`source-missing` (invalid), `destination-collision` (blocked), `dry-run`,
`links-unsupported` (blocked), `lock-held`, `record-invalid` (incomplete),
`interrupted-partial`, `invalid-input`.

## Statuses and headlines

| Status                  | When                                                                          | Headline                                                                                     | Exit | Stream |
| ----------------------- | ----------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | registered and linked                                                         | `Registered the <id> Library from <source>.`                                                 |    0 | stdout |
| completed               | registered, source empty                                                      | `Registered the <id> Library from <source>. It has no eligible files yet.`                   |    0 | stdout |
| completed (dry run)     | planned                                                                       | `Would register the <id> Library from <source>.`                                             |    0 | stdout |
| completed-with-warnings | recovery bundle retained                                                      | + family row                                                                                 |    2 | stdout |
| incomplete              | source, inventory, Entries, record or recovery unreadable                    | `The <id> Library could not be attached: <limitation>. Nothing was changed.`                 |    3 | stdout |
| invalid-input           | bad ID, missing or non-folder source, bad `--to`                              | `Cannot attach <id>: <problem>.`                                                             |    4 | stderr |
| blocked                 | ID already registered, collision, unsafe, links unsupported, permission, lock | `Cannot attach <id>: <reason>.`                                                              |    5 | stderr |
| failed                  | after effects                                                                 | `Library attach stopped after <n> of <m> links were created.`                                |    1 | stderr |
| cancelled               | prompt cancelled, Ctrl+C                                                      | `Library attach was cancelled. Nothing was changed.` / `... Stopped after <n> of <m> links.` |  130 | stderr |

## Text by level

`minimal`:

```text
Registered the team-knowledge Library from shared/team.
  Created 12 links under docs
  Updated the Entries section of docs/_docs.md
  Saved a grant for docs to .agents/open-forge.json
```

`minimal`, permission required outside a terminal (stderr):

```text
Cannot attach team-knowledge: docs is outside .agents and no grant allows writing there.
Next: open-forge library attach team-knowledge shared/team --to docs --allow-path docs
```

`minimal`, interrupted after one link (stderr):

```text
Library attach was cancelled. Stopped after 1 of 2 links were created.
  docs/first.md    created
  docs/second.md   not started
  The Library was not recorded. Recovery data: <path>
Next: open-forge doctor
```

`standard` adds `Workspace:`, every link as a row with its target, the
inventory count, and the lock row.

`full` adds the permission evaluation, expected states, and verification
and recovery facts in words.

## Prompts

Permission (`Allow always / Allow once / Cancel`) for destinations outside
`.agents`; plan review; `Apply these changes? [y/N]` unless `--automatic`
(added by 04).

## Findings catalogue

| Code                                           | Severity | Family                     | Message                                                                         | Next                              |
| ---------------------------------------------- | -------- | -------------------------- | ------------------------------------------------------------------------------- | --------------------------------- |
| library-attach.invalid-input                   | error    | invalid-input              |                                                                                 |                                   |
| library-attach.confirmation-required           | error    | confirmation-required      |                                                                                 |                                   |
| library-attach.invalid-id                      | error    | local                      | `<value> is not a valid Library ID. Use lowercase letters, digits and hyphens.` | none                              |
| library-attach.duplicate-id                    | error    | local                      | `A Library with the ID <id> is already registered.`                             | `open-forge library inspect <id>` |
| library-attach.source-root-invalid             | error    | local                      | `<source> is not a folder inside the workspace.`                                | none                              |
| library-attach.source-root-unavailable         | warning  | local                      | `<source> cannot be read.`                                                      | none                              |
| library-attach.source-root-blocked             | error    | local                      | `<source> resolves to an unsafe location.`                                      | none                              |
| library-attach.destination-root-invalid        | error    | local                      | `--to <value> must be a folder inside the workspace.`                           | none                              |
| library-attach.destination-collision           | error    | local                      | `<path> already exists and is not a link this Library would create.`            | choose another `--to` folder      |
| library-attach.inventory-incomplete            | warning  | local                      | `Some files under <source> could not be listed.`                                | none                              |
| library-attach.mapping-unavailable             | warning  | local                      | `The link plan for <source> could not be completed: <reason>.`                  | `open-forge doctor`               |
| library-attach.mapping-blocked                 | error    | local                      | `<source path> cannot be linked: <reason>.`                                     | none                              |
| library-attach.link-capability-unavailable     | error    | local                      | `This system cannot create the file links Libraries need.`                      | none                              |
| library-attach.consumer-blocked                | error    | local                      | `<path> is managed by Open Forge and cannot receive Library links.`             | none                              |
| library-attach.ownership-conflict              | error    | ownership-conflict         |                                                                                 |                                   |
| library-attach.ownership-observation           | info     | ownership-observation      |                                                                                 |                                   |
| library-attach.record-invalid                  | warning  | local                      | `The Library section of .agents/open-forge.lock.json is invalid: <reason>.`     | `open-forge doctor`               |
| library-attach.record-unavailable              | warning  | lifecycle-unavailable      |                                                                                 |                                   |
| library-attach.record-blocked                  | error    | lifecycle-blocked          |                                                                                 |                                   |
| library-attach.permission-required             | error    | permission-required        |                                                                                 |                                   |
| library-attach.permission-declined             | error    | permission-declined        |                                                                                 |                                   |
| library-attach.permission-invalid              | error    | permissions-invalid        |                                                                                 |                                   |
| library-attach.permission-unavailable          | warning  | permissions-unavailable    |                                                                                 |                                   |
| library-attach.permission-changed              | error    | permissions-changed        |                                                                                 |                                   |
| library-attach.permission-write-failed         | error    | permission-write-failed    |                                                                                 |                                   |
| library-attach.generated-navigation-blocked    | error    | generated-region-unsafe    |                                                                                 |                                   |
| library-attach.generated-navigation-incomplete | warning  | projection-unavailable     |                                                                                 |                                   |
| library-attach.lock-unavailable                | error    | workspace-lock-unavailable |                                                                                 |                                   |
| library-attach.recovery-unavailable            | warning  | recovery-unavailable       |                                                                                 |                                   |
| library-attach.recovery-retained               | warning  | recovery-artifact-retained |                                                                                 |                                   |
| library-attach.application-failed              | error    | write-failed               | `Creating the link <path> failed. Stopped after <n> of <m> links.`              |                                   |
| library-attach.verification-failed             | error    | verification-failed        |                                                                                 |                                   |
| library-attach.operation-failed                | error    | operation-failed           |                                                                                 |                                   |
| library-attach.interrupted                     | error    | interrupted                |                                                                                 |                                   |

## Effects wording

`Created <N> links under <folder>` at `minimal`; rows `<path>  -> <target>`
at `standard`; `Updated the Entries section of <path>`; `Saved a grant for
<path> to .agents/open-forge.json`; lock `.agents/open-forge.lock.json
records the Library` at `standard`. Dry run: `Would create`, `Would update`,
`Would save`. Partial: `created`, `not started`, `final state unknown`.

## Counts

`linksCreated`, `sectionsUpdated`, `grantsSaved`, `sourceFiles`.

## JSON data by level

| Level    | `data`                                                                                                              |
| -------- | ------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ mode, id, sourceFolder, destinationFolder, recorded: bool, permissions { decision, required, missing, saved } }` |
| standard | + `links: [ { path, target } ]`, `inventory { eligible, excluded }`                                                 |
| full     | + `expectedStates`, `verification`, `recovery` details                                                              |

## References

- `src/cli/core/OpenForge.Cli.Core/Commands/Library/Attach/Shared/Rendering/*` — replaced by `Presentation/Library/Attach/`.
- `Commands/Library/Shared/Permissions/LibraryPermissionOperation.cs:42`, `LibraryPermissionPresentation.cs:24` — prompt per 04.
- Library attach interface Output (ledger only).

## Preconditions

- [ ] 03, 04, 34 merged.

## Steps

1. [ ] Write `LibraryAttachReportSelector` and `LibraryAttachDataTextRenderer`.
2. [ ] Wire the permission prompt, plan review and confirmation per 04, adding `--automatic`.
3. [ ] Delete the old renderers.
4. [ ] Regenerate snapshots and review.
5. [ ] Three suites green.

## Acceptance

- [ ] The permission refusal names the folder and the flag.
- [ ] The interrupted case names created and not-started links and the recovery path.

## Changes ledger

- file layout/type: the legacy Library Attach renderers under `Presentation/Legacy/Library/Attach/` are deleted; native `Presentation/Library/Attach/` owns the presentation, data model, selection, rendering, help and wording. `CliLibraryComposer` closes the binding over it. **With this lane the Library family is complete**: no legacy *command* binding remains in that composer, only the shared `Legacy/Library/Shared/Rendering` helpers.
- message: `Library attach preview completed.` is replaced by the catalogue headlines for the registered, preview, empty, refusal, failure and cancellation outcomes.
- message: the legacy plan and application status lines are replaced by the catalogue effect wording, link target rows, inventory, lock, permission, verification and recovery text.
- JSON member: the raw result graph is replaced by a level-gated shape — `minimal` carries `mode`, `id`, `sourceFolder`, `destinationFolder`, `recorded` and `permissions`; `standard` adds `links` and `inventory`; `full` adds `expectedStates`, `verification` and `recovery`.
- JSON member: raw permission action and outcome fields are replaced by `permissions.saved`, carrying the required and missing paths and a rerunnable `--allow-path` command.
- message: an invalid `--to` previously reported the **source** path back to the user; the exact supplied destination is now reported. This is a defect the worker found and fixed, not a catalogue change.
- flag/help: native help and the confirmation flow retain `--automatic`, which continues to bypass prompts.
- snapshots: the 48 legacy captures under `Commands/Library/Attach/__snapshots__/` are replaced by **119** captures at the four native detail levels, text and JSON, under `src/cli/tests/integration/snapshots/LibraryAttachBeforeOutputSnapshotTests/`.
- test: legacy snapshots and raw-graph assertions are migrated to native presentation tests, detail snapshots and migrated callers. `PublishedLibraryAttachProcessTests` was migrated by the worker and passes 3/3 against the installed binary.
- evidence, overseer: re-run on the merge commit, not taken from the worker's report. Unit 3,366 passed, 0 failed, 0 skipped; integration 2,228 total, 2,211 passed, 0 failed, 17 skipped; published classes for all three Library commands plus the shared grant journey pass — Attach 3, Shared grant 6, Sync 3, Detach 3, 0 failed; `npm run check:dotnet` exactly the five documented errors.

- status or exit: `lock-held` returned `failed` at exit 1 with `library-attach.operation-failed` -> it now raises `library-attach.lock-unavailable` as `blocked` at exit 5, as required by the maintainer ruling.
- snapshots: the `lock-held` captures showed the failed/exit-1 operation-failed result -> they now show the blocked/exit-5 lock-unavailable result, as required by the maintainer ruling.

- status or exit: malformed or duplicate Library records now return `incomplete` / exit 3 before attachment planning, with `library-attach.record-invalid`; lock-held remains blocked / exit 5.
- message: the existing attach lock-held output remains byte-identical, while shared cause rendering protects any record or write cause from raw .NET text.
- snapshots: no Attach capture changed; the published Attach class and the shared ownership boundary test cover the unchanged lock-held output and new record status.

- catalogue: added `library-attach.confirmation-required` with the shared
  `confirmation-required` family; the emitted finding is `Invalid` and exits 4.

## Divergences observed

- The runtime emits `library-attach.confirmation-required` when final
  confirmation is unavailable, but the findings catalogue had no row for it.
  The shared-family row now records that existing output; no source or capture
  changed.

1. **`lock-held` returns failed/exit 1 instead of the catalogue's blocked/exit 5
   — the third independent report of the same shape.**
   [36](36-library-sync.md) and [37](37-library-detach.md) record it too. Three
   workers, three commands, one behaviour: a held workspace lock surfaces as
   `operation-failed` with an `IOException` rather than
   `lock-unavailable`/blocked. Every other command in the packet treats a held
   lock as blocked. This is no longer a per-command catalogue question — it is a
   shared Library defect where the catalogue is very likely right and the code
   wrong. Existing behaviour was preserved in all three lanes.
   **Maintainer decision, and one fix settles all three.**

2. **`links-unsupported` has no deterministic fixture.** The catalogue lists the
   situation, but it cannot be produced reproducibly, so its row is the one
   situation in this batch that remains entirely unevidenced.
   **Test-owner decision:** build a fixture, or drop the situation.

3. **The `interrupted` fixture reports `1 of 1`, not the catalogue's `1 of 2`.**
   The fixture plans a single link. The catalogue's numbers are illustrative, so
   this is presumed benign, but it means the "partial" shape is captured with
   nothing actually left over. **Confirm.**

4. **The worker left its retired capture tree in place.** All situations were
   converted and the 119 native captures written, but the 48 legacy files were
   not deleted; the overseer removed them before committing. Fifth lane in a
   row, after [29](29-extension-create.md), [36](36-library-sync.md),
   [22](22-route-init.md) and [37](37-library-detach.md). The command packet
   does not ask for it, so this is chance, not instruction.

5. **Merge conflict in the shared fixture, resolved as [37](37-library-detach.md)
   divergence 8 predicted.**
   `Commands/Library/Shared/Permissions/LibraryPermissionTestPrompt.cs` conflicted
   again, this time between Attach's native imports and the stale legacy Detach
   and Sync imports from its older base. Resolved on what the merged body calls:
   `LibraryAttachPresentation` and `LibraryAttachData`. Because all three Library
   commands are now native, **the file contains no `Presentation.Legacy.Library`
   import at all**, which was asserted during the resolution rather than assumed.
   The build and all four published Library classes confirm it.

6. **Five of the six shared legacy Library renderers are now dead; one is not.**
   Measured after this merge, counting references outside
   `Presentation/Legacy/Library/Shared/` itself:

   | Renderer | External references | Held by |
   | --- | ---: | --- |
   | `LibraryApplicationHumanRenderer` | 0 | nothing |
   | `LibraryHumanText` | 0 | nothing |
   | `LibraryObservationHumanRenderer` | 1 | `LibraryHumanRendererTests` only |
   | `LibraryPermissionHumanRenderer` | 1 | `LibraryHumanRendererTests` only |
   | `LibraryPlanHumanRenderer` | 1 | `LibraryHumanRendererTests` only |
   | `LibraryHelpSections` | 1 | **`CliLibraryComposer`, production** |

   So five form a self-contained dead cluster with the single legacy test
   `Commands/Library/Shared/Rendering/LibraryHumanRendererTests.cs`, which exists
   only to exercise them: delete the test and all five go. `LibraryHelpSections`
   is still bound in production and must not be removed with them.
   **Belongs to [40](40-verification.md)**, which should delete the five and the
   test together, and decide separately what replaces `LibraryHelpSections`.

5. **The `lock-held` divergence was resolved by maintainer ruling.** The
   catalogue already specified blocked/exit 5; the code now raises
   `library-attach.lock-unavailable` with blocked status, because a held
   workspace lock is expected and retryable rather than an unexpected fault.

- The Attach status and finding catalogue rows needed the record case added to `incomplete` and its severity changed from error to warning. The headline template itself was not changed.

## Rollback

Restore the bridge registration for library attach.
