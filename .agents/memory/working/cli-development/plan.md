---
open-forge:
  description: Current selection and order of open work after the public beta
  tags: [LoadNow, Memory, Working, CLI, Plan, Contextual, Active]
---

# Development Plan

## Current selection

Version `0.9.0-beta.1` is published, and the documentation site is live. On
2026-09-28 the maintainer accepted the
[open task review](../../emerging/analysis/open-task-review/_open-task-review.md):
ten Tasks closed, two folded into others, and one order for the rest.

- [Task 61](tasks/task61-documentation-accuracy-and-voice.md) is included in
  `develop`. The beta 2 delivery will promote the verified candidate to `main`.
- [Task 59](tasks/task59-beta-2-release.md) releases beta 2 once the
  verified Task 62 changes reach `develop` and the complete CI Build is green.
  The maintainer authorized integration, push and conditional beta publication.
- [Task 53](tasks/task53-loading-and-scoping-audit.md) and
  [Task 64](tasks/task64-cli-defects-and-contract-drift.md) come next.
- Decisions before 1.0 are pending on Tasks 36, 37, 63, and 65.
- On 2026-09-29, [Task 62](tasks/task62-glob-scoped-loading.md) was narrowed to
  optional `applyTo` loading and CLI filtering. The maintainer accepted the
  recommendations and authorized implementation. Its [execution packets](tasks/task62/_task62.md)
  record the completed implementation and passing managed/native qualification.
  The isolated `task62-applyto` worktree is verified. Squash integration and
  publication qualification are now authorized through Task 59.
- [Task 55](tasks/task55-alternative-root.md) is restored as a separate open
  investigation of alternative roots and APM interoperability. Its scheduling
  is independent of Task 62.

## Open work

The [Task index](tasks/_tasks.md#current-order) holds the full order, and the
[ledger](project-control.md#active-task-ledger) holds each Task's state. The
remaining beta follow-ups stay in [Beta follow-ups](tasks/beta-follow-ups.md).

## History

The plan as it stood at the beta release is preserved as
[Plan at beta](../../archived/cli-development/plan-beta.md). Completed Tasks and
beta preparation records moved to Archived Memory on 2026-09-25.
