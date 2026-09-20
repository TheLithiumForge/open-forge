---
open-forge:
  description: Install output catalogue and confirmation flow
  tags: [Memory, Working, CLI, Task, Plan, G4, Install, Contextual, Active]
---

# 12 — install

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md) and [04](04-interaction-system.md).

## Goal

`install` tells a person what it created, names every existing file it
touched, and points at the lock file for the roster. It never prints the
44-line plan on a fresh directory and never refuses after printing a plan.

## Depends on / Blocks

- Depends on: 03, 04. Lane B, first.
- Blocks: 40.

## Shape

Change report. Human-first (C11): creations summarized, existing files named.

## Situations

`fresh-directory`, `fresh-directory-dry-run`, `already-installed`,
`existing-agents-md`, `occupied-without-force`, `occupied-with-force`,
`changed-framework-file` (blocked, points at update), `confirmation-unavailable`,
`recovery-store-unavailable`, `write-failed-partial`, `cancelled`,
`invalid-input`. Each at all levels, text and JSON.

## Statuses and headlines

| Status                  | When                                                 | Headline                                                                                        | Exit | Stream |
| ----------------------- | ---------------------------------------------------- | ----------------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | fresh install                                        | `Installed the Open Forge Framework into <workspace>.`                                          |    0 | stdout |
| completed               | force replaced existing files                        | `Installed the Open Forge Framework into <workspace>, replacing <N> existing files.`            |    0 | stdout |
| completed               | already installed and current                        | `Open Forge is already installed and current. Nothing to do.`                                   |    0 | stdout |
| completed (dry run)     | any plan                                             | `Would install the Open Forge Framework into <workspace>.` (+ `, replacing <N> existing files`) |    0 | stdout |
| completed-with-warnings | recovery bundle retained after success               | headline as completed + family `recovery-artifact-retained` row                                 |    2 | stdout |
| incomplete              | bundled Framework, lock or recovery store unreadable | `Install could not start: <limitation>. Nothing was changed.`                                   |    3 | stdout |
| invalid-input           | bad input; confirmation unavailable                  | family `invalid-input` / `confirmation-required`                                                |    4 | stderr |
| blocked                 | occupied paths without force                         | `Cannot install: <N> files already exist where the Framework would write.`                      |    5 | stderr |
| blocked                 | changed Framework files (managed divergence)         | `Cannot install: <N> Framework files have changed since they were installed.`                   |    5 | stderr |
| blocked                 | other boundary                                       | `Cannot install: <reason>.`                                                                     |    5 | stderr |
| failed                  | write or verification failed after effects           | `Install stopped after <n> of <m> changes.`                                                     |    1 | stderr |
| cancelled               | no at the prompt, Ctrl+C, end of input               | `Install was cancelled. Nothing was changed.`                                                   |  130 | stderr |

## Text by level

`minimal`, fresh:

```text
Installed the Open Forge Framework into D:/work/myrepo.
  Created 21 files and 20 directories under .agents (listed in .agents/open-forge.lock.json).
  Created AGENTS.md and CLAUDE.md with an Open Forge section.
```

`minimal`, existing `AGENTS.md`:

```text
Installed the Open Forge Framework into D:/work/myrepo.
  Created 21 files and 20 directories under .agents (listed in .agents/open-forge.lock.json).
  AGENTS.md   Open Forge section added; your content was kept
  Created CLAUDE.md.
```

`minimal`, dry run:

```text
Would install the Open Forge Framework into D:/work/myrepo.
  Would create 21 files and 20 directories under .agents, plus AGENTS.md and CLAUDE.md.
  Nothing that already exists would be changed.
No files were changed.
```

`minimal`, occupied, `--force`:

```text
Installed the Open Forge Framework into D:/work/myrepo, replacing 2 existing files.
  .agents/loader.md      replaced (your previous file is in the recovery bundle)
  .agents/maps/_maps.md  replaced (your previous file is in the recovery bundle)
  Created 19 files and 20 directories under .agents (listed in .agents/open-forge.lock.json).
  Created AGENTS.md and CLAUDE.md with an Open Forge section.
```

`minimal`, occupied without `--force` (stderr):

```text
Cannot install: 2 files already exist where the Framework would write.
  .agents/loader.md
  .agents/maps/_maps.md
Next: open-forge install --force --dry-run  (preview replacing them)
```

`minimal`, confirmation unavailable (stderr):

```text
Install needs confirmation, and this session cannot ask.
Next: open-forge install --automatic  (or --dry-run to see the plan first)
```

`minimal`, partial (stderr):

```text
Install stopped after 12 of 44 changes.
  Writing .agents/memory/_memory.md failed: <reason>.
  12 files and directories were created and are listed in .agents/open-forge.lock.json.
  Recovery data: <path>
Next: open-forge doctor
```

`standard` adds `Workspace:`, every created file as a row (`  <path>  created`),
directories as one count line, the two host files as rows, and the lock file
row `  .agents/open-forge.lock.json  created; records the 21 files above`.

`full` adds the directories as rows, the source asset path per file, the
bundled Framework fingerprint, and the recovery and verification facts in
words.

## Prompts

In a terminal without `--automatic`: plan review at `minimal` on stderr, then
`Apply these changes? [y/N]`. When existing files would be replaced under
`--force`, the question reads `Replace the 2 existing files listed above?
[y/N]`. See [04](04-interaction-system.md).

## Findings catalogue

| Code                                 | Severity | Family                       | Message                                                                                 | Next                                   |
| ------------------------------------ | -------- | ---------------------------- | --------------------------------------------------------------------------------------- | -------------------------------------- |
| install.invalid-input                | error    | invalid-input                |                                                                                         |                                        |
| install.confirmation-required        | error    | confirmation-required        |                                                                                         | `open-forge install --automatic`       |
| install.workspace-unavailable        | error    | workspace-unavailable        |                                                                                         |                                        |
| install.workspace-unsafe             | error    | workspace-unsafe             | also `workspace-lock-unavailable` when the lock could not be acquired                   |                                        |
| install.managed-divergence           | error    | managed-divergence           | `<path> has changed since it was installed. Install does not replace changed files.`    | `open-forge update`                    |
| install.target-occupied              | error    | target-occupied              | row `<path>` under the blocked headline                                                 | `open-forge install --force --dry-run` |
| install.ownership-conflict           | error    | ownership-conflict           |                                                                                         |                                        |
| install.target-unsafe                | error    | target-unsafe                |                                                                                         |                                        |
| install.generated-region-unsafe      | error    | generated-region-unsafe      |                                                                                         |                                        |
| install.lifecycle-blocked            | error    | lifecycle-blocked            |                                                                                         |                                        |
| install.recovery-conflict            | error    | recovery-conflict            |                                                                                         |                                        |
| install.payload-unavailable          | warning  | payload-unavailable          |                                                                                         |                                        |
| install.payload-invalid              | error    | payload-invalid              |                                                                                         |                                        |
| install.lifecycle-unavailable        | warning  | lifecycle-unavailable        |                                                                                         |                                        |
| install.projection-unavailable       | warning  | projection-unavailable       |                                                                                         |                                        |
| install.recovery-unavailable         | warning  | recovery-unavailable         |                                                                                         |                                        |
| install.recovery-artifact-retained   | warning  | recovery-artifact-retained   |                                                                                         |                                        |
| install.write-failed                 | error    | write-failed                 |                                                                                         |                                        |
| install.verification-failed          | error    | verification-failed          |                                                                                         |                                        |
| install.lifecycle-publication-failed | error    | lifecycle-publication-failed |                                                                                         |                                        |
| install.recovery-failed              | error    | recovery-failed              |                                                                                         |                                        |
| install.operation-failed             | error    | operation-failed             |                                                                                         |                                        |
| install.interrupted                  | error    | interrupted                  | `Install was cancelled. Nothing was changed.` / `... Stopped after <n> of <m> changes.` |                                        |

## Effects wording

| Effect                               | `minimal`                                                                    | `standard` row                                                   |
| ------------------------------------ | ---------------------------------------------------------------------------- | ---------------------------------------------------------------- |
| create directory                     | counted                                                                      | counted (`20 directories created`)                               |
| create file under `.agents`          | counted, with the lock file named                                            | `<path>  created`                                                |
| create `AGENTS.md` or `CLAUDE.md`    | `Created AGENTS.md and CLAUDE.md with an Open Forge section.`                | `<file>  created with an Open Forge section`                     |
| append section to existing host file | `<file>   Open Forge section added; your content was kept`                   | same                                                             |
| replace existing file (`--force`)    | `<path>  replaced (your previous file is in the recovery bundle)`            | same                                                             |
| create lock                          | named in the count sentence                                                  | `.agents/open-forge.lock.json  created; records the files above` |
| planned (dry run)                    | `Would ...` forms of the above                                               | same                                                             |
| not started, unknown (partial)       | listed under the partial headline with `not started` / `final state unknown` | same                                                             |

## Counts

`filesCreated`, `directoriesCreated`, `sectionsAdded`, `filesReplaced`.

## Next rules

Blocked occupied -> `open-forge install --force --dry-run`; managed divergence
-> `open-forge update`; confirmation unavailable -> `open-forge install
--automatic`; partial or retained recovery -> `open-forge doctor` or
`open-forge cleanup`; completed -> none (the old `open-forge context`
suggestion is not printed; help covers it).

## JSON data by level

| Level    | `data`                                                                                                                                      |
| -------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ mode, force, automatic, classification, footprint { files, directories, sections }, lockPath }`                                          |
| standard | same                                                                                                                                        |
| full     | + `source { inventoryFingerprint, assetCount }`, per-effect `sourceAssetPath` in `effects`, `lifecycle { action, outcome }`, `verification` |

`effects` lists all 44 entries at every level (receipts are complete in JSON).

## References

- `src/cli/core/OpenForge.Cli.Core/Commands/Install/Shared/Rendering/*` — replaced by `Presentation/Install/`.
- `src/cli/core/OpenForge.Cli.Core/Commands/Install/InstallOperation.cs:20,195` — prompt flow.
- Install interface contract sections Output And Streams, Human Confirmation, Compact JSON Output (ledger only).

## Preconditions

- [ ] 03 and 04 merged.

## Steps

1. [ ] Write `InstallReportSelector` and `InstallDataTextRenderer`.
2. [ ] Wire plan review and confirmation per 04.
3. [ ] Delete the old renderers.
4. [ ] Regenerate snapshots and review.
5. [ ] Three suites green.

## Acceptance

- [ ] Fresh `minimal` is three lines.
- [ ] The plan is never printed before a refusal; a refusal is at most three lines.
- [ ] Every replaced existing file is named at `minimal`.

## Changes ledger

- Added the native Install report pipeline under `src/cli/core/OpenForge.Cli.Core/Presentation/Install/`: command-owned `InstallData` facts, source-generated JSON metadata, detail-aware selector, text renderer, local wording/help factories, and the `CliCommandShape.ChangeReport` presentation registration. The selector carries typed dry-run/detail decisions into the data renderer; Presentation has no Framework namespace dependency.
- Wired `BuildInstall` to the shared plan-confirmation delegate with `InstallResult`, `InstallData`, and `InstallConfirmationFacts`. The existing operation remains the authority for its retained pure dry-run preview, one confirmation boundary, force/automatic policy, receipts, and cancellation behavior.
- Root JSON keeps complete effect receipts at every detail; command `data.effects` is omitted below full detail and contains only path/source-asset metadata at full/debug. The command data omits source/lifecycle/verification fields below full detail, excludes the ownership record from ordinary created/replacement counts, and derives directory/section values from typed effects/footprint facts. Human minimal output summarizes fresh creation, names replacements, hides the plan on refusal, and preserves the approved `replaced` and partial-summary wording exceptions.
- Migrated the Install integration snapshot harness and operation/serialization assertions to `InstallPresentation.Rendering`; focused native contract and wording tests cover property/nullability boundaries, count grammar, generated JSON, bounded confirmation refusal, pure dry-run data, and diagnostics. The two multi-situation cases use one shared snapshot collector so each test retains both its apply/preview or force/no-force captures.
- Focused evidence in this increment: Core, root CLI, Unit, Integration, and EndToEnd projects built with zero warnings/errors; native contract/wording Unit filter passed 10/10; Install operation plus generated serialization Integration filter passed 10/10; composed Install, revalidation, and preservation Integration filter passed 23/23; the native Install four-level snapshot suite passed 28/28 after writing 118 new snapshots under `src/cli/tests/integration/snapshots/InstallBeforeOutputSnapshotTests/`. The original Task01 snapshots remain unchanged; later legacy-renderer retirement remains outside this checkpoint.

- Corrected the native selection boundary after output review: `InstallData` now carries only the JSON contract and a narrow typed text projection (selected rows, summaries, detail lines, recovery state/path, and verification state). The selector owns detail visibility and derives apply counts from verified receipts, preview counts from planned effects, actual host/section rows from the plan, and replacement counts from distinct non-ownership paths. The writer formats that projection without a second facts graph, wire-mode policy, or duplicate cell escaping.
- Corrected Install wording and receipts: replacement headlines name `existing` files with invariant singular/plural grammar; lock and recovery claims are emitted only from verified/retained facts; partial summaries count actual physical file and directory creations with the approved aggregate wording; fresh preview host text is emitted once; full output uses lifecycle/recovery/verification wording; and non-progressed file or directory effects remain listed as `not started` or `final state unknown`.
- Moved the unchanged `InstallFindingCode` enum to `Commands/Install/Models/Result/InstallFindingCode.cs` so native Presentation imports command-owned result facts without importing behavior-heavy `InstallDefinitions`. The selector uses one typed cancellation-progress fact to suppress untouched human rows and the duplicate interrupted finding only for zero-progress cancellation; JSON retains complete effects and findings, while partial receipts remain visible.
- Narrowed command data effects to `{ path, sourceAssetPath }` at full/debug while the root effects list remains the sole complete receipt authority. Classification and footprint retain explicit `null` when unavailable; `BlockedOccupied` keeps the authored `already` wording; failure rows use named local wording factories with actual causes; and mixed partial summaries count verified physical file/directory creates, including host files and excluding the ownership record.
- Closure evidence on the released branch: Core and EndToEnd builds passed with 0 warnings/errors; Install Unit namespaces passed 30/30; Install Integration namespace passed 44/44; the four-level native snapshot suite passed 28/28; and the published `PublishedInstallProcessTests` process gate passed 4/4 against a freshly rebuilt managed publication. The original Task01 snapshots remain unchanged.
- Final R-INST9 selection correction: the selector derives `(InstallFindingCode, subject path)` identities from typed findings, and minimal blocked output suppresses only identities represented by selected blocked rows; unrelated unsafe, recovery, and other findings remain visible.
- Final R-INST10 footprint correction: directory counts come from the established typed effect plan, with `AlreadyCurrent` reporting known zero; `FromBuild` results with no effects and no established lifecycle plan retain `directories: null`. The `ChangedFrameworkFile`, `OccupiedGeneratedRegion/occupied-without-force`, and `RecoveryStoreUnavailable` four-level snapshots record that unknown state.
- Final R-INST9/R-INST10 evidence: `InstallPresentationContractTests` passed 8/8 for the focused native contracts and mixed blocked findings; `InstallBeforeOutputSnapshotTests` passed 28/28 both while updating and with updates disabled. Only the 12 expected nullable-directory snapshot files changed in this increment.

- wave 0 repair: a workspace-unavailable finding carried no subject, so its
  message read `Cannot use install as the workspace: ...` and the text
  renderer threw `A listed finding requires a path or identifier`, crashing
  `open-forge install --workspace <missing> --format text` at every detail
  level with exit `-532462766` -> `InstallWorkspaceResultFactory` and
  `InstallRequestBinder` carry the requested workspace path on the finding,
  the message names that path, and `InstallReportSelector` keeps the subject
  kind `workspace` with that path as its identifier and falls back to the
  command name rather than null. This is what `index` and `extension list`
  already do; the sentence itself is unchanged.
- test: `InstallGeneratedSerializationTests.NativeDataPreservesDryRunProjectionAndCompleteEffects`
  read `data.effects` at standard detail -> it asserts `data.effects` is
  absent and reads the complete 44-entry receipt list from the root
  envelope, which is where this ledger already records it.
- test: `PublishedEmbeddedPayloadProcessTests.RelocatedArtifactReachesEmbeddedPayload`
  ran Install at the default detail and read `data.source.assetCount` and
  `data.effects` -> it passes `--detail=full`, the level at which the
  command data carries them.

- message: `write-failed` finding messages now use the shared filesystem cause vocabulary; the exception type and HRESULT are retained only as `cause` evidence at `full`/`debug` and no longer appear in Install's headline, summary, finding message, count, or `minimal`/`standard` output.
- snapshots: regenerated all eight `PartialWriteFailure/write-failed-partial` text and JSON captures at the four native detail levels; each changed only its finding message, with the raw cause added under `cause` at `full`/`debug`.

- title: the `install.confirmation-required` finding title read
  `Confirmation required` -> it reads `Confirmation is required`. The same
  code carried two titles across eleven commands, six one way and five the
  other; the majority form also matches the house style of every other
  finding title, such as `Workspace lock is unavailable`.

## Divergences observed

- **Accepted Task12 wording exception.** The final successful force row retains `replaced` without a backup-retention claim. A successful force Install protects replaced bytes in a recovery bundle during apply, then removes the bundle after verified completion; dry-run reports no created bundle. `src/cli/core/OpenForge.Cli.Core/Commands/Install/Shared/Operation/InstallRecoveryOperation.cs` maps `RecoveryBundleDeletionState.Deleted` to `InstallRecoveryState.Removed`, and `InstallBeforeOutputSnapshotTests.OccupiedGeneratedRegion` verifies force completion with zero recovery candidates. Recovery behavior is unchanged.
- The partial summary retains `<N> files and directories were created.` without an unverified lock-list claim. Ownership publication is the final file effect after target effects and verification; `InstallBeforeOutputSnapshotTests.PartialWriteFailure` verifies `.agents/open-forge.lock.json` is absent while recovery is retained. `WorkspaceOwnershipDocument` records verified mutation receipts, and Install's planner includes only file effects in Framework paths; directories are not recorded. Recovery and lock references must be emitted only from actual receipts; ownership behavior is unchanged.
- Published process assertions now target native Install output: full detail verifies `All written targets were verified.`, no-op checks the native `Open Forge is already installed and current. Nothing to do.` headline plus unchanged workspace state, and redirected confirmation checks `Install needs confirmation, and this session cannot ask.` with no prompt or writes. The native process gate still exercises automatic apply, convergence, JSON dry-run read-only behavior, and redirected human refusal.

- The pre-change broad sweep found raw exception text in Install's minimal, standard, full and debug text/JSON partial-write captures; the supplied list named only the minimal and standard variants. The shared vocabulary already covered the access-denied cause, so no shared member was added. The final primary captures keep the raw cause only under `cause` evidence at `full`/`debug`; no raw cause remains in a headline, summary, finding message, count, or `minimal`/`standard` output, and no unrelated capture changed.

## Rollback

Restore the bridge registration for install.
