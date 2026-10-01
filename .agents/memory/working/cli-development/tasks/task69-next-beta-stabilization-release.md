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
smoke, and a truthful source/version receipt. The pending crash fix present at
`c27e63e4d` must be included and requalified. The existing PATH/dev executable
is beta 1 and is not candidate evidence.

The execution profile is Batch. The horizon has three phases, implementation,
qualification, and integration/release, with five milestones: packets frozen;
fixes, documentation, and the checker integrated; local gates and fresh review
green; squash plus the hosted six-platform gate green; and publication,
package, and demonstration receipts verified.

## Current release state and focused baseline

Beta 3 remains the published release. Version beta 4 is allocated locally in
`package.json`, `package-lock.json`, and `Directory.Build.props`; it has not
been pushed, merged, or released. Historical beta 2 records remain unchanged.

A full solution build before the version bump succeeded with 0 warnings and
errors. The subsequent beta4 Integration project build also succeeded with 0
warnings and errors. The earlier `ContextApplyToIntegrationTests` 14/14
baseline remains focused historical evidence. The first managed pass is also
historical: 3,871/3,875 unit tests passed, with four stale assertions, and the
integration run recorded 2,658 passed, 10 failed, and 17 Unix exclusions. The
10 failures were 9 reviewed snapshots and 1 Context parity test. All fixes are
now integrated, and the correction pass made no additional production changes.
Final qualification remains pending, and subsequent source edits invalidate
affected evidence.

## Plan

1. Freeze the packets and integrate the authorized Task 64, Task 61, Task 32,
   Task 68, and crash-fix boundaries without changing shared contracts by
   inference.
2. Run focused tests per defect, then the final local managed, Native AOT,
   package, site, offline-link, delivery, and dotnet gates after the integrated
   code and version are final. Obtain the one fresh whole-candidate review.
3. When the local gates, review, and candidate cleanliness are green, squash
   merge to `develop`, run the hosted six-platform release process, then verify
   publication, packages, installation, beta upgrade, demonstration smoke, and
   the source/version receipt.

Tasks 53 loading policy and 55 APM implementation are excluded. Inventory old
worker worktrees during cleanup, preserve dirty or unmerged work, and remove
only verified disposable exact targets while recording counts.

The confirmation discussion is closed: preserve existing runtime final
confirmations and correct the Create and Install help and contracts. This
makes no runtime policy change. B6 recovery coexistence remains unresolved:
the user must answer whether verified finalized recovery bundles may coexist
while unsafe or draft residuals continue to block. The B7 recommendation is
to preserve `completed` for successful recovery retention and align the
contradictory interface. Dangerous or new policy choices return to Root.

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
scope and checker contract remain unchanged. `npm run verify` exited 0 on
beta4: `check:delivery` was green, 55 delivery tests and 7 package-layout
tests passed across all six targets (layout only, not host execution), dotnet
whitespace/analyzers were green, and 13 `RepositoryMarkdownTests` passed with
0 skipped against the current repository inventory. The four-class targeted
snapshot refresh passed 47/47 with 0 skipped. Its 68 changes were independently
reviewed by Root: 32 JSON changes only at top-level `next`, and 36 human
`Next` or empty-Extension-list wording changes. The beta4 managed run passed
via `npm run test -- --no-restore`: unit 3,875 passed with 0 excluded,
integration 2,668 passed with 17 documented Unix platform exclusions, public
263 passed with 0 excluded, and zero failures. The build completed with 0
warnings/errors. Exact reports are at
`artifacts/delivery/managed-reports/{unit,integration,public}/results.json`.
Native AOT/package gates, the whole-candidate review, merge, and release
remain pending, so the overall state remains milestone 0/5. Keep the
recovery-coexistence user question pending.

## Execution Capsule

- Current owner: Root; execution via Worker Watch.
- Responsibility: Root owns acceptance and shared contracts. Luna max owns small specified code and documentation packets. Three read-only investigators prepare packets. This record does not authorize a new semantic choice.
- Current boundary: Phase 1/3, milestone 0/5; managed qualification green,
  with Native AOT/package gates, review, merge, and release boundaries pending.
- Review budget: 1 independent whole-candidate Astra review covering C#, tests, contracts, prose, and the release boundary.
- Council budget: 0.
- Correction budget: 1 grouped review-correction pass.
- Invariants: preserve the existing layer graph; add no dependency or parser duplicate without Root acceptance; preserve containment, user bytes, and recovery; keep JSON shape unchanged except for an explicit accepted change; make snapshot mutations explicit; and do not weaken or skip gates.
- Required evidence: focused tests per defect; final full managed and local Native AOT/package gates after final code and version; site typecheck/build; the offline link gate; `npm run check:delivery`; `npm run check:dotnet`; and the exact post-merge hosted six-platform release process.
- Recoverability: keep the current worktree/branch baseline, commands, and receipts in these Task records. Baseline is the `beta-stabilization` worktree on branch `beta-stabilization`, based at `c27e63e4dcafe18245a7e162b784069ab1cf341d`.
- External boundary: no push, publication, or release is performed in this packet. The release boundary opens only after the accepted local merge condition.
- Completion grace: None consumed.
- Stop condition: Return to Root for any protected-path conflict, new policy, dependency, parser, contract, safety, or release-boundary choice, or for any gate that is not reproducibly green.

## Current State

**Task 69 “Next beta stabilization and release” (phase 1/3): milestone 0/5 —
integrated packets, managed qualification green.** The authorized bounded
beta-stabilization horizon remains milestone 0/5. B6 recovery coexistence is
the sole pending user decision. The confirmation discussion is closed by
preserving existing runtime final confirmations and correcting Create and
Install help and contracts, with no runtime policy change. Root has integrated
B1–B5, W1/W3/W4/W5, Context, Library mapped-leaf ownership, N1, N2, and the
`ExtensionUpdateMutation` regression. Mutation no-follow guards remain
unchanged. Version beta4 is allocated locally, and Task 68 is complete.

The first managed pass and its four stale unit assertions, nine reviewed
snapshot failures, and one Context parity failure are resolved historical
evidence. The targeted four-class snapshot refresh passed 47/47 with 0
skipped. Root independently reviewed all 68 changes: 32 JSON-only top-level
`next` changes and 36 human `Next` or empty-Extension-list wording changes.
`npm run verify` exited 0 on beta4, including the delivery, package-layout,
dotnet whitespace/analyzer, and 13-test repository-documentation evidence
recorded above. No source beyond Task 68's named roots is silently excluded.
The beta4 managed run passed via `npm run test -- --no-restore`: unit 3,875
passed with 0 excluded, integration 2,668 passed with 17 documented Unix
platform exclusions, public 263 passed with 0 excluded, and zero failures.
The build completed with 0 warnings/errors. Exact reports are at
`artifacts/delivery/managed-reports/{unit,integration,public}/results.json`.

Native AOT/package gates, the whole-candidate Astra review, grouped correction,
squash merge, hosted six-platform qualification, and publication remain
pending. No changed policy or release-green claim is made. The CLI wave remains locally
committed at `3901e6156`; docs, checker, and version changes remain local
edits. Cleanup remains verified: 66 old clean Worker Watch worktrees removed,
13 base-equal branches deleted, 53 unique-commit branches retained, and 13
dirty worktrees plus one unregistered clean orphan preserved. The complete
bundle history and hash are verified at
`D:/Repositories/open-forge-cleanup-archive/20261001`. No completion grace has
been consumed. The beta 1 PATH/dev executable is not candidate evidence.
