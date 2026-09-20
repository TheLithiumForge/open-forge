---
open-forge:
  description: Route move output catalogue
  tags: [Memory, Working, CLI, Task, Plan, G4, Route, Contextual, Active]
---

# 25 — route move

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md) and [04](04-interaction-system.md).

## Goal

`route move` says what moved, which Entries sections it refreshed, and which
links it rewrote so they still resolve. A category move lists every member.

## Depends on / Blocks

- Depends on: 03, 04, 24. Lane D.
- Blocks: 26.

## Shape

Change report.

## Behavior this catalogue reflects

The complete reference pass rewrites every authored local link whose
destination would otherwise stop resolving after the move. Rewritten links
are effects, not warnings.

## Situations

`leaf-move`, `leaf-move-with-rewritten-links`, `category-move`, `dry-run`,
`destination-exists` (blocked), `destination-inside-source` (invalid),
`self-move` (invalid), `managed-source` (blocked), `source-not-found`,
`ambiguous-source-prompt`, `reference-scan-incomplete` (incomplete),
`lock-held`, `write-failed-partial`, `cancelled`.

## Statuses and headlines

| Status                  | When                                                                             | Headline                                                      | Exit | Stream |
| ----------------------- | -------------------------------------------------------------------------------- | ------------------------------------------------------------- | ---: | ------ |
| completed               | leaf moved                                                                       | `Moved <id> to <new path>`                                    |    0 | stdout |
| completed               | category moved                                                                   | `Moved the route <id> to <new folder>  (<N> files)`           |    0 | stdout |
| completed (dry run)     | planned                                                                          | `Would move <id> to <new path>`                               |    0 | stdout |
| completed-with-warnings | recovery bundle retained                                                         | + family row                                                  |    2 | stdout |
| incomplete              | catalogue, reference scan or record unreadable                                   | `<id> could not be moved: <limitation>. Nothing was changed.` |    3 | stdout |
| invalid-input           | bad source or destination, self move, destination inside source, consumed source | `Cannot move <ref>: <problem>.`                               |    4 | stderr |
| blocked                 | destination exists, managed source, unsafe, lock                                 | `Cannot move <id>: <reason>.`                                 |    5 | stderr |
| failed                  | after effects                                                                    | `Route move stopped after <n> of <m> changes.`                |    1 | stderr |
| cancelled               | prompt cancelled, Ctrl+C                                                         | `Route move was cancelled. Nothing was changed.`              |  130 | stderr |

## Text by level

`minimal`, leaf with rewritten links:

```text
Moved memory/emerging/ideas/pricing/tiers to .agents/memory/emerging/ideas/pricing/tier-options.md
  Entry updated in .agents/memory/emerging/ideas/pricing/_pricing.md
  Rewrote 2 links that pointed at the old path:
    .agents/maps/_maps.md:12:3
    .agents/guidance/team.md:40:5
```

`minimal`, category:

```text
Moved the route memory/projects/alpha to .agents/memory/archived/alpha  (6 files)
  .agents/memory/projects/alpha/_alpha.md        -> .agents/memory/archived/alpha/_alpha.md
  .agents/memory/projects/alpha/plan.md          -> .agents/memory/archived/alpha/plan.md
  ...
  Entry removed from .agents/memory/projects/_projects.md
  Entry added to .agents/memory/archived/_archived.md
  Rewrote 1 link that pointed at the old paths: .agents/maps/_maps.md:20:3
```

`standard` adds `Workspace:` and per rewritten link `<old destination> ->
<new destination>`, plus the overwrite file rows when a pair moved.

`full` adds hashes per effect and the reference scan summary (`28 files
scanned`).

## Prompts

Select when the source ID matches several files.

## Findings catalogue

| Code                                     | Severity | Family                      | Message                                                                                                | Next                                |
| ---------------------------------------- | -------- | --------------------------- | ------------------------------------------------------------------------------------------------------ | ----------------------------------- |
| route-move.invalid-input                 | error    | invalid-input               |                                                                                                        |                                     |
| route-move.invalid-source                | error    | local                       | `<ref> is not a source ID or a path under .agents.`                                                    | `open-forge route list --depth=all` |
| route-move.source-not-found              | error    | unknown-source              | (also the repeated-move case)                                                                          |                                     |
| route-move.invalid-subject               | error    | local                       | `<ref> is the Loader and cannot be moved.` / `<ref> is an overwrite file; move its base file.`         | none                                |
| route-move.invalid-destination           | error    | local                       | `<target> must be a Markdown file path under .agents.` / `... an entrypoint path when moving a route.` | `open-forge route move --help`      |
| route-move.self-move                     | error    | local                       | `The source and the destination are the same.`                                                         | none                                |
| route-move.destination-inside-source     | error    | local                       | `The destination is inside the folder being moved.`                                                    | none                                |
| route-move.workspace-unavailable         | error    | workspace-unavailable       |                                                                                                        |                                     |
| route-move.workspace-unsafe              | error    | workspace-unsafe            |                                                                                                        |                                     |
| route-move.source-unsafe                 | error    | source-unsafe               |                                                                                                        |                                     |
| route-move.destination-unsafe            | error    | target-unsafe               |                                                                                                        |                                     |
| route-move.destination-occupied          | error    | local                       | `<path> already exists.`                                                                               | choose another destination          |
| route-move.destination-parent-missing    | error    | local                       | `<folder> does not exist or has no entrypoint, so the moved file could not be listed.`                 | `open-forge route init <id>`        |
| route-move.category-unsafe               | error    | local                       | `<folder> contains a file that cannot be moved safely: <path> (<reason>).`                             | none                                |
| route-move.route-ambiguous               | error    | route-ambiguous             |                                                                                                        |                                     |
| route-move.identity-collision            | error    | identity-collision          | (blocking when the new ID would collide; the prompt resolves source ambiguity)                         |                                     |
| route-move.overwrite-ambiguous           | error    | local                       | `<name>.overwrite.md could belong to more than one base file.`                                         | fix by hand                         |
| route-move.ownership-claimed             | error    | ownership-claimed           |                                                                                                        | `open-forge update` / the Extension |
| route-move.reference-unsafe              | error    | local                       | `The link at <file>:l:c cannot be rewritten safely: <reason>.`                                         | fix by hand                         |
| route-move.generated-region-unsafe       | error    | generated-region-unsafe     |                                                                                                        |                                     |
| route-move.workspace-lock-unavailable    | error    | workspace-lock-unavailable  |                                                                                                        |                                     |
| route-move.target-changed                | error    | target-changed              |                                                                                                        |                                     |
| route-move.recovery-conflict             | error    | recovery-conflict           |                                                                                                        |                                     |
| route-move.ownership-unavailable         | warning  | lifecycle-unavailable       |                                                                                                        |                                     |
| route-move.inspection-incomplete         | warning  | inspection-incomplete       |                                                                                                        |                                     |
| route-move.category-inventory-incomplete | warning  | local                       | `Some files under <folder> could not be listed, so the move was not planned.`                          | `open-forge doctor`                 |
| route-move.reference-coverage-incomplete | warning  | local                       | `Some files could not be scanned for links to <path>, so the move was not planned.`                    | `open-forge doctor`                 |
| route-move.projection-incomplete         | warning  | projection-unavailable      |                                                                                                        |                                     |
| route-move.recovery-unavailable          | warning  | recovery-unavailable        |                                                                                                        |                                     |
| route-move.recovery-artifact-retained    | warning  | recovery-artifact-retained  |                                                                                                        |                                     |
| route-move.target-changed-during-apply   | error    | target-changed-during-apply |                                                                                                        |                                     |
| route-move.write-failed                  | error    | write-failed                |                                                                                                        |                                     |
| route-move.verification-failed           | error    | verification-failed         |                                                                                                        |                                     |
| route-move.recovery-failed               | error    | recovery-failed             |                                                                                                        |                                     |
| route-move.operation-failed              | error    | operation-failed            |                                                                                                        |                                     |
| route-move.interrupted                   | error    | interrupted                 |                                                                                                        |                                     |

## Effects wording

`<old> -> <new>` per moved file; `Entry updated in <parent>` / `Entry
removed from <old parent>` / `Entry added to <new parent>`; `Rewrote <N>
links that pointed at the old path:` with `<file>:l:c` rows; dry run uses
`Would move`, `Would rewrite`. Partial: `moved`, `not started`, `final state
unknown`.

## Counts

`filesMoved`, `sectionsUpdated`, `linksRewritten`, `filesScanned`.

## JSON data by level

| Level    | `data`                                                                                                                                                 |
| -------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| minimal  | `{ mode, subject: "file" \| "route", source { id, path }, destination { id, path }, moved: [ { from, to } ], rewrittenLinks: [ { path, location } ] }` |
| standard | + per link `from`, `to` destinations, `overwrite { from, to }`                                                                                         |
| full     | + per effect `before`, `after`, `scan { filesScanned, occurrences }`                                                                                   |

## References

- `src/cli/core/OpenForge.Cli.Core/Presentation/Legacy/Route/Move/Shared/Rendering/*` — deleted, replaced by `Presentation/Route/Move/`. (Corrected during [41](41-documentation-propagation.md): this line previously named `Commands/Route/Move/Shared/Rendering/*`, a path the sources had already left.)
- `RouteMoveSubjectSelector` prompt site — per 04.
- Route move interface Complete Reference Pass, Human output (ledger only).

## Preconditions

- [ ] 03, 04, 24 merged.

## Steps

1. [ ] Write `RouteMoveReportSelector` and `RouteMoveDataTextRenderer`.
2. [ ] Wire the ambiguity prompt.
3. [ ] Delete the old renderers.
4. [ ] Regenerate snapshots and review.
5. [ ] Three suites green.

## Acceptance

- [ ] Every moved file and every rewritten link is a row at `minimal`.
- [ ] No `file:<hash>` notation below `full`.

## Changes ledger

- file layout/type: the legacy Route Move renderers under `Presentation/Legacy/Route/Move/` (10 files) and `Commands/Route/Move/Models/Presentation/RouteMoveJsonDocument` are deleted; native `Presentation/Route/Move/` owns the data model, selector, text renderer, wording, help and JSON context. `CliRouteComposer` closes the binding over it.
- message: the completion headlines `The routed file was moved.` and `The routed category was moved.` are replaced by `Moved <id> to <new path>` and `Moved the route <id> to <new folder>  (<N> files)`.
- message: the dry-run path replaced the legacy completed-status output with `Would move <id> to <new path>` followed by `No files were changed.`
- message: the legacy status, identity, projection and diagnostic blocks are replaced by native workspace, entry, rewritten-link, overwrite, effect and scan rows.
- JSON member: the legacy nested operation graph is replaced by native `data` carrying `mode`, `subject`, source and destination, `moved`, `rewrittenLinks`, overwrite and link destinations, and the full scan facts at `full`.
- stream: the legacy presentation bridge is replaced by shared report selection and rendering, with stdout/stderr routing chosen by result status.
- file layout: help moved from `Presentation/Legacy/Route/Move/Shared/Rendering` to `Presentation/Route/Move/Shared/Help`; the wording is unchanged.
- snapshots: 56 legacy integration captures and 4 legacy unit captures are replaced by **139** reviewed native captures under `src/cli/tests/integration/snapshots/RouteMoveBeforeOutputSnapshotTests/`, across 14 situations at four levels in text and JSON. The four deleted unit captures came from the shared root `__snapshots__` tree, which is part of the orphan set [40](40-verification.md) inherits.
- test: legacy renderer and result-field assertions are migrated to native `CliReport`, data and finding assertions. `PublishedRouteMoveProcessTests` was migrated by the worker and passes 3/3 against the installed binary.
- strictness: `RouteMoveDestinationProjector` now classifies only `self-move` and `destination-inside-source` as invalid input; destination occupancy, unsafe paths, locks, target changes and other environmental refusals remain blocked.
- snapshots: regenerated only the 12 JSON/debug captures for `self-move` and `destination-inside-source`; their human text headlines and all other Route Move captures remained unchanged.
- evidence, worker: affected Route Move planning and published classes passed; the full gates passed with unit 3,179/0/0 and integration 2,219 total/2,202 passed/0 failed/17 skipped. Whitespace retained the five documented pre-existing errors.
- doc: this file's `References` section names `Commands/Route/Move/Shared/Rendering/*`; the sources actually deleted were under `Presentation/Legacy/Route/Move/Shared/Rendering/*`. The reference is stale.
- evidence, overseer: re-run on the merge commit, not taken from the worker's report. Unit 3,406 passed, 0 failed, 0 skipped; integration 2,234 total, 2,217 passed, 0 failed, 17 skipped; `PublishedRouteMoveProcessTests` 3 passed; `PublishedShellBoundaryProcessTests` 26 passed, checked because it is a cross-command reader of Route output; `npm run check:dotnet` exactly the five documented errors.

- message: Route Move cause-bearing wording now delegates to the shared cause vocabulary; the self-move and destination-inside-source invalid-input rulings remain unchanged.
- snapshots: regenerated the owned Route Move class; no primary capture changed, and its four raw debug-diagnostic captures remained unchanged.

## Divergences observed

1. **Resolved: `self-move` is invalid input.** The destination projector now
   returns invalid-input at exit 4, with the existing finding and wording
   preserved.

2. **Resolved: `destination-inside-source` is invalid input.** The destination
   projector now returns invalid-input at exit 4; other destination and
   environment safety boundaries remain blocked.

3. **`identity-collision` severity disagrees between two catalogues.** This
   file's table says error; the shared finding table says warning. The existing
   error severity was preserved. **The two catalogues must be reconciled**, not
   just this command.

4. **`reference-unsafe` cannot render the location the catalogue requires.** The
   catalogue asks for `file:line:column`; the result carries no coordinates, so
   the row renders the path only. **Contract decision:** either the result must
   carry coordinates, or the catalogue row must drop them.

5. **`reference-scan-incomplete` next-action reason leaks internal vocabulary.**
   The catalogue gives `open-forge doctor` alone; the existing shared reason
   text adds internal wording. Left unchanged. **Wording decision.**

6. **`ownership-unavailable` lost its dedicated no-op headline.** The legacy
   renderer had a distinct no-op message for this case; the native path emits
   the ordinary move or would-move headline plus a warning row. **Catalogue
   decision:** whether the dedicated headline should be restored.

7. **A parser-level missing operand publishes `data: null`.** This file's
   command data table implies `data` is always an object. At the parser
   boundary, before the command runs, `data` is null. Left unchanged.
   **Document or confirm this boundary.**

8. **This file's `References` section points at paths that no longer exist.**
   It names `Commands/Route/Move/Shared/Rendering/*`; the legacy sources lived
   under `Presentation/Legacy/Route/Move/Shared/Rendering/*`. A named symbol
   that no longer exists is a divergence. **Durable record to change:** this
   file's References.

9. **The sandbox cannot read the user's NuGet configuration.** The prescribed
   build could not read `%APPDATA%\NuGet\NuGet.Config`; the worker rebuilt
   through isolated temporary .NET and build directories. Same cause as
   [24](24-route-update.md) divergence 3. No product divergence.

10. **The shared compiler could not create DLL outputs in the sandbox.** The
    final build and gates succeeded with `-p:UseSharedCompilation=false` and the
    prescribed offline NuGet flags; no product files were affected.

- The broad sweep found four raw exception/HRESULT matches in existing Route Move `*.debug.diagnostics.txt` files only. There was no minimal or standard leak; the recent invalid-input statuses were not revisited.

## Rollback

Restore the bridge registration for route move.
