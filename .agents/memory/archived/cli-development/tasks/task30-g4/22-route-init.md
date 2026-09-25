---
open-forge:
  description: Route init output catalogue and the complete-status change
  tags: [Memory, CLI, Task, Plan, G4, Route, Contextual, Archived, Historical]
---

# 22 — route init

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md).

## Goal

`route init` says which entrypoints it created and where they were listed,
tells the user that the placeholders need editing, and exits 0 when it did
its job. Escaped file bodies never appear in text.

## Depends on / Blocks

- Depends on: 03, 05 (block boundary), 21. Lane D.
- Blocks: 23.

## Shape

Change report.

## Status change (C9)

A scaffold whose description and tags are placeholders is `completed`, exit
0, with the advisory sentence and `needsAuthoring: true` in JSON. The finding
`route-init.needs-authoring` becomes `info`. Record the change in the ledger:
contract, help and docs currently say `attention`.

## Situations

`new-chain`, `already-initialized`, `dry-run`, `framework-scaffold`,
`explicit-metadata`, `invalid-target`, `invalid-metadata`,
`framework-not-installed` (blocked), `lock-held`, `write-failed-partial`,
`cancelled`.

## Statuses and headlines

| Status                  | When                                                                   | Headline                                                                 | Exit | Stream |
| ----------------------- | ---------------------------------------------------------------------- | ------------------------------------------------------------------------ | ---: | ------ |
| completed               | entrypoints created                                                    | `Created <path>` (one entrypoint) / `Created <N> entrypoints for <id>.`  |    0 | stdout |
| completed               | already initialized                                                    | `<id> is already initialized. Nothing to do.`                            |    0 | stdout |
| completed (dry run)     | planned                                                                | `Would create <path>` / `Would create <N> entrypoints for <id>.`         |    0 | stdout |
| completed-with-warnings | recovery bundle retained                                               | + family row                                                             |    2 | stdout |
| incomplete              | a fact could not be read                                               | `The route could not be initialized: <limitation>. Nothing was changed.` |    3 | stdout |
| invalid-input           | bad target or metadata                                                 | `Cannot initialize <target>: <problem>.`                                 |    4 | stderr |
| blocked                 | unsafe target, ambiguity, Framework mode needs install or update, lock | `Cannot initialize <target>: <reason>.`                                  |    5 | stderr |
| failed                  | after effects                                                          | `Route init stopped after <n> of <m> changes.`                           |    1 | stderr |
| cancelled               | Ctrl+C                                                                 | `Route init was cancelled. Nothing was changed.`                         |  130 | stderr |

## Text by level

`minimal`, one new entrypoint:

```text
Created .agents/memory/emerging/ideas/pricing/_pricing.md
  Listed in .agents/memory/emerging/ideas/_ideas.md
  Its description and tags are placeholders. Edit them before relying on this route.
```

`minimal`, a chain of two with explicit metadata:

```text
Created 2 entrypoints for memory/projects/alpha.
  .agents/memory/projects/_projects.md
  .agents/memory/projects/alpha/_alpha.md
  Listed in .agents/memory/_memory.md
```

`minimal`, dry run:

```text
Would create .agents/memory/emerging/ideas/pricing/_pricing.md
  Would list it in .agents/memory/emerging/ideas/_ideas.md
No files were changed.
```

`standard` adds `Workspace:`, every entrypoint in the chain with `created` or
`already present`, the scaffold kind (`generic` or `Framework`), the metadata
written, and the lock row when Framework ownership was recorded.

`full` shows the content of each created file verbatim with real line breaks
under a `--- <path> (new file) ---` header, the before and after hashes of
rewritten Entries sections, and recovery facts in words.

## Findings catalogue

| Code                                     | Severity | Family                       | Message                                                                                                                                                                                                | Next                           |
| ---------------------------------------- | -------- | ---------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------ |
| route-init.invalid-input                 | error    | invalid-input                |                                                                                                                                                                                                        |                                |
| route-init.invalid-target                | error    | local                        | `<target> is not a route ID or an entrypoint path under .agents.` / `The Loader cannot be initialized.`                                                                                                | `open-forge route init --help` |
| route-init.invalid-metadata              | error    | local                        | `--description must not be blank.` / `--tag <value> is not a valid tag: use letters and digits with single hyphens.` / `--framework cannot be combined with --description, --responsibility or --tag.` | corrected command              |
| route-init.workspace-unavailable         | error    | workspace-unavailable        |                                                                                                                                                                                                        |                                |
| route-init.workspace-unsafe              | error    | workspace-unsafe             |                                                                                                                                                                                                        |                                |
| route-init.target-unsafe                 | error    | target-unsafe                |                                                                                                                                                                                                        |                                |
| route-init.route-ambiguous               | error    | route-ambiguous              |                                                                                                                                                                                                        |                                |
| route-init.identity-collision            | error    | identity-collision           | (blocking here: the new ID would collide)                                                                                                                                                              | choose another name            |
| route-init.loader-unsafe                 | error    | local                        | `.agents/loader.md could not be verified safely.`                                                                                                                                                      | `open-forge doctor`            |
| route-init.framework-payload-invalid     | error    | payload-invalid              |                                                                                                                                                                                                        |                                |
| route-init.framework-payload-unavailable | warning  | payload-unavailable          |                                                                                                                                                                                                        |                                |
| route-init.framework-install-required    | error    | local                        | `The Framework scaffold needs an installed Framework.`                                                                                                                                                 | `open-forge install --dry-run` |
| route-init.framework-update-required     | error    | local                        | `The installed Framework is older than the one this CLI ships.`                                                                                                                                        | `open-forge update`            |
| route-init.framework-alignment-blocked   | error    | local                        | `The installed Framework does not match the version this CLI ships, so the Framework scaffold cannot be used.`                                                                                         | `open-forge doctor`            |
| route-init.metadata-unsafe               | error    | metadata-unsafe              |                                                                                                                                                                                                        |                                |
| route-init.generated-region-unsafe       | error    | generated-region-unsafe      |                                                                                                                                                                                                        |                                |
| route-init.lifecycle-blocked             | error    | lifecycle-blocked            |                                                                                                                                                                                                        |                                |
| route-init.workspace-lock-unavailable    | error    | workspace-lock-unavailable   |                                                                                                                                                                                                        |                                |
| route-init.target-changed                | error    | target-changed               |                                                                                                                                                                                                        |                                |
| route-init.recovery-conflict             | error    | recovery-conflict            |                                                                                                                                                                                                        |                                |
| route-init.inspection-incomplete         | warning  | inspection-incomplete        |                                                                                                                                                                                                        |                                |
| route-init.metadata-incomplete           | warning  | metadata-incomplete          |                                                                                                                                                                                                        |                                |
| route-init.projection-incomplete         | warning  | projection-unavailable       |                                                                                                                                                                                                        |                                |
| route-init.lifecycle-unavailable         | warning  | lifecycle-unavailable        |                                                                                                                                                                                                        |                                |
| route-init.recovery-unavailable          | warning  | recovery-unavailable         |                                                                                                                                                                                                        |                                |
| route-init.needs-authoring               | info     | local                        | `Its description and tags are placeholders. Edit them before relying on this route.` (rendered as the advisory line, not as an Info row)                                                               | none                           |
| route-init.recovery-artifact-retained    | warning  | recovery-artifact-retained   |                                                                                                                                                                                                        |                                |
| route-init.target-changed-during-apply   | error    | target-changed-during-apply  |                                                                                                                                                                                                        |                                |
| route-init.write-failed                  | error    | write-failed                 |                                                                                                                                                                                                        |                                |
| route-init.verification-failed           | error    | verification-failed          |                                                                                                                                                                                                        |                                |
| route-init.lifecycle-publication-failed  | error    | lifecycle-publication-failed |                                                                                                                                                                                                        |                                |
| route-init.recovery-failed               | error    | recovery-failed              |                                                                                                                                                                                                        |                                |
| route-init.operation-failed              | error    | operation-failed             |                                                                                                                                                                                                        |                                |
| route-init.interrupted                   | error    | interrupted                  |                                                                                                                                                                                                        |                                |

## Effects wording

`Created <path>` / `Would create <path>`; `Listed in <parent>` / `Would list
it in <parent>`; at `standard`, `<path>  already present`; lock:
`.agents/open-forge.lock.json  updated` (Framework scaffold only). Partial:
`created`, `not started`, `final state unknown`.

## Counts

`entrypointsCreated`, `entrypointsPresent`, `sectionsUpdated`.

## Next rules

Framework mode blocked -> `open-forge install --dry-run` or `open-forge
update`; invalid -> the corrected command; otherwise none. The old `Next:
open-forge route update` without an operand is never printed.

## JSON data by level

| Level    | `data`                                                                                                            |
| -------- | ----------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ mode, target { id, path }, scaffold, entrypoints: [ { path, outcome, needsAuthoring } ], listedIn: [ path ] }` |
| standard | + `metadata { description, responsibility, tags, sources }`, `lockPath`                                           |
| full     | + per entrypoint `content` (string), per section `before`, `after`, `frameworkFingerprint`, `verification`        |

## References

- `src/cli/core/OpenForge.Cli.Core/Presentation/Legacy/Route/Init/Shared/Rendering/*` — deleted, replaced by `Presentation/Route/Init/`. (Corrected during [41](41-documentation-propagation.md): this line previously named `Commands/Route/Init/Shared/Rendering/*`.)
- `src/cli/core/OpenForge.Cli.Core/Commands/Route/Init/Shared/Planning/*` — status mapping of `needs-authoring`.
- Route init interface Semantic Results, Exact next actions (ledger only).
- E2E tests asserting exit 2 for the scaffold — change to 0.

## Preconditions

- [ ] 03, 05, 21 merged.

## Steps

1. [ ] Change the `needs-authoring` status contribution to none (Info) and
       the result status to completed.
2. [ ] Write `RouteInitReportSelector` and `RouteInitDataTextRenderer`
       (content at `full` through authored spans).
3. [ ] Delete the old renderers and `RouteTextEscaping` uses.
4. [ ] Regenerate snapshots and review; update the e2e exit assertion.
5. [ ] Three suites green.

## Acceptance

- [ ] The scaffold case exits 0 and prints three lines at `minimal`.
- [ ] No escaped file body (`\n`, `<`) appears at any level.
- [ ] No `Unchanged:` roster below `full`.

## Changes ledger

- status or exit for a condition: `needs-authoring` contributed attention at exit 2; it now contributes nothing, the finding `route-init.needs-authoring` is `info`, and the command completes at exit 0 with the advisory sentence and `needsAuthoring: true` in JSON. This is the change `Steps` item 1 and the note above the status table require, not a worker choice.
- message: the Framework next action was `open-forge install`; it is now `open-forge install --dry-run`, matching the `route-init.framework-install-required` row in `Findings catalogue`. The code, not the catalogue, was wrong.
- file layout/type: the legacy Route Init bridge under `Presentation/Legacy/Route/Init/` (6 files) is deleted; native `Presentation/Route/Init/` owns the selector, text renderer, wording, help and JSON context. `CliRouteComposer` closes the binding over it, leaving only List and Remove on the legacy bridge in that file.
- text: the legacy renderer output is replaced by the catalogue headlines, metadata rows, authored full-level spans, hash-only section receipts and the dry-run wording.
- JSON member: the legacy projection is replaced by the schema v3 envelope carrying level-specific `RouteInitData`, per the `JSON data by level` table.
- test: legacy assertions are migrated across the integration, unit, serialization, Framework and published-process suites. The legacy unit class `Commands/Route/Init/Shared/Rendering/RouteInitOutputSnapshotTests.cs` is deleted. `PublishedRouteInitProcessTests` was migrated by the worker and passes 3/3 against the installed binary.
- test identity: the snapshot theories shared one updater identity, so grouped cases were converted to aggregate facts and each case now captures separately.
- snapshots: the 44 legacy captures under `Commands/Route/Init/__snapshots__/` are replaced by **110** captures at the four native detail levels, text and JSON, across 11 situations, under `src/cli/tests/integration/snapshots/RouteInitBeforeOutputSnapshotTests/`.
- evidence, overseer: re-run on the merge commit, not taken from the worker's report. Unit 3,382 passed, 0 failed, 0 skipped; integration 2,228 total, 2,211 passed, 0 failed, 17 skipped; `PublishedRouteInitProcessTests` 3 passed; `PublishedShellBoundaryProcessTests` 26 passed, checked because it is a cross-command reader of Route output; `npm run check:dotnet` exactly the five documented errors.

- message: Route Init cause-bearing wording now delegates to the shared cause vocabulary; raw causes remain confined to the existing debug diagnostic channel.
- snapshots: regenerated the owned Route Init class; no primary capture changed, and its four raw debug-diagnostic captures remained unchanged.

## Divergences observed

1. **The two output changes above were verified against this file before they
   were accepted.** A worker that changes an exit code and a next-action string
   is normally a failed task. Both were checked: `Findings catalogue` line for
   `route-init.framework-install-required` gives `open-forge install --dry-run`,
   and the note above `Statuses and headlines` plus `Steps` item 1 require the
   `needs-authoring` move to `info` at exit 0. This is the first lane in the
   packet where the **code** was wrong and the catalogue right; every earlier
   divergence preserved existing behaviour against a conflicting catalogue.

2. **Direct operation-level `invalid-metadata` keeps a generic cause.** The
   command binder emits the catalogue's exact tag-specific causes, but a result
   produced directly at the operation level retains its frozen generic cause, so
   the two paths report the same situation differently. Native output was left
   unchanged. **Maintainer decision.**

3. **This file's `References` section points at paths that no longer exist.** It
   names `Commands/Route/Init/Shared/Rendering`; the legacy files were under
   `Presentation/Legacy/Route/Init/Shared/Rendering`. The same stale-path defect
   as [25](25-route-move.md) divergence 8. **Durable record to change:** this
   file's References.

4. **The worker left its retired capture tree in place.** All 11 situations were
   converted and the 110 native captures written, but the 44 legacy files were
   not deleted. The overseer removed them before committing. Third lane in a row
   with this omission, after [29](29-extension-create.md) and
   [36](36-library-sync.md); the command packet does not ask for the deletion,
   so it is chance rather than instruction.

5. **The prescribed build could not run.** It hit `NU1900` reading the user
   NuGet configuration, then `CS2012` access-denied writing default output; an
   isolated `artifacts/verify` build succeeded. The worker removed the
   `NuGet/NuGet.Config` it had created and reported that the sandbox refused to
   delete the now-empty directory — which git does not track, so nothing leaked
   into the merge. Same root cause as [24](24-route-update.md) divergence 3.

- The broad sweep found four raw exception/HRESULT matches in existing Route Init `*.debug.diagnostics.txt` files only. There was no minimal or standard leak and no new cause category was needed, so those diagnostic captures were intentionally unchanged.

## Rollback

Restore the bridge registration and the previous status mapping.
