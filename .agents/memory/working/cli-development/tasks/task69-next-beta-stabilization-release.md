---
open-forge:
  description: Coordinate the next beta stabilization, qualification, merge, and release boundary
  tags: [Memory, Working, Task, CLI, Contextual, Active]
---

# Task 69 — Next beta stabilization and release

## Outcome

On 2026-10-01, the maintainer accepted the direction to complete
stabilization and qualification in this isolated worktree. Only after the
final reviewed local gates are green and the candidate is clean may it be
squash-merged to `develop`. Only after that merge may the next beta be pushed
and released. There is no feature-branch push.

This horizon preserves [Task 64](task64-cli-defects-and-contract-drift.md)
behavior and contract corrections, [Task 61](task61-documentation-accuracy-and-voice.md)
documentation reconciliation, [Task 32](task32-minimal-output-sweep.md)
narrow output ownership, and the [Task 68](task68-repository-link-validation.md)
offline link checker.

The release must qualify the exact merged commit with successful Windows,
Linux, and macOS CI on x64 and ARM64, package checks, seven npm packages, six
portable archives and checksums, installation and beta-upgrade or demonstration
smoke, and a truthful source/version receipt. The crash fix from
`c27e63e4d` is included and requalified in the frozen candidate. The existing
PATH/dev executable is beta 1 and is not candidate evidence.

The execution profile is Batch. The horizon has three phases, implementation,
qualification, and integration/release, with five milestones: packets frozen;
fixes, documentation, and the checker integrated; local gates and fresh review
green; squash plus the hosted six-platform gate green; and publication,
package, and demonstration receipts verified.

## Current release state and focused baseline

Beta 3 remains the published release. Beta 4 remains unpublished. The frozen
candidate is `ff84af47c4b404d3cc1908c6754567bc30730b2d` with tree
`549e81a774af4059809920100a794fedd899f0b5`. Task 69 is phase 3/3, milestone
3/5: implementation, local qualification, and whole-candidate review are
green, and the clean squash merge is complete. Original Build 36804134087
finished with the hosted fixture failure described below. The correction is
integrated, the corrected-candidate repository gate passed, and full
native/test/package requalification, a second clean squash, and a new
exact-SHA hosted gate remain pending. Publication has not started. Historical
beta 2 records remain unchanged.

A full solution build before the version bump succeeded with 0 warnings and
errors. The subsequent beta4 Integration project build also succeeded with 0
warnings and errors. The earlier `ContextApplyToIntegrationTests` 14/14
baseline remains focused historical evidence. The first managed pass is also
historical: 3,871/3,875 unit tests passed, with four stale assertions, and the
integration run recorded 2,658 passed, 10 failed, and 17 Unix exclusions. The
10 failures were 9 reviewed snapshots and 1 Context parity test. All fixes are
now integrated, and the correction pass made no additional production changes.
These failed or stale-assertion runs are resolved historical evidence, not
current blockers. The final candidate and merge evidence is recorded below.

## Verified candidate and merge receipt

The frozen candidate is `ff84af47c4b404d3cc1908c6754567bc30730b2d`, with tree
`549e81a774af4059809920100a794fedd899f0b5`. Whole-candidate Astra review
`r_82e9214c3640` found no material findings, verified the initial and final
candidate were clean with an unchanged tree, and independently inspected the
managed CTRF counts. One whole-candidate review remains consumed. Focused
Astra recheck `r_45b235a83ee3` found no material findings and independently
checked all 72 correction captures and reports. The source was unchanged by
the reviewer. The focused recheck was part of the CI correction. One grouped
correction pass was consumed. No new whole-candidate review or council was
consumed, and zero completion grace was consumed.

`npm run verify` and the site type-check/build are green, as the existing
records describe. `npm run build:native -- --no-restore` exited 0 with zero
warnings and errors. `npm run test:built` exited 0 with zero failures. Counts were
unit 3,875, managed integration 2,668 plus 17 documented Unix-only
exclusions, managed public 263, native integration 2,668 plus the same 17
exclusions, native public 263, and the managed public runner against the native
CLI 263. Reports are under
`artifacts/delivery/win-x64/reports/{unit,integration,public,native-integration,native-public,public-native}/results.json`.

`npm run pack` exited 0, and the installed native package test passed on
Windows x64. Both `artifacts/delivery/win-x64/manifest.json` and
`artifacts/delivery/win-x64/packages/package.json`
record `tested: true` and the exact candidate SHA
`ff84af47c4b404d3cc1908c6754567bc30730b2d`.

The local upgrade rehearsal passed from a public beta3 npm install to candidate
native beta4. Framework and Core Templates installed, the intentional template
removal remained absent after Framework and Extension updates, custom source
and overwrite exact hashes were preserved, and the previous Loader bytes were
found in recovery by SHA-256 comparison. Plain Context text and JSON produced
the expected incomplete exit 3 without a crash, while the matching
`open-forge context --for src/Demo.cs` invocation exited 0 and included the
source and overwrite. The receipt is `artifacts/beta4-upgrade/receipt.json`.
This is local-candidate evidence, not published-beta4 proof.

Root safely backed up 11 preexisting main-checkout documents at
`D:/Repositories/open-forge-cleanup-archive/20261001/main-before-beta4-squash`,
confirmed their inclusion, and squash-merged `develop` clean at
`1c6e752c6ac7dcbfe83a95ece9ca9fba22e4cdf1`. The squash tree is exactly
`549e81a774af4059809920100a794fedd899f0b5`. Root pushed `develop` only after
that squash. [Build 36804134087](https://github.com/TheLithiumForge/open-forge/actions/runs/36804134087)
finished with failure for the exact merged SHA. Its Linux x64 native build and 3,875-unit
stage passed before managed integration recorded the fixture failure described
below. The original run finished with the Windows x64 and ARM64 full
build/test/package jobs passed and all four Linux/macOS jobs failing the same
snapshot case. The correction and its verification are recorded in the hosted
correction follow-up below. Beta 3 is still published, and beta 4 is not
published.

Cleanup remains verified: 66 old clean Worker Watch worktrees were removed, 13
base-equal branches were deleted, 53 unique-commit branches were retained,
and 13 dirty worktrees plus one unregistered clean orphan were preserved. The
complete bundle history/hash receipt is at
`D:/Repositories/open-forge-cleanup-archive/20261001`.
Milestone 4 requires the hosted six-platform gate to succeed. Milestone 5
requires the publication, package, and demonstration receipts.

## Hosted correction follow-up

Original Build 36804134087 finished with failure. The Windows x64 and ARM64
full build, test, and package jobs passed. All four Linux/macOS jobs failed on
the same `RouteInspectMatchingFilesOutputTests.MatchingFiles` snapshot case,
with no other failure in their logs.

The correction is integrated. It is one `BuildBody` LF return plus 72
size/token captures, with 36 JSON captures and 36 text captures. It changes no
production, comparer, or assertion code. Windows `RouteInspect` passed 164/164
with 0 skipped. WSL `RouteInspect` passed 164/164 with 0 skipped, and the WSL
fresh build passed with 0 warnings and errors. Root compared all 140 old Linux
CI received captures against the new expected captures using only the existing
path and EOL adaptation. All matched.

The correction artifacts are
`artifacts/beta-platform-correction/windows-results.json`,
`artifacts/beta-platform-correction/linux-results.json`, and
`artifacts/beta-platform-correction/linux-ci-capture-parity.json`.

Focused Astra recheck `r_45b235a83ee3` found no material findings and
independently checked all 72 captures and reports. The source was unchanged by
the reviewer. The recheck is part of the CI correction. One grouped correction
pass was consumed. The original whole-candidate review remains the only whole
candidate review, with no new whole review, council, or policy change.

Corrected-candidate `npm run verify` exited 0 with 55 delivery tests, 7 layout
tests, format/analyzers, and 13 repository Markdown tests with 0 skips. Full
native, test, and package requalification, a second clean squash, and a new
exact-SHA hosted gate remain pending. Beta4 remains unpublished, and no
publication occurred. This correction remains within phase 3/3, milestone 3/5.

The candidate worktree remains branch `beta4-platform-qualification` at the
`develop` `1c6e752c6ac7dcbfe83a95ece9ca9fba22e4cdf1` base, with current
documentation edits preserved. The earlier complete local evidence remains
explicitly historical qualified-candidate evidence for frozen candidate
`ff84af47c4b404d3cc1908c6754567bc30730b2d` and squash merge
`1c6e752c6ac7dcbfe83a95ece9ca9fba22e4cdf1` with tree
`549e81a774af4059809920100a794fedd899f0b5`. It does not qualify the corrected
source.

## Plan

1. Freeze the packets and integrate the authorized Task 64, Task 61, Task 32,
   Task 68, and crash-fix boundaries without changing shared contracts by
   inference.
2. Run focused tests per defect, then the final local managed, Native AOT,
   package, site, offline-link, delivery, and dotnet gates after the integrated
   code and version are final. The whole-candidate review is complete, and the
   focused Astra recheck is part of the CI correction.
3. The local gates, review, candidate cleanliness, and squash merge are green
   and complete. The corrected-candidate repository gate passed. Full native,
   test, and package requalification, a second clean squash, and a new
   exact-SHA hosted gate remain pending. After those gates pass, verify
   publication, packages, installation, beta upgrade, demonstration smoke, and
   the source/version receipt.

Tasks 53 loading policy and 55 APM implementation are excluded. Inventory old
worker worktrees during cleanup, preserve dirty or unmerged work, and remove
only verified disposable exact targets while recording counts.

The confirmation discussion is closed: preserve existing runtime final
confirmations and correct the Create and Install help and contracts. This
makes no runtime policy change. The proposed B6 recovery-coexistence relaxation
is explicitly deferred pending maintainer direction. Beta4 preserves current
recovery blocking behavior, no answer or acceptance of the relaxation is
inferred, and this separate follow-up does not block shipping the accepted
concrete fixes. B7 is resolved: successful retained recovery remains
`completed`/exit 0, and the interface is aligned.
Dangerous or new policy choices return to Root.

## Applicability note

This is a local workspace tool affecting user Markdown. Its failure boundary is
ordinary malformed input, concurrency, interruption, and managed link
boundaries. Preserve expected-state checks, the lock, and recovery. Reuse the
pinned .NET runtime, Markdown parser, route facts, file-expectation capability,
and result-presentation capability. Exceptional machinery: none.

Use Unit evidence for pure presentation and Integration evidence for filesystem
command effects. Trigger the final managed, Native AOT, package, and CI gates
at the release and integration-wave boundaries. Existing `setup --offline`
succeeded, but proves dependencies only; it is not qualification evidence.
The first wave, N2, and the `ExtensionUpdateMutation` regression are
integrated. Task 68 is complete at phase 2/2, milestone 3/3, and its exact
scope and checker contract remain unchanged. The final candidate, local
qualification, whole-candidate review, clean merge, hosted run, package, and
upgrade evidence is in the [verified candidate and merge receipt](#verified-candidate-and-merge-receipt).
The [hosted correction follow-up](#hosted-correction-follow-up) records the
completed first-run failure and integrated correction. The corrected-candidate
repository gate passed. Full native, test, and package requalification, a
second clean squash, and a new exact-SHA hosted gate remain pending. Publication
has not started, and beta4 remains unpublished.
The proposed recovery-coexistence relaxation is deferred pending maintainer
direction. Beta4 preserves current recovery blocking behavior, and this
follow-up is separate from shipping the accepted concrete fixes.

## Execution Capsule

- Current owner: Root; execution via Worker Watch.
- Responsibility: Root owns acceptance and shared contracts. Luna max owns small specified code and documentation packets. Three read-only investigators prepare packets. This record does not authorize a new semantic choice.
- Current boundary: Phase 3/3, milestone 3/5, implementation, local
  qualification, and whole-candidate review are green, the clean merge is
  complete. The original hosted run finished with the fixture failure. The
  correction is integrated, the corrected-candidate repository gate passed, and
  full native, test, and package requalification, a second clean squash, and a
  new exact-SHA hosted gate remain pending.
- Review budget: 1 independent whole-candidate Astra review covering C#, tests, contracts, prose, and the release boundary, consumed with no material findings.
- Council budget: 0.
- Correction budget: 1 grouped review-correction pass consumed.
- Invariants: preserve the existing layer graph; add no dependency or parser duplicate without Root acceptance; preserve containment, user bytes, and recovery; keep JSON shape unchanged except for an explicit accepted change; make snapshot mutations explicit; and do not weaken or skip gates.
- Required evidence: focused tests per defect; final full managed and local Native AOT/package gates after final code and version; site typecheck/build; the offline link gate; `npm run check:delivery`; `npm run check:dotnet`; and the exact post-merge hosted six-platform release process.
- Recoverability: keep the current worktree/branch baseline, commands, and receipts in these Task records. The current candidate worktree is branch `beta4-platform-qualification` at the `develop` `1c6e752c6ac7dcbfe83a95ece9ca9fba22e4cdf1` base, with current documentation edits preserved. The original `beta-stabilization` baseline is historical.
- External boundary: Root pushed `develop` only after the clean squash merge. The correction is integrated, the corrected-candidate repository gate passed, and beta4 publication has not started while full requalification and the new hosted gate remain pending.
- Completion grace: None consumed.
- Stop condition: Return to Root for any protected-path conflict, new policy, dependency, parser, contract, safety, or release-boundary choice, or for any gate that is not reproducibly green.

## Current State

**Task 69 “Next beta stabilization and release” (phase 3/3): milestone 3/5 —
implementation, local qualification, and whole-candidate review green; clean
merge complete; the original hosted run failed on the fixture, the correction
is integrated, the corrected-candidate repository gate passed, and publication
has not started.** The confirmation
discussion is closed by preserving existing runtime final confirmations and
correcting Create and Install help and contracts, with no runtime policy
change. Root has integrated B1–B5, W1/W3/W4/W5, Context, Library mapped-leaf
ownership, N1, N2, and the `ExtensionUpdateMutation` regression. Mutation
no-follow guards remain unchanged, and Task 68 is complete.

The frozen candidate, review, local qualification, package and upgrade checks,
clean merge, hosted run, and cleanup evidence are recorded in the [verified
candidate and merge receipt](#verified-candidate-and-merge-receipt). Prior
failed or stale-assertion runs are resolved historical evidence, not current
blockers. No source beyond Task 68's named roots is silently excluded. The
complete local evidence applies only to frozen candidate
`ff84af47c4b404d3cc1908c6754567bc30730b2d` and squash merge
`1c6e752c6ac7dcbfe83a95ece9ca9fba22e4cdf1` with tree
`549e81a774af4059809920100a794fedd899f0b5`. It does not qualify the corrected
source.

Beta4 preserves current recovery blocking behavior. The proposed
recovery-coexistence relaxation is explicitly deferred pending maintainer
direction, no answer or acceptance of the relaxation is inferred, and this
separate follow-up does not block shipping the accepted concrete fixes. Beta 3
remains published and beta 4 remains unpublished. The test-only correction is
integrated. Windows and WSL `RouteInspect` verification, the WSL fresh build,
and 72-capture parity are green as recorded above. The corrected-candidate
repository gate passed. Full native, test, and package requalification, a
second clean squash, and a new exact-SHA hosted gate remain pending. Publication
has not started. No completion grace has been consumed.
The beta 1 PATH/dev executable is not candidate evidence.
