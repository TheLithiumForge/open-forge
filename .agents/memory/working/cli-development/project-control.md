---
open-forge:
  description: Current CLI task selection and retained task ownership
  tags: [LoadNow, Memory, Working, CLI, Task, Contextual, Active]
---

# CLI Project Control

## Current release hold, 2026-10-02

Publication is stopped by the maintainer pending the reused-worktree stale
issue report in Task 70. Both remote branches reached `634ab070`, and the
six-host Build passed. Release cancellation raced the npm and GitHub publish
steps. The GitHub prerelease is now draft. npm beta5 remains published because
local npm authentication is unavailable. No further release is authorized.
The noon release automation is paused. The integration and publication boundary
in the older ledger rows below is superseded by this hold.

This ledger owns current identity, queue state, ownership, and the next
boundary. The [1.0 polish wave capsule](one-zero-polish-wave.md) records the
selected task horizons and frozen decisions. The ledger as it stood at the beta
release is preserved as the
[beta control ledger](../../archived/cli-development/project-control-beta.md).
Older receipts and retired queue state are in the
[archived control ledger](../../archived/cli-development/project-control.md).

## Noon release state

The frozen candidate was integrated and pushed at 12:01 as
`4bb4059e319e5bf536aa7f0a0969a5fd246547f4`. Hosted qualification found a missing
Windows ConPTY skip reason in the delivery exclusion list. The exact reason and
its regression check are corrected; delivery checks pass. A new hosted Build
must pass before publication. Beta5 is not released, and main has not moved.
The Task 70 noon record owns this correction and evidence. The original local
qualification remains intact; its complete tree is the initial squash tree.

## Active task ledger

| ID  | Task                                                                                                                                           | State                                                                                                                                | Current boundary                                                                                                                                                                                                                                                                              | Owner                         |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------- |
| 71  | [Streamline build and release delivery](tasks/task71-streamline-build-release-pipeline.md)                                                     | Open follow-up requested during beta5 publication                                                                                    | Compare platform steps and timings, diagnose stalls, reduce repeated builds, and make publication cancellation effective. No redesign started                                                                                                                                                 | Root                          |
| 54  | [Tag trimming for 1.0](tasks/task54-tag-trimming.md)                                                                                           | Individually qualified and squash-integrated into `develop`; not closed                                                              | Wave phase 3/3, milestone 2/3; all six modes plus package qualified; accepted 89-path patch includes 64 numeric-only loading-cost captures; commit `29c22d5f651ebaf9fac11e16ca60049b7c8e762d` on 2026-10-02. Local combined gates pass; hosted release gates remain                           | Root (combined qualification) |
| 39  | [Output Audit](tasks/task39-output-audit.md)                                                                                                   | Paused by user, 2026-10-01                                                                                                           | Wave phase 2/3, milestone 1/3; final checkpoint `r_f4bdc49aa2c4` saved; all workers stopped; no beta5 integration or captures                                                                                                                                                                 | Root; Worker Watch            |
| 34  | [Interpolated Value Markup](tasks/task34-interpolated-value-markup.md)                                                                         | Paused by user, 2026-10-01                                                                                                           | Wave phase 2/3, milestone 1/3; final literal-correction checkpoint `r_b68ae7955e36` saved; all workers stopped; no beta5 integration or captures                                                                                                                                              | Root; Worker Watch            |
| 47  | [Entrypoint reachability](tasks/task47-entrypoint-reachability.md); [Default Skill indexing follow-up](tasks/task47-default-skill-indexing.md) | Individually qualified and squash-integrated into `develop`; not closed                                                              | Wave phase 3/3, milestone 2/3; all six modes and package/planning journeys qualified; commit `27a916dcd1c165f2610dd20b625dbe10beb0491b` on 2026-10-02. Local combined gates pass; hosted release gates remain; no new Task ID                                                                 | Root (combined qualification) |
| 64  | [CLI defects and contract drift](tasks/task64-cli-defects-and-contract-drift.md)                                                               | Independently qualified in all required individual modes and Root-verified, squash-integrated into clean local `develop`; not closed | Wave phase 3/3, milestone 2/3; commit `292ab17bc641f4d7cd3933f976dc2586e78e9899`; local combined qualification passes; hosted release qualification remains                                                                                                                                   | Root (combined qualification) |
| 70  | [Existing workspace adoption during installation](tasks/task70-existing-workspace-adoption-during-installation.md)                             | Publication held; immediate stale-recovery correction locally validated on its branch                                                | Original candidate integrated and six-host Build passed. Fresh correction passes 7,087 managed checks with 17 accepted platform exclusions and 79 focused Native AOT Install checks. Reviewable uncommitted fix; renewed release authorization and new-candidate release qualification remain | Root (release coordination)   |
| 48  | [Scoping for Extension routes](tasks/task48-scoping-for-extension-routes.md)                                                                   | Frozen at A/B1; deferred beyond beta5; required before 1.0                                                                           | Wave phase 2/3, milestone 1/3; corrected Task 64 Init accepted for later resume; Core-first scope unchanged. Checkpoint receipts remain in wave capsule                                                                                                                                       | Root; Worker Watch            |
| 32  | [Minimal Output Sweep](tasks/task32-minimal-output-sweep.md)                                                                                   | Independently qualified in all required individual modes and Root-verified, squash-integrated into clean local `develop`; not closed | Wave phase 3/3, milestone 2/3; commit `38956e8f77654127f15f98bb33384ddd616444f6`; local combined qualification passes; hosted release qualification remains                                                                                                                                   | Root (combined qualification) |
| 53  | [Loading and scoping audit](tasks/task53-loading-and-scoping-audit.md)                                                                         | Open, deferred                                                                                                                       | Full audit postponed by the 2026-10-01 selection                                                                                                                                                                                                                                              | Unassigned                    |
| 44  | [Template and core file content](tasks/task44-template-content.md)                                                                             | Open, deferred                                                                                                                       | Template and Core polish is not selected                                                                                                                                                                                                                                                      | Unassigned                    |
| 36  | [Extension merge and guards](tasks/task36-extension-merge-and-guards.md)                                                                       | Open, deferred                                                                                                                       | Partial merging is postponed                                                                                                                                                                                                                                                                  | Maintainer                    |
| 63  | [Keeping edits through updates](tasks/task63-keeping-edits-through-updates.md)                                                                 | Open, deferred                                                                                                                       | Managed-file edit policy is postponed                                                                                                                                                                                                                                                         | Maintainer                    |
| 61  | [Documentation accuracy and voice](tasks/task61-documentation-accuracy-and-voice.md)                                                           | Open                                                                                                                                 | Current beta5 documentation reconciliation is Root-owned in this wave. Maintainer review remains postponed                                                                                                                                                                                    | Root                          |
| 66  | [Council polish](tasks/task66-council-polish.md)                                                                                               | Open, deferred                                                                                                                       | Existing committed review material remains; prior review is postponed                                                                                                                                                                                                                         | Root                          |
| 67  | [Diagram labels](tasks/task67-diagram-labels.md)                                                                                               | Open, deferred                                                                                                                       | Existing committed review material remains; prior review is postponed                                                                                                                                                                                                                         | Root                          |
| 37  | [Wording review](tasks/task37-wording-review-against-proposals.md)                                                                             | Open, unselected                                                                                                                     | Prior scope decision remains pending; no selection in this wave                                                                                                                                                                                                                               | Maintainer                    |
| 40  | [Capture coverage](tasks/task40-capture-coverage.md)                                                                                           | Open, deferred                                                                                                                       | Broad capture expansion postponed until after 1.0                                                                                                                                                                                                                                             | Unassigned                    |
| 58  | [Demo-based evaluations](tasks/task58-demo-evals.md)                                                                                           | Open, deferred                                                                                                                       | Remains after 1.0                                                                                                                                                                                                                                                                             | Unassigned                    |
| 55  | [Alternative workspace root such as `.apm`](tasks/task55-alternative-root.md)                                                                  | Complete                                                                                                                             | Phase 1/1, milestone 1/1; current root retained; APM coexistence is not certified                                                                                                                                                                                                             | Root                          |
| 65  | [Where open Tasks live](tasks/task65-where-open-tasks-live.md)                                                                                 | Complete                                                                                                                             | Phase 1/1, milestone 1/1; open Tasks stay in Working Memory; broader Local Planning review remains open                                                                                                                                                                                       | Root                          |
| 59  | [Beta 2 release](tasks/task59-beta-2-release.md)                                                                                               | Complete                                                                                                                             | `0.9.0-beta.2` publication and package/site verification remain historical receipts                                                                                                                                                                                                           | Root                          |
| 62  | [Glob-scoped loading](tasks/task62-glob-scoped-loading.md)                                                                                     | Complete                                                                                                                             | Verified candidate and accepted implementation receipt remain unchanged                                                                                                                                                                                                                       | Root                          |
| 68  | [Repository link validation](tasks/task68-repository-link-validation.md)                                                                       | Complete                                                                                                                             | Phase 2/2, milestone 3/3; accepted checker and integration evidence remain unchanged                                                                                                                                                                                                          | Root (Worker Watch)           |
| 69  | [Next beta stabilization and release](tasks/task69-next-beta-stabilization-release.md)                                                         | Complete                                                                                                                             | Phase 3/3, milestone 5/5; beta4 release receipts remain unchanged                                                                                                                                                                                                                             | Root (Worker Watch)           |

No completion grace has been consumed for Tasks 68 or 69.

**Closed on 2026-09-28**, following the
[open task review](../../emerging/analysis/open-task-review/_open-task-review.md):
[30](../../archived/cli-development/tasks/task30-cli-experience-remediation.md), [31](../../archived/cli-development/tasks/task31-implementation-duplication.md), [33](../../archived/cli-development/tasks/task33-managed-content-removal.md), [35](../../archived/cli-development/tasks/task35-removal-and-suppression-model.md), [41](../../archived/cli-development/tasks/task41-beta-journey-scenarios.md), [42](../../archived/cli-development/tasks/task42-minimal-core.md), [43](../../archived/cli-development/tasks/task43-workflows-as-skill.md), [52](../../archived/cli-development/tasks/task52-documentation-site.md), [57](../../archived/cli-development/tasks/task57-onboarding-and-demos.md), [60](../../archived/cli-development/tasks/task60-cli-skill.md). Task 46 folded into Task 47's Skill indexing follow-up and Task
55 into Task 62. Task 55 was restored separately on 2026-09-29 at the maintainer's
request. Its archived record preserves that earlier folding. Task 56 has no separate record: its
outcome is the 2026-09-25 archive.

Other records, such as the [beta follow-ups](tasks/beta-follow-ups.md) and the
[candidate queue](tasks/potential/_potential.md), are available on demand in
[Open CLI Tasks](tasks/_tasks.md).

## Ownership rules

- The Task record is the current authority for its scope, state, decisions, and
  implementation reality.
- A phase subtask may hold measured analysis data and acceptance evidence, but
  it cannot broaden the parent Task or create a second current plan.
- The Emerging [CLI Experience Audit](../../emerging/analysis/cli-experience-audit/_cli-experience-audit.md)
  and [CLI Design Retrospective](../../emerging/analysis/cli-design-retrospective/_cli-design-retrospective.md)
  are contextual reasoning sources. Do not rewrite them into execution records
  when code findings change.
- Completed work may be recovered from
  [Archived CLI Development](../../archived/cli-development/_cli-development.md),
  but it is not active context.

## Current next action

The combined candidate for Tasks 32, 47, 54, 64, and 70 is locally qualified
and accepted for integration. The [Task 70 record](tasks/task70-existing-workspace-adoption-during-installation.md)
contains the final gate results and source/artifact identities. Local develop
remains clean at `292ab17bc641f4d7cd3933f976dc2586e78e9899`, with Tasks 32,
47, 54, and 64 already squash-integrated. Task 70 and beta5 version preparation
remain on `ww/a_6ecaa48917f3` until the authorized window.

From 12:00 through 13:00 Europe/Zurich on 2026-10-02, squash the frozen complete
tree into develop, verify equality, push, require the exact-commit six-host
hosted Build, complete normal release integration and publish beta5, then run
public postchecks. Check the clock before each commit, merge, push, or release
start. The one-time noon follow-up is scheduled in the current thread. Preserve
the candidate and report any failed gate rather than bypassing it.

Tasks 34 and 39 remain paused with their worktrees preserved. Task 48 remains
frozen at A/B1, deferred beyond beta5 and required before 1.0. Tasks 55 and 65
remain complete. No additional task scope or individual test run is selected.
Beta4 remains public until beta5 publication succeeds. No selected beta5 task
is closed before release acceptance.
