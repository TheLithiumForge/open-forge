---
open-forge:
  description: Current selection and order of open work after the public beta
  tags: [LoadNow, Memory, Working, CLI, Plan, Contextual, Active]
---

# Development Plan

## Accepted beta-stabilization selection

On 2026-10-01, the maintainer accepted [Task 69 — Next beta stabilization and
release](tasks/task69-next-beta-stabilization-release.md) as the bounded next
horizon. [Task 68 — Repository link validation](tasks/task68-repository-link-validation.md)
is its offline link-gate packet. Root is the active owner, execution is through
Worker Watch, and no completion grace has been consumed. The current boundary
is Task 69 phase 3/3, milestone 5/5: implementation, qualification, clean
squash, hosted six-host gate, publication, package, documentation, upgrade,
and demonstration receipts are complete. Beta4 is the current public release.
The detailed final receipt is in [Task 69's candidate and merge receipt](tasks/task69-next-beta-stabilization-release.md#verified-candidate-and-merge-receipt).
The proposed B6
recovery-coexistence relaxation is explicitly deferred pending maintainer
direction. Beta4 preserves current recovery blocking behavior, and this
separate follow-up does not block shipping the accepted concrete fixes.

- Task 64 remains open for the separate B6 follow-up after its accepted bounded
  fixes. Task 61 covers stale documentation and its maintainer review remains
  unchecked. Task 32 owns bounded pending/next-action polish while its full
  sweep remains queued.
- Task 68 is complete at phase 2/2, milestone 3/3. Its exact specification,
  named-root scope, parser/resolver boundary, checker, tests, and wiring remain
  frozen and accepted. The two stale anchors are fixed, and no source beyond
  the named roots is silently excluded.
- The confirmation discussion is closed by preserving existing runtime final
  confirmations and correcting Create and Install help and contracts. This
  makes no runtime policy change.
- The current public release on npm and GitHub is `0.9.0-beta.4`. The detailed
  qualification, review, merge, hosted-run, package, upgrade, documentation,
  and publication receipts are in [Task 69's candidate and merge receipt](tasks/task69-next-beta-stabilization-release.md#verified-candidate-and-merge-receipt).
  One whole-candidate review and one focused recheck were consumed. One grouped
  correction pass was consumed, with zero completion grace consumed.
- Task 53's full loading audit remains outside this horizon. Task 55's full
  investigation remains open outside this horizon; its bounded local triage is
  recorded, and no APM implementation is authorized. Task 59's beta 2 receipt
  remains historical and unchanged.

## Current selection

Version `0.9.0-beta.4` is public on npm and GitHub, and the documentation site
is deployed. Task 69 retains the completed beta4 release receipt. Task 59
retains the historical beta 2 release and package verification receipt. On 2026-09-28 the maintainer accepted the
[open task review](../../emerging/analysis/open-task-review/_open-task-review.md):
ten Tasks closed, two folded into others, and one order for the rest.

- [Task 61](tasks/task61-documentation-accuracy-and-voice.md) tracks the current
  documentation reconciliation. Its local documentation gates are green, and
  the maintainer review checkbox remains unchecked.
- The root-authored [1.0 release readiness assessment](../../emerging/analysis/one-zero-release-readiness.md)
  remains contextual input and does not authorize implementation.
- [Task 64](tasks/task64-cli-defects-and-contract-drift.md) remains open for its
  separate B6 follow-up after the accepted bounded fixes. The full [Task 53](tasks/task53-loading-and-scoping-audit.md) audit remains outside the current horizon.
- [Task 68](tasks/task68-repository-link-validation.md) is complete at phase
  2/2, milestone 3/3. Its exact named-root scope and checker contract remain
  accepted, including the two explicit exclusions and no arbitrary directory
  filtering. Task 69 is complete at phase 3/3, milestone 5/5; its release
  receipt records the hosted qualification and publication.
- Decisions before 1.0 are pending on Tasks 36, 37, 63, and 65.
- On 2026-09-29, [Task 62](tasks/task62-glob-scoped-loading.md) was narrowed to
  optional `applyTo` loading and CLI filtering. The maintainer accepted the
  recommendations and authorized implementation. Its implementation is
  complete and integrated in the verified candidate on `develop` and `main`.
  The [execution packets](tasks/task62/_task62.md) record the passing
  managed/native qualification as historical evidence, not current beta 4
  qualification. Task 59 retains the completed beta 2 release checks.
- [Task 55](tasks/task55-alternative-root.md) remains a separate open
  investigation of alternative roots and APM interoperability outside the
  current horizon. Its bounded local triage is recorded; the full investigation
  remains open.

## Open work

The [Task index](tasks/_tasks.md#current-order) holds the full order, and the
[ledger](project-control.md#active-task-ledger) holds each Task's state. The
remaining beta follow-ups stay in [Beta follow-ups](tasks/beta-follow-ups.md).

## History

The plan as it stood at the beta release is preserved as
[Plan at beta](../../archived/cli-development/plan-beta.md). Completed Tasks and
beta preparation records moved to Archived Memory on 2026-09-25.
