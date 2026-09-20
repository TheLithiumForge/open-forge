---
open-forge:
  description: Update output catalogue and confirmation flow
  tags: [Memory, Working, CLI, Task, Plan, G4, Update, Contextual, Active]
---

# 13 — update

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md) and [04](04-interaction-system.md).

## Goal

`update` says which Framework files it replaced, restored, deleted or kept,
why, and where the previous content went. It never prints fingerprints for
unchanged files below `full`.

## Depends on / Blocks

- Depends on: 03, 04. Lane B, after 12.
- Blocks: 40.

## Shape

Change report.

## Behavior this catalogue reflects

Ordinary `update` replaces changed owned files and restores missing ones.
`--force` grants no extra authority (it stays accepted as a flag). `--prune`
deletes files that are no longer part of this release; without it they are
kept with a warning. The recovery bundle is removed after verification. The
previous content of a replaced file is reachable through `git diff` when a
`.git` directory exists (C15).

## Situations

`up-to-date`, `changed-file-replaced`, `missing-file-restored`,
`retired-kept`, `retired-pruned`, `dry-run-changes`, `no-ownership-record`,
`confirmation-unavailable`, `write-failed-partial`, `cancelled`, `invalid-input`.

## Statuses and headlines

| Status                  | When                                                      | Headline                                                                                                                                                       | Exit | Stream |
| ----------------------- | --------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | nothing to change                                         | `The Framework is up to date. Nothing to do.`                                                                                                                  |    0 | stdout |
| completed               | files replaced, restored or deleted                       | `Updated <N> Framework files.` (`Updated 1 Framework file.`)                                                                                                   |    0 | stdout |
| completed (dry run)     | changes planned                                           | `Would update <N> Framework files.`                                                                                                                            |    0 | stdout |
| completed               | no ownership record (Info observation)                    | `No ownership record exists, so update cannot tell which files it manages. Nothing was changed.`                                                               |    0 | stdout |
| completed-with-warnings | retired files kept without `--prune`                      | `Updated <N> Framework files. <K> files from an earlier version were kept.` or `The Framework is up to date, but <K> files from an earlier version were kept.` |    2 | stdout |
| completed-with-warnings | recovery bundle retained                                  | + family row                                                                                                                                                   |    2 | stdout |
| incomplete              | bundled Framework, record, target or recovery unreadable  | `Update could not start: <limitation>. Nothing was changed.`                                                                                                   |    3 | stdout |
| invalid-input           | bad input; confirmation unavailable                       | families                                                                                                                                                       |    4 | stderr |
| blocked                 | invalid record, conflict, unsafe target, prune ineligible | `Cannot update: <reason>.`                                                                                                                                     |    5 | stderr |
| failed                  | after effects                                             | `Update stopped after <n> of <m> changes.`                                                                                                                     |    1 | stderr |
| cancelled               | prompt refused, Ctrl+C                                    | `Update was cancelled. Nothing was changed.`                                                                                                                   |  130 | stderr |

## Text by level

`minimal`, changes:

```text
Updated 3 Framework files.
  .agents/guidance/_guidance.md     replaced (you had changed it)
  .agents/patterns/_patterns.md     restored (it was missing)
  .agents/workflows/_workflows.md   replaced (new content in this release)
  Previous content: git diff
```

`minimal`, retired kept:

```text
Updated 1 Framework file. 1 file from an earlier version was kept.
  .agents/guidance/_guidance.md     replaced (new content in this release)
  .agents/guidance/old-advice.md    kept; no longer part of this release
Next: open-forge update --prune --dry-run  (preview deleting it)
```

`minimal`, dry run:

```text
Would update 3 Framework files.
  .agents/guidance/_guidance.md     replace (you have changed it; a recovery bundle is written first)
  .agents/patterns/_patterns.md     restore
  .agents/workflows/_workflows.md   replace
No files were changed.
```

`standard` adds `Workspace:`, every other Framework file as `  <path>  unchanged`
grouped after the changed rows as one count line (`18 files unchanged`), the
Entries sections rewritten as rows, and the lock row.

`full` adds the current and shipped SHA-256 per changed path, the source
asset path, the bundled Framework identity and fingerprint, and the recovery
and verification facts in words.

## Prompts

Plan review at `minimal` then `Apply these changes? [y/N]`. When retired
files would be deleted under `--prune`, the question reads `Delete the <K>
files listed above? [y/N]`.

## Findings catalogue

| Code                                | Severity | Family                       | Message                                                                                                               | Next                                  |
| ----------------------------------- | -------- | ---------------------------- | --------------------------------------------------------------------------------------------------------------------- | ------------------------------------- |
| update.invalid-input                | error    | invalid-input                |                                                                                                                       |                                       |
| update.confirmation-required        | error    | confirmation-required        |                                                                                                                       | `open-forge update --automatic`       |
| update.workspace-unavailable        | error    | workspace-unavailable        |                                                                                                                       |                                       |
| update.workspace-unsafe             | error    | workspace-unsafe             | also lock unavailable                                                                                                 |                                       |
| update.payload-unavailable          | warning  | payload-unavailable          |                                                                                                                       |                                       |
| update.payload-invalid              | error    | payload-invalid              |                                                                                                                       |                                       |
| update.lifecycle-missing            | warning  | local                        | `No ownership record exists (.agents/open-forge.lock.json is missing), so update cannot tell which files it manages.` | `open-forge doctor`                   |
| update.lifecycle-unavailable        | warning  | lifecycle-unavailable        |                                                                                                                       |                                       |
| update.lifecycle-blocked            | error    | lifecycle-blocked            |                                                                                                                       |                                       |
| update.ownership-observation        | info     | ownership-observation        | `The ownership record names files that cannot be interpreted, so nothing was changed.`                                | `open-forge doctor`                   |
| update.ownership-conflict           | error    | ownership-conflict           |                                                                                                                       |                                       |
| update.target-unavailable           | warning  | local                        | `<path> could not be read.`                                                                                           | `open-forge doctor`                   |
| update.target-unsafe                | error    | target-unsafe                |                                                                                                                       |                                       |
| update.source-provenance-invalid    | error    | local                        | `The ownership record names an unknown source for <path>.`                                                            | `open-forge doctor`                   |
| update.fingerprint-unsupported      | error    | local                        | `<path> could not be compared with the version this CLI ships.`                                                       | `open-forge doctor`                   |
| update.managed-divergence           | removed  |                              | legacy member, never emitted; delete (ledger)                                                                         |                                       |
| update.managed-target-missing       | removed  |                              | legacy member, never emitted; delete (ledger)                                                                         |                                       |
| update.retired-content-preserved    | warning  | local                        | row `<path>  kept; no longer part of this release`                                                                    | `open-forge update --prune --dry-run` |
| update.retirement-ineligible        | error    | local                        | `<path> is no longer part of this release but cannot be deleted safely: <reason>.`                                    | fix by hand                           |
| update.projection-unavailable       | warning  | projection-unavailable       |                                                                                                                       |                                       |
| update.generated-region-unsafe      | error    | generated-region-unsafe      |                                                                                                                       |                                       |
| update.plan-blocked                 | error    | local                        | `<path> prevents the update: <reason>.`                                                                               | `open-forge doctor`                   |
| update.recovery-conflict            | error    | recovery-conflict            |                                                                                                                       |                                       |
| update.recovery-unavailable         | warning  | recovery-unavailable         |                                                                                                                       |                                       |
| update.recovery-artifact-retained   | warning  | recovery-artifact-retained   |                                                                                                                       |                                       |
| update.write-failed                 | error    | write-failed                 |                                                                                                                       |                                       |
| update.verification-failed          | error    | verification-failed          |                                                                                                                       |                                       |
| update.lifecycle-publication-failed | error    | lifecycle-publication-failed |                                                                                                                       |                                       |
| update.recovery-failed              | error    | recovery-failed              |                                                                                                                       |                                       |
| update.operation-failed             | error    | operation-failed             |                                                                                                                       |                                       |
| update.interrupted                  | error    | interrupted                  |                                                                                                                       |                                       |

## Effects wording

| Relation (current, shipped)   | Action    | Row                                                                 |
| ----------------------------- | --------- | ------------------------------------------------------------------- |
| changed, same shipped content | replaced  | `<path>  replaced (you had changed it)`                             |
| same current, changed shipped | replaced  | `<path>  replaced (new content in this release)`                    |
| changed both                  | replaced  | `<path>  replaced (you had changed it and this release changes it)` |
| missing                       | restored  | `<path>  restored (it was missing)`                                 |
| new in this release           | created   | `<path>  created (new in this release)`                             |
| retired, `--prune`            | deleted   | `<path>  deleted (no longer part of this release)`                  |
| retired, no `--prune`         | kept      | `<path>  kept; no longer part of this release`                      |
| format-only difference        | unchanged | counted; at `full`: `<path>  unchanged (line endings differ)`       |
| Entries section rewritten     | section   | `<path>  Entries section updated`                                   |
| lock                          | record    | `.agents/open-forge.lock.json  updated`                             |

Dry-run rows use the bare verb (`replace`, `restore`, `delete`, `keep`).
The previous-content line is `Previous content: git diff` when `.git` exists
and is omitted otherwise.

## Counts

`filesReplaced`, `filesRestored`, `filesCreated`, `filesDeleted`,
`filesKept`, `filesUnchanged`, `sectionsUpdated`.

## Next rules

Retired kept -> `open-forge update --prune --dry-run`; confirmation ->
`--automatic`; partial or retained recovery -> `open-forge doctor` /
`open-forge cleanup`; no record -> `open-forge doctor`; otherwise none.

## JSON data by level

| Level    | `data`                                                                                                                                    |
| -------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ mode, force, prune, automatic, previousContent: "git-diff" \| null, lockPath }`                                                        |
| standard | + `unchanged: [ { path } ]`, `entriesSections: [ { path, state } ]`                                                                       |
| full     | + per-effect `before`, `after`, `sourceAssetPath`, `relation { current, shipped }`, `source { id, version, fingerprint }`, `verification` |

## References

- `src/cli/core/OpenForge.Cli.Core/Commands/Update/Shared/Rendering/*` — replaced by `Presentation/Update/`.
- `src/cli/core/OpenForge.Cli.Core/Commands/Update/Shared/Planning/UpdatePlanningPolicy.cs:34-80` — the relations above.
- `src/cli/core/OpenForge.Cli.Core/Commands/Update/UpdateOperation.cs:13,98` — prompt flow.
- Update interface and help text: the help still says normal mode "preserves changed" content; ledger entry `doc:` required. `docs/cli.md:438` same.

## Preconditions

- [ ] 03, 04, 12 merged.

## Steps

1. [ ] Write `UpdateReportSelector` and `UpdateDataTextRenderer`.
2. [ ] Wire plan review and confirmation.
3. [ ] Delete the two legacy finding members and the old renderers.
4. [ ] Regenerate snapshots and review.
5. [ ] Three suites green.

## Acceptance

- [ ] Up-to-date `minimal` is one line.
- [ ] No fingerprint appears below `full`.
- [ ] Every replaced, restored, deleted and kept file is a row at `minimal`.

## Changes ledger

- file layout/type: the legacy Update renderers, projections and models are deleted, along with the obsolete renderer snapshots; native `Presentation/Update/` owns the data model, JSON context, selector, text renderer, wording, help and presentation wiring. `CliStandaloneComposer` closes the binding over it.
- message: output moved to the catalogue headlines and level-gated detail for all 11 situations — up-to-date, changed replacement, missing restoration, retired kept, retired pruned, dry run, no ownership, confirmation unavailable, partial failure, cancelled and invalid input.
- message: the dry-run path emits `Would update`, bare effects and the no-change statement.
- message: `invalid input` deliberately preserves the raw parser error without a report envelope, because the failure happens before a report exists.
- finding: the obsolete `ManagedDivergence` and `ManagedTargetMissing` findings are removed for this command. (Note the contrast with [31](31-extension-update.md), where the same-named code was **kept** because that catalogue's own text example requires the behaviour. These are different commands with different catalogues; the decision does not transfer.)
- effects/counts: root effect receipts remain available at every detail level; data effects remain `full` and `debug` only.
- snapshots: the legacy captures are replaced by **108** captures at the four native detail levels, text and JSON, across the 11 catalogue situations, under `src/cli/tests/integration/snapshots/UpdateBeforeOutputSnapshotTests/`. The worker deleted its own retired tree.
- test: Update integration snapshots, interaction helpers, contract tests, managed-host tests and published-process tests are migrated. `PublishedUpdateProcessTests` was migrated by the worker and passes 3/3 against the installed binary.
- evidence, overseer: re-run on the merge commit, not taken from the worker's report. Unit 3,238 passed, 0 failed, 0 skipped; integration 2,208 total, 2,191 passed, 0 failed, 17 skipped; `PublishedUpdateProcessTests` 3 passed; `npm run check:dotnet` exactly the five documented errors.

- message: `write-failed` finding messages now use the shared filesystem cause vocabulary; the exception type and HRESULT are retained only as `cause` evidence at `full`/`debug` and no longer appear in Update's headline, summary, finding message, count, or `minimal`/`standard` output.
- snapshots: regenerated all eight `PartialWriteFailure/write-failed-partial` text and JSON captures at the four native detail levels; each changed only its finding message, with the raw cause added under `cause` at `full`/`debug`.

- title: the `update.confirmation-required` finding title read
  `Confirmation required` -> it reads `Confirmation is required`. The same
  code carried two titles across eleven commands, six one way and five the
  other; the majority form also matches the house style of every other
  finding title, such as `Workspace lock is unavailable`.

## Divergences observed

1. **`minimal` for an up-to-date run prints `Workspace:` when `--workspace` is
   explicit, which this file's one-line example does not show.** The shared G4
   presentation rule requires the workspace echo; this file's `minimal` example
   shows only the headline. Native output was left unchanged. **Maintainer
   decision: confirm that the shared rule takes precedence over a command
   catalogue's example.** This is the same conflict recorded in
   [14](14-index.md) divergence 2, where `all-current` at `minimal` also carries
   the workspace echo against a one-line acceptance. One ruling settles both.

2. **`docs/cli.md:438` still says Update preserves changed content, which is no
   longer true.** The worker identified the stale sentence and correctly did
   **not** edit it, because `docs/` is outside the scope a lane may change.
   **Durable record to change:** this belongs to
   [41](41-documentation-propagation.md).

3. **Closeout was blocked, as expected for a worktree.** The task-record write
   and `git add` were both refused. The overseer wrote this ledger and
   committed.

4. **This lane was completed hours before it was merged.** Its run finished
   while the overseer was handling [11](11-doctor.md)'s capacity failure, and it
   was passed over until a later sweep of the run list found it complete and
   unmerged. No work was lost, but a finished lane sat idle. **Process note:**
   reconcile the run list against the branch's merges before concluding a batch,
   rather than tracking lanes from notifications alone.

- The pre-change broad sweep found raw exception text in Update's minimal, standard, full and debug text/JSON partial-write captures; the supplied list named only the minimal and standard variants. The shared vocabulary already covered the access-denied cause, so no shared member was added. The final primary captures keep the raw cause only under `cause` evidence at `full`/`debug`; no raw cause remains in a headline, summary, finding message, count, or `minimal`/`standard` output, and no unrelated capture changed.

## Rollback

Restore the bridge registration for update.
