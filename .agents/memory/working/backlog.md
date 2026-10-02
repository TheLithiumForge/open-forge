---
open-forge:
  description: Minimal current backlog after the public beta and the 1.0 Memory trim
  tags: [Memory, Working, Backlog, Contextual]
---

# Backlog

Only work that is still a live priority belongs here. The former mixed backlog
is preserved as [historical backlog](../archived/cli-development/backlog-pre-cleanup.md).

## Current priorities

1. The current public release remains beta4. [Task 69](cli-development/tasks/task69-next-beta-stabilization-release.md) records its
   completed qualification and release, and [Task 68](cli-development/tasks/task68-repository-link-validation.md) records
   the completed link gate. Beta5 is selected. Publication is authorized only
   after a green, clean squash integration on `develop` and the exact six-host,
   package, site, and user-journey gates pass. The [wave capsule](cli-development/one-zero-polish-wave.md) and
   [project-control ledger](cli-development/project-control.md#active-task-ledger) carry the release boundary and task details.
2. [Task 70 — Existing workspace adoption during
   installation](cli-development/tasks/task70-existing-workspace-adoption-during-installation.md) is the top beta5
   priority. Its local v9 qualification and the one fresh review with grouped
   F1–F5 corrections are complete; the guarded 77-source handoff is verified.
   It remains unintegrated. Combined managed/native/package/site and actual
   published beta4-to-beta5 upgrade gates, develop squash/push, final Root
   acceptance, release and public postchecks remain pending.
   Tasks 32 Minimal Output Sweep, 47 Entrypoint reachability and its default
   Skill indexing follow-up, 54 Tag trimming, and 64 CLI defects and contract
   drift are independently qualified and squash-integrated locally on
   `develop`; final combined qualification remains pending.
3. Tasks 39 Output Audit and 34 Interpolated Value Markup are paused with saved
   worktree state. Task 48 Scoping for Extension routes is deferred beyond beta5
   but remains required before 1.0. Follow the [wave capsule](cli-development/one-zero-polish-wave.md) for postponed and
   unselected work and the release boundary.
4. Other beta work stays in [Beta follow-ups](cli-development/tasks/beta-follow-ups.md). Use the protected
   [Authors' Findings](../emerging/authors-findings/_authors-findings.md) route for maintainer notes that must not be
   archived or pruned without explicit direction.
5. The Task 65 placement question is closed. The broader [Local Planning review](local-planning.md) remains open
   before changes to task lifecycle, completion tracking, organization, or
   archival semantics.

## Archived on 2026-09-25

Beta preparation records no longer needed for current work moved to
[Archived Beta Preparation](../archived/beta-preparation/_beta-preparation.md):
the approved beta Template changes, the CLI experience coverage audit and run
record, the CLI modularization and extension experience reviews, and the
applied wording inventory and proposal.
