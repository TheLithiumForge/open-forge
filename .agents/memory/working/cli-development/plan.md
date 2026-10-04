---
open-forge:
  description: Current selection and order of open work after the public beta
  tags: [LoadNow, Memory, Working, CLI, Plan, Contextual, Active]
---

# Development Plan

## Current order

The [project-control ledger](project-control.md#active-task-ledger) owns the queue and permanent Task identities. Each Task defines its remaining outcome and evidence.

1. Execute the [renewed release authorization](tasks/task70-existing-workspace-adoption-during-installation.md#current-release-authorization-2026-10-04): verify the requested changes, squash into `develop`, push and publish beta6 after exact-candidate hosted qualification. The reused-worktree correction is included in committed base `de54c1d32`. Beta5's npm publication and draft GitHub release remain history.
2. Retain the locally accepted [Task 73 setup and documentation](../../archived/cli-development/tasks/task73-layered-adoption-and-installation-choices.md), [Task 72 wizard correction](../../archived/cli-development/tasks/task72-extension-wizard-terminal-layout.md) and [Task 74 Library ignore choice](../../archived/cli-development/tasks/task74-library-attachment-git-ignore-choice.md) for integration. The latest combined six-mode qualification passed 10,718 tests with 34 declared platform exclusions and unchanged executable closures. The changes remain uncommitted, unmerged and unpublished.
3. Keep [Task 71](tasks/task71-streamline-build-release-pipeline.md) queued for pipeline analysis.
4. Keep Tasks 34 and 39 paused, and Task 48 frozen at A/B1. Its Extension scoping work is deferred beyond beta5 and remains required before 1.0. The [wave capsule](one-zero-polish-wave.md) retains the resume boundaries.
5. Retain Task 62's pending maintainer review, the deferred documentation reviews, and the broader [Local Planning review](../local-planning.md). Other unfinished work stays in the [Task index](tasks/_tasks.md) and [candidate queue](tasks/potential/_potential.md).

## Completed work and history

Completed Tasks and superseded analyses live in [Archived CLI Development](../../archived/cli-development/_cli-development.md). Tasks 32, 47, 54 and 64 are task-local complete and locally integrated. Task 70 owns the separate combined release boundary.

The [plan before this trim](../../archived/cli-development/plan-2026-10-04.md) preserves the earlier setup selection, beta4/beta5 checkpoints and release schedule. Those dated instructions do not authorize current publication.
