---
open-forge:
  description: Repair output catalogue and guided selection flow
  tags: [Memory, CLI, Task, Plan, G4, Repair, Contextual, Archived, Historical]
---

# 15 — repair

> Read [00 — G4 conventions](00-conventions.md) first. Depends on
> [03](03-rendering-system.md) and [04](04-interaction-system.md).

## Goal

`repair` says which links it rewrote, old to new, how many problems still
need a choice or a hand, and never claims to have verified anything when it
changed nothing.

## Depends on / Blocks

- Depends on: 03, 04. Lane B, after 14.
- Blocks: 40.

## Shape

Change report.

## Situations

`nothing-to-repair`, `automatic-two-links`, `automatic-nothing-safe-two-guided`,
`dry-run-automatic`, `relink-one`, `relink-invalid`, `contradictory-relinks`,
`non-interactive-no-selection`, `library-recovery-step`, `lock-held`,
`write-failed-partial`, `cancelled`, `invalid-input`.

## Statuses and headlines

| Status                  | When                                                                                      | Headline                                                                                  | Exit | Stream |
| ----------------------- | ----------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------- | ---: | ------ |
| completed               | nothing in scope needs repair                                                             | `Nothing to repair.`                                                                      |    0 | stdout |
| completed               | repairs applied, nothing left in scope                                                    | `Repaired <N> links.` (`Repaired 1 link.`)                                                |    0 | stdout |
| completed (dry run)     | repairs planned                                                                           | `Would repair <N> links.`                                                                 |    0 | stdout |
| completed-with-warnings | repairs applied, problems remain that need a choice or hand                               | `Repaired <N> links. <K> problems still need a choice.` / `... need a choice or a hand.`  |    2 | stdout |
| completed-with-warnings (dry run) | repairs planned, problems remain that need a choice or hand                 | `Would repair <N> links. <K> problems still need a choice.` / `... need a choice or a hand.` |    2 | stdout |
| completed-with-warnings | nothing safe, problems remain                                                             | `Nothing could be repaired automatically. <K> problems need a choice.`                    |    2 | stdout |
| completed-with-warnings | recovery bundle retained                                                                  | + family row                                                                              |    2 | stdout |
| incomplete              | diagnosis or a required fact could not finish                                             | `Repair could not check the workspace completely. Nothing was changed.` + limitation rows |    3 | stdout |
| invalid-input           | bad flags, invalid or contradictory `--relink`                                            | `Cannot repair: <problem>.`                                                               |    4 | stderr |
| blocked                 | no selection outside a terminal, diagnosis blocked, conflicting facts, stale target, missing authority, lock | `Cannot repair: <reason>. Nothing was changed.`                                           |    5 | stderr |
| failed                  | after effects                                                                             | `Repair stopped after <n> of <m> links were rewritten.`                                   |    1 | stderr |
| cancelled               | prompt cancelled, Ctrl+C                                                                  | `Repair was cancelled. Nothing was changed.`                                              |  130 | stderr |

## Text by level

`minimal`, applied:

```text
Repaired 2 links.
  .agents/loader.md:105:3    ../patterns/_patterns.md -> patterns/_patterns.md
  .agents/maps/_maps.md:32:3   nowhere/_Nope.md -> nowhere/_nope.md
```

`minimal`, applied with remaining problems:

```text
Repaired 1 link. 2 problems still need a choice.
  .agents/loader.md:105:3    ../patterns/_patterns.md -> patterns/_patterns.md
  Warning  .agents/maps/_maps.md:32:3       Broken link; 2 possible targets
  Warning  .agents/guidance/team.md:12:8    Broken link; no possible target, fix by hand
Next: open-forge repair  (choose a target for each remaining link)
```

`minimal`, nothing safe outside a terminal:

```text
Nothing could be repaired automatically. 2 problems need a choice.
  Warning  .agents/maps/_maps.md:32:3       Broken link; 2 possible targets
  Warning  .agents/guidance/team.md:12:8    Broken link; no possible target, fix by hand
Next: open-forge repair  (run it in a terminal to choose, or use --relink)
```

`minimal`, no selection possible (stderr):

```text
Cannot repair: repair needs to know which repairs to apply, and this session cannot ask.
Next: open-forge repair --automatic  (apply the 6 repairs that are safe)
```

`minimal`, partial (stderr):

```text
Repair stopped after 1 of 2 links were rewritten.
  .agents/loader.md:105:3    rewritten
  .agents/maps/_maps.md:32:3   not started
  Recovery data: <path>
Next: open-forge doctor
```

`standard` adds `Workspace:`, each remaining problem with its possible targets
as rows, the Library recovery steps selected or skipped, and the reason for
`Next`.

`full` adds why each possible target was suggested, the expected and new
SHA-256 of each rewritten file, the checks that ran (diagnosis coverage as a
sentence), and recovery facts in words.

## Prompts

Per [04](04-interaction-system.md): summary line, `Apply the <N> repairs that
are safe? [y/N]`, then for each guided link a Select among possible targets
with `skip` as the last row, then plan review, then `Apply these changes?
[y/N]`. The old typed words (`select`, `skip`, `back`, `cancel`) and the
SHA-256 confirmation text are removed.

## Findings catalogue

| Code                              | Severity | Family                     | Message                                                                                        | Next                                                             |
| --------------------------------- | -------- | -------------------------- | ---------------------------------------------------------------------------------------------- | ---------------------------------------------------------------- |
| repair.invalid-input              | error    | invalid-input              |                                                                                                |                                                                  |
| repair.confirmation-required      | error    | confirmation-required      |                                                                                                |                                                                  |
| repair.relink-invalid             | error    | local                      | `--relink <value> is not valid. Use: <path>:line:column, the link as written, the new target.` | `open-forge repair --help`                                       |
| repair.contradictory-relink       | error    | local                      | `Two --relink values name the link at <path>:l:c with different targets.`                      | none                                                             |
| repair.selection-required         | error    | local                      | `Repair needs to know which repairs to apply, and this session cannot ask.`                    | `open-forge repair --automatic`                                  |
| repair.missing-authority          | error    | local                      | `The Library recovery step for <id> writes outside .agents, and no grant allows that.`         | add the path to `allowInstallPaths` in `.agents/open-forge.json` |
| repair.diagnosis-blocked          | error    | local                      | `The workspace could not be checked: <reason>.`                                                | `open-forge doctor`                                              |
| repair.diagnosis-incomplete       | warning  | local                      | `<path> could not be checked, so its links were not repaired.`                                 | `open-forge doctor`                                              |
| repair.proposal-unavailable       | warning  | local                      | `The link at <path>:l:c is not one doctor reports as repairable.`                              | `open-forge doctor --detail standard`                            |
| repair.facts-conflicting          | error    | local                      | `The current Repair facts disagree about <path>:l:c.`                                         | `open-forge doctor --detail standard`                            |
| repair.proposal-unsupported       | error    | local                      | `The repair for <path>:l:c cannot be applied: <reason>.`                                       | fix by hand                                                      |
| repair.plan-conflict              | error    | local                      | `Two repairs change the same text at <path>:l:c.`                                              | none                                                             |
| repair.guided-finding-remaining   | warning  | local                      | `<path>:l:c  Broken link; <N> possible targets`                                                | `open-forge repair`                                              |
| repair.manual-finding-remaining   | warning  | local                      | `<path>:l:c  <title>; fix by hand`                                                             | none                                                             |
| repair.target-changed             | error    | target-changed             |                                                                                                |                                                                  |
| repair.target-unsafe              | error    | target-unsafe              |                                                                                                |                                                                  |
| repair.workspace-lock-unavailable | error    | workspace-lock-unavailable |                                                                                                |                                                                  |
| repair.recovery-conflict          | error    | recovery-conflict          |                                                                                                |                                                                  |
| repair.recovery-unavailable       | warning  | recovery-unavailable       |                                                                                                |                                                                  |
| repair.recovery-artifact-retained | warning  | recovery-artifact-retained |                                                                                                |                                                                  |
| repair.write-failed               | error    | write-failed               |                                                                                                |                                                                  |
| repair.verification-failed        | error    | verification-failed        |                                                                                                |                                                                  |
| repair.recovery-failed            | error    | recovery-failed            |                                                                                                |                                                                  |
| repair.operation-failed           | error    | operation-failed           |                                                                                                |                                                                  |
| repair.interrupted                | error    | interrupted                |                                                                                                |                                                                  |

## Effects wording

`<path>:l:c  <old destination> -> <new destination>` for a rewritten link;
`<path>  restored from recovery` for a Library recovery step. Dry run: same
rows under `Would repair`. Partial: `rewritten`, `not started`, `final state
unknown`.

## Counts

`linksRepaired`, `problemsRemaining`, `problemsNeedingChoice`,
`problemsNeedingHand`, `librarySteps`.

## Next rules

Remaining guided -> `open-forge repair`; no selection -> `open-forge repair
--automatic`; partial or retained -> `open-forge doctor` / `open-forge
cleanup`; completed -> none. Never `verified` wording anywhere.

## JSON data by level

| Level    | `data`                                                                                                                                                        |
| -------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| minimal  | `{ mode, selection: "automatic" \| "relink" \| "prompt", repairs: [ { path, location, from, to } ], remaining: [ { path, location, kind, candidates: n } ] }` |
| standard | + `remaining[].candidates: [ { path } ]`, `libraryRecovery: [ { id, path, selected } ]`                                                                       |
| full     | + `candidates[].reasons`, per repair `before`, `after`, `diagnosis { coverage per category }`, `verification`                                                 |

## References

- `src/cli/core/OpenForge.Cli.Core/Commands/Repair/Shared/Rendering/*` — replaced by `Presentation/Repair/`.
- `src/cli/core/OpenForge.Cli.Core/Commands/Repair/Shared/Interaction/RepairWizard.cs`, `RepairLibraryRecoveryWizard.cs` — replaced per 04.
- Repair interface and help `Catalogue`, `Selection`, `Preview and safety` sections (ledger only).

## Preconditions

- [ ] 03, 04, 14 merged.

## Steps

1. [ ] Write `RepairReportSelector` and `RepairDataTextRenderer`.
2. [ ] Wire the prompt flow per 04.
3. [ ] Delete old renderers and wizards.
4. [ ] Regenerate snapshots and review.
5. [ ] Three suites green.

## Acceptance

- [ ] `Nothing to repair.` is the entire `minimal` output on a healthy workspace.
- [ ] A zero-effect result never contains `verified`.
- [ ] Every rewritten link shows old and new at `minimal`.

## Execution capsule

- Applicability: this slice changes one Repair finding projection and one internal conflict name. Its affected boundary is the Repair result and presentation pipeline; the change is local, reversible through the working-tree patch, and does not alter filesystem or recovery authority.
- Standard capabilities: reuse the existing typed `RepairConflictKind` to `RepairFindingCode` map, `RepairFinding` location fields, and Repair wording/selection path. No new dependency or exceptional machinery is required.
- Evidence: Unit tests prove the finite conflict and finding mappings, the reachable conflicting-facts plan, and the new message/next action. Integration snapshots and safety coverage prove the command boundary; the required published Repair process class is the only EndToEnd selection.

## Changes ledger

> Started by a worker whose run ended at `Selected model is at capacity`, continued
> by a second run, and finished, reviewed and corrected by the overseer.

- file layout/type: the legacy Repair renderers and the `Commands/Repair/Models/Presentation` wire models are removed; native `Presentation/Repair/` owns the presentation, data model, selector, text renderer, wording, help and prompt adapters. `CliStandaloneComposer` closes the Repair binding over it.
- message: the legacy `Open Forge repair` header, the `Status:` line and the `Application: applied (N effects)` line are gone. The headline states the outcome, for example `Repaired 2 links.`, and each repaired link is a row beneath it. The retired `N effects` line also carried a plural-grammar defect for a single effect.
- message: the interactive plan review no longer prints a `Preview of selected repairs` header. It renders the preview report itself at minimal detail, so the preview opens with that report's own headline: `Would repair <N> links.` before an unapplied plan, and `Repaired <N> link...` where a Library recovery step has already run.
- JSON member: `data` became `{ mode, selection, repairs, remaining }`. `selection` is a string such as `automatic`, replacing the legacy `selection` object and its `selectedLibraries` array. Library recovery is reported as an envelope effect (`{ path, kind, action, outcome }`) rather than a `libraryExecution.receipts` array inside the command data.
- JSON member: the legacy `data.plan`, `data.preflight`, `data.application`, `data.verification`, `data.postDiagnosis`, `data.counts` and `data.automatic` are gone from `data`. The facts they carried are published by the envelope instead: `effects[].outcome` (`planned` or `done`), root `counts` (`linksRepaired`, `problemsRemaining`, `problemsNeedingChoice`, `problemsNeedingHand`, `librarySteps`) and `recovery.disposition` (`not-required` or `removed`).
- snapshots: all thirteen situations named in `Situations` captured at the four native detail levels, text and JSON, under `src/cli/tests/integration/snapshots/RepairBeforeOutputSnapshotTests/` (127 files). The legacy `Commands/Repair/__snapshots__` tree (52 files, `.compact`/`.expanded`) is now unreferenced but was left in place; see divergence 7.
- test: `LibraryRepairRecoveryIntegrationTests` (16 cases) and `RepairCompositionInteractionIntegrationTests` were migrated from the legacy graph and wording to the native ones.
- test, overseer: `RepairCompositionInteractionIntegrationTests.InteractiveFlowRendersPreviewsBeforeQuestionsAndAppliesOnce` located each preview by the deleted legacy header `Preview of selected repairs`, and now orders on `Would repair `, the preview report's own headline. `LibraryResidualSelectUsesRealPromptAndFinalConfirmation` asserted `Would ` and now asserts `No files were changed.`, because that flow previews a result whose Library recovery step has already run and so opens with `Repaired ...`.
- test, overseer: `PublishedRepairProcessTests` still read the legacy `data` dump in two of its three journeys; both were migrated to the native envelope. `AutomaticJsonDryRunPreviewsSafeExactWithoutEffects` now asserts `data.repairs` and `data.remaining`, `effects[0].outcome == planned`, the root `counts`, and `recovery.disposition == not-required`. `ExplicitRelinkPreservesContentAndConvergesToNoOp` now asserts `data.selection == relink`, `effects[0].outcome == done`, `recovery.disposition == removed`, and for the converged pass the `Nothing to repair.` headline with empty `effects`, `repairs` and `remaining`.
- test, overseer: `PublishedShellBoundaryProcessTests.OptionLikeOperandAfterTerminatorRemainsDomainInput` read `data.selection.requestedReference`, which the native Route Inspect report does not publish. It now asserts the operand survived as domain input through `findings[0].code == route-inspect.unknown-source` and `findings[0].subject.id == --view`. This caller belongs to [21](21-route-inspect.md) and was already failing on the branch before this subtask merged; see divergence 4.
- evidence: on the merge commit, unit 3,423 passed, 0 failed, 0 skipped; integration 2,252 total, 2,235 passed, 0 failed, 17 skipped; end-to-end 163 passed, 0 failed, 0 skipped; `dotnet format whitespace` exactly the five documented errors.

- message: the `completed-with-warnings` dry-run headline changed from `Repaired <N> links. <K> problems still need a choice.` to `Would repair <N> links. <K> problems still need a choice.` (and the corresponding `choice or a hand` form), so a preview does not claim that files were changed.
- message: the remaining-problem verb now uses `CliText.Plural(remaining, "needs", "need")`; the singular forms are `Repaired 1 link. 1 problem still needs a choice.`, `Would repair 1 link. 1 problem still needs a choice.`, and `Nothing could be repaired automatically. 1 problem needs a choice.`
- finding code: the stale explicit relink changed from `repair.plan-conflict` to `repair.proposal-unavailable`, with its current proposal location retained so the catalogue message can render exactly.
- next action: the stale explicit relink changed from no next action / `open-forge doctor` to `open-forge doctor --detail standard`, matching `repair.proposal-unavailable`.
- test: `PublishedRepairProcessTests.AutomaticJsonDryRunPreviewsSafeExactWithoutEffects` now asserts the mixed safe-exact/guided JSON headline and the matching minimal headline while retaining the no-write assertions; `RepairFiniteMappingTests` asserts every conflict-kind mapping and `RepairResultTests` asserts the proposal-unavailable next command.
- snapshots: the ten `ExplicitRelink/relink-invalid` native captures were regenerated for the ruled code/message/next-action change; no other Repair capture changed.
- evidence: the final Release build passed with 0 warnings and 0 errors; unit tests passed 3,177/3,177 with 0 failed and 0 skipped; integration tests passed 2,202/2,202 with 17 skipped and 0 failed (2,219 total); the targeted published Repair class passed 3/3; the targeted snapshot class passed 8/8; whitespace reported exactly the five documented pre-existing errors.

- severity: the `repair.proposal-unavailable` row said `error` -> it says
  `warning`, which is what the code renders. The finding carries
  `CliSemanticStatus.Incomplete` (`RepairDefinitions.cs:306`) and
  `CliReportVocabulary.Severity` maps `Attention or Incomplete` to `Warning`.
  The row was already inaccurate; adopting this code for the stale explicit
  relink made it visible. A sweep of every catalogue row against its finding's
  status found no other mismatch.

- catalogue: added `repair.confirmation-required` with the shared
  `confirmation-required` family; the finding carries `CliSemanticStatus.Invalid`
  (`RepairDefinitions.cs:282`) and so renders as an error at exit 4. It reached
  users with no row, the same gap found across nine commands.
- finding code and severity: `TargetIdentityMismatch` changed from
  `repair.proposal-unavailable` / warning to `repair.facts-conflicting` / error;
  `RepairConflictKind.SelectionUnmatched` keeps both proposal-selection sites on
  `repair.proposal-unavailable` / warning.
- message: the reachable facts-disagreement situation changed from `The link at
  <path>:l:c is not one doctor reports as repairable.` to `The current Repair
  facts disagree about <path>:l:c.`; the proposal-unavailable message stayed
  unchanged for both selection-unmatched sites.
- next action: facts disagreement now points to `open-forge doctor --detail
  standard` with `Inspect the conflicting Repair facts before relying on this
  result.`; proposal-unavailable keeps its existing command and reason.
- type name: internal `RepairConflictKind.MissingAuthority` became
  `RepairConflictKind.SelectionUnmatched`; `RepairFindingCode.MissingAuthority`
  and its permission message remain unchanged, so no user-visible wire code,
  message, or exit changed from the rename.
- test: `RepairC1SelectionTests` proves the duplicate-occurrence planning path
  reaches `TargetIdentityMismatch` and forms `repair.facts-conflicting` with its
  source location; finite mappings, finding definitions, next action, and
  wording coverage were extended without removing existing coverage.
- snapshots: the Repair snapshot class was regenerated serially; all 8 tests
  passed and no capture changed because none of the named situations emits
  `TargetIdentityMismatch`.
- evidence: the final Release build passed with 0 warnings and 0 errors; unit
  tests passed 3,186/3,186 with 0 failed and 0 skipped (two above the 3,184
  baseline); integration passed 2,202/2,202 with 17 skipped and 0 failed (2,219
  total); the targeted published Repair class passed 3/3; whitespace reported
  exactly the five documented pre-existing errors.

## Divergences observed

1. **The `relink-invalid` situation never exercises `repair.relink-invalid`.** The
   prior run recorded that `ExplicitRelink/relink-invalid` captures `next=none`
   while this file's findings catalogue gives `repair.relink-invalid` the next
   action `open-forge repair --help`, and suspected either the selector or the
   upstream result. Neither is at fault. The fixture in
   `RepairBeforeOutputSnapshotTests.ExplicitRelink` passes a well-formed relink
   with a stale expected destination (`expectedDestination: "changed.md"`) and
   asserts `CliSemanticStatus.Blocked`, so the situation emits
   `repair.plan-conflict`, whose catalogue row gives `none`. The capture is
   correct for what the situation actually produces. What is missing is coverage:
   no situation feeds a malformed `--relink` value, so the `repair.relink-invalid`
   row, its message and its `open-forge repair --help` next action are
   unevidenced, and the `Statuses and headlines` row sending "invalid or
   contradictory `--relink`" to `invalid-input` at exit 4 is only half covered, by
   `contradictory-relinks`. **Maintainer decision:** either rename this situation
   to `plan-conflict` and add a real `relink-invalid`, or add the second situation
   alongside it. Not done here, because the `Situations` list is the
   specification and names exactly one of the two.
   The stale request itself now emits `repair.proposal-unavailable`;
   the malformed-value gap remains unchanged and still has no evidence.

2. **`repair.plan-conflict` emits a message the catalogue does not carry.** The
   catalogue gives `Two repairs change the same text at <path>:l:c.` The stale
   explicit relink above emits `The explicit relink does not match one current
   admitted Repair proposal.` from `RepairSelectionPlanner`, which raises a
   `RepairConflict` of kind `MissingAuthority` that `RepairOperation` flattens to
   `PlanConflict`, discarding the kind. The catalogue's
   `repair.proposal-unavailable` (`The link at <path>:l:c is not one doctor
   reports as repairable.`, next `open-forge doctor --detail standard`) is the
   closer match. Before the ruling, output strings were frozen, so nothing was
   changed.
   **Maintainer ruling:** implemented by preserving the typed conflict through
   result formation, mapping MissingAuthority to ProposalUnavailable, and
   retaining the matching proposal occurrence for the catalogue formatter.

3. **The plan review was present all along; two assertions were stale.** The prior
   run recorded that the interactive flows "show no plan review before the
   confirmation" and proposed a defect in the Repair prompt adapters, leaving the
   two assertions strict. Captured stderr refutes it. `PlanConfirmationCoreAsync`
   renders the preview at minimal detail before each confirmation, in the required
   order: `Would repair 1 link.`, `Apply the 1 repair that is safe? [y/N]`,
   `choose a target for "missing.md".`, `Would repair 2 links.`,
   `Apply these changes? [y/N]`. Both assertions were looking for the deleted
   legacy header rather than for the preview report. The adapters are correct and
   were not changed; the assertions were migrated, as recorded in the ledger.
   `CliPrompts.PlanReview` named in the prior note is a file,
   `CliPrompts.PlanReview.cs`; the member is `CliPrompts.PlanConfirmation`.

4. **The published journeys had not been migrated, here or in Route Inspect.**
   `PublishedRepairProcessTests` failed 2 of 3 against the installed binary, both
   with `KeyNotFoundException` on legacy `data` members, and the prior run
   reported green without running the class. Separately, the full end-to-end suite
   on this branch was **not** at 163 passed, 0 failed as the handover recorded:
   `PublishedShellBoundaryProcessTests.OptionLikeOperandAfterTerminatorRemainsDomainInput`
   was already failing on `data.selection.requestedReference` after
   [21](21-route-inspect.md) merged, and the Repair merge touches neither
   `Commands/Route/` nor `Presentation/Route/`. Both callers are now migrated, as
   recorded in the ledger. **Durable record to change:**
   [21](21-route-inspect.md)'s ledger does not record this caller.

5. **A dry run reports links as repaired when problems remain.**
   `repair --automatic --dry-run --format=json` over a workspace with one
   safe-exact and one guided link emits
   `summary.headline: "Repaired 1 link. 1 problem still need a choice."` with
   `counts.linksRepaired: 1`, although `effects[0].outcome` is correctly `planned`
   and nothing is written. The `Statuses and headlines` table gives a dry-run
   headline only for the `completed` row (`Would repair <N> links.`); the
   `completed-with-warnings` row has no dry-run form, and `RepairWording` does not
   branch on mode for it. The maintainer ruling is implemented as
   `Would repair <N> links. <K> problems still need a choice.`; the catalogue now
   has a dedicated `completed-with-warnings (dry run)` row, and the published
   mixed journey asserts both JSON and minimal output.

6. **`still need a choice` does not agree with a single problem.**
   `RepairWording` pluralises `link` and `problem` through `CliText.Plural` but
   hard-codes `need`, so `K == 1` prints `1 problem still need a choice.` The
   catalogue gives only the plural template and is explicit about singular forms
   elsewhere (`(Repaired 1 link.)`), so the `K == 1` form is unspecified rather
   than contradicted. The maintainer ruling is implemented through the
   existing `CliText.Plural` helper with explicit singular/plural verbs,
   covering the applied-with-remaining, dry-run-with-remaining, and
   nothing-automatic sentences.

7. **The legacy `__snapshots__` trees are orphaned across the whole effort, not
   just here.** Repair's thirteen situations are captured natively, and nothing
   references `Commands/Repair/__snapshots__` any more, so its 52 `.compact` and
   `.expanded` files are dead. The same leftover exists for every other command
   whose output has already gone native: 23 integration `__snapshots__`
   directories survive, about 1,150 files in total, including Context, Find,
   Index, Install, every Library verb, References, Update and every Route verb.
   They were not deleted here, because `MatchSnapshot` is still live for
   `DoctorBeforeOutputSnapshotTests` and for the shared
   `CommandOutputSnapshotTests` base, so whether each tree is dead has to be
   decided per command rather than by pattern, and deleting one command's tree
   while twenty-two identical ones remain would leave the repository less
   consistent, not more. **Belongs to [40](40-verification.md)**, as a
   cross-command invariant: for each command, assert that a native
   `snapshots/<Command>BeforeOutputSnapshotTests/` tree exists and that the
   legacy `__snapshots__` tree for the same command does not.

8. **The plan-conflict output path has no current native situation after the
   stale relink ruling.** `OverlappingChanges` maps to
   `repair.plan-conflict` and remains reachable in the result mapper; the
   plan-level `RepairPlanningModelTests` still constructs that conflict.
   No current before-output fixture produces the finding, so its catalogue
   message remains an output-coverage gap. The stale explicit relink now covers
   `repair.proposal-unavailable` instead.

9. **The facts-conflicting path has no current native situation either.** The
   `Combine` path in `RepairSelectionPlanner` is reachable when duplicate
   inputs share one occurrence but disagree, and the new Unit test proves that
   path and its finding projection. The named before-output situations do not
   create duplicate facts, so regeneration produced no capture diff and no
   situation was renamed or added. `repair.plan-conflict` likewise remains
   unchanged and unexercised at the native output boundary.

Historical: the first worker reported three presentation issues it had begun
correcting when its run ended, namely full text exposing internal state labels,
single-link count rows using plural grammar, and partial failures omitting the
required recovery-data row. All three are addressed in the merged result; the
third is evidenced by the `PartialWriteFailure` captures.

## Rollback

Restore the bridge registration for repair.
