---
open-forge:
  description: Coordinate the next beta stabilization, qualification, merge, and release boundary
  tags: [Memory, Working, Task, CLI, Contextual, Complete]
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

Beta 3 remains historical. Beta 4 is the current public release. The corrected
local candidate is `d04e1106fca987c12288a15f6e92ac2e84bc8fef` with tree
`081ad06987e553a47ceaedcbbde064886ba58034`. Task 69 is complete at phase 3/3,
milestone 5/5: local qualification, clean squash, hosted six-host gate,
publication, package, documentation, upgrade, and demonstration receipts are
complete. Zero completion grace was consumed. Historical beta 2 records remain
unchanged.

The final candidate, public release, package, upgrade, and documentation
evidence is recorded below. The earlier `ff84af...`/`1c6e752...` qualified
candidate and first hosted correction remain concise historical evidence; they
do not qualify the corrected release.

## Verified candidate and merge receipt

The corrected local candidate is `d04e1106fca987c12288a15f6e92ac2e84bc8fef`,
with tree `081ad06987e553a47ceaedcbbde064886ba58034`. `npm run verify` passed;
`npm run build:native -- --no-restore` exited 0 with 0 warnings and errors;
`npm run test:built` recorded 10,000 executions with 34 documented platform
exclusions; and `npm run pack` plus package installation passed.

The clean second squash of `develop` is
`80f0b46dd71837fc65593e9bcd1656ef5a6314a6`, matching that tree. Root pushed
`develop` only after the squash, and `main` was fast-forwarded to the same SHA.
[Build 36809625271](https://github.com/TheLithiumForge/open-forge/actions/runs/36809625271)
passed all six host jobs. [Release 36813167101](https://github.com/TheLithiumForge/open-forge/actions/runs/36813167101)
and [Documentation 36813142627](https://github.com/TheLithiumForge/open-forge/actions/runs/36813142627)
passed. The [GitHub prerelease v0.9.0-beta.4](https://github.com/TheLithiumForge/open-forge/releases/tag/v0.9.0-beta.4)
points exactly to `80f0b46dd71837fc65593e9bcd1656ef5a6314a6`.

All seven npm packages have exact matching versions and agreeing `beta` and
`latest` tags. The six wrapper dependencies and platform metadata are exact.
All six portable archives match `SHA256SUMS`. The public Windows x64 portable
and npm executables are identical at SHA-256
`73F3AC762C0F42F0CBE287F2D0D16442E88A09D3508BB67FC592DC6B0DF4B3FC`.

Fresh exact-version and `@beta` installs ran the actual `0.9.0-beta.4` npm
shim. Fresh Framework, Core Templates, Context, Status, and Doctor checks
passed. The published beta3-to-beta4 upgrade passed: the intentional template
removal stayed absent, custom source and overwrite exact bytes were retained,
the old edited Loader bytes were found by SHA-256 inside the retained recovery
ZIP, plain Context text and JSON exited 3 without a crash, matching
`--for src/Demo.cs` exited 0 with source and overwrite, and Doctor exited 0.
Public Loading, Frontmatter, diagram, and Customizing removal/overwrite
guidance was verified after deployment.

Evidence is retained in `artifacts/beta4-public/{final-receipt.json,npm-records.json,archive-checks.json,smoke-receipt.json,site-receipt.json}`,
`artifacts/beta4-published-upgrade/receipt.json`, and
`artifacts/beta-platform-correction/local-qualification.json` with the hosted
logs. The transient npm ETARGET/mixed-metadata result resolved after propagation
and a fresh-cache retry; there is no current blocker.

Whole-candidate Astra review `r_82e9214c3640` found no material findings.
Focused Astra recheck `r_45b235a83ee3` found no material findings and checked
the correction captures and reports. One whole-candidate review, one focused
correction recheck, and one grouped correction pass were consumed; zero
completion grace was consumed.

Historical pre-release evidence: Root safely backed up 11 preexisting
main-checkout documents at
`D:/Repositories/open-forge-cleanup-archive/20261001/main-before-beta4-squash`,
confirmed their inclusion, and squash-merged `develop` clean at
`1c6e752c6ac7dcbfe83a95ece9ca9fba22e4cdf1` with tree
`549e81a774af4059809920100a794fedd899f0b5`. The original
[Build 36804134087](https://github.com/TheLithiumForge/open-forge/actions/runs/36804134087)
finished with failure: Windows x64 and ARM64 passed, while all four Linux/macOS
jobs failed on the same fixture. That first hosted run and its correction remain
historical evidence; the final release receipt above is authoritative.

Cleanup remains verified: 66 old clean Worker Watch worktrees were removed, 13
base-equal branches were deleted, 53 unique-commit branches were retained,
and 13 dirty worktrees plus one unregistered clean orphan were preserved. The
complete bundle history/hash receipt is at
`D:/Repositories/open-forge-cleanup-archive/20261001`.
The cleanup receipt and the original candidate evidence remain historical and
unchanged.

## Hosted correction follow-up

This section preserves the first hosted failure and its correction as historical
evidence for the final release receipt above.

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
tests, format/analyzers, and 13 repository Markdown tests with 0 skips. This
correction was part of phase 3/3, milestone 3/5; the final native, test,
package, hosted, and publication receipts are recorded above at milestone 5/5.

At that time, the candidate worktree was branch `beta4-platform-qualification` at the
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
3. The local gates, review, candidate cleanliness, second squash, hosted
   six-host gate, publication, package, installation, beta upgrade,
   demonstration smoke, documentation, and source/version receipts are green
   and complete. Task 69 is complete at phase 3/3, milestone 5/5.

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
first-run failure and integrated correction as historical evidence. Task 69 is
complete at phase 3/3, milestone 5/5, and beta4 is public. The proposed
recovery-coexistence relaxation is deferred pending maintainer direction.
Beta4 preserves current recovery blocking behavior, and this follow-up is
separate from shipping the accepted concrete fixes.

## Execution Capsule

- Current owner: Root; execution via Worker Watch.
- Responsibility: Root owns acceptance and shared contracts. Luna max owns small specified code and documentation packets. Three read-only investigators prepare packets. This record does not authorize a new semantic choice.
- Current boundary: Phase 3/3, milestone 5/5. Implementation, local
  qualification, whole-candidate review, clean merge, hosted six-host gate,
  publication, package, documentation, upgrade, and demonstration receipts are
  complete; beta4 is public.
- Review budget: 1 independent whole-candidate Astra review and 1 focused Astra
  correction recheck, consumed with no material findings.
- Council budget: 0.
- Correction budget: 1 grouped review-correction pass consumed.
- Invariants: preserve the existing layer graph; add no dependency or parser duplicate without Root acceptance; preserve containment, user bytes, and recovery; keep JSON shape unchanged except for an explicit accepted change; make snapshot mutations explicit; and do not weaken or skip gates.
- Required evidence: focused tests per defect; final full managed and local Native AOT/package gates after final code and version; site typecheck/build; the offline link gate; `npm run check:delivery`; `npm run check:dotnet`; and the exact post-merge hosted six-platform release process.
- Recoverability: keep the current worktree/branch baseline, commands, and receipts in these Task records. The closeout branch `beta4-release-receipt` is based on release commit `80f0b46dd71837fc65593e9bcd1656ef5a6314a6`, so it cannot become stale after its receipt commit. The original `beta-stabilization` baseline is historical.
- External evidence: A hashed external copy of the release evidence is retained at `D:/Repositories/open-forge-cleanup-archive/20261001/beta4-release-proof`. The unused clean `beta-worker-base` helper worktree was archived recoverably after verification.
- External boundary: Root pushed `develop` only after the clean second squash. `main` was fast-forwarded to the same SHA, and the hosted, release, documentation, package, upgrade, and publication receipts are complete.
- Completion grace: None consumed.
- Stop condition: Return to Root for any protected-path conflict, new policy, dependency, parser, contract, safety, or release-boundary choice, or for any gate that is not reproducibly green.

## Current State

**Task 69 “Next beta stabilization and release” (phase 3/3): milestone 5/5 —
implementation, qualification, clean second squash, hosted six-host gate,
publication, package, documentation, upgrade, and demonstration receipts
complete; beta4 is public.** The confirmation
discussion is closed by preserving existing runtime final confirmations and
correcting Create and Install help and contracts, with no runtime policy
change. Root has integrated B1–B5, W1/W3/W4/W5, Context, Library mapped-leaf
ownership, N1, N2, and the `ExtensionUpdateMutation` regression. Mutation
no-follow guards remain unchanged, and Task 68 is complete.

The final candidate, review, qualification, package, upgrade, hosted, and
publication evidence is recorded in the [verified candidate and merge receipt](#verified-candidate-and-merge-receipt).
The first failed hosted run and earlier qualified candidate are resolved
historical evidence, not current blockers. No source beyond Task 68's named
roots is silently excluded. The earlier local evidence applies only to frozen candidate
`ff84af47c4b404d3cc1908c6754567bc30730b2d` and squash merge
`1c6e752c6ac7dcbfe83a95ece9ca9fba22e4cdf1` with tree
`549e81a774af4059809920100a794fedd899f0b5`. It does not qualify the corrected
source.

Beta4 preserves current recovery blocking behavior. The proposed
recovery-coexistence relaxation is explicitly deferred pending maintainer
direction, no answer or acceptance of the relaxation is inferred, and this
separate follow-up does not block shipping the accepted concrete fixes. Beta 3
is historical and beta 4 is public. The test-only correction, Windows and WSL
`RouteInspect` verification, WSL fresh build, and 72-capture parity are
historical correction evidence; the final release receipt above is authoritative.
No completion grace has been consumed.
The beta 1 PATH/dev executable is not candidate evidence.
