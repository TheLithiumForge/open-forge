---
open-forge:
  description: Route remove output catalogue
  tags: [Memory, Working, CLI, Task, Plan, G4, Route, Contextual, Active]
---

# 26 — route remove

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md) and [04](04-interaction-system.md).

## Goal

`route remove` lists every file it deleted, the Entries section it refreshed,
every incoming link it detached (the link markup goes, the text stays), and
where the recovery bundle is.

## Depends on / Blocks

- Depends on: 03, 04, 25. Lane D, last.
- Blocks: 40.

## Shape

Change report. Every deletion is listed at every level.

## Situations

`leaf-removed`, `leaf-with-detached-links`, `category-removed`, `dry-run`,
`source-not-found`, `managed-source` (blocked), `unsafe-link-detach`
(blocked), `ambiguous-source-prompt`, `reference-scan-incomplete`, `lock-held`,
`write-failed-partial`, `cancelled`.

## Statuses and headlines

| Status                  | When                                                                     | Headline                                                           | Exit | Stream |
| ----------------------- | ------------------------------------------------------------------------ | ------------------------------------------------------------------ | ---: | ------ |
| completed               | leaf removed                                                             | `Removed <path>`                                                   |    0 | stdout |
| completed               | category removed                                                         | `Removed the route <id>  (<N> files)`                              |    0 | stdout |
| completed (dry run)     | planned                                                                  | `Would remove <path>` / `Would remove the route <id>  (<N> files)` |    0 | stdout |
| completed-with-warnings | recovery bundle retained after success                                   | + family row                                                       |    2 | stdout |
| incomplete              | catalogue, scan or record unreadable                                     | `<id> could not be removed: <limitation>. Nothing was changed.`    |    3 | stdout |
| invalid-input           | bad source, the Loader, an overwrite file, missing source                | `Cannot remove <ref>: <problem>.`                                  |    4 | stderr |
| blocked                 | managed source, a link that cannot be detached safely, unsafe path, lock | `Cannot remove <id>: <reason>.`                                    |    5 | stderr |
| failed                  | after effects                                                            | `Route remove stopped after <n> of <m> changes.`                   |    1 | stderr |
| cancelled               | prompt cancelled, Ctrl+C                                                 | `Route remove was cancelled. Nothing was changed.`                 |  130 | stderr |

## Text by level

`minimal`:

```text
Removed .agents/memory/emerging/ideas/pricing/tiers.md
  Entry removed from .agents/memory/emerging/ideas/pricing/_pricing.md
  Detached 1 link that pointed at it; the link text was kept:
    .agents/maps/_maps.md:12:3
  The deleted file is kept in a recovery bundle at <recovery-path>.
Next: open-forge cleanup  (after reviewing the bundle)
```

`minimal`, category:

```text
Removed the route memory/projects/alpha  (6 files)
  .agents/memory/projects/alpha/_alpha.md
  .agents/memory/projects/alpha/plan.md
  ...
  Entry removed from .agents/memory/projects/_projects.md
  The deleted files are kept in a recovery bundle at <recovery-path>.
Next: open-forge cleanup  (after reviewing the bundle)
```

When the bundle was removed after verification (the ordinary case for route
remove), the bundle sentence and `Next` are omitted. Record in the ledger
which case applies; the contract says the bundle is deleted after
verification.

`standard` adds `Workspace:` and per detached link the text that remained
(`[Tiers](tiers.md) -> Tiers`).

`full` adds hashes and the scan summary.

## Prompts

Select when the source ID matches several files; plan review listing every
deletion, then `Delete the <N> files listed above? [y/N]` in a terminal
without `--automatic` (04 adds `--automatic` to this command).

## Findings catalogue

| Code                                       | Severity | Family                      | Message                                                                                            | Next                                                     |
| ------------------------------------------ | -------- | --------------------------- | -------------------------------------------------------------------------------------------------- | -------------------------------------------------------- |
| route-remove.invalid-input                 | error    | invalid-input               |                                                                                                    |                                                          |
| route-remove.confirmation-required         | error    | confirmation-required       |                                                                                                    |                                                          |
| route-remove.invalid-source                | error    | local                       | `<ref> is not a source ID or a path under .agents.`                                                | `open-forge route list --depth=all`                      |
| route-remove.source-not-found              | error    | unknown-source              |                                                                                                    |                                                          |
| route-remove.invalid-subject               | error    | local                       | `<ref> is the Loader and cannot be removed.` / `<ref> is an overwrite file; remove its base file.` | none                                                     |
| route-remove.workspace-unavailable         | error    | workspace-unavailable       |                                                                                                    |                                                          |
| route-remove.workspace-unsafe              | error    | workspace-unsafe            |                                                                                                    |                                                          |
| route-remove.source-unsafe                 | error    | source-unsafe               |                                                                                                    |                                                          |
| route-remove.category-unsafe               | error    | local                       | `<folder> contains a file that cannot be removed safely: <path> (<reason>).`                       | none                                                     |
| route-remove.route-ambiguous               | error    | route-ambiguous             |                                                                                                    |                                                          |
| route-remove.identity-collision            | error    | identity-collision          | (the prompt resolves it in a terminal)                                                             |                                                          |
| route-remove.overwrite-ambiguous           | error    | local                       | `<name>.overwrite.md could belong to more than one base file.`                                     | fix by hand                                              |
| route-remove.ownership-claimed             | error    | ownership-claimed           |                                                                                                    | `open-forge update` / `open-forge extension remove <id>` |
| route-remove.reference-unsafe              | error    | local                       | `The link at <file>:l:c cannot be detached safely: <reason>.`                                      | fix by hand                                              |
| route-remove.generated-region-unsafe       | error    | generated-region-unsafe     |                                                                                                    |                                                          |
| route-remove.workspace-lock-unavailable    | error    | workspace-lock-unavailable  |                                                                                                    |                                                          |
| route-remove.target-changed                | error    | target-changed              |                                                                                                    |                                                          |
| route-remove.recovery-conflict             | error    | recovery-conflict           |                                                                                                    |                                                          |
| route-remove.ownership-unavailable         | warning  | lifecycle-unavailable       |                                                                                                    |                                                          |
| route-remove.inspection-incomplete         | warning  | inspection-incomplete       |                                                                                                    |                                                          |
| route-remove.category-inventory-incomplete | warning  | local                       | `Some files under <folder> could not be listed, so the removal was not planned.`                   | `open-forge doctor`                                      |
| route-remove.reference-coverage-incomplete | warning  | local                       | `Some files could not be scanned for links to <path>, so the removal was not planned.`             | `open-forge doctor`                                      |
| route-remove.projection-incomplete         | warning  | projection-unavailable      |                                                                                                    |                                                          |
| route-remove.recovery-unavailable          | warning  | recovery-unavailable        |                                                                                                    |                                                          |
| route-remove.recovery-artifact-retained    | warning  | recovery-artifact-retained  |                                                                                                    |                                                          |
| route-remove.target-changed-during-apply   | error    | target-changed-during-apply |                                                                                                    |                                                          |
| route-remove.write-failed                  | error    | write-failed                |                                                                                                    |                                                          |
| route-remove.verification-failed           | error    | verification-failed         |                                                                                                    |                                                          |
| route-remove.recovery-failed               | error    | recovery-failed             |                                                                                                    |                                                          |
| route-remove.operation-failed              | error    | operation-failed            |                                                                                                    |                                                          |
| route-remove.interrupted                   | error    | interrupted                 |                                                                                                    |                                                          |

## Effects wording

`Removed <path>` rows for every deleted file (category), `Entry removed from
<parent>`, `Detached <N> links that pointed at it; the link text was kept:`
with `<file>:l:c` rows, dry run `Would remove`, `Would detach`. Partial:
`removed`, `not started`, `final state unknown`.

## Counts

`filesRemoved`, `sectionsUpdated`, `linksDetached`, `filesScanned`.

## JSON data by level

| Level    | `data`                                                                                                                |
| -------- | --------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ mode, subject: "file" \| "route", source { id, path }, removed: [ path ], detachedLinks: [ { path, location } ] }` |
| standard | + per link `before`, `after` text                                                                                     |
| full     | + per effect `before`, `after`, `scan { filesScanned, occurrences }`                                                  |

## References

- `src/cli/core/OpenForge.Cli.Core/Commands/Route/Remove/Shared/Rendering/*` — replaced by `Presentation/Route/Remove/`.
- `RouteRemoveSubjectSelector` prompt site — per 04.
- Route remove interface Complete Reference Pass And Detachment (ledger only).

## Preconditions

- [ ] 03, 04, 25 merged.

## Steps

1. [ ] Write `RouteRemoveReportSelector` and `RouteRemoveDataTextRenderer`.
2. [ ] Wire the prompts per 04, including the new `--automatic`.
3. [ ] Delete the old renderers.
4. [ ] Regenerate snapshots and review.
5. [ ] Three suites green.

## Acceptance

- [ ] Every deleted file is a row at `minimal`.
- [ ] Every detached link is a row at `minimal`.
- [ ] The recovery sentence appears exactly when a bundle remains.

## Changes ledger

- file layout/type: the legacy Route Remove renderers and `Commands/Route/Remove/Models/Presentation/RouteRemoveJsonDocument` are deleted; native `Presentation/Route/Remove/` owns the selector, renderers, model, help and wiring. `Presentation/Legacy/Route/Remove/` holds no files. **With this lane the Route family is complete** — List, Inspect, Init, Create, Update, Move and Remove are all native, and `CliRouteComposer` retains only the shared `Legacy/Route/Shared/Rendering` helpers.
- text: the legacy status and metadata output is replaced by the catalogue headlines, effect rows, recovery facts and next-action wording, at each detail level.
- JSON member: the legacy operation-shaped data is replaced by levelled schema v3 `mode`, `subject`, `source`, `removed`, `detachedLinks` and scan data.
- JSON member: detached links carried legacy location objects; they now carry the path plus a `line:column` location, with before and after content at `standard`.
- flag/help: `--automatic` is added, with native confirmation handling.
- test: legacy status, machine-code and JSON-member assertions are migrated. `PublishedRouteRemoveProcessTests` was migrated by the worker and passes 3/3 against the installed binary.
- snapshots: the 48 legacy captures under `Commands/Route/Remove/__snapshots__/` are replaced by **119** captures at the four native detail levels, text and JSON, across the 12 catalogue situations, under `src/cli/tests/integration/snapshots/RouteRemoveBeforeOutputSnapshotTests/`.
- test identity: the theory cases needed explicit per-situation identities for snapshot isolation; they now have them.
- evidence, overseer: re-run on the merge commit, not taken from the worker's report. Unit 3,261 passed, 0 failed, 0 skipped; integration 2,208 total, 2,191 passed, 0 failed, 17 skipped; `PublishedRouteRemoveProcessTests` 3 passed; `PublishedShellBoundaryProcessTests` 26 passed; `npm run check:dotnet` exactly the five documented errors.
- status/exit: a missing source previously flowed through the absence fallback as completed with exit 0 and no finding -> it now returns invalid-input with exit 4 and `route-remove.source-not-found`; resolved-source planning and legitimate no-effect completion remain unchanged.
- test: the Route Remove snapshot and direct integration expectations for `source-not-found` now assert invalid-input and the source-not-found finding; the absence-planning tests retain residual-reference evidence, `target-changed`, `invalid-subject` for an overwrite companion, and `ownership-claimed`; the owned published-process repeat-after-removal expectation asserts JSON invalid-input with exit 4 and text invalid-input with exit 4 on stderr.
- snapshots: all 10 `source-not-found` captures now show the catalogue headline, one finding, zero scan effects, and the invalid-input next action; no other Route Remove situation changed.

- message: Route Remove cause-bearing wording now delegates to the shared cause vocabulary; no removal status or exit mapping changed.
- snapshots: regenerated the owned Route Remove class; no primary capture changed, and its four raw debug-diagnostic captures remained unchanged.

- catalogue: added `route-remove.confirmation-required` with the shared
  `confirmation-required` family; the emitted finding is `Invalid` at exit 4
  (`RouteRemoveResultBuilder.cs:30`). It reached users with no row, the same
  gap found across nine commands.

## Divergences observed

1. **Removing a source that does not exist reports success.** This file requires
   invalid-input with a `route-remove.source-not-found` finding; the operation
   returns **completed** with `Nothing to do.` and **no finding at all**, so a
   typo in the source id is indistinguishable from a successful no-op. Existing
   behaviour was preserved and no capture was regenerated over it.
   **Maintainer decision.**

   This is the clearest instance of a pattern now visible across the batch: the
   error paths have drifted toward permissiveness relative to the catalogues.
   Alongside it sit [36](36-library-sync.md) divergence 1, where
   `registered-link-gone` **creates** the link the catalogue says to refuse;
   [20](20-route-list.md) divergences 1 and 2, where unreadable metadata and a
   malformed loader both downgrade to incomplete; and the `lock-held` finding
   reported independently by [35](35-library-attach.md), [36](36-library-sync.md)
   and [37](37-library-detach.md). These are better decided as one question
   about intended strictness than as eleven separate wording calls.

2. **The worker found and migrated stale cross-folder readers.** It reports that
   legacy readers of Route Remove output existed outside the command's own
   output tests and were migrated with the rest. This is the check added to the
   packet after [21](21-route-inspect.md) shipped a broken
   `PublishedShellBoundaryProcessTests` that went unnoticed across two merges —
   the third lane to run it, and the first to actually find something.

3. **The worker left its retired capture tree in place.** All 12 situations were
   converted and the 119 native captures written, but the 48 legacy files were
   not deleted; the overseer removed them before committing. Seventh lane with
   this omission.

4. **Merge conflict in a shared help test, resolved by the overseer.**
   `Shell/Presentation/Shared/Help/CliResultHelpTests.cs` conflicted four ways:
   the branch carried native Remove with legacy List, Move and Update, while
   HEAD carried native List, Move and Update with legacy Remove — each side
   having branched before the others merged. Resolved by keeping all four native
   help imports and dropping every legacy one, then asserting that no
   `Presentation.Legacy.Route` import survived. The legacy imports remaining in
   that file are Doctor and Update, which is exactly the set of commands still
   unconverted at this point. That correspondence — surviving legacy imports
   matching the unconverted set — has now held across four resolutions and is a
   cheap check that a family is genuinely finished rather than merely compiling.

5. **The first strictness correction pre-empted the absence-planning gateway.**
   Returning the resolver boundary directly changed residual-reference,
   target-changed, overwrite, and ownership observations into
   `source-not-found`. The gateway is restored: absence planning still runs,
   the original source-not-found boundary is returned only when it finds no
   retained evidence, and the affected assertions and test names are restored.
   The overwrite companion is classified as `invalid-subject`/invalid per the
   catalogue; no maintainer decision is pending.

- The broad sweep found four raw exception/HRESULT matches in existing Route Remove `*.debug.diagnostics.txt` files only. There was no minimal or standard leak and no new cause category was needed.

## Rollback

Restore the bridge registration for route remove.
