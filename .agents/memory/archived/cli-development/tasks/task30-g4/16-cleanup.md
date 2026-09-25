---
open-forge:
  description: Cleanup output catalogue
  tags: [Memory, CLI, Task, Plan, G4, Cleanup, Contextual, Archived, Historical]
---

# 16 — cleanup

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md).

## Goal

`cleanup` lists every recovery bundle and draft it removed, or would remove,
and says plainly when a damaged one was left in place or another command
holds the lock.

## Depends on / Blocks

- Depends on: 03. Lane B, after 15.
- Blocks: 40.

## Shape

Change report. No prompt: running the command is the intent.

## Situations

`nothing-to-remove`, `two-bundles-one-draft`, `dry-run`, `damaged-bundle`,
`lock-held`, `store-unreadable`, `deletion-failed-partial`, `cancelled-partial`,
`invalid-input`.

## Statuses and headlines

| Status              | When                                            | Headline                                                                                                                        | Exit | Stream |
| ------------------- | ----------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------- | ---: | ------ |
| completed           | empty catalogue                                 | `No recovery data to remove.`                                                                                                   |    0 | stdout |
| completed           | removed                                         | `Removed <N> recovery bundles and <K> unfinished drafts.` (omit a zero part; singular forms)                                    |    0 | stdout |
| completed (dry run) | planned                                         | `Would remove <N> recovery bundles and <K> unfinished drafts.`                                                                  |    0 | stdout |
| incomplete          | store could not be read completely              | `The recovery store could not be read completely. Nothing was removed.`                                                         |    3 | stdout |
| invalid-input       | operand or bad flag                             | family                                                                                                                          |    4 | stderr |
| blocked             | lock held                                       | `Cannot clean up: another Open Forge command holds the workspace lock. Nothing was removed.`                                    |    5 | stderr |
| blocked             | damaged, unsupported or unreadable candidate    | `Cannot clean up: <path> is <damaged \| from an unsupported version \| unreadable> and was left in place. Nothing was removed.` |    5 | stderr |
| blocked             | catalogue changed under the lock                | `Cannot clean up: the recovery store changed while cleanup was running. Nothing was removed.`                                   |    5 | stderr |
| failed              | a deletion or verification failed after effects | `Cleanup stopped after removing <n> of <m> items.`                                                                              |    1 | stderr |
| cancelled           | Ctrl+C                                          | `Cleanup was cancelled after removing <n> of <m> items.` / `... Nothing was removed.`                                           |  130 | stderr |

## Text by level

`minimal`, removed:

```text
Removed 2 recovery bundles and 1 unfinished draft.
  <store>/myrepo-2026-09-13T21-04-11.zip
  <store>/myrepo-2026-09-13T21-09-52.zip
  <store>/myrepo-2026-09-13T21-11-30.draft
```

`minimal`, dry run: the same rows under `Would remove ...` and `No files were
changed.`

`minimal`, partial (stderr):

```text
Cleanup stopped after removing 1 of 3 items.
  <path-1>   removed
  <path-2>   could not be removed: <reason>
  <path-3>   not started
Next: open-forge cleanup
```

`standard` adds `Workspace:` and per row the kind and origin: `(bundle from
update, verified)`, `(unfinished draft from extension install)`, and rows for
items seen but not eligible (`<path>  left in place: not recognized`).

`full` adds the lock and final-check facts in words and each item's
integrity check.

## Findings catalogue

| Code                                   | Severity | Family                     | Message                                                                      | Next                           |
| -------------------------------------- | -------- | -------------------------- | ---------------------------------------------------------------------------- | ------------------------------ |
| cleanup.invalid-input                  | error    | invalid-input              |                                                                              |                                |
| cleanup.workspace-unavailable          | error    | workspace-unavailable      |                                                                              |                                |
| cleanup.workspace-not-directory        | error    | workspace-not-directory    |                                                                              |                                |
| cleanup.workspace-unsafe               | error    | workspace-unsafe           |                                                                              |                                |
| cleanup.workspace-lock-unavailable     | error    | workspace-lock-unavailable | `... Nothing was removed.`                                                   | `open-forge cleanup`           |
| cleanup.catalogue-incomplete           | warning  | local                      | `The recovery store at <path> could not be read completely.`                 | `open-forge doctor`            |
| cleanup.recovery-final-malformed       | error    | local                      | `<path> is damaged and was left in place.`                                   | remove it by hand after review |
| cleanup.recovery-final-unsupported     | error    | local                      | `<path> was written by an unsupported version and was left in place.`        | remove it by hand after review |
| cleanup.recovery-final-unavailable     | error    | local                      | `<path> could not be read and was left in place.`                            | none                           |
| cleanup.recovery-draft-unsafe          | error    | local                      | `<path> is not an ordinary file and was left in place.`                      | none                           |
| cleanup.catalogue-changed-during-apply | error    | local                      | `The recovery store changed while cleanup was running. Nothing was removed.` | `open-forge cleanup`           |
| cleanup.candidate-changed-during-apply | error    | local                      | `<path> changed while cleanup was running and was left in place.`            | `open-forge cleanup`           |
| cleanup.deletion-failed                | error    | local                      | `<path>  could not be removed: <reason>`                                     | `open-forge cleanup`           |
| cleanup.verification-failed            | error    | local                      | `<path>  still exists after removal`                                         | `open-forge cleanup`           |
| cleanup.operation-failed               | error    | operation-failed           |                                                                              |                                |
| cleanup.interrupted                    | error    | interrupted                |                                                                              |                                |

## Effects wording

`<path>  removed` / `would be removed` / `could not be removed: <reason>` /
`not started` / `left in place: <reason>`.

## Counts

`bundlesRemoved`, `draftsRemoved`, `itemsLeftInPlace`.

## Next rules

Partial, lock or changed -> `open-forge cleanup`; damaged -> a sentence;
completed -> none.

## JSON data by level

| Level    | `data`                                                                               |
| -------- | ------------------------------------------------------------------------------------ |
| minimal  | `{ mode, items: [ { path, kind: "bundle" \| "draft", outcome } ] }`                  |
| standard | + per item `origin` (command), `integrity`, plus `notEligible: [ { path, reason } ]` |
| full     | + `lock`, `finalCheck` in words                                                      |

## References

- `src/cli/core/OpenForge.Cli.Core/Commands/Cleanup/Shared/Rendering/CleanupPresentation.cs`, `CleanupWireVocabulary.cs` — replaced by `Presentation/Cleanup/`.
- Cleanup interface Output section (ledger only).

## Preconditions

- [ ] 03 merged.

## Steps

1. [ ] Write `CleanupReportSelector`; no data renderer is needed (rows are effects).
2. [ ] Delete the old presentation.
3. [ ] Regenerate snapshots and review.
4. [ ] Three suites green.

## Acceptance

- [ ] Empty `minimal` is one line.
- [ ] Every removed path is a row at `minimal`.
- [ ] No `Preflight`, `Workspace lock`, `Final workspace check` or `Verification` line at any level.

## Changes ledger

> Recorded by the overseer from the worker's closeout and the committed diff
> (`e152365a`). The worker could not write this file or commit: a linked
> worktree's `.git` points outside its writable sandbox, so `index.lock` is
> refused, and `.agents` is refused by the same policy.

- file layout/type: the Cleanup presentation bridge is replaced by a native `Presentation/Cleanup/` with its own selector and a command-specific text renderer. The shared renderer alone could not express the frozen Cleanup wording, so the command keeps a data text renderer of its own, as Install and Library list do.
- message: the legacy `Recovery-data cleanup` header, `Status:` line and `Removed and verified` rows are replaced by one headline that states the outcome — `Removed 2 recovery bundles and 1 unfinished draft.` — followed by every removed path. A run with nothing to remove says `No recovery data to remove.`
- JSON data: the legacy graph (`catalogue`, `plan`, `preflight`, `lease`, `revalidation`, `effects`, `residuals`, `verification`, `findings`) is replaced by `{ mode, items[] }`, where each item is `{ path, kind, outcome }`. `mode` is `dry-run` or `apply`, and `outcome` is `would-be-removed` or `removed`. The lease, revalidation and verification stages remain operation facts; they are no longer published as output.
- test: six library-recovery assertions that read the retired JSON graph were updated to the native one.
- snapshots: nine situations captured at four detail levels in text and JSON, 88 files under `src/cli/tests/integration/snapshots/CleanupBeforeOutputSnapshotTests/`.
- evidence, worker: build 0 warnings and 0 errors; unit 3,518 passed, 0 failed, 0 skipped; integration 2,278 passed, 0 failed, 17 skipped; `check:dotnet` exactly the five documented errors.
- evidence, overseer after merging: unit 3,441 passed, 0 failed, 0 skipped; integration 2,287 total, 2,270 passed, 0 failed, 17 skipped.
- test, overseer: `PublishedCleanupProcessTests` still asserted the legacy graph and wording and was migrated after the merge — the `data` member list became `["mode", "items"]`, the removal journey asserts the new headline and the absence of any `Status:` line, and the repeat run asserts `No recovery data to remove.`

- message: recovery effect reasons now use the shared cause vocabulary, so filesystem exception type names and HRESULTs are absent from Cleanup output while existing diagnostic evidence remains available internally.
- snapshots: regenerated the eight `RecoveryCatalogue/deletion-failed-partial` native captures across text and JSON detail levels; only the user-facing effect reason changed.

## Divergences observed

- The shared rendering system from [03](03-rendering-system.md) could not express the frozen Cleanup wording on its own, so this command keeps its own data text renderer rather than relying only on shared row rendering. The worker reported no product wording needing a maintainer decision.
- The worker could not complete its own closeout. Writing `.agents` and creating a git `index.lock` are both outside a worktree agent's writable sandbox, so the ledger above and the commit were made by the overseer.

- The recovery effect model still carries the raw cause for diagnostics; the presentation boundary owns its plain-English projection. No Cleanup catalogue headline or template changed.

## Rollback

Restore the bridge registration for cleanup.
