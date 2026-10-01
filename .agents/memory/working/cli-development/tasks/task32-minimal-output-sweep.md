---
open-forge:
  description: Open Task 32 to reconcile minimal output across all 29 operations, including root Remove
  tags: [Memory, Working, CLI, Task, Presentation, Minimal, Contextual, Active]
---

# Task 32 â€” Minimal Output Sweep

**Reviewed on 2026-09-28:** [review](../../../emerging/analysis/open-task-review/task32-minimal-output-sweep.md). Recommendation:
Do before 1.0. The review names any details in this record that are out of date.

## Task state

- State: **Complete independent qualification.** The reconstructed Task32
  candidate passes the full managed, native/public, and local package gates.
  Root owns scheduled integration and combined beta5 qualification.
- Owner: Task32 completion owner. Root owns integration and final acceptance.
- Trigger: the maintainer's ruling on
  [40 â€” verification](../../../archived/cli-development/tasks/task30-g4/40-verification.md) divergence 6 and
  [30 â€” extension install](../../../archived/cli-development/tasks/task30-g4/30-extension-install.md), recorded
  2026-09-16. Both were settled in favour of the shared presentation rule for
  now, with this sweep queued to revisit the underlying question.

## The question

Two lines are currently required in `minimal` output by shared presentation
rules, and both were questioned by the command catalogues that had to carry
them:

- The `Workspace:` echo, required by accepted decision **C12** in
  [the G4 packet](../../../archived/cli-development/tasks/task30-g4/_task30-g4.md#accepted-decisions).
- The `Next:` action line.

`index` at `all-current` and `update` at `up-to-date` both print a `Workspace:`
line at `minimal` that their own one-line catalogue examples omit. The examples
were written as if the smallest useful answer were a single sentence; the shared
rule makes it three lines. The immediate conflict was resolved by keeping the
rule and correcting the examples, but that resolution did not decide whether the
rule itself is right for `minimal`.

`Next:` raises the same question from the other direction: Extension Install
requires a continuation line after it, which forced the "at most one `Next:`
line, and it is the last line" invariant to be relaxed.

The question this Task answers: **at `minimal`, which of these two lines earn
their place in every command, and which are better at `standard` and above?**

`minimal` is the default detail level, so this decides what most users see most
of the time.

## Actionable boundary

- Use the frozen inventory of all 29 operations, including root `remove`. The
  former 28-command sweep count is historical. This receipt does not authorize
  a fresh audit.
- A `Workspace:` echo earns its place where the workspace is ambiguous â€” where
  the command may have resolved a different workspace than the reader expects.
  It does not earn its place merely because a rule requires it.
- A `Next:` line earns its place where there is a genuine next step. A next
  action that restates what the reader just did, or that every run always emits,
  is noise at the default level.
- Decide per situation, not per command: the same command may warrant the echo
  when it changed something and not when it reports that nothing needed doing.
- Any change here is behaviour-changing and touches every affected capture.
  Treat it as a G4-scale presentation change with the same review discipline,
  not as a tidy-up.

## Acceptance

- A recorded decision for the `Workspace:` echo and for `Next:` at `minimal`,
  with the reasoning and the situations each applies to.
- If the rule changes, C12 and the shared presentation rules in
  [Human Presentation](../../../crystallized/documents/cli/shared-operation-contract.md#human-presentation) are amended, every affected
  catalogue's `Text by level` examples agree with the code, and every capture is
  regenerated and reviewed.
- If the rule stands, the two catalogues whose examples omitted the echo are
  corrected and the question is recorded as settled.
- All four gates green, and no other output changes.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Historical source qualification and transfer

**Done:** The bounded beta4 polish remains historical. The cache-only solution
restore exited 0; its log is artifacts/task32/restore.log. The original frozen
candidate and its initial root gate failures remain historical evidence.
Original candidate.patch, original manifests, prior failed logs, and raw
independent-review evidence were preserved.

Grouped repair r_53fe09b69239 closed T32-R1 and T32-R2 by changing exactly the
three authorized test files. The other 96 candidate files retain their
original hashes: 11 production files, 83 text captures, and two other tests.
No JSON captures or production files changed during repair; no capture suite
was rerun. All 17 duplicate Next situations remain, and the frozen inventory
is all 29 operations including root Remove.

The repaired focused qualification passed 102/102 with zero failures and zero
skips: Task32WorkspacePolicyTests 1/1, CliReportInvariantsTests 8/8,
ExtensionInstallPermissionNextPolicyTests 2/2, and CliReportRenderingTests
91/91. The scoped formatter for the two changed test files, full whitespace
verify, and full warning analyzer verify each exited 0 with no stdout or
stderr. No assertion changes were made. Mastermind revalidated the strict
diagnostic/envelope counterguards and whitespace-only B/C diffs with
git diff --no-index --ignore-all-space --ignore-blank-lines, exit 0; this is
focused closure evidence, not another whole-task review. Detailed commands,
counts, exits, and hashes are in
artifacts/task32/grouped-repair/repair-evidence.json and
artifacts/task32/repaired-review-closure-evidence.json.

One independent whole-task review completed as run r_b85fdd23a797 (Sol 6.1
high). It found no material production defect within the accepted scope,
99/99 hashes, 83 changed text captures, and 98 valid capture pairs. T32-R1 and
T32-R2 are closed by the accepted single grouped repair. Review budget is 1
consumed; grouped repair budget is 1 consumed by r_53fe09b69239; councils are
0. The repair lane is frozen.

**Now:** The repaired candidate is frozen in the Root worktree. Its 99 files
match the repaired source manifest; 96 retain the original hashes and exactly
three authorized tests have repaired hashes. Candidate patch SHA256 is
8a6051473203d624ae11976b06ba516c8691a6ddcf2ce4a95c3d130e4567c42b; the
repaired source manifest SHA256 is
8ca3203a96c25f3f0a775d8919d2fe4f9d2be3e826f5f328972f1c8721eef293; the final
99-file transfer manifest SHA256 is
9b987d7d3c39c52e0e2c4fe98e938a93cb69d93a500afdc2d5af824b65222580. The
three repaired test hashes and exact transfer proof are in the repaired
addition and final transfer manifests. The original selector patch is
unchanged.

The previous root full-gate attempt remains historical at 0/4 green. Its npm
wrapper failures involved unavailable semver, and its earlier formatter check
reported whitespace errors before the grouped repair. No root gates were rerun
here. Root integration owns the full managed, native, and package gates when
its verified prerequisites are available; no package was installed here.

**Pre-integration next step (source checkpoint):** The earlier Root integration
plan referenced Task39 selector/D04 typed failure, Task64 recovery, Task34
markup, and Task70 priority integration. The beta5 horizon later paused Tasks
39 and 34 and removed them from Task32 acceptance prerequisites. The Root
integration acceptance is recorded below. Root owns the final managed, native,
and package gates. Root-owned document proposals were applied to the three
approved contracts in this integration packet; the source candidate made no
contract, docs/cli.md, or ledger edits. This packet did not refresh captures.

**Source-checkpoint blocker:** None for the local grouped correction. At that
checkpoint, integration qualification and Root acceptance remained pending;
the later integration disposition is recorded below. Earlier environment
gate failures remain historical.

## Accepted Implementation Capsule

This capsule records frozen, user-authorized design direction. It is not
implementation evidence, a review result, qualification, or root acceptance.

### Applicability and consequences

The user authorized implementation against the frozen current `Workspace`
policy and its seven override removals. This is local CLI presentation
correctness and actionability. It is reversible through Git and regenerated
captures. Operation behavior, effects, status, exit, and safety stay unchanged.
Standard typed selection is sufficient. Exceptional machinery: none.

### Design and boundaries

- Suppress the healthy/complete `ExtensionList` hint only in `minimal` text
  output through the task-local `IsHealthyAvailableHint` Boolean with
  `JsonIgnore`. Keep JSON and higher detail unchanged. Preserve corrective,
  warning, and incomplete-result selection.
- For `PermissionRequired` with `MissingPermissions`, use typed
  `TextNextLines` to place the alternative before the final `Next:` and retain
  both exact options.
- The frozen inventory covers all 29 operations, including root Remove. Retain
  all 17 duplicate `Next:` situations unchanged. Task 39's D04 and failure
  semantics, Task 64's recovery, and Task 34's markup remain unchanged.
- Shared-document proposals remain root-owned. Shared contracts stay
  unchanged. The only seam is task-local and typed.

### Evidence and execution budget

- Cheapest decisive evidence: Unit selected-report tests, focused Integration
  and host-output tests, plus affected captures and invariants.
- Run the complete managed suite and supported Native AOT gate at the
  shared-presentation trigger and final integration.
- Budget: one fresh whole-task review (Sol 6.1 high or Astra medium), one
  grouped repair, and zero councils. Consumption: review 1 (run
  r_b85fdd23a797), grouped repair 1 (run r_53fe09b69239), councils 0. The
  repair lane is frozen.

### Provenance

Frozen design `r_5f32c13b1dee`; base commit
`2e0da10a5657c084b2847245775858123e09a5ed`.


## Root integration acceptance addendum â€” 2026-10-01

Root accepted the frozen Task 32 candidate for inclusion in beta5 combined
qualification after exact integration r_7e676fedfbd6. The earlier attempt
r_572300b52200 verified 96 of 99 reviewed files and stopped before builds or
tests because three already-reviewed additions were omitted from that handoff.
This was a transfer omission, not a product defect. The replacement copied the
three additions, matched all 99 manifest hashes, preserved the original 96
destination files and 94 unrelated status/hash records, and passed two Release
builds with zero warnings or errors. Four focused selections passed 102 tests,
zero failed or skipped, with all exits zero. The source candidate's separate
102-test run and full whitespace/analyzer verification also passed. Task 32 is
accepted for combined beta5 qualification, not closed; final managed, native,
package, and user-journey gates remain.


## Historical independent qualification checkpoint — 2026-10-02

**Done:** Reconstructed the repaired candidate at base
`2e0da10a5657c084b2847245775858123e09a5ed`. All 99 accepted raw file hashes
match the manifest and both recorded author trees. The accepted three contract
changes are included. No JSON capture, production file, or original text capture
was changed during this completion attempt. Beta4 remains the candidate version.

Fresh managed focused selections passed 118 tests, with zero failures or skips:
rendering 91, Extension List 6, permission ordering 2, workspace policy 1,
report invariants 8, and root Remove 10. The complete Release solution build,
full whitespace verification, and full warning-analyzer verification exited 0.
The build reported zero warnings and errors.

The first full Unit gate found three stale exact-string expectations in
`src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Repair/RepairProjectionTests.cs`.
Their fixtures select an explicit workspace. Completion correction T32-Q1 adds
only the required Workspace line to the existing no-op, invalid-input, and
cancellation equality assertions. No assertion was removed or loosened.
The rebuilt Unit project reported zero warnings/errors, scoped whitespace and
warning-analyzer checks exited 0, and all 3,939 Unit tests passed. This one
neighboring test file supplements the accepted 99-file inventory.

All three Windows x64 Native AOT publications exited 0: CLI, Integration, and
EndToEnd. The managed EndToEnd build targeting the native CLI also exited 0,
with zero warnings/errors. Publication is separate from test execution.

**Now:** Full managed Integration executed 2,687 cases: 2,669 passed, one
failed, and 17 skipped. All skips match the repository's declared Windows
platform exclusions. T32-Q2 is the unchanged Extension Create
`CatalogueUnreadable` diagnostic capture. Its text and JSON debug diagnostics
expect one ellipsis shape and receive another after bounded path normalization.
The Extension Create operation, presentation, tests, and shared rendering remain
byte-equivalent to this base. This is outside Task32's frozen policy boundary.
No snapshot update, filter, or assertion relaxation was used to pass it.

An earlier artifact-local TEMP setup caused parent Git discovery and longer
bounded diagnostics. A fresh task-owned short temporary root outside Git
cleared eight of nine Integration failures. The remaining capture conflict is
preserved in `artifacts/task32/sol-completion/blocking-conflict.json`, including
expected/received differences and the unchanged-boundary proof.

**Next:** Root must provide a qualified baseline or authorize disposition of
that single Extension Create diagnostic capture. Then rerun the complete
managed gate, execute native Integration and EndToEnd plus managed EndToEnd on
the native CLI, and qualify the local package. The full managed EndToEnd stage,
native executions, and package stage have not run. They are not green.

**Blocker:** T32-Q2. Independent merge readiness is **false**. The complete
owned patch is mechanically applicable to the exact base, but Task32 is not
closed. No commits, staging, merges, pushes, release, version changes, or shared
ledger edits were performed. Tasks 39 and 34 remain paused, Task 48 remains
deferred, and Task 70 ownership is unchanged.

### Completion receipt and reproduction

`artifacts/task32/sol-completion/completion-receipt.json` records exact commands,
exits, counts, skips, tool versions, artifact identities, remaining requirements,
and the 104-file before/after inventory. Before hashes identify base Git blob
bytes. After hashes identify current raw worktree bytes. The patch contains all
three additions, the accepted 99-file delta, the three approved contracts, this
record, and the single qualification correction. Ignored artifacts and prompt
files are excluded. Historical failures remain in the same evidence directory.

The execution uses cache-only restore, disabled build servers, single-node
builds, isolated product data/AppData, an owned external temporary root, and
repository-local reports. Existing cached Node dependencies are consumed
through artifact-local copies of the unchanged delivery modules. No dependency
was downloaded or added. The original whole-task review remains 1/1 consumed,
the original grouped repair remains 1/1 consumed, and councils remain 0. This
completion loop did not create another review phase.

Required executable commands remain the repository's ordinary build and test
surface. Restore uses `NuGet.Config` and the existing local NuGet cache with
`NuGetAudit=false`. Build the solution with:

```text
dotnet build OpenForge.Cli.slnx -c Release --no-restore --disable-build-servers -m:1
```

Run each managed test assembly from `artifacts/bin/<assembly>/release/`, and
publish the CLI, Integration, and EndToEnd projects for `win-x64` with
`--self-contained true --no-restore -p:OpenForgeSkipDevelopmentPublish=true
--disable-build-servers -m:1`. Each full suite uses the unchanged repository
runner settings:

```text
--minimum-expected-tests 1 --parallel collections --fail-warns on --fail-skips off --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory <owned-report-directory>
```

The repository qualifier rejects failures, unknown skips, and zero-test runs.
The ordinary delivery commands are `npm run test -- --no-restore`,
`npm run build:native -- --no-restore --rid win-x64`,
`npm run test:built -- --rid win-x64`, and
`npm run pack -- --rid win-x64 --targets win-x64`. No unqualified package is
accepted as proof. Exact commands actually executed are in the JSON receipt.


## Independent completion — 2026-10-02

Task32 is independently qualified and ready for Root integration at the exact
beta4 base. The accepted 99-file implementation still matches every frozen
hash. The final owned delta contains 107 files: those 99, the three approved
contracts, this record, the Repair equality-assertion correction, and the
three CatalogueUnreadable qualification files. All three original additions
are included. No production file or primary JSON capture changed during closure.

### Qualification findings

T32-Q1 is fixed: three Repair equality assertions include the explicit
Workspace line required by the accepted shared policy. Their other content
and assertions remain intact.

Root authorized narrow disposition of T32-Q2 after the first independent gate.
It was a capture-ordering defect, not a product diagnostic defect. The debug
renderer correctly escapes and bounds each line to 240 characters. Normalizing
an already bounded path made the expected suffix depend on the owned temporary
path length. CatalogueUnreadable now uses the existing Extension Create comparer
to expand its complete expected fixture coordinate before bounding it. Received
diagnostics remain untouched. The two diagnostic expectations retain the full
catalogue-unavailable subject and access-denied cause.

The real ACL case covers 0, 20, and 60 additional path characters. It directly
asserts the complete subject and raw cause, two intended effects, zero applied
effects, and unchanged workspace hashes. All 13 adjacent snapshot cases passed
with no skips. Six preserved raw diagnostics match the independently expanded
and bounded templates; 30 mutations of visible semantic facts fail comparison.
The full raw cause is also asserted before the diagnostic bound can omit its
suffix. No shared normalization or production renderer changed.

All original failure artifacts remain. The initial closure run also exposed
a terminal newline added to the two diagnostic expectations; its report and
received output are retained. The corrected expectations match the renderer's
exact framing. This closes new qualification evidence without another review
phase. The accepted whole-task review and grouped repair budgets remain 1/1
consumed, and councils remain 0.

### Fresh executable qualification

The six managed/native targets below passed the unchanged repository report
qualifier. Required tests were not filtered out. Integration skips are the
17 declared Windows platform exclusions on each runtime target; all other
targets have zero skips, and every target has zero failures and warnings.

| Target | Reported cases | Passed | Failed | Declared skips |
| --- | ---: | ---: | ---: | ---: |
| unit | 3939 | 3939 | 0 | 0 |
| integration | 2689 | 2672 | 0 | 17 |
| public | 263 | 263 | 0 | 0 |
| native-integration | 2689 | 2672 | 0 | 17 |
| native-public | 263 | 263 | 0 | 0 |
| public-native | 263 | 263 | 0 | 0 |

The six original focused selections passed 118 tests. Catalogue closure adds
13 passing focused cases, for 131 accepted focused checks. The complete Release
solution build, full whitespace and warning-analyzer checks, and the scoped
checks after both qualification corrections exited 0. Correction builds
reported zero warnings/errors. CLI, Integration, and EndToEnd Native AOT
publications passed; the changed Integration executable was republished after
the final fixture correction. Actual native execution is recorded separately
from publication. The CLI and EndToEnd production inputs remain unchanged.

The local package gate passed its installed-launcher version journey and
verified native-byte identity. Five further Task32 package journeys passed
through eight invocations of the installed npm launcher: healthy Extension List
minimal/standard/JSON policy, Repair workspace echo, Cleanup workspace echo,
permission alternatives before final Next with preserved bytes, and root Remove
workspace echo. The install is a local setup step. These checks use owned
scratch resources, local tarballs, isolated data/AppData and npm settings,
offline package-manager flags, and no repository Doctor invocation.

### Completion checklist and transfer

- [x] Reconstruct all accepted 99 files and preserve their hashes.
- [x] Include meaningful formerly untracked test sources.
- [x] Close T32-Q1 and authorized T32-Q2 with exact semantic assertions.
- [x] Pass complete managed Unit, Integration, and EndToEnd qualification.
- [x] Publish and execute the supported native/public targets.
- [x] Qualify the local package and Task32 installed-package journeys.
- [x] Preserve failed reports and unchanged received diagnostics.
- [x] Reconcile the canonical task record and approved contracts.
- [x] Prepare complete owned binary patch, exact before/after inventory,
  safe addition payloads, and exact-base apply check; pass `git diff --check`.
- [x] Preserve beta4, Root shared ledgers, all other task trees, and review budget.

`artifacts/task32/sol-completion/completion-receipt.json` binds the exact source
inventory, commands, exits, runtime targets, counts, skips, artifact hashes,
package identity, correction disposition, and patch. `owned-task.patch` includes
all 107 paths and applies mechanically to
`2e0da10a5657c084b2847245775858123e09a5ed`. `file-inventory.json` gives exact base
Git-blob before hashes and current raw worktree after hashes; `payload/` contains
the exact owned files. The earlier blocked checkpoint and failure evidence are
retained separately. Ordinary reproduction commands are recorded above; exact
executed argument arrays and the cache-only tooling arrangement are in the receipt.

**Independent merge readiness: true. Blocker: none.** No execution requirement
remains for this independent candidate. Root owns scheduled integration,
combined beta5 qualification, and release decisions. No commit, staging, merge,
push, release, version bump, shared-ledger edit, Task70 change, or new delegate
was performed. Tasks 39 and 34 remain paused and Task48 remains deferred.
